using System.Globalization;
using UnityEngine;

// Capa social del agente
// Concentra la interpretacion de todos los mensajes ACL recibidos
// y delega cambios de plan en CapaDeliberativa
public class CapaSocial : MonoBehaviour
{
    private CerebroGuardia cerebro;
    private MemoriaGuardia memoria;
    private Comunicacion comunicacion;
    private CapaDeliberativa deliberativa;
    private GestorSubastas gestorSubastas;
    // ACCEPT_PROPOSAL recibido al ganar una subasta
    // Lo guardamos para cerrar el hilo CNP con un CONFIRM al completar la cobertura
    private MensajeACL compromisoCnpActivo = null;

    // Referencias a los componentes
    void Start()
    {
        cerebro = GetComponent<CerebroGuardia>();
        memoria = GetComponent<MemoriaGuardia>();
        comunicacion = GetComponent<Comunicacion>();
        deliberativa = GetComponent<CapaDeliberativa>();
        gestorSubastas = GetComponent<GestorSubastas>();
    }

    // Procesa todos los mensajes pendientes en el buzon
    // Lo llama el cerebro al inicio de cada Update antes del razonamiento
    public void ProcesarMensajes()
    {
        if (comunicacion == null) return;
        bool huboMensajeRelevante = false;
        // Vaciamos la cola completa antes de devolver el control al cerebro
        while (comunicacion.HayMensajes())
        {
            MensajeACL msg = comunicacion.LeerSiguiente();
            Interpretar(msg);
            huboMensajeRelevante = true;
        }
        // Si llego algo nuevo replanteamos la intencion en el momento
        if (huboMensajeRelevante && deliberativa != null)
        {
            deliberativa.DeliberarAhora();
        }
    }

    // Llamado desde la deliberativa cuando se cierra una intencion de cobertura
    // Cerramos el hilo CNP del lado del contractor con un CONFIRM al iniciador
    public void NotificarCoberturaCompletada()
    {
        if (compromisoCnpActivo == null) return;
        if (comunicacion == null) return;
        comunicacion.Responder(compromisoCnpActivo, Performativa.CONFIRM, "cobertura-completada");
        Debug.Log("[CN] " + name + ": CONFIRM enviado a " + compromisoCnpActivo.emisor.name + " cerrando subasta.");
        compromisoCnpActivo = null;
    }

    // Dispatch por tipo de performativa hacia el manejador correspondiente
    private void Interpretar(MensajeACL msg)
    {
        switch (msg.performativa)
        {
            case Performativa.INFORM:
                OnInform(msg);
                break;
            case Performativa.REQUEST:
                OnRequest(msg);
                break;
            case Performativa.AGREE:
                OnAgree(msg);
                break;
            case Performativa.REFUSE:
                OnRefuse(msg);
                break;
            case Performativa.CFP:
                OnCFP(msg);
                break;
            case Performativa.PROPOSE:
                OnPropose(msg);
                break;
            case Performativa.ACCEPT_PROPOSAL:
                OnAcceptProposal(msg);
                break;
            case Performativa.REJECT_PROPOSAL:
                OnRejectProposal(msg);
                break;
            case Performativa.CONFIRM:
                OnConfirm(msg);
                break;
            case Performativa.FAILURE:
                OnFailure(msg);
                break;
            default:
                Debug.Log("[ACL] " + name + ": performativa no gestionada: " + msg.performativa);
                break;
        }
    }

    // Otro agente nos comunica la posicion del ladron
    private void OnInform(MensajeACL msg)
    {
        Vector3 pos;
        if (!ParsearPosicion(msg.contenido, out pos))
        {
            Debug.LogWarning("[ACL] " + name + ": INFORM mal formado de " + msg.emisor.name + ": '" + msg.contenido + "'");
            return;
        }
        // Marcamos en memoria que hemos oido un grito y donde
        memoria.oyeGrito = true;
        memoria.posicionGrito = pos;
        memoria.tiempoUltimoGrito = Time.time;
    }

    // Un guardia perseguidor nos pide refuerzos
    // Aceptamos solo si estamos patrullando libres
    private void OnRequest(MensajeACL msg)
    {
        Vector3 posLadron;
        if (!ParsearPosicion(msg.contenido, out posLadron)) return;
        if (deliberativa == null || comunicacion == null) return;
        bool puedoAyudar = deliberativa.PuedeAceptarApoyo();
        if (puedoAyudar)
        {
            // Confirmamos que vamos y adoptamos la meta de apoyo
            comunicacion.Responder(msg, Performativa.AGREE, "");
            deliberativa.AdoptarApoyo(posLadron);
        }
        else
        {
            // No podemos ayudar ahora mismo
            comunicacion.Responder(msg, Performativa.REFUSE, "");
        }
    }

    // Un companero confirma que viene a apoyarnos
    // El perseguidor solo lo registra en log
    private void OnAgree(MensajeACL msg)
    {
        Debug.Log("[REQ] " + name + ": " + msg.emisor.name + " viene a apoyar la persecucion.");
    }

    // Un companero rechaza la peticion porque esta ocupado
    private void OnRefuse(MensajeACL msg)
    {
        Debug.Log("[REQ] " + name + ": " + msg.emisor.name + " no puede apoyar ahora.");
    }

    // Un guardia perseguidor subasta una salida
    // Anotamos el CFP en memoria para que GestorSubastas pueda suprimir subastas duplicadas
    // Si podemos participar mandamos PROPOSE con nuestro coste si no REFUSE
    private void OnCFP(MensajeACL msg)
    {
        // Anotamos que hay una subasta ajena en curso
        // Lo usa GestorSubastas para no relanzar otra ronda paralela
        memoria.tiempoUltimaSubastaRecibida = Time.time;
        string idSalida;
        Vector3 posSalida;
        if (!ParsearContenidoCFP(msg.contenido, out idSalida, out posSalida))
        {
            Debug.LogWarning("[CN] " + name + ": CFP mal formado: '" + msg.contenido + "'");
            return;
        }
        if (deliberativa == null || comunicacion == null) return;
        // Si no podemos participar contestamos REFUSE para cerrar el lado del contractor
        if (!deliberativa.PuedeParticiparEnSubasta())
        {
            comunicacion.Responder(msg, Performativa.REFUSE, "");
            return;
        }
        // Calculamos nuestro coste (distancia + penalizacion segun ocupacion)
        float coste = deliberativa.CalcularCoste(posSalida);
        CultureInfo inv = CultureInfo.InvariantCulture;
        comunicacion.Responder(msg, Performativa.PROPOSE, coste.ToString("0.000", inv));
    }

    // Recibimos una propuesta de uno de los CFPs que lanzamos
    // Se la pasamos al gestor de subastas para que la registre
    private void OnPropose(MensajeACL msg)
    {
        if (gestorSubastas == null) return;
        float coste;
        CultureInfo inv = CultureInfo.InvariantCulture;
        if (!float.TryParse(msg.contenido, NumberStyles.Float, inv, out coste)) return;
        gestorSubastas.RegistrarPropuesta(msg, coste);
    }

    // Hemos sido elegidos para cubrir una salida
    // Guardamos el mensaje para poder cerrar el hilo con CONFIRM al completar la tarea
    private void OnAcceptProposal(MensajeACL msg)
    {
        string idSalida;
        Vector3 posSalida;
        if (!ParsearContenidoCFP(msg.contenido, out idSalida, out posSalida)) return;
        if (deliberativa == null) return;
        Debug.Log("[CN] " + name + ": ACCEPT recibido para " + idSalida + ", adoptando cobertura.");
        compromisoCnpActivo = msg;
        deliberativa.AdoptarCubrirSalida(idSalida, posSalida);
    }

    // Otro guardia ha sido elegido para esa salida
    private void OnRejectProposal(MensajeACL msg)
    {
        Debug.Log("[CN] " + name + ": propuesta rechazada por " + msg.emisor.name + ".");
    }

    // El contractor nos confirma que ha completado la cobertura
    private void OnConfirm(MensajeACL msg)
    {
        Debug.Log("[CN] " + name + ": " + msg.emisor.name + " ha completado su cobertura.");
    }

    // El contractor nos avisa de que no ha podido completar la cobertura
    private void OnFailure(MensajeACL msg)
    {
        Debug.Log("[CN] " + name + ": " + msg.emisor.name + " ha fallado la cobertura.");
    }

    // Parseamos un string "x,y,z"
    private bool ParsearPosicion(string contenido, out Vector3 pos)
    {
        pos = Vector3.zero;
        string[] partes = contenido.Split(',');
        if (partes.Length != 3) return false;
        CultureInfo inv = CultureInfo.InvariantCulture;
        float x;
        float y;
        float z;
        if (!float.TryParse(partes[0], NumberStyles.Float, inv, out x)) return false;
        if (!float.TryParse(partes[1], NumberStyles.Float, inv, out y)) return false;
        if (!float.TryParse(partes[2], NumberStyles.Float, inv, out z)) return false;
        pos = new Vector3(x, y, z);
        return true;
    }

    // Parseamos un contenido de la forma "idSalida;x,y,z"
    // Lo usan CFP y ACCEPT_PROPOSAL
    private bool ParsearContenidoCFP(string contenido, out string idSalida, out Vector3 pos)
    {
        idSalida = "";
        pos = Vector3.zero;
        string[] partes = contenido.Split(';');
        if (partes.Length != 2) return false;
        idSalida = partes[0];
        return ParsearPosicion(partes[1], out pos);
    }
}
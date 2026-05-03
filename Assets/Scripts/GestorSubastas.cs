using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Gestor de subastas Contract Net
// Subasta secuencial: una salida a la vez para evitar adjudicaciones duplicadas
// Tras terminar la ronda emite un REQUEST de refuerzos a los que han quedado libres
public class GestorSubastas : MonoBehaviour
{
    [Header("Configuracion")]
    public float duracionSubasta = 0.5f;
    [Tooltip("Numero maximo de salidas a subastar por ronda. Si es 0 se subastan todas las disponibles.")]
    public int maxSalidasASubastar = 4;
    [Tooltip("Si recibimos un CFP de otro guardia hace menos de este tiempo no lanzamos nuestra propia ronda.")]
    public float umbralCnpAjeno = 1.5f;

    // Subasta que esta abierta en este momento
    private SubastaActiva subastaActual;
    private Comunicacion comunicacion;
    private MemoriaGuardia memoria;
    // Cola de salidas pendientes de subastar en esta ronda
    private List<Transform> salidasPendientes = new List<Transform>();
    // Guardias que ya han ganado una subasta en esta ronda
    // Los excluimos de las siguientes para que cada uno cubra una sola salida
    private List<GameObject> ganadoresPrevios = new List<GameObject>();
    // Posicion del ladron en esta ronda
    private Vector3 posLadronRonda;
    // True mientras hay una ronda en marcha
    private bool rondaEnCurso = false;

    void Awake()
    {
        comunicacion = GetComponent<Comunicacion>();
        memoria = GetComponent<MemoriaGuardia>();
    }

    void Update()
    {
        // Si la subasta actual ha expirado la resolvemos y pasamos a la siguiente
        if (subastaActual != null && !subastaActual.resuelta && subastaActual.HaExpirado())
        {
            ResolverSubasta(subastaActual);
            subastaActual = null;
            // Si quedan salidas pendientes lanzamos la siguiente sin esperar
            if (salidasPendientes.Count > 0)
            {
                LanzarSiguienteSubasta();
            }
            else if (rondaEnCurso)
            {
                // Hemos cerrado todas las subastas: la ronda termina
                rondaEnCurso = false;
                OnRondaSubastasTerminada();
            }
        }
    }

    // Inicia una ronda de subastas secuenciales por las salidas mas cercanas al ladron
    public void LanzarSubastaCobertura(Vector3 posicionLadron)
    {
        // Sin salidas registradas no hay nada que subastar
        if (Salidas.instancia == null || Salidas.instancia.puntos == null) return;
        if (comunicacion == null) return;
        // Si otro guardia ya inicio una ronda hace muy poco no relanzamos
        // Esto evita que dos guardias que ven al ladron a la vez subasten en paralelo
        if (memoria != null)
        {
            float deltaCfpAjeno = Time.time - memoria.tiempoUltimaSubastaRecibida;
            if (deltaCfpAjeno < umbralCnpAjeno)
            {
                Debug.Log("[CN] " + name + ": ronda suprimida ya hay otra en curso.");
                return;
            }
        }
        posLadronRonda = posicionLadron;
        // Reiniciamos el estado de la ronda
        ganadoresPrevios.Clear();
        salidasPendientes.Clear();
        // Calculamos las salidas mas cercanas al ladron y las metemos en la cola
        List<Transform> ordenadas = SalidasOrdenadasPorCercaniaAlLadron(posicionLadron);
        // Limitamos al maximo configurado para no subastar todas si hay muchas
        int limite = ordenadas.Count;
        if (maxSalidasASubastar > 0 && maxSalidasASubastar < limite)
        {
            limite = maxSalidasASubastar;
        }
        for (int i = 0; i < limite; i++)
        {
            salidasPendientes.Add(ordenadas[i]);
        }
        Debug.Log("[CN] " + name + ": iniciando ronda de " + salidasPendientes.Count + " subastas.");
        rondaEnCurso = true;
        // Lanzamos ya la primera subasta de la ronda
        LanzarSiguienteSubasta();
    }

    // Devuelve las salidas ordenadas de mas cercana a mas lejana al ladron
    private List<Transform> SalidasOrdenadasPorCercaniaAlLadron(Vector3 posLadron)
    {
        // Copiamos las salidas validas a una lista propia
        List<Transform> resultado = new List<Transform>();
        for (int i = 0; i < Salidas.instancia.puntos.Length; i++)
        {
            Transform s = Salidas.instancia.puntos[i];
            if (s != null) resultado.Add(s);
        }
        // Ordenacion por insercion
        for (int i = 1; i < resultado.Count; i++)
        {
            Transform actual = resultado[i];
            float distActual = Vector3.Distance(actual.position, posLadron);
            int j = i - 1;
            // Vamos desplazando hacia la derecha los elementos mayores que el actual
            while (j >= 0)
            {
                float distJ = Vector3.Distance(resultado[j].position, posLadron);
                if (distJ <= distActual) break;
                resultado[j + 1] = resultado[j];
                j--;
            }
            // Y colocamos el actual en su hueco
            resultado[j + 1] = actual;
        }
        return resultado;
    }

    // Saca la siguiente salida de la cola y emite su CFP a todo el sistema
    private void LanzarSiguienteSubasta()
    {
        if (salidasPendientes.Count == 0) return;
        // Sacamos la primera salida de la cola
        Transform salida = salidasPendientes[0];
        salidasPendientes.RemoveAt(0);
        CultureInfo inv = CultureInfo.InvariantCulture;
        // Generamos un id de conversacion unico para esta subasta
        string idHilo = System.Guid.NewGuid().ToString();
        // Contenido: nombre de la salida y su posicion separados por punto y coma
        string contenido = salida.name + ";" +
                           salida.position.x.ToString(inv) + "," +
                           salida.position.y.ToString(inv) + "," +
                           salida.position.z.ToString(inv);
        // Creamos el objeto de subasta activo y emitimos el CFP marcado como FIPA-CNP
        subastaActual = new SubastaActiva(idHilo, salida.name, salida.position, duracionSubasta);
        Debug.Log("[CN] " + name + ": subastando " + salida.name + ".");
        comunicacion.BroadcastEnConversacion(Performativa.CFP, contenido, idHilo, MensajeACL.ProtocoloContractNet);
    }

    // Recibe un PROPOSE de la subasta actual via CapaSocial
    public void RegistrarPropuesta(MensajeACL mensaje, float coste)
    {
        // Sin subasta activa o conversacion equivocada lo ignoramos
        if (subastaActual == null) return;
        if (subastaActual.conversationId != mensaje.conversationId) return;
        if (subastaActual.resuelta) return;
        // Si el proponente ya gano otra subasta en esta ronda lo descartamos
        // Asi cada guardia solo cubre una salida a lo sumo
        if (ganadoresPrevios.Contains(mensaje.emisor)) return;
        // Encolamos la propuesta para evaluarla al cerrar la subasta
        subastaActual.propuestas.Add(new PropuestaRecibida(mensaje.emisor, coste, mensaje));
    }

    // Cierra una subasta: elige al mejor proponente y notifica a todos
    private void ResolverSubasta(SubastaActiva subasta)
    {
        subasta.resuelta = true;
        // Si nadie propuso simplemente cerramos sin adjudicar
        if (subasta.propuestas.Count == 0)
        {
            Debug.Log("[CN] " + name + ": subasta de " + subasta.idSalida + " cerrada sin propuestas.");
            return;
        }
        // Buscamos la propuesta con menor coste
        PropuestaRecibida mejor = subasta.propuestas[0];
        for (int i = 1; i < subasta.propuestas.Count; i++)
        {
            if (subasta.propuestas[i].coste < mejor.coste)
            {
                mejor = subasta.propuestas[i];
            }
        }
        CultureInfo inv = CultureInfo.InvariantCulture;
        // Construimos el contenido del ACCEPT con la salida y su posicion
        string contenidoAccept = subasta.idSalida + ";" +
                                 subasta.posicionSalida.x.ToString(inv) + "," +
                                 subasta.posicionSalida.y.ToString(inv) + "," +
                                 subasta.posicionSalida.z.ToString(inv);
        // Aceptamos al mejor y rechazamos al resto
        for (int i = 0; i < subasta.propuestas.Count; i++)
        {
            PropuestaRecibida p = subasta.propuestas[i];
            if (p == mejor)
            {
                comunicacion.Responder(p.mensajeOriginal, Performativa.ACCEPT_PROPOSAL, contenidoAccept);
            }
            else
            {
                comunicacion.Responder(p.mensajeOriginal, Performativa.REJECT_PROPOSAL, "");
            }
        }
        // Marcamos al ganador para excluirlo de las subastas siguientes de esta ronda
        ganadoresPrevios.Add(mejor.proponente);
        Debug.Log("[CN] " + name + ": subasta de " + subasta.idSalida + " adjudicada a " +
                  mejor.proponente.name + " (coste " + mejor.coste.ToString("0.0", inv) + ")");
    }

    // Tras cerrar todas las subastas pedimos refuerzos directos via REQUEST
    // Los guardias que han quedado libres podran ofrecerse con un AGREE
    private void OnRondaSubastasTerminada()
    {
        Debug.Log("[CN] " + name + ": ronda terminada, pidiendo refuerzos.");
        CerebroGuardia cerebro = GetComponent<CerebroGuardia>();
        if (cerebro == null) return;
        if (comunicacion == null) return;
        // El contenido del REQUEST es la posicion del ladron en formato "x,y,z"
        CultureInfo inv = CultureInfo.InvariantCulture;
        Vector3 pos = cerebro.memoria.posicionJugador;
        string contenido = pos.x.ToString(inv) + "," + pos.y.ToString(inv) + "," + pos.z.ToString(inv);
        string idHilo = System.Guid.NewGuid().ToString();
        // Marcamos el hilo como FIPA-Request
        comunicacion.BroadcastEnConversacion(Performativa.REQUEST, contenido, idHilo, MensajeACL.ProtocoloRequest);
    }
}
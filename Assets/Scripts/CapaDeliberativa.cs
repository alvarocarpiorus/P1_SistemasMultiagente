using System.Collections.Generic;
using UnityEngine;

// Capa deliberativa del guardia
// Sigue BDI: actualiza creencias genera deseos filtra intenciones
public class CapaDeliberativa : MonoBehaviour
{
    [Header("Frecuencia de deliberacion")]
    public float intervaloDeliberacion = 1f;

    [Header("Persistencia de la intencion")]
    public float persistenciaMinima = 3f;

    [Header("Memoria del avistamiento")]
    [Tooltip("Segundos durante los cuales un avistamiento o grito siguen siendo informacion valida.")]
    public float tiempoMemoriaAvistamiento = 8f;

    [Header("Costes de las subastas")]
    public float penalizacionInvestigando = 50f;
    public float penalizacionCubriendo = 30f;

    [Header("Estado actual (lectura)")]
    public Intencion intencionActual;

    private CerebroGuardia cerebro;
    private MemoriaGuardia memoria;
    private CapaSocial social;
    // Marca temporal de la ultima vez que reconsideramos
    private float ultimaDeliberacion;

    // Inicializamos referencias y la intencion por defecto
    // Lo hacemos en Awake para estar listos antes de que nadie nos llame
    void Awake()
    {
        cerebro = GetComponent<CerebroGuardia>();
        memoria = GetComponent<MemoriaGuardia>();
        social = GetComponent<CapaSocial>();
        intencionActual = new Intencion(TipoMeta.Patrullar, Vector3.zero, "Patrullar zona");
        EscribirOrdenEnMemoria(intencionActual);
    }

    // Disparador periodico de seguridad
    void Update()
    {
        if (Time.time - ultimaDeliberacion >= intervaloDeliberacion)
        {
            Deliberar();
        }
    }

    // Disparador externo cuando pasa algo relevante
    public void DeliberarAhora()
    {
        Deliberar();
    }

    // Consulta usada por la CapaSocial al recibir un REQUEST de apoyo
    // Aceptamos si estamos patrullando libres o investigando un grito viejo
    public bool PuedeAceptarApoyo()
    {
        if (intencionActual == null) return false;
        if (intencionActual.tipo == TipoMeta.Patrullar) return true;
        if (intencionActual.tipo == TipoMeta.Investigar) return true;
        return false;
    }

    // Excluimos a los que ya estan dedicados al ladron
    public bool PuedeParticiparEnSubasta()
    {
        if (intencionActual == null) return true;
        if (intencionActual.tipo == TipoMeta.Perseguir) return false;
        if (intencionActual.tipo == TipoMeta.ApoyarPersecucion) return false;
        return true;
    }

    // Coste de cubrir una salida concreta
    // Distancia euclidea mas penalizacion segun lo ocupados que estemos
    public float CalcularCoste(Vector3 posSalida)
    {
        float distancia = Vector3.Distance(transform.position, posSalida);
        if (intencionActual == null) return distancia;
        if (intencionActual.tipo == TipoMeta.Investigar) return distancia + penalizacionInvestigando;
        if (intencionActual.tipo == TipoMeta.CubrirSalida) return distancia + penalizacionCubriendo;
        return distancia;
    }

    // Adoptamos la meta de apoyar una persecucion en curso
    // Se invoca desde CapaSocial cuando aceptamos un REQUEST
    public void AdoptarApoyo(Vector3 posLadron)
    {
        Intencion nueva = new Intencion(TipoMeta.ApoyarPersecucion, posLadron, "Apoyar persecucion");
        intencionActual = nueva;
        EscribirOrdenEnMemoria(nueva);
        Debug.Log("[BDI] " + name + " -> " + nueva.descripcion);
    }

    // Adoptamos la meta de cubrir una salida tras ganar una subasta
    public void AdoptarCubrirSalida(string idSalida, Vector3 posSalida)
    {
        Intencion nueva = new Intencion(TipoMeta.CubrirSalida, posSalida, "Cubrir " + idSalida + " (subasta)");
        intencionActual = nueva;
        EscribirOrdenEnMemoria(nueva);
        Debug.Log("[BDI] " + name + " -> " + nueva.descripcion);
    }

    // Llamado por la FSM al terminar una tarea ordenada por la deliberativa
    // Retiramos la intencion para que el guardia pueda volver a patrullar
    public void MarcarIntencionCompletada()
    {
        if (intencionActual == null) return;
        // Patrullar no se "completa" porque es la meta por defecto
        if (intencionActual.tipo == TipoMeta.Patrullar) return;
        Debug.Log("[BDI] " + name + ": intencion " + intencionActual.descripcion + " completada.");
        // Si cerramos una cobertura adjudicada por subasta cerramos el hilo CNP
        if (intencionActual.tipo == TipoMeta.CubrirSalida && social != null)
        {
            social.NotificarCoberturaCompletada();
        }
        Intencion nueva = new Intencion(TipoMeta.Patrullar, Vector3.zero, "Patrullar zona");
        intencionActual = nueva;
        EscribirOrdenEnMemoria(nueva);
    }

    // Ciclo deliberativo principal
    // Las creencias B ya estan en memoria los sensores y la social las refrescan
    // Aqui generamos deseos D filtramos a una intencion I y comparamos con la actual
    private void Deliberar()
    {
        if (intencionActual == null || memoria == null) return;
        ultimaDeliberacion = Time.time;
        // Bloqueo de persistencia para evitar comportamiento erratico
        bool puedeReconsiderar = (Time.time - intencionActual.momentoAdopcion) >= persistenciaMinima;
        // Ver al ladron es prioritario y rompe cualquier bloqueo
        bool emergencia = memoria.veAlJugador;
        if (!puedeReconsiderar && !emergencia) return;
        // Estas dos metas no se abandonan a medio camino salvo emergencia
        if (intencionActual.tipo == TipoMeta.ApoyarPersecucion && !emergencia) return;
        if (intencionActual.tipo == TipoMeta.CubrirSalida && !emergencia) return;
        // generamos los deseos activables segun nuestras creencias actuales
        List<TipoMeta> deseos = GenerarDeseos();
        // Filtramos los deseos para quedarnos con una sola intencion
        Intencion mejor = FiltrarIntenciones(deseos);
        // Solo cambiamos si es realmente distinta
        if (HaCambiado(intencionActual, mejor))
        {
            intencionActual = mejor;
            EscribirOrdenEnMemoria(mejor);
            Debug.Log("[BDI] " + name + " -> " + mejor.descripcion);
        }
    }

    // Generacion de deseos
    // Devuelve el conjunto de metas activables segun las creencias del agente
    private List<TipoMeta> GenerarDeseos()
    {
        List<TipoMeta> deseos = new List<TipoMeta>();
        // Patrullar siempre esta disponible como meta por defecto
        deseos.Add(TipoMeta.Patrullar);
        // Si tenemos cualquier informacion del ladron podemos investigar
        Vector3 posInformada;
        if (ObtenerUltimaPosicionInformada(out posInformada))
        {
            deseos.Add(TipoMeta.Investigar);
        }
        // Ver directamente al ladron habilita la meta mas urgente
        if (memoria.veAlJugador)
        {
            deseos.Add(TipoMeta.Perseguir);
        }
        return deseos;
    }

    // Filtrado de intenciones
    // Aplica el orden de prioridad sobre los deseos y construye una intencion concreta
    // Prioridad: Perseguir -> Investigar -> Patrullar
    // (CubrirSalida y ApoyarPersecucion se adoptan via mensajes ACL)
    private Intencion FiltrarIntenciones(List<TipoMeta> deseos)
    {
        if (deseos.Contains(TipoMeta.Perseguir))
        {
            return new Intencion(TipoMeta.Perseguir, memoria.posicionJugador, "Perseguir al ladron");
        }
        if (deseos.Contains(TipoMeta.Investigar))
        {
            Vector3 posInformada;
            ObtenerUltimaPosicionInformada(out posInformada);
            return new Intencion(TipoMeta.Investigar, posInformada, "Investigar avistamiento");
        }
        return new Intencion(TipoMeta.Patrullar, Vector3.zero, "Patrullar zona");
    }

    // Devuelve la posicion mas relevante del ladron disponible para nosotros
    // Considera: avistamiento directo reciente -> grito ACL reciente -> ruido pasivo
    public bool ObtenerUltimaPosicionInformada(out Vector3 pos)
    {
        pos = Vector3.zero;
        // Un avistamiento propio reciente sigue siendo informacion valida
        bool avistamientoFresco = (Time.time - memoria.tiempoUltimoAvistamiento) <= tiempoMemoriaAvistamiento;
        if (avistamientoFresco)
        {
            pos = memoria.ultimaPosicionConocida;
            return true;
        }
        // Un grito reciente tambien lo es
        bool gritoFresco = memoria.oyeGrito && (Time.time - memoria.tiempoUltimoGrito) <= tiempoMemoriaAvistamiento;
        if (gritoFresco)
        {
            pos = memoria.posicionGrito;
            return true;
        }
        if (memoria.escuchaUnSonido)
        {
            pos = memoria.posicionSonido;
            return true;
        }
        return false;
    }

    // Comparamos dos intenciones para decidir si vale la pena cambiarla
    // Distancias menores a 0.5 las consideramos el mismo destino
    private bool HaCambiado(Intencion a, Intencion b)
    {
        if (a.tipo != b.tipo) return true;
        if (Vector3.Distance(a.destino, b.destino) > 0.5f) return true;
        return false;
    }

    // Volcamos la intencion en la memoria para que la FSM la lea
    private void EscribirOrdenEnMemoria(Intencion i)
    {
        if (memoria == null) return;
        // Patrullar es el estado base no requiere "orden" explicita
        memoria.tieneOrden = (i.tipo != TipoMeta.Patrullar);
        memoria.destinoOrdenado = i.destino;
    }
}
using System.Globalization;
using UnityEngine;

// Cerebro del dragon
// Agente puramente reactivo: vuela y mientras vea al ladron lo informa
// No persigue ni ataca ni escucha mensajes
public class CerebroDragon : MonoBehaviour
{
    [Header("Frecuencia de aviso")]
    [Tooltip("Segundos minimos entre INFORMs sucesivos del mismo avistamiento.")]
    public float intervaloAviso = 1f;

    public MemoriaDragon memoria;
    public Comunicacion comunicacion;
    // Conversacion activa mientras dure el avistamiento continuo
    private string idHiloAvistamiento = null;
    // Marca temporal del ultimo INFORM emitido
    private float tiempoUltimoAviso = -999f;

    // Referencias a los componentes
    void Start()
    {
        memoria = GetComponent<MemoriaDragon>();
        comunicacion = GetComponent<Comunicacion>();
        if (comunicacion == null)
        {
            Debug.LogWarning(name + ": dragon sin Comunicacion no podra informar.");
        }
    }

    // Mientras veamos al ladron emitimos un INFORM espaciado en el tiempo
    void Update()
    {
        if (comunicacion == null) return;
        // Si dejamos de ver al ladron cerramos la conversacion abierta
        if (!memoria.veAlJugador)
        {
            idHiloAvistamiento = null;
            return;
        }
        // Si es el primer frame de avistamiento abrimos hilo nuevo
        if (idHiloAvistamiento == null)
        {
            idHiloAvistamiento = System.Guid.NewGuid().ToString();
            // Forzamos el primer aviso inmediato sin esperar al intervalo
            tiempoUltimoAviso = -999f;
        }
        // Aplicamos el throttle: solo emitimos cada intervaloAviso segundos
        if (Time.time - tiempoUltimoAviso < intervaloAviso) return;
        tiempoUltimoAviso = Time.time;
        // InvariantCulture para que el separador decimal sea siempre "."
        CultureInfo inv = CultureInfo.InvariantCulture;
        Vector3 pos = memoria.posicionJugador;
        string contenido = pos.x.ToString(inv) + "," + pos.y.ToString(inv) + "," + pos.z.ToString(inv);
        // Reusamos el mismo conversationId todos los avisos del mismo avistamiento
        comunicacion.BroadcastEnConversacion(Performativa.INFORM, contenido, idHiloAvistamiento);
    }
}
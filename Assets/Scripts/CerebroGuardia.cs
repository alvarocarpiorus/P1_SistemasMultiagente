using UnityEngine;
using UnityEngine.AI;

// Cerebro del guardia
// Maneja el ciclo del agente: percepcion comunicacion razonamiento accion

public class CerebroGuardia : MonoBehaviour
{
    [Header("Velocidades del Guardia")]
    public float velocidadPatrulla = 1.5f;
    public float velocidadInvestigacion = 2.5f;
    public float velocidadPersecucion = 3.5f;

    // Distancia minima a la que el guardia atrapa al jugador
    public float distanciaAtaque = 1.5f;

    [Header("Configuracion de Patrulla")]
    public Transform[] puntosDePatrulla;
    public float tiempoEsperaEnPunto = 2f;
    public int indicePuntoActual = 0;

    [Header("Configuracion de Busqueda")]
    public float tiempoBusqueda = 3f;

    public NavMeshAgent agente;
    public MemoriaGuardia memoria;
    public Comunicacion comunicacion;
    public CapaSocial capaSocial;
    public CapaDeliberativa capaDeliberativa;
    public GameManager gameManager;
    public Animator anim;

    // Estado actual de la FSM reactiva
    private EstadoGuardia estadoActual;
    public EstadoGuardia EstadoActual
    {
        get { return estadoActual; }
    }

    // Referencias
    // arrancamos en patrulla
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        memoria = GetComponent<MemoriaGuardia>();
        comunicacion = GetComponent<Comunicacion>();
        capaSocial = GetComponent<CapaSocial>();
        capaDeliberativa = GetComponent<CapaDeliberativa>();
        gameManager = FindObjectOfType<GameManager>();
        // Debug por consola si falta alguna pieza
        if (comunicacion == null) Debug.LogWarning(name + ": falta Comunicacion.");
        if (capaSocial == null) Debug.LogWarning(name + ": falta CapaSocial.");
        if (capaDeliberativa == null) Debug.LogWarning(name + ": falta CapaDeliberativa.");
        CambiarEstado(new EstadoPatrullando(this));
    }

    // Ciclo principal del agente
    void Update()
    {
        // Procesamos primero los mensajes recibidos
        // Asi cuando el estado razone ya tiene la memoria actualizada
        if (capaSocial != null) capaSocial.ProcesarMensajes();
        // Razonamiento del estado actual de la FSM reactiva
        if (estadoActual != null) estadoActual.Update();
        // Sincronizamos el animator con la velocidad real del NavMeshAgent
        if (anim != null)
        {
            anim.SetFloat("Velocidad", agente.velocity.magnitude);
        }
    }

    // Cambio de estado con su Exit y Enter correspondientes
    public void CambiarEstado(EstadoGuardia nuevoEstado)
    {
        if (estadoActual != null) estadoActual.Exit();
        estadoActual = nuevoEstado;
        estadoActual.Enter();
    }
}
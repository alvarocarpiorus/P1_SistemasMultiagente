using UnityEngine;
using UnityEngine.AI;

public class CerebroGuardia : MonoBehaviour
{
    // Comportamientos
    public enum Estado { Patrullando, Esperando, Persiguiendo, Investigando, Buscando, Atacando }
    
    [Header("Estado Actual del Cerebro")]
    public Estado estadoActual = Estado.Patrullando;

    [Header("Velocidades del Guardia")]
    public float velocidadPatrulla = 1.5f;       
    public float velocidadInvestigacion = 2.5f;  
    public float velocidadPersecucion = 3.5f;    
    
    // Distancia mínima a la que el guardia atrapa al jugador
    public float distanciaAtaque = 1.5f; 

    [Header("Configuración de Patrulla")]
    public Transform[] puntosDePatrulla;
    public float tiempoEsperaEnPunto = 2f;
    private int indicePuntoActual = 0;
    private float temporizadorEspera = 0f;

    [Header("Configuración de Búsqueda")]
    public float tiempoBusqueda = 3f;
    private float temporizadorBusqueda = 0f;

    // Referencias a los componentes
    private NavMeshAgent agente;
    private MemoriaGuardia memoria;
    public Animator anim; 

    void Start()
    {
        // Obtenemos los componentes
        agente = GetComponent<NavMeshAgent>();
        memoria = GetComponent<MemoriaGuardia>();
        
        // Asignamos el primer destino de la ruta
        IrAlSiguientePunto(); 
    }

    void Update()
    {
        // Si el juego ha terminado para de actualizarse
        if (estadoActual == Estado.Atacando) return;

        // Bucle principal
        EvaluarBaseDeHechos(); 
        Actuar();              
        
        // Sincronizamos la velocidad física del agente con el Animator
        if (anim != null)
        {
            anim.SetFloat("Velocidad", agente.velocity.magnitude);
        }
    }

    void EvaluarBaseDeHechos()
    {
        // Si ve al jugador empieza la persecución
        if (memoria.veAlJugador)
        {
            estadoActual = Estado.Persiguiendo;
        }
        // Si lo pierde de vista va a investigar
        else if (estadoActual == Estado.Persiguiendo && !memoria.veAlJugador)
        {
            estadoActual = Estado.Investigando;
        }
    }

    void Actuar()
    {
        // Actúa según el estado actual
        switch (estadoActual)
        {
            case Estado.Patrullando:
                agente.speed = velocidadPatrulla; 
                
                // Si llega al punto de destino
                if (!agente.pathPending && agente.remainingDistance < 0.5f)
                {
                    estadoActual = Estado.Esperando; 
                    temporizadorEspera = 0f;
                }
                break;

            case Estado.Esperando:
                temporizadorEspera += Time.deltaTime; 
                
                // Al terminar el tiempo de espera vuelve a la patrulla
                if (temporizadorEspera >= tiempoEsperaEnPunto)
                {
                    IrAlSiguientePunto();
                    estadoActual = Estado.Patrullando; 
                }
                break;

            case Estado.Persiguiendo:
                agente.speed = velocidadPersecucion; 
                // Actualiza el destino constantemente
                agente.destination = memoria.posicionJugador; 

                // Comprobamos si ha alcanzado al jugador
                float distanciaAlJugador = Vector3.Distance(transform.position, memoria.posicionJugador);
                if (distanciaAlJugador <= distanciaAtaque)
                {
                    estadoActual = Estado.Atacando; 
                    agente.isStopped = true; // Se para para atacar
                    
                    if (anim != null) anim.SetTrigger("Atacar"); 
                    
                    // Avisamos al gameManager
                    FindObjectOfType<GameManager>().PerderJuego(); 
                }
                break;

            case Estado.Investigando:
                agente.speed = velocidadInvestigacion; 
                // Va a las últimas coordenadas donde vio al jugador
                agente.destination = memoria.ultimaPosicionConocida; 
                
                if (!agente.pathPending && agente.remainingDistance < 0.5f)
                {
                    estadoActual = Estado.Buscando; 
                    temporizadorBusqueda = 0f;
                }
                break;

            case Estado.Buscando:
                // Rota sobre sí mismo
                transform.Rotate(0, 60 * Time.deltaTime, 0); 
                temporizadorBusqueda += Time.deltaTime; 
                
                // Si acaba el tiempo vuelve a la ruta
                if (temporizadorBusqueda >= tiempoBusqueda)
                {
                    IrAlSiguientePunto();
                    estadoActual = Estado.Patrullando; 
                }
                break;
        }
    }

    void IrAlSiguientePunto()
    {
        if (puntosDePatrulla.Length == 0) return; 
        
        // Asignamos el destino
        agente.destination = puntosDePatrulla[indicePuntoActual].position;
        
        // Avanza al siguiente índice
        indicePuntoActual = (indicePuntoActual + 1) % puntosDePatrulla.Length;
    }
}
using UnityEngine;

public class SensoresGuardia : MonoBehaviour
{
    [Header("Configuración de Visión")]
    public float distanciaVision = 10f;
    public float anguloVision = 90f;
    
    // Posición del ladrón
    public Transform objetivoJugador; 
    
    // LayerMask para que el motor físico sepa qué objetos son muros
    public LayerMask capaParedes; 

    private MemoriaGuardia memoria;

    void Start()
    {
        memoria = GetComponent<MemoriaGuardia>();
    }

    void Update()
    {
        // En cada frame reseteamos la visión por defecto
        memoria.veAlJugador = false;

        if (objetivoJugador == null) return;

        float distanciaAlJugador = Vector3.Distance(transform.position, objetivoJugador.position);

        // Comprobamos si el jugador está lo suficientemente cerca para ser visto
        if (distanciaAlJugador <= distanciaVision)
        {
            // Obtenemos el vector direccional desde los ojos del guardia hacia el jugador
            Vector3 direccionAlJugador = (objetivoJugador.position - transform.position).normalized;

            // Calculamos el angulo
            float anguloAlJugador = Vector3.Angle(transform.forward, direccionAlJugador);

            // Comprobamos si está dentro del cono de visión 
            if (anguloAlJugador <= anguloVision / 2f)
            {
                // Comprobamos si hay línea de visión directa
                if (!Physics.Raycast(transform.position, direccionAlJugador, distanciaAlJugador, capaParedes))
                {
                    memoria.veAlJugador = true;
                    memoria.posicionJugador = objetivoJugador.position;
                    memoria.ultimaPosicionConocida = objetivoJugador.position;
                }
            }
        }
    }
}
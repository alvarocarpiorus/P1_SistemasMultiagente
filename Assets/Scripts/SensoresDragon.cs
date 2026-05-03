using UnityEngine;

// Sensores del dragon

public class SensoresDragon : MonoBehaviour
{
    [Header("Configuracion de Vision")]
    public float distanciaVision = 30f;
    [Tooltip("Angulo del cono de vision dirigido hacia abajo. 180 = ve toda la hemiesfera inferior.")]
    public float anguloVision = 120f;
    public Transform objetivoJugador;
    public LayerMask capaParedes;

    private MemoriaDragon memoria;

    void Start()
    {
        memoria = GetComponent<MemoriaDragon>();
    }

    void Update()
    {
        // Reseteamos el flag al principio de cada frame
        memoria.veAlJugador = false;
        if (objetivoJugador == null) return;
        // Filtro 1: distancia maxima
        float distancia = Vector3.Distance(transform.position, objetivoJugador.position);
        if (distancia > distanciaVision) return;
        // Filtro 2: angulo dentro del cono cenital
        // Comparamos con Vector3.down en lugar de transform.forward para que el cono apunte siempre abajo
        Vector3 direccion = (objetivoJugador.position - transform.position).normalized;
        float angulo = Vector3.Angle(Vector3.down, direccion);
        if (angulo > anguloVision / 2f) return;
        // Filtro 3: linea de vision libre (sin techos en medio)
        if (Physics.Raycast(transform.position, direccion, distancia, capaParedes)) return;
        // Pasados los tres filtros vemos al ladron
        memoria.veAlJugador = true;
        memoria.posicionJugador = objetivoJugador.position;
    }
}
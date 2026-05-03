using UnityEngine;

// Sensores fisicos del guardia
// Vision por cono frontal con raycast contra paredes
// Audicion pasiva por radio segun el nivel de sonido del jugador
public class SensoresGuardia : MonoBehaviour
{
    [Header("Configuracion de Vision")]
    public float distanciaVision = 10f;
    public float anguloVision = 90f;
    public Transform objetivoJugador;
    public LayerMask capaParedes;

    private MemoriaGuardia memoria;
    // Cacheamos el JugadorControlador para leer su nivel de sonido
    private JugadorControlador jugador;

    void Start()
    {
        memoria = GetComponent<MemoriaGuardia>();
        if (objetivoJugador != null)
        {
            jugador = objetivoJugador.GetComponent<JugadorControlador>();
        }
    }

    void Update()
    {
        // Procesamos los dos sentidos en cada frame
        ActualizarVision();
        ActualizarSonidoDelLadron();
    }

    // Vision por cono frontal con raycast para detectar paredes en medio
    private void ActualizarVision()
    {
        // Reseteamos el flag al principio de cada frame
        memoria.veAlJugador = false;
        if (objetivoJugador == null) return;
        // Filtro 1: distancia maxima
        float distanciaAlJugador = Vector3.Distance(transform.position, objetivoJugador.position);
        if (distanciaAlJugador > distanciaVision) return;
        // Filtro 2: angulo dentro del cono frontal
        Vector3 direccionAlJugador = (objetivoJugador.position - transform.position).normalized;
        float anguloAlJugador = Vector3.Angle(transform.forward, direccionAlJugador);
        if (anguloAlJugador > anguloVision / 2f) return;
        // Filtro 3: linea de vision libre (sin paredes en medio)
        if (Physics.Raycast(transform.position, direccionAlJugador, distanciaAlJugador, capaParedes))
        {
            return;
        }
        // Pasados los filtros vemos al ladron y refrescamos memoria
        memoria.veAlJugador = true;
        memoria.posicionJugador = objetivoJugador.position;
        // Tambien actualizamos la "ultima posicion conocida" para usarla en investigaciones
        memoria.ultimaPosicionConocida = objetivoJugador.position;
        memoria.tiempoUltimoAvistamiento = Time.time;
    }

    // Audicion pasiva: el ladron emite ruido y nosotros lo oimos si estamos en su radio
    private void ActualizarSonidoDelLadron()
    {
        // Reseteamos el flag al principio de cada frame
        memoria.escuchaUnSonido = false;
        // Si no tenemos referencia al jugador o no esta haciendo ruido salimos
        if (jugador == null || jugador.nivelSonidoActual <= 0f) return;
        // Comparamos distancia con el radio de sonido emitido por el ladron
        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.transform.position);
        if (distanciaAlJugador <= jugador.nivelSonidoActual)
        {
            memoria.escuchaUnSonido = true;
            memoria.posicionSonido = jugador.transform.position;
        }
    }
}
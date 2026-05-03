using UnityEngine;

// Base de hechos del guardia
public class MemoriaGuardia : MonoBehaviour
{
    [Header("Base de Hechos - Sensores Visuales")]
    // True cuando vemos al ladron en este frame
    public bool veAlJugador = false;
    public Vector3 posicionJugador;

    [Header("Base de Hechos - Memoria a Corto Plazo")]
    // Ultima posicion donde lo vimos
    public Vector3 ultimaPosicionConocida;
    [Tooltip("Time.time en el momento del ultimo avistamiento directo del ladron.")]
    public float tiempoUltimoAvistamiento = -999f;

    [Header("Base de Hechos - Sonido Pasivo")]
    // True cuando hemos oido al ladron en este frame
    public bool escuchaUnSonido = false;
    public Vector3 posicionSonido;

    [Header("Base de Hechos - Grito ACL")]
    // True cuando hemos recibido un INFORM con la posicion del ladron
    public bool oyeGrito = false;
    public Vector3 posicionGrito;
    [Tooltip("Time.time del ultimo INFORM recibido con la posicion del ladron.")]
    public float tiempoUltimoGrito = -999f;

    [Header("Base de Hechos - Coordinacion CNP")]
    [Tooltip("Time.time del ultimo CFP recibido de otro guardia. Permite suprimir subastas duplicadas.")]
    public float tiempoUltimaSubastaRecibida = -999f;

    [Header("Base de Hechos - Orden de la Capa Deliberativa")]
    [Tooltip("True cuando la capa deliberativa ha decidido un destino distinto de patrullar.")]
    public bool tieneOrden = false;
    [Tooltip("Posicion destino indicada por la capa deliberativa.")]
    public Vector3 destinoOrdenado;
}
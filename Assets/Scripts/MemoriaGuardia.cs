using UnityEngine;

public class MemoriaGuardia : MonoBehaviour
{
    [Header("Base de Hechos - Sensores Visuales")]
    public bool veAlJugador = false;
    public Vector3 posicionJugador;
    
    [Header("Base de Hechos - Memoria a Corto Plazo")]
    public Vector3 ultimaPosicionConocida;
}
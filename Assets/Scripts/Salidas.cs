using UnityEngine;

// Lista global de salidas del mapa
// Provee la lista de puntos como un directorio
public class Salidas : MonoBehaviour
{
    // Array de Transforms asignado en el Inspector
    public Transform[] puntos;

    // Acceso global desde cualquier agente
    public static Salidas instancia;

    void Awake()
    {
        // Nos registramos como instancia unica al cargar la escena
        instancia = this;
    }
}
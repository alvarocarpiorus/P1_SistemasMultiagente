using UnityEngine;

// Mueve al dragon entre puntos aereos predefinidos

public class MovimientoDragon : MonoBehaviour
{
    [Header("Puntos de patrulla aerea")]
    public Transform[] puntosDePatrulla;

    [Header("Velocidades")]
    public float velocidadVuelo = 5f;
    public float velocidadGiro = 90f;

    [Header("Llegada a un punto")]
    // Distancia al punto a la que damos por valida la llegada
    public float distanciaLlegada = 1f;

    // Indice del punto al que nos dirigimos actualmente
    private int indicePuntoActual = 0;

    void Update()
    {
        // Sin puntos configurados no hacemos nada
        if (puntosDePatrulla == null || puntosDePatrulla.Length == 0) return;
        Transform destino = puntosDePatrulla[indicePuntoActual];
        // Calculamos direccion al destino y giramos suavemente hacia ella
        Vector3 direccion = (destino.position - transform.position).normalized;
        if (direccion.sqrMagnitude > 0.001f)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rotObjetivo,
                velocidadGiro * Time.deltaTime
            );
        }
        // Avanzamos en linea recta hacia el destino a velocidad constante
        transform.position = Vector3.MoveTowards(
            transform.position,
            destino.position,
            velocidadVuelo * Time.deltaTime
        );
        // Si hemos llegado pasamos al siguiente punto de la patrulla
        if (Vector3.Distance(transform.position, destino.position) <= distanciaLlegada)
        {
            // El operador modulo hace ciclica la patrulla
            indicePuntoActual = (indicePuntoActual + 1) % puntosDePatrulla.Length;
        }
    }
}
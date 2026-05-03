using UnityEngine;

// Camara en tercera persona
// Sigue al ladron y permite inclinar la vista vertical con el raton
public class CamaraNueva : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform objetivo;

    [Header("Posicion de la camara")]
    public float distanciaAtras = 6f;
    public float altura = 3f;
    public float alturaMirada = 1.5f;

    [Header("Seguimiento")]
    [Tooltip("Mas alto = mas rigido. 20 es casi instantaneo.")]
    public float velocidadSeguimiento = 20f;

    [Header("Inclinacion vertical (raton Y)")]
    public float sensibilidadVertical = 2f;
    public float pitchMinimo = -30f;
    public float pitchMaximo = 60f;
    public bool invertirY = false;

    // Inclinacion vertical acumulada de la camara
    private float pitch = 10f;

    // Ejecutamos la camara despues del movimiento del jugador
    void LateUpdate()
    {
        if (objetivo == null) return;
        // Leemos el delta del raton en vertical
        float deltaY = Input.GetAxis("Mouse Y") * sensibilidadVertical;
        // Por defecto subir el raton significa mirar hacia arriba
        if (!invertirY) deltaY = -deltaY;
        // Acumulamos el pitch dentro de los limites configurados
        pitch = Mathf.Clamp(pitch + deltaY, pitchMinimo, pitchMaximo);
        // La rotacion final combina nuestro pitch con el yaw del ladron
        Quaternion rotacionCamara = Quaternion.Euler(pitch, objetivo.eulerAngles.y, 0f);
        // Punto al que apuntamos la camara aproximadamente a la altura de la cabeza
        Vector3 puntoMira = objetivo.position + Vector3.up * alturaMirada;
        // Calculamos la posicion ideal detras del objetivo segun la rotacion deseada
        Vector3 offset = rotacionCamara * new Vector3(0f, 0f, -distanciaAtras);
        Vector3 posicionIdeal = puntoMira + offset + Vector3.up * (altura - alturaMirada);
        // Smoothing exponencial independiente del framerate
        float t = 1f - Mathf.Exp(-velocidadSeguimiento * Time.deltaTime);
        // Interpolamos posicion y rotacion para que el seguimiento no de tirones
        transform.position = Vector3.Lerp(transform.position, posicionIdeal, t);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionCamara, t);
    }
}
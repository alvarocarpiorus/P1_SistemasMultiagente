using UnityEngine;

public class CamaraNueva : MonoBehaviour
{
    [Header("Ajustes de Cámara")]
    public Transform objetivo; 
    
    // Parámetros
    public float distanciaAtras = 6f; 
    public float altura = 8f;         
    public float suavidad = 1f;       

    // La cámara se actualiza después del movimiento del jugador
    void LateUpdate()
    {
        if (objetivo == null) return;

        // Posición a la que se desplazará la cámara
        Vector3 posicionIdeal = objetivo.position - (objetivo.forward * distanciaAtras) + (Vector3.up * altura);

        // Interpolamos para conseguir un movimiento fluido
        transform.position = Vector3.Lerp(transform.position, posicionIdeal, suavidad * Time.deltaTime);

        // Elevamos el punto de enfoque
        Vector3 puntoDeMira = objetivo.position + new Vector3(0, 1.5f, 0); 
        transform.LookAt(puntoDeMira);
    }
}
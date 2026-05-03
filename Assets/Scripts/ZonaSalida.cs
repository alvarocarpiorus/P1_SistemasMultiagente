using UnityEngine;

// Zona objetivo del ladron
// Si entra con el tesoro gana la partida si entra sin el solo le avisamos
public class ZonaSalida : MonoBehaviour
{
    private void OnTriggerEnter(Collider otro)
    {
        // Solo reaccionamos al ladron
        if (otro.CompareTag("Player"))
        {
            // Consultamos al GameManager el estado del tesoro
            GameManager director = FindObjectOfType<GameManager>();
            if (director == null) return;
            if (director.tieneTesoro)
            {
                // Tiene el tesoro y ha llegado a la salida: victoria
                director.GanarJuego();
            }
            else
            {
                // Sin tesoro la salida no cuenta solo le recordamos el objetivo
                Debug.Log("Debes recoger el tesoro!");
            }
        }
    }
}
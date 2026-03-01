using UnityEngine;

public class ZonaSalida : MonoBehaviour
{
    private void OnTriggerEnter(Collider otro)
    {
        // Si el que entra en la zona es el ladrón
        if (otro.CompareTag("Player"))
        {
            // Comprueba si tiene el tesoro
            GameManager director = FindObjectOfType<GameManager>();

            if (director.tieneTesoro == true)
            {
                director.GanarJuego();
            }
            else
            {
                // Si no tiene el tesoro
                Debug.Log("¡Debes recoger el tesoro!");
            }
        }
    }
}
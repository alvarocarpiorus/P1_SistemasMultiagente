using UnityEngine;

// Tesoro
// Al tocarlo el ladron lo recoge y desaparece visualmente
public class Tesoro : MonoBehaviour
{
    private void OnTriggerEnter(Collider otro)
    {
        // Solo el ladron puede recoger el tesoro
        if (otro.CompareTag("Player"))
        {
            // Notificamos al GameManager que el tesoro ya esta en posesion del ladron
            GameManager director = FindObjectOfType<GameManager>();
            if (director != null)
            {
                director.RecogerTesoro();
            }
            // Ocultamos el GameObject para que no se vea mas en la escena
            gameObject.SetActive(false);
        }
    }
}
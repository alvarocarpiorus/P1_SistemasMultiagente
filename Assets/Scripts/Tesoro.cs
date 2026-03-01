using UnityEngine;

public class Tesoro : MonoBehaviour
{
    private void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Player"))
        {
            // indica al GameManager que hemos cogido el tesoro
            FindObjectOfType<GameManager>().RecogerTesoro();
            
            // Hace desaparecer las monedas
            gameObject.SetActive(false);
        }
    }
}
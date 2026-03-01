using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Textos de la Interfaz")]
    public GameObject textoVictoria;
    public GameObject textoDerrota;

    public bool tieneTesoro = false;

    private bool juegoTerminado = false;

    // Método que llamamos desde el script del Tesoro
    public void RecogerTesoro()
    {
        tieneTesoro = true;
        Debug.Log("¡Tesoro recogido!");
    }

    public void GanarJuego()
    {
        if (juegoTerminado) return; 
        
        juegoTerminado = true;
        
        // Se activa la pantalla de victoria
        textoVictoria.SetActive(true); 
        
        // Pausamos la partida
        Time.timeScale = 0f; 
    }

    public void PerderJuego()
    {
        if (juegoTerminado) return; 
        
        juegoTerminado = true;
        
        // Se activa la pantalla de Game Over
        textoDerrota.SetActive(true); 
        
        // Pausamos la partida
        Time.timeScale = 0f; 
    }
}
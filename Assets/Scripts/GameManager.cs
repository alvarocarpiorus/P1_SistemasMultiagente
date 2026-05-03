using UnityEngine;

// Gestiona el tesoro y las condiciones de victoria y derrota
public class GameManager : MonoBehaviour
{
    [Header("Textos de la Interfaz")]
    public GameObject textoVictoria;
    public GameObject textoDerrota;

    // True cuando el ladron ha recogido el tesoro
    public bool tieneTesoro = false;

    // Flag para evitar que se dispare victoria y derrota a la vez
    private bool juegoTerminado = false;

    // Llamado desde el script Tesoro cuando el ladron lo recoge
    public void RecogerTesoro()
    {
        tieneTesoro = true;
        Debug.Log("Tesoro recogido!");
    }

    // Activa la pantalla de victoria y pausa la partida
    public void GanarJuego()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        // Mostramos la pantalla de victoria en la UI
        textoVictoria.SetActive(true);
        // Pausamos el tiempo del juego
        Time.timeScale = 0f;
    }

    // Activa la pantalla de Game Over y pausa la partida
    public void PerderJuego()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        // Mostramos la pantalla de derrota en la UI
        textoDerrota.SetActive(true);
        // Pausamos el tiempo del juego
        Time.timeScale = 0f;
    }
}
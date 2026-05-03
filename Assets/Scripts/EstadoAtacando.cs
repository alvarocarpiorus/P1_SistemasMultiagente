using UnityEngine;
using System.Collections;

// Estado final: el guardia ha alcanzado al jugador
// Bloquea al jugador reproduce la animacion de ataque y dispara Game Over
public class EstadoAtacando : EstadoGuardia
{
    // Tiempo que dura la animacion antes de cerrar la partida
    private const float RetardoGameOver = 1.0f;

    public EstadoAtacando(CerebroGuardia cerebro) : base(cerebro) { }

    public override void Enter()
    {
        // Paramos al guardia en seco y limpiamos su ruta para que no siga moviendose
        cerebro.agente.isStopped = true;
        cerebro.agente.ResetPath();
        // Orientamos al guardia hacia el jugador para que el ataque se vea bien
        Vector3 dirAlJugador = cerebro.memoria.posicionJugador - cerebro.transform.position;
        // Anulamos la componente vertical para que la rotacion sea solo en horizontal
        dirAlJugador.y = 0f;
        if (dirAlJugador.sqrMagnitude > 0.001f)
        {
            cerebro.transform.rotation = Quaternion.LookRotation(dirAlJugador);
        }
        // Bloqueamos al jugador para que no pueda escapar durante la animacion
        BloquearJugador(true);
        // Disparamos el trigger de la animacion de ataque del Animator
        if (cerebro.anim != null)
        {
            cerebro.anim.SetTrigger("Atacar");
        }
        // Lanzamos la espera y luego activara el Game Over
        cerebro.StartCoroutine(EsperarYPerder());
    }

    // Espera el retardo y avisa al GameManager
    private IEnumerator EsperarYPerder()
    {
        yield return new WaitForSecondsRealtime(RetardoGameOver);
        cerebro.gameManager.PerderJuego();
    }

    public override void Update() { }

    public override void Exit() { }

    // Activa o desactiva el flag de bloqueo en el JugadorControlador
    private void BloquearJugador(bool bloquear)
    {
        JugadorControlador jugador = Object.FindObjectOfType<JugadorControlador>();
        if (jugador != null)
        {
            jugador.bloqueado = bloquear;
        }
    }
}
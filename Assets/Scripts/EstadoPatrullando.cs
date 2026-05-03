using UnityEngine;

// Estado base del guardia: recorrer en orden los puntos de patrulla asignados
public class EstadoPatrullando : EstadoGuardia
{
    public EstadoPatrullando(CerebroGuardia cerebro) : base(cerebro) { }

    public override void Enter()
    {
        // Configuramos al agente con la velocidad de patrulla y lo activamos
        cerebro.agente.speed = cerebro.velocidadPatrulla;
        cerebro.agente.isStopped = false;
        // Si entramos con una orden activa transitamos directamente a investigar
        if (cerebro.memoria.tieneOrden)
        {
            cerebro.CambiarEstado(new EstadoInvestigando(cerebro));
            return;
        }
        // Empezamos a movernos hacia el siguiente punto de la ronda
        IrAlSiguientePunto();
    }

    public override void Update()
    {
        // Ver al ladron interrumpe la patrulla
        if (cerebro.memoria.veAlJugador)
        {
            cerebro.CambiarEstado(new EstadoPersiguiendo(cerebro));
            return;
        }
        // Si la deliberativa adopta una orden mientras patrullamos la atendemos
        if (cerebro.memoria.tieneOrden)
        {
            cerebro.CambiarEstado(new EstadoInvestigando(cerebro));
            return;
        }
        // Hemos llegado al punto de patrulla: pasamos a esperar
        if (!cerebro.agente.pathPending && cerebro.agente.remainingDistance < 0.5f)
        {
            cerebro.CambiarEstado(new EstadoEsperando(cerebro));
        }
    }

    public override void Exit() { }

    // Mueve al guardia al siguiente punto de la patrulla y avanza el indice ciclico
    private void IrAlSiguientePunto()
    {
        if (cerebro.puntosDePatrulla.Length == 0) return;
        cerebro.agente.destination = cerebro.puntosDePatrulla[cerebro.indicePuntoActual].position;
        // El operador modulo hace que volvamos al inicio al pasar del ultimo punto
        cerebro.indicePuntoActual = (cerebro.indicePuntoActual + 1) % cerebro.puntosDePatrulla.Length;
    }
}
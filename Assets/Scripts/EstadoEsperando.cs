using UnityEngine;

// Estado de espera al llegar a un punto de patrulla
// Da una pausa breve antes de seguir con el siguiente punto
public class EstadoEsperando : EstadoGuardia
{
    // Tiempo acumulado parado en este punto
    private float temporizador = 0f;

    public EstadoEsperando(CerebroGuardia cerebro) : base(cerebro) { }

    public override void Enter()
    {
        temporizador = 0f;
        // Paramos al agente para que se quede quieto mientras espera
        cerebro.agente.isStopped = true;
    }

    public override void Update()
    {
        // Ver al ladron rompe la espera y pasamos a perseguir
        if (cerebro.memoria.veAlJugador)
        {
            cerebro.CambiarEstado(new EstadoPersiguiendo(cerebro));
            return;
        }
        // Si la deliberativa nos da una orden mientras esperamos la atendemos
        if (cerebro.memoria.tieneOrden)
        {
            cerebro.CambiarEstado(new EstadoInvestigando(cerebro));
            return;
        }
        // Acumulamos tiempo y al pasar el limite seguimos patrullando
        temporizador += Time.deltaTime;
        if (temporizador >= cerebro.tiempoEsperaEnPunto)
        {
            cerebro.CambiarEstado(new EstadoPatrullando(cerebro));
        }
    }

    public override void Exit()
    {
        // Reactivamos al agente para que el siguiente estado pueda moverlo
        cerebro.agente.isStopped = false;
    }
}
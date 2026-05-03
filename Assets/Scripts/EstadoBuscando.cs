using UnityEngine;

// Estado de busqueda
// Llegamos a un destino sin encontrar al ladron y rastreamos rotando un rato
public class EstadoBuscando : EstadoGuardia
{
    // Tiempo acumulado en este estado
    private float temporizador = 0f;
    // Destino que teniamos al entrar lo recordamos para detectar cambios de plan
    private Vector3 destinoEntrada;

    public EstadoBuscando(CerebroGuardia cerebro) : base(cerebro) { }

    public override void Enter()
    {
        temporizador = 0f;
        // Paramos al agente para que no se mueva mientras gira sobre si mismo
        cerebro.agente.isStopped = true;
        // Guardamos el destino actual para poder comparar despues
        destinoEntrada = cerebro.memoria.destinoOrdenado;
    }

    public override void Update()
    {
        // Ver al ladron interrumpe todo y pasamos a perseguir
        if (cerebro.memoria.veAlJugador)
        {
            cerebro.CambiarEstado(new EstadoPersiguiendo(cerebro));
            return;
        }
        // Si la deliberativa nos asigna un destino distinto al de entrada
        // significa que ha habido un cambio de plan
        if (cerebro.memoria.tieneOrden)
        {
            float dist = Vector3.Distance(cerebro.memoria.destinoOrdenado, destinoEntrada);
            if (dist > 1f)
            {
                cerebro.CambiarEstado(new EstadoInvestigando(cerebro));
                return;
            }
        }
        // Rotamos sobre nosotros mismos para rastrear visualmente la zona
        cerebro.transform.Rotate(0, 60 * Time.deltaTime, 0);
        temporizador += Time.deltaTime;
        // Cuando se acaba el tiempo de busqueda damos la tarea por completada
        if (temporizador >= cerebro.tiempoBusqueda)
        {
            // Limpiamos los flags reseteables cada frame
            cerebro.memoria.escuchaUnSonido = false;
            cerebro.memoria.tiempoUltimoAvistamiento = -999f;
            // Avisamos a la deliberativa de que la intencion esta completada
            // para que retire la orden y vuelva a Patrullar
            CapaDeliberativa deliberativa = cerebro.GetComponent<CapaDeliberativa>();
            if (deliberativa != null)
            {
                deliberativa.MarcarIntencionCompletada();
            }
            cerebro.CambiarEstado(new EstadoPatrullando(cerebro));
        }
    }

    public override void Exit()
    {
        // Reactivamos el agente para que el siguiente estado pueda moverse
        cerebro.agente.isStopped = false;
    }
}
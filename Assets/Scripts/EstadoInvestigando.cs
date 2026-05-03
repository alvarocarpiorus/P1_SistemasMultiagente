using UnityEngine;

// Estado de investigacion: ir hacia un punto donde se sospecha que esta el ladron
public class EstadoInvestigando : EstadoGuardia
{
    // Destino al que vamos guardado al entrar al estado
    private Vector3 destinoCongelado;
    // True si tenemos un destino valido al que ir
    private bool tieneDestino = false;

    public EstadoInvestigando(CerebroGuardia cerebro) : base(cerebro) { }

    public override void Enter()
    {
        // Activamos el agente y le ponemos la velocidad de investigacion
        cerebro.agente.isStopped = false;
        cerebro.agente.speed = cerebro.velocidadInvestigacion;
        // Resolvemos el destino una vez al entrar y lo guardamos
        Vector3 destino;
        if (ResolverDestinoInicial(out destino))
        {
            destinoCongelado = destino;
            tieneDestino = true;
            cerebro.agente.destination = destino;
        }
        else
        {
            // No hay destino valido caemos a patrullar en el primer Update
            tieneDestino = false;
        }
    }

    public override void Update()
    {
        // Ver al ladron interrumpe todo y pasamos a perseguir
        if (cerebro.memoria.veAlJugador)
        {
            cerebro.CambiarEstado(new EstadoPersiguiendo(cerebro));
            return;
        }
        // Si no teniamos destino al entrar volvemos a patrullar
        if (!tieneDestino)
        {
            cerebro.CambiarEstado(new EstadoPatrullando(cerebro));
            return;
        }
        // Si la deliberativa nos da una orden NUEVA con destino distinto al congelado
        // actualizamos el destino y lo congelamos al nuevo
        if (cerebro.memoria.tieneOrden)
        {
            float dist = Vector3.Distance(cerebro.memoria.destinoOrdenado, destinoCongelado);
            if (dist > 1f)
            {
                destinoCongelado = cerebro.memoria.destinoOrdenado;
                cerebro.agente.destination = destinoCongelado;
                return;
            }
        }
        // Hemos llegado al destino congelado: pasamos a buscar
        if (!cerebro.agente.pathPending && cerebro.agente.remainingDistance < 0.5f)
        {
            cerebro.CambiarEstado(new EstadoBuscando(cerebro));
        }
    }

    public override void Exit() { }

    // Resolvemos el destino al entrar al estado
    // Prioridad: orden deliberativa -> grito ACL -> sonido pasivo -> avistamiento previo reciente
    private bool ResolverDestinoInicial(out Vector3 destino)
    {
        destino = Vector3.zero;
        if (cerebro.memoria.tieneOrden)
        {
            destino = cerebro.memoria.destinoOrdenado;
            return true;
        }
        if (cerebro.memoria.oyeGrito)
        {
            destino = cerebro.memoria.posicionGrito;
            return true;
        }
        if (cerebro.memoria.escuchaUnSonido)
        {
            destino = cerebro.memoria.posicionSonido;
            return true;
        }
        // Solo usamos ultimaPosicionConocida si el avistamiento es reciente
        // Reusamos el umbral central de la capa deliberativa para no duplicar la constante
        CapaDeliberativa deliberativa = cerebro.GetComponent<CapaDeliberativa>();
        float umbral = deliberativa != null ? deliberativa.tiempoMemoriaAvistamiento : 8f;
        bool avistamientoFresco = (Time.time - cerebro.memoria.tiempoUltimoAvistamiento) <= umbral;
        if (avistamientoFresco)
        {
            destino = cerebro.memoria.ultimaPosicionConocida;
            return true;
        }
        return false;
    }
}
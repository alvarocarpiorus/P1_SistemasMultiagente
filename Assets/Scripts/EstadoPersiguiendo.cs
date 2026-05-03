using System.Globalization;
using UnityEngine;

// Estado de persecucion
// Al entrar dispara INFORM global y arranca la subasta Contract Net
public class EstadoPersiguiendo : EstadoGuardia
{
    public EstadoPersiguiendo(CerebroGuardia cerebro) : base(cerebro) { }

    public override void Enter()
    {
        // Configuramos al agente con la velocidad maxima y lo activamos
        cerebro.agente.isStopped = false;
        cerebro.agente.speed = cerebro.velocidadPersecucion;
        // Avisamos a todos los demas agentes con la posicion del ladron
        if (cerebro.comunicacion != null)
        {
            CultureInfo inv = CultureInfo.InvariantCulture;
            Vector3 pos = cerebro.memoria.posicionJugador;
            string contenido = pos.x.ToString(inv) + "," + pos.y.ToString(inv) + "," + pos.z.ToString(inv);
            // Generamos un id de conversacion para este broadcast
            string idHilo = System.Guid.NewGuid().ToString();
            cerebro.comunicacion.BroadcastEnConversacion(Performativa.INFORM, contenido, idHilo);
        }
        // Lanzamos las subastas Contract Net por las salidas mas cercanas al ladron
        // El REQUEST de refuerzos lo emitira el GestorSubastas al terminar la ronda
        GestorSubastas gestor = cerebro.GetComponent<GestorSubastas>();
        if (gestor != null)
        {
            gestor.LanzarSubastaCobertura(cerebro.memoria.posicionJugador);
        }
    }

    public override void Update()
    {
        // Refrescamos el destino del agente con la posicion actual del ladron
        cerebro.agente.destination = cerebro.memoria.posicionJugador;
        // Si lo hemos alcanzado pasamos al estado de ataque
        float distanciaAlJugador = Vector3.Distance(cerebro.transform.position, cerebro.memoria.posicionJugador);
        if (distanciaAlJugador <= cerebro.distanciaAtaque)
        {
            cerebro.CambiarEstado(new EstadoAtacando(cerebro));
            return;
        }
        // Si lo perdemos de vista pasamos a investigar su ultima posicion conocida
        if (!cerebro.memoria.veAlJugador)
        {
            cerebro.CambiarEstado(new EstadoInvestigando(cerebro));
        }
    }

    public override void Exit() { }
}
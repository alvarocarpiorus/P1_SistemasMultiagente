using System.Collections.Generic;
using UnityEngine;

// Modulo de comunicacion ACL del agente
// Unifica recepcion (cola y historial) y envio (broadcast y punto a punto)

public class Comunicacion : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Si esta activo loguea cada mensaje recibido. Desactivar para limpiar la consola.")]
    public bool logRecepcion = true;
    [Tooltip("Si esta activo silencia los mensajes provenientes de cualquier dragon.")]
    public bool silenciarDragon = true;

    // Cola de entrada que la CapaSocial drena cada frame
    private Queue<MensajeACL> mensajesPendientes = new Queue<MensajeACL>();
    // Historial completo para reconstruir conversaciones por su id
    public List<MensajeACL> historialMensajes = new List<MensajeACL>();

    // Cache estatica de todos los modulos Comunicacion del mapa
    private static List<Comunicacion> agentesEnEscena;

    // Punto de entrada de mensajes desde otros agentes
    public void Recibir(MensajeACL mensaje)
    {
        // Encolamos para procesarlo despues y guardamos en el historial
        mensajesPendientes.Enqueue(mensaje);
        historialMensajes.Add(mensaje);
        if (logRecepcion)
        {
            // El silencio del dragon afecta solo al log el mensaje se procesa igual
            bool esDragon = mensaje.emisor.GetComponent<CerebroDragon>() != null;
            if (!(esDragon && silenciarDragon))
            {
                Debug.Log("[ACL] " + name + " <- " + mensaje.performativa +
                          " de " + mensaje.emisor.name +
                          " [conv " + mensaje.conversationId.Substring(0, 6) + "]");
            }
        }
    }

    // True si hay al menos un mensaje pendiente de procesar
    public bool HayMensajes()
    {
        return mensajesPendientes.Count > 0;
    }

    // Saca el siguiente mensaje de la cola o null si esta vacia
    public MensajeACL LeerSiguiente()
    {
        if (mensajesPendientes.Count == 0) return null;
        return mensajesPendientes.Dequeue();
    }

    // Reconstruye un hilo de conversacion completo a partir del historial
    public List<MensajeACL> ObtenerConversacion(string conversationId)
    {
        List<MensajeACL> hilo = new List<MensajeACL>();
        for (int i = 0; i < historialMensajes.Count; i++)
        {
            if (historialMensajes[i].conversationId == conversationId)
            {
                hilo.Add(historialMensajes[i]);
            }
        }
        return hilo;
    }

    // Envia un mensaje a un agente concreto iniciando una conversacion nueva
    public void EnviarA(GameObject destinatario, Performativa performativa, string contenido)
    {
        Comunicacion buzonDestino = destinatario.GetComponent<Comunicacion>();
        if (buzonDestino == null)
        {
            Debug.LogWarning("[ACL] " + name + ": el destinatario " + destinatario.name + " no tiene Comunicacion.");
            return;
        }
        MensajeACL msg = new MensajeACL(performativa, gameObject, destinatario, contenido);
        buzonDestino.Recibir(msg);
    }

    // Envia un mensaje como respuesta a otro existente
    // Mantenemos el conversationId el protocol y rellenamos inReplyTo con el replyWith del original
    public void Responder(MensajeACL original, Performativa performativa, string contenido)
    {
        Comunicacion buzonDestino = original.emisor.GetComponent<Comunicacion>();
        if (buzonDestino == null)
        {
            Debug.LogWarning("[ACL] " + name + ": el emisor original no tiene Comunicacion.");
            return;
        }
        // Una respuesta dentro de una conversacion FIPA hereda su protocolo
        MensajeACL msg = new MensajeACL(
            performativa,
            gameObject,
            original.emisor,
            contenido,
            original.conversationId,
            original.replyWith,
            original.protocol
        );
        buzonDestino.Recibir(msg);
    }

    // Difunde un mensaje a todos los demas agentes con Comunicacion
    // Cada destinatario recibe un MensajeACL individual con conversationId propio
    public void Broadcast(Performativa performativa, string contenido)
    {
        if (agentesEnEscena == null)
        {
            RefrescarAgentesEnEscena();
        }
        for (int i = 0; i < agentesEnEscena.Count; i++)
        {
            Comunicacion otro = agentesEnEscena[i];
            if (otro == null || otro == this) continue;
            MensajeACL msg = new MensajeACL(performativa, gameObject, otro.gameObject, contenido);
            otro.Recibir(msg);
        }
    }

    // Difunde un mensaje compartiendo conversationId entre todos los destinatarios
    public void BroadcastEnConversacion(Performativa performativa, string contenido, string conversationId)
    {
        BroadcastEnConversacion(performativa, contenido, conversationId, null);
    }

    // Sobrecarga que ademas etiqueta el mensaje con un protocolo FIPA concreto
    public void BroadcastEnConversacion(Performativa performativa, string contenido, string conversationId, string protocol)
    {
        if (agentesEnEscena == null)
        {
            RefrescarAgentesEnEscena();
        }
        for (int i = 0; i < agentesEnEscena.Count; i++)
        {
            Comunicacion otro = agentesEnEscena[i];
            if (otro == null || otro == this) continue;
            MensajeACL msg = new MensajeACL(
                performativa,
                gameObject,
                otro.gameObject,
                contenido,
                conversationId,
                null,
                protocol
            );
            otro.Recibir(msg);
        }
    }

    // Refresca la cache de agentes
    public static void RefrescarAgentesEnEscena()
    {
        agentesEnEscena = new List<Comunicacion>(FindObjectsOfType<Comunicacion>());
    }
}
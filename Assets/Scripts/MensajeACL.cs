using System;
using UnityEngine;

// Performativas del protocolo FIPA-ACL
public enum Performativa
{
    INFORM,
    REQUEST,
    AGREE,
    REFUSE,
    CFP,
    PROPOSE,
    ACCEPT_PROPOSAL,
    REJECT_PROPOSAL,
    CONFIRM,
    FAILURE
}

// Mensaje ACL
// Lleva los identificadores que permiten reconstruir conversaciones
public class MensajeACL
{
    // Constantes con los nombres FIPA estandar de los protocolos
    public const string ProtocoloRequest = "fipa-request";
    public const string ProtocoloContractNet = "fipa-contract-net";

    public Performativa performativa;
    public GameObject emisor;
    public GameObject receptor;
    // Payload del mensaje en formato string
    // El formato depende de la performativa
    public string contenido;

    // Identificador unico de la conversacion a la que pertenece este mensaje
    // Todos los mensajes de un mismo hilo lo comparten
    public string conversationId;

    // Identificador unico de este mensaje
    // Quien nos responda lo pondra en su inReplyTo
    public string replyWith;

    // Si este mensaje es una respuesta a otro aqui va el replyWith del original
    public string inReplyTo;

    // Nombre del protocolo FIPA al que pertenece la conversacion
    // Null para INFORMs sueltos que no forman parte de un protocolo concreto
    public string protocol;

    // Marca temporal del momento en que se construyo
    public float timestamp;

    // Constructor para mensajes sueltos: inicia conversacion nueva sin responder a nadie
    public MensajeACL(Performativa perf, GameObject emi, GameObject rec, string cont)
    {
        performativa = perf;
        emisor = emi;
        receptor = rec;
        contenido = cont;
        // Conversacion nueva con id propio
        conversationId = Guid.NewGuid().ToString();
        // Cada mensaje tiene su propio id por si alguien quiere responderlo
        replyWith = Guid.NewGuid().ToString();
        inReplyTo = null;
        protocol = null;
        timestamp = Time.time;
    }

    // Constructor completo: usado al continuar una conversacion existente
    // El conversationId se hereda del hilo y inReplyTo apunta al mensaje anterior
    public MensajeACL(Performativa perf, GameObject emi, GameObject rec, string cont, string conversationId, string inReplyTo)
    {
        performativa = perf;
        emisor = emi;
        receptor = rec;
        contenido = cont;
        this.conversationId = conversationId;
        this.replyWith = Guid.NewGuid().ToString();
        this.inReplyTo = inReplyTo;
        protocol = null;
        timestamp = Time.time;
    }

    // Constructor con protocolo explicito para mensajes que abren un hilo FIPA
    public MensajeACL(Performativa perf, GameObject emi, GameObject rec, string cont, string conversationId, string inReplyTo, string protocol)
    {
        performativa = perf;
        emisor = emi;
        receptor = rec;
        contenido = cont;
        this.conversationId = conversationId;
        this.replyWith = Guid.NewGuid().ToString();
        this.inReplyTo = inReplyTo;
        this.protocol = protocol;
        timestamp = Time.time;
    }
}
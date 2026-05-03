using System.Collections.Generic;
using UnityEngine;

// Estado interno de una subasta Contract Net en curso 
public class SubastaActiva
{
    // Identificador unico de la conversacion FIPA-ACL asociada
    public string conversationId;
    // Nombre de la salida que se subasta
    public string idSalida;
    // Posicion fisica de la salida (la propaga el ACCEPT al ganador)
    public Vector3 posicionSalida;
    // Marca temporal de cuando se lanzo la subasta
    public float momentoInicio;
    // Tiempo que dejamos abierta la subasta antes de resolverla
    public float duracion;
    // Propuestas recibidas hasta el momento
    public List<PropuestaRecibida> propuestas = new List<PropuestaRecibida>();
    // True una vez resuelta para no procesarla de nuevo
    public bool resuelta = false;

    public SubastaActiva(string conversationId, string idSalida, Vector3 posicionSalida, float duracion)
    {
        this.conversationId = conversationId;
        this.idSalida = idSalida;
        this.posicionSalida = posicionSalida;
        // Guardamos cuando empezo para calcular la expiracion
        this.momentoInicio = Time.time;
        this.duracion = duracion;
    }

    // True cuando ha pasado la duracion configurada desde el inicio
    public bool HaExpirado()
    {
        return (Time.time - momentoInicio) >= duracion;
    }
}

// Datos minimos de un PROPOSE recibido por el iniciador
public class PropuestaRecibida
{
    // Quien hizo la propuesta
    public GameObject proponente;
    // Coste declarado por el proponente (menor = mejor)
    public float coste;
    // Mensaje original guardado para poder responderle con ACCEPT o REJECT
    public MensajeACL mensajeOriginal;

    public PropuestaRecibida(GameObject proponente, float coste, MensajeACL mensajeOriginal)
    {
        this.proponente = proponente;
        this.coste = coste;
        this.mensajeOriginal = mensajeOriginal;
    }
}
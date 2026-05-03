using UnityEngine;

// Tipos de meta que un guardia puede perseguir
// La capa deliberativa elige una de estas y la traduce en orden para la FSM
public enum TipoMeta
{
    Patrullar,
    Investigar,
    Perseguir,
    CubrirSalida,
    ApoyarPersecucion
}

// Una intencion adoptada por el agente
// Combina el tipo de meta su destino fisico y la marca temporal de adopcion
[System.Serializable]
public class Intencion
{
    public TipoMeta tipo;
    public Vector3 destino;
    // Debug
    public string descripcion;
    // Time.time en el momento en que adoptamos esta intencion
    // La deliberativa lo usa para aplicar la persistencia minima
    public float momentoAdopcion;

    public Intencion(TipoMeta tipo, Vector3 destino, string descripcion)
    {
        this.tipo = tipo;
        this.destino = destino;
        this.descripcion = descripcion;
        this.momentoAdopcion = Time.time;
    }
}
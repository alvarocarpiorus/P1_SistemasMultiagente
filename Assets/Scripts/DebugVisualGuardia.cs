using UnityEngine;

// Debug visual para guardias y dragon
public class DebugVisualGuardia : MonoBehaviour
{
    [Header("Posicion de la etiqueta")]
    public float alturaTexto = 2.5f;

    [Header("Visibilidad")]
    public bool mostrarEstado = true;
    public bool mostrarMemoria = true;
    public bool mostrarUltimoMensaje = true;
    public bool mostrarIntencion = true;

    // Componentes de guardia (null si es dragon)
    private CerebroGuardia cerebroGuardia;
    private MemoriaGuardia memoriaGuardia;
    private CapaDeliberativa deliberativa;
    // Componentes de dragon (null si es guardia)
    private CerebroDragon cerebroDragon;
    private MemoriaDragon memoriaDragon;
    // Comunicacion la usan los dos
    private Comunicacion comunicacion;
    private GUIStyle estilo;
    private Camera camPrincipal;

    // Referencias y camara principal
    void Start()
    {
        cerebroGuardia = GetComponent<CerebroGuardia>();
        memoriaGuardia = GetComponent<MemoriaGuardia>();
        deliberativa = GetComponent<CapaDeliberativa>();
        cerebroDragon = GetComponent<CerebroDragon>();
        memoriaDragon = GetComponent<MemoriaDragon>();
        comunicacion = GetComponent<Comunicacion>();
        camPrincipal = Camera.main;
    }

    // Pintamos la etiqueta encima del agente cada frame
    void OnGUI()
    {
        // Si la camara no esta lista la recuperamos y salimos
        if (camPrincipal == null)
        {
            camPrincipal = Camera.main;
            return;
        }
        if (estilo == null)
        {
            estilo = new GUIStyle();
            estilo.fontSize = 14;
            estilo.alignment = TextAnchor.MiddleCenter;
            estilo.normal.textColor = Color.white;
            estilo.fontStyle = FontStyle.Bold;
        }
        // Convertimos la posicion 3D a coordenadas de pantalla
        Vector3 posMundo = transform.position + Vector3.up * alturaTexto;
        Vector3 posPantalla = camPrincipal.WorldToScreenPoint(posMundo);
        // Si esta detras de la camara no dibujamos nada
        if (posPantalla.z < 0) return;
        string texto;
        Color color;
        // Decidimos texto y color segun el tipo de agente
        if (cerebroGuardia != null)
        {
            texto = ConstruirTextoGuardia();
            color = ColorPorEstadoGuardia(cerebroGuardia.EstadoActual);
        }
        else if (cerebroDragon != null)
        {
            texto = ConstruirTextoDragon();
            // Rojo si esta viendo al ladron azul claro mientras patrulla
            if (memoriaDragon != null && memoriaDragon.veAlJugador)
            {
                color = Color.red;
            }
            else
            {
                color = new Color(0.6f, 0.8f, 1f);
            }
        }
        else
        {
            // No es ni guardia ni dragon no pintamos
            return;
        }
        estilo.normal.textColor = color;
        // OnGUI dibuja con Y invertido respecto al sistema 3D asi que restamos del alto de pantalla
        Rect rect = new Rect(posPantalla.x - 110, Screen.height - posPantalla.y - 50, 220, 100);
        GUI.Label(rect, texto, estilo);
    }

    // Construye el texto de un guardia: estado intencion flags de memoria y ultimo mensaje
    private string ConstruirTextoGuardia()
    {
        string texto = "";
        if (mostrarEstado && cerebroGuardia.EstadoActual != null)
        {
            texto += "[" + name + "] " + cerebroGuardia.EstadoActual.GetType().Name + "\n";
        }
        if (mostrarIntencion && deliberativa != null && deliberativa.intencionActual != null)
        {
            texto += "Meta: " + deliberativa.intencionActual.descripcion + "\n";
        }
        if (mostrarMemoria && memoriaGuardia != null)
        {
            // Concatenamos solo los flags activos para no llenar de etiquetas vacias
            string flags = "";
            if (memoriaGuardia.veAlJugador) flags += "VE ";
            if (memoriaGuardia.escuchaUnSonido) flags += "OYE ";
            if (memoriaGuardia.oyeGrito) flags += "GRITO ";
            if (flags.Length > 0) texto += flags + "\n";
        }
        if (mostrarUltimoMensaje && comunicacion != null && comunicacion.historialMensajes.Count > 0)
        {
            // Sacamos el ultimo mensaje recibido del historial
            MensajeACL ultimo = comunicacion.historialMensajes[comunicacion.historialMensajes.Count - 1];
            texto += "Recibido: " + ultimo.performativa + " de " + ultimo.emisor.name;
        }
        return texto;
    }

    // Texto del dragon muy corto porque solo tiene dos modos
    private string ConstruirTextoDragon()
    {
        string texto = "[" + name + "] Dragon\n";
        if (memoriaDragon != null && memoriaDragon.veAlJugador)
        {
            texto += "VE -> INFORM\n";
        }
        else
        {
            texto += "Patrullando aire\n";
        }
        return texto;
    }

    // Color segun el estado FSM
    private Color ColorPorEstadoGuardia(EstadoGuardia estado)
    {
        if (estado == null) return Color.white;
        switch (estado.GetType().Name)
        {
            case "EstadoPatrullando":
                return Color.green;
            case "EstadoEsperando":
                return Color.cyan;
            case "EstadoInvestigando":
                return Color.yellow;
            case "EstadoBuscando":
                return new Color(1f, 0.5f, 0f);
            case "EstadoPersiguiendo":
                return Color.red;
            case "EstadoAtacando":
                return Color.magenta;
            default:
                return Color.white;
        }
    }
}
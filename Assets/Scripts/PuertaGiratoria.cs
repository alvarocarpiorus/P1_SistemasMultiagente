using UnityEngine;
using UnityEngine.AI;

// Puerta giratoria
// Se abre cuando alguien entra en su trigger y se cierra al salir

[RequireComponent(typeof(NavMeshObstacle))]
public class PuertaGiratoria : MonoBehaviour
{
    [Header("Configuracion de Rotacion")]
    public float anguloApertura = 90f;
    public float velocidadApertura = 3f;

    // Rotacion inicial guardada como referencia de "cerrada"
    private Quaternion rotacionCerrada;
    // Rotacion calculada de "abierta" segun el lado del que vienen
    private Quaternion rotacionAbierta;
    // True mientras hay alguien dentro del trigger
    private bool abriendo = false;

    private NavMeshObstacle obstaculo;

    // Numero de cuerpos actualmente dentro del trigger
    // Permite que la puerta se mantenga abierta si entran varios a la vez
    private int ocupantes = 0;

    void Start()
    {
        // Guardamos la rotacion inicial como referencia de cerrada
        rotacionCerrada = transform.rotation;
        // Por defecto la apertura va hacia +anguloApertura
        rotacionAbierta = rotacionCerrada * Quaternion.Euler(0f, anguloApertura, 0f);
        obstaculo = GetComponent<NavMeshObstacle>();
        // Carving permite al NavMesh "tallar" el obstaculo dinamicamente
        obstaculo.carving = true;
        // Empieza activo bloqueando el paso
        obstaculo.enabled = true;
    }

    void Update()
    {
        // Elegimos la rotacion destino segun si estamos abriendo o cerrando
        Quaternion destino = abriendo ? rotacionAbierta : rotacionCerrada;
        // Rotamos hacia el destino a velocidad constante
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            destino,
            velocidadApertura * 100f * Time.deltaTime
        );
        // Mientras esta abierta o casi abierta desactivamos el obstaculo del NavMesh
        // Y lo reactivamos cuando volvemos a estar casi cerrados
        float anguloRestante = Quaternion.Angle(transform.rotation, rotacionAbierta);
        if (abriendo && anguloRestante < 5f)
        {
            obstaculo.enabled = false;
        }
        else if (!abriendo && Quaternion.Angle(transform.rotation, rotacionCerrada) < 5f)
        {
            obstaculo.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider otro)
    {
        // Solo la abren el jugador y los guardias
        if (!otro.CompareTag("Player") && !otro.CompareTag("Guardia")) return;
        ocupantes++;
        // Calculamos el lado del que viene el agente para abrir hacia el lado opuesto
        Vector3 dirAlActor = (otro.transform.position - transform.position).normalized;
        float lado = Vector3.Dot(transform.forward, dirAlActor);
        // Si esta delante el signo es positivo y abriremos hacia atras y al reves
        float signo = lado >= 0f ? -1f : 1f;
        rotacionAbierta = rotacionCerrada * Quaternion.Euler(0f, signo * anguloApertura, 0f);
        abriendo = true;
    }

    private void OnTriggerExit(Collider otro)
    {
        if (!otro.CompareTag("Player") && !otro.CompareTag("Guardia")) return;
        // Decrementamos asegurando que no baja de cero
        ocupantes = Mathf.Max(0, ocupantes - 1);
        // Solo cerramos cuando ya no queda nadie dentro
        if (ocupantes == 0)
        {
            abriendo = false;
        }
    }
}
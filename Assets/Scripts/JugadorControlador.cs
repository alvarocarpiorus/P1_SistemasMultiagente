using UnityEngine;

// Controlador del ladron
// Movimiento adelante/atras con W/S y rotacion horizontal con el raton
// El nivel de sonido emitido depende de si va andando o corriendo
public class JugadorControlador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMarcha = 3f;
    public float velocidadCarrera = 6f;

    [Header("Control de Camara / Rotacion")]
    public float sensibilidadRaton = 2f;
    public bool invertirRaton = false;

    [Header("Sistema de Sonido")]
    public float nivelSonidoMarcha = 3f;
    public float nivelSonidoCarrera = 8f;
    // Lo lee SensoresGuardia para decidir si oye al ladron
    public float nivelSonidoActual = 0f;

    [Header("Animacion")]
    public Animator anim;

    // Flag publico para bloquear el control desde fuera
    // (cuando un guardia inicia EstadoAtacando)
    [HideInInspector] public bool bloqueado = false;

    private Rigidbody rb;
    // Yaw acumulado del personaje
    private float yaw;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Inicializamos el yaw con la orientacion actual en la escena
        yaw = transform.eulerAngles.y;
        // Bloqueamos el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Lectura de input en Update
    // El movimiento fisico va en FixedUpdate
    void Update()
    {
        if (bloqueado) return;
        // Acumulamos el delta horizontal del raton en el yaw
        float deltaRaton = Input.GetAxis("Mouse X") * sensibilidadRaton;
        if (invertirRaton) deltaRaton = -deltaRaton;
        yaw += deltaRaton;
    }

    void FixedUpdate()
    {
        // Si estamos bloqueados anulamos toda fisica y salimos
        if (bloqueado)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            nivelSonidoActual = 0f;
            if (anim != null) anim.SetFloat("Velocidad", 0f);
            return;
        }
        // Aplicamos el yaw acumulado como rotacion del Rigidbody
        Quaternion rotObjetivo = Quaternion.Euler(0f, yaw, 0f);
        rb.MoveRotation(rotObjetivo);
        // Leemos el avance del eje vertical (W/S o flechas arriba/abajo)
        float avance = Input.GetAxisRaw("Vertical");
        // Shift para correr
        bool estaCorriendo = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float velocidadActual = estaCorriendo ? velocidadCarrera : velocidadMarcha;
        // Calculamos el desplazamiento en la direccion forward del personaje
        Vector3 movimiento = transform.forward * avance * velocidadActual * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movimiento);
        // Actualizamos el nivel de sonido segun si nos movemos y a que velocidad
        if (avance != 0f)
        {
            nivelSonidoActual = estaCorriendo ? nivelSonidoCarrera : nivelSonidoMarcha;
        }
        else
        {
            nivelSonidoActual = 0f;
        }
        // Sincronizamos la animacion con el avance
        if (anim != null)
        {
            anim.SetFloat("Velocidad", Mathf.Abs(avance));
        }
    }

    // Al perder o recuperar foco de la ventana liberamos o volvemos a bloquear el cursor
    void OnApplicationFocus(bool enFoco)
    {
        Cursor.lockState = enFoco ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enFoco;
    }
}
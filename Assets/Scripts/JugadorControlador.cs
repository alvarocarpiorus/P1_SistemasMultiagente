using UnityEngine;

public class JugadorControlador : MonoBehaviour
{
    [Header("Ajustes del Jugador")]
    // Parámetros
    public float velocidad = 5f;
    public float velocidadRotacion = 150f; 

    [Header("Animación")]
    public Animator anim; 
    private Rigidbody rb;

    void Start()
    {
        // Obtenemos y guardamos el Rigidbody
        rb = GetComponent<Rigidbody>();
    }

    // FixedUpdate porque es un objeto con físicas
    void FixedUpdate() 
    {
        // Inputs del teclado
        float rotacionX = Input.GetAxis("Horizontal");  // giro progresivo 
        float movimientoZ = Input.GetAxisRaw("Vertical"); // avance y retroceso instantáneo

        // Calculamos y aplicamos el desplazamiento
        Vector3 movimiento = transform.forward * movimientoZ * velocidad * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movimiento);
        // Calculamos y aplicamos la rotación
        Quaternion giro = Quaternion.Euler(0f, rotacionX * velocidadRotacion * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * giro);

        // Sincronizamos la acción del teclado con el sistema de animaciones
        if (anim != null)
        {
            // Valor absoluto para que la animación de caminar se reproduzca también si caminamos hacia atrás
            anim.SetFloat("Velocidad", Mathf.Abs(movimientoZ)); 
        }
    }
}
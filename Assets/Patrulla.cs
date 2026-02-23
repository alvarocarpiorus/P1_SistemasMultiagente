using UnityEngine;

using UnityEngine.AI; // IMPORTANTE: Necesario para usar el NavMesh



public class Patrulla : MonoBehaviour {



    // Variables que veremos en el Inspector

    public NavMeshAgent agente; // El motor de movimiento del cubo

    public Transform[] puntosDePatrulla; // Una lista (array) para poner tus puntos A y B



    // Variable privada para saber a qué punto estamos yendo (0 es el primero, 1 el segundo, etc.)

    private int indiceActual = 0;



    void Start() {

        // Configuramos el agente para que no se frene solo al llegar, para que el movimiento sea fluido

        agente.autoBraking = false;



        // Mandamos al cubo al primer punto nada más empezar

        IrAlSiguientePunto();

    }



    void Update() {

        // Aquí preguntamos en cada frame:

        // ¿El agente está activo? Y... ¿Le falta muy poco (menos de 0.5 metros) para llegar?

        if (!agente.pathPending && agente.remainingDistance < 0.5f) {

            // Si ya llegó, le decimos que vaya al siguiente

            IrAlSiguientePunto();

        }

    }



    void IrAlSiguientePunto() {

        // Si no hemos puesto puntos en el inspector, no hacemos nada para evitar errores

        if (puntosDePatrulla.Length == 0) return;



        // Le decimos al agente que su destino es la posición del punto actual

        agente.destination = puntosDePatrulla[indiceActual].position;



        // Sumamos 1 al índice para que el próximo destino sea el siguiente punto

        // El símbolo % (módulo) hace que si llegamos al final, vuelva a empezar desde 0 (bucle)

        indiceActual = (indiceActual + 1) % puntosDePatrulla.Length;

    }

}
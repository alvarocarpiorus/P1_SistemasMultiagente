using UnityEngine;

// Clase abstracta base para todos los estados de la FSM del guardia

public abstract class EstadoGuardia
{
    // Referencia al cerebro para acceder a todos los componentes del guardia
    protected CerebroGuardia cerebro;

    // El estado guarda la referencia al cerebro al que pertenece
    public EstadoGuardia(CerebroGuardia cerebro)
    {
        this.cerebro = cerebro;
    }

    // Se llama una sola vez al entrar en el estado
    public abstract void Enter();

    // Se llama en cada frame mientras el estado esta activo
    public abstract void Update();

    // Se llama una sola vez al salir del estado
    public abstract void Exit();
}
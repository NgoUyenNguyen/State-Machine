using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Provides a base class for managing state transitions in a state machine, where each state is represented by an
/// enumeration value.
/// </summary>
/// <remarks>This class is designed to be extended by derived classes that define specific states and their
/// behaviors.  It manages the lifecycle of states, including entering, updating, and exiting states, as well as
/// handling transitions between them.  The state machine operates by maintaining a dictionary of states, where each
/// state is associated with a key of type <typeparamref name="Estate"/>. The current state is updated on each frame,
/// and transitions are triggered based on the logic defined in the current state's implementation.  This class also
/// integrates with Unity's event system, forwarding collision and trigger events to the current state.</remarks>
/// <typeparam name="Estate">The enumeration type representing the possible states of the state machine. Each state must be a unique value of
/// this enumeration.</typeparam>
public abstract class StateManager<Estate> : MonoBehaviour where Estate : Enum
{
    /// <summary>
    /// Dictionary to hold all states with their corresponding keys
    /// </summary>
    protected Dictionary<Estate, BaseState<Estate>> statesDictionary = new Dictionary<Estate, BaseState<Estate>>();
   
    private BaseState<Estate> currentState;
    /// <summary>
    /// The current state of the state machine
    /// </summary>
    public BaseState<Estate> CurrentState
    {
        get => currentState;
        protected set => currentState = value;
    }

    /// <summary>
    /// Delegate fired when the state machine transitions from one state to another.
    /// </summary>
    /// <typeparam name="Estate">Enum type representing states.</typeparam>
    /// <param name="fromState">The previous state before the transition.</param>
    /// <param name="toState">The new state after the transition.</param>
    public delegate void StateChangedHandler(Estate fromState, Estate toState);
    /// <summary>
    /// Callback fired whenever state changes
    /// </summary>
    public event StateChangedHandler OnStateChanged;

    private bool isTransitioning;











    private void Awake()
    {
        // Initialize the states dictionary with states defined in derived classes
        InitializeStates();
        // Call the OnAwake method to allow derived classes to setup logic
        OnAwake();
    }

    private void Start()
    {
        // Assert that the current state is set before starting the state machine
        Assert.IsNotNull(currentState, $"Current state of {this.name} must be set before starting the state machine.");
        // Initialize the state machine with the states defined in derived classes
        CurrentState.EnterState();
        // Call the OnStart method to allow derived classes to perform any additional setup
        OnStart();
    }

    private void Update()
    {
        // Assert that the current state is not null before updating
        Assert.IsNotNull(CurrentState, $"Current state of {this.name} must be not null.");
        // Field to hold the next state key
        Estate nextStateKey = CurrentState.GenerateNextState();

        // Check if needing to transition to a new state or using update mathod of the current state
        if (!isTransitioning && nextStateKey.Equals(currentState.StateKey))
        {
            // If the next state is the same as the current state, meaning no transition is needed
            CurrentState.UpdateState();
        }
        else if (!isTransitioning)
        {
            // If a transition is needed, call the TransitionToState method with the next state key
            TransitionToState(nextStateKey);
        }
    }








    


    // Method to change the current state
    private void TransitionToState(Estate nextStateKey)
    {
        if (!statesDictionary.TryGetValue(nextStateKey, out var nextState))
        {
            Debug.LogError($"{name}: Cannot transition to undefined state: {nextStateKey}");
            return;
        }

        isTransitioning = true;

        Estate previousStateKey = CurrentState.StateKey;
        CurrentState.ExitState();
        CurrentState = statesDictionary[nextStateKey];
        CurrentState.EnterState();

        OnStateChanged?.Invoke(previousStateKey, nextStateKey);

        isTransitioning = false;
    }

    /// <summary>
    /// Method to initialize states in derived classes
    /// </summary>
    /// <remarks>
    /// You must register all states by adding them to the statesDictionary and set the current state to one of the states in the Dictionary.
    /// </remarks>
    public abstract void InitializeStates();
    /// <summary>
    /// Method is called in the Awake() method of the StateManager 
    /// </summary>
    public virtual void OnAwake() { }
    /// <summary>
    /// Method is called in the Start() method of the StateManager 
    /// </summary>
    public virtual void OnStart() { }
}

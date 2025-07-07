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
/// state is associated with a key of type <typeparamref name="EState"/>. The current state is updated on each frame,
/// and transitions are triggered based on the logic defined in the current state's implementation.  This class also
/// integrates with Unity's event system, forwarding collision and trigger events to the current state.</remarks>
/// <typeparam name="EState">The enumeration type representing the possible states of the state machine. Each state must be a unique value of
/// this enumeration.</typeparam>
public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    /// <summary>
    /// Dictionary to hold all states with their corresponding keys
    /// </summary>
    private Dictionary<EState, BaseState<EState>> statesDictionary = new Dictionary<EState, BaseState<EState>>();

    private BaseState<EState> _currentState;
    /// <summary>
    /// The current state of the state machine
    /// </summary>
    public BaseState<EState> CurrentState
    {
        get => _currentState;
        private set => _currentState = value;
    }

    /// <summary>
    /// Delegate fired when the state machine transitions from one state to another.
    /// </summary>
    /// <typeparam name="Estate">Enum type representing states.</typeparam>
    /// <param name="fromState">The previous state before the transition.</param>
    /// <param name="toState">The new state after the transition.</param>
    public delegate void StateChangedHandler(EState fromState, EState toState);
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
        // Set the CurrentState to the EntryState
        CurrentState = GetState(InitializeEntryState());
        // Assert that the current state is set before starting the state machine
        Assert.IsNotNull(_currentState, $"Current state of {this.name} must be set before starting the state machine.");
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
        EState nextStateKey = CurrentState.GenerateNextState();

        // Check if needing to transition to a new state or using update mathod of the current state
        if (!isTransitioning && nextStateKey.Equals(CurrentState.StateKey))
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
    private void TransitionToState(EState nextStateKey)
    {
        if (!statesDictionary.TryGetValue(nextStateKey, out var nextState))
        {
            Debug.LogError($"{name}: Cannot transition to undefined state: {nextStateKey}");
            return;
        }

        isTransitioning = true;

        EState previousStateKey = CurrentState.StateKey;
        CurrentState.ExitState();
        CurrentState = statesDictionary[nextStateKey];
        CurrentState.EnterState();

        OnStateChanged?.Invoke(previousStateKey, nextStateKey);

        isTransitioning = false;
    }

    private 

    /// <summary>
    /// Method to add a states to the state machine
    /// </summary>
    protected void AddStates(params BaseState<EState>[] states)
    {
        foreach (var state in states)
        {
            if (statesDictionary.ContainsKey(state.StateKey))
            {
                Debug.LogError($"{name}: State {state.StateKey} already exists in the state machine.");
                return;
            }
            statesDictionary.Add(state.StateKey, state);
        }
    }

    /// <summary>
    /// Method to remove a states from the state machine
    /// </summary>
    protected void RemoveStates(params BaseState<EState>[] states)
    {
        foreach (var state in states)
        {
            if (!statesDictionary.ContainsKey(state.StateKey))
            {
                Debug.LogError($"{name}: State {state.StateKey} does not exist in the state machine.");
                return;
            }
            statesDictionary.Remove(state.StateKey);
        }
    }

    protected BaseState<EState> GetState(EState key)
    {
        if (!statesDictionary.ContainsKey(key))
        {
            Debug.LogError($"{name}: State {key} does not exist in the state machine.");
            return null;
        }
        return statesDictionary[key];
    }

    /// <summary>
    /// Method to initialize states in StatesDictionary
    /// </summary>
    /// <remarks>
    /// Register all states by AddStates() to the statesDictionary.
    /// </remarks>
    protected abstract void InitializeStates();
    /// <summary>
    /// Method to set the EntryState
    /// </summary>
    /// <returns>
    /// returns key of EntryState
    /// </returns>
    protected abstract EState InitializeEntryState();
    /// <summary>
    /// Method is called in the Awake() method of the StateManager 
    /// </summary>
    protected virtual void OnAwake() { }
    /// <summary>
    /// Method is called in the Start() method of the StateManager 
    /// </summary>
    protected virtual void OnStart() { }
}

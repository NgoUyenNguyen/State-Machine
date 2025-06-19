# State-Machine
Base system to create a state framework, including an abstract class BaseState and an abstract class StateMachine. For using, recommend creating an intermediate abstract class State inheriting from the BaseState to add more logic.

The BaseState includes:
- StateKey: a key equivalent to each State for StateMachine managing
- abstract void EnterState(): Methods called once when a State is entered
- abstract void UpdateState(): Method called every frame while in the State
- abstract void ExitState(): Methods called once when a State is exited
- abstract Estate GetNextState(): Method to get the next StateKey based on the current state logic
- 
The StateMachine includes:
- statesDictionary: Dictionary to hold all states with their corresponding keys
- CurrentState: The current state of the state machine (get; protected set;)
- abstract void InitializeStates(): Initialize states in derived classes. You must register all states by adding them to the statesDictionary and set the current state to one of the states.
- virtual void OnAwake(): Method is called in the Awake() method of the StateManager
- virtual void OnStart(): Method is called in the Start() method of the StateManager

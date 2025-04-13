namespace SoulEngine.SequenceScript.Machine;

public class InterruptContext
{

    public bool PauseExecution
    {
        get => ExecutionState.PauseExecution;
        set => ExecutionState.PauseExecution = value;
    }
    
    public readonly ExecutionState ExecutionState;

    public string InterruptID;
    
    public string InvalidInterruptID;
    public OpCode InvalidOpCodeID;
    
    public InterruptContext(ExecutionState executionState)
    {
        ExecutionState = executionState;
    }

    public void PushStack(DynValue dynValue)
    {
        ExecutionState.PushStack(dynValue);
    }
    
    public DynValue PopStack()
    {
        return ExecutionState.PopStack();
    }
    
    public T? GetHostData<T>(string id)
    {
        return ExecutionState.GetHostData<T>(id);
    }

    public void SetHostData<T>(string id, T? value)
    {
        ExecutionState.SetHostData(id, value);
    }
}

public delegate void InterruptCallback(InterruptContext interruptContext);
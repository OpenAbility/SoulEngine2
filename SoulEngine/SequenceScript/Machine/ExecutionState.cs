namespace SoulEngine.SequenceScript.Machine;

public class ExecutionState
{
    public bool IsFinished { get; private set; } = false;

    public bool PauseExecution;

    public event Action<ExecutionState> OnFinished = state => {};
    
    private readonly List<StackLevel> executionStack = new List<StackLevel>();
    private readonly Stack<DynValue> stack = new Stack<DynValue>();

    private readonly Dictionary<string, object?> hostData = new Dictionary<string, object?>();

    public ExecutionState()
    {
        
    }

    internal void TriggerFinish()
    {
        IsFinished = true;
        OnFinished(this);
    }

    public void PushStack(DynValue dynValue)
    {
        stack.Push(dynValue);
    }

    public DynValue PopStack()
    {
        return stack.Pop();
    }
    
    public void PushExecutionStack(BinaryModule module, string functionName)
    {
        executionStack.Add(new StackLevel(module, functionName));
    }

    public void Step()
    {
        StackLevel stackLevel = executionStack[^1];
        stackLevel.CurrentInstructionIndex++;
        executionStack[^1] = stackLevel;
    }
    
    public void Jump(int instruction)
    {
        StackLevel stackLevel = executionStack[^1];
        stackLevel.CurrentInstructionIndex = instruction;
        executionStack[^1] = stackLevel;
    }
    
    public void PopExecutionStack()
    {
        executionStack.RemoveAt(executionStack.Count - 1);
    }

    public StackLevel CurrentExecutionStackLevel => executionStack[^1];

    public IEnumerable<StackLevel> StackTrace => executionStack;

    public int ExecutionStackSize => executionStack.Count;

    public void SetValue(int resolutionIndex, string parameter, DynValue value)
    {
        if (resolutionIndex == 0)
        {
            CurrentExecutionStackLevel.LocalVariables[parameter] = value;
            return;
        }

        CurrentExecutionStackLevel.Module.GetResolve(resolutionIndex).SetGlobal(parameter, value);
    }
    
    public DynValue GetValue(int resolutionIndex, string parameter)
    {
        if (resolutionIndex == 0)
        {
            return CurrentExecutionStackLevel.LocalVariables[parameter];
        }
        
        return CurrentExecutionStackLevel.Module.GetResolve(resolutionIndex).GetGlobal(parameter);
    }

    public T? GetHostData<T>(string id)
    {
        object? stored = hostData.GetValueOrDefault(id, null);
        if (stored is T?)
            return (T?)stored;
        return default;
    }

    public void SetHostData<T>(string id, T? value)
    {
        hostData[id] = value;
    }
}

public class StackLevel
{
    public readonly BinaryModule Module;
    public readonly string FunctionName;
    public readonly Dictionary<string, DynValue> LocalVariables;
    
    public int CurrentInstructionIndex;
    
    public StackLevel(BinaryModule module, string functionName)
    {
        Module = module;
        FunctionName = functionName;
        LocalVariables = new Dictionary<string, DynValue>();
        
        CurrentInstructionIndex = 0;
    }

    public Instruction GetCurrentInstruction()
    {
        return Module.GetProcedureInstruction(FunctionName, CurrentInstructionIndex);
    }
}
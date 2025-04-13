using System.Text;
using SoulEngine.SequenceScript.Utility;

namespace SoulEngine.SequenceScript.Machine;

public class ExecutionContext
{
    public const string InvalidInterrupt = "$INVALID_INTERRUPT";
    public const string InvalidOpCode = "$INVALID_OPCODE";
    
    private readonly IModuleResolver moduleResolver;
    private readonly Dictionary<string, BinaryModule> modules = new Dictionary<string, BinaryModule>();
    private readonly Dictionary<string, InterruptCallback> interrupts = new Dictionary<string, InterruptCallback>();
    public ExecutionContext(IModuleResolver moduleResolver)
    {
        this.moduleResolver = moduleResolver;
    }

    public BinaryModule LoadModule(string resolvePath)
    {
        if (modules.TryGetValue(resolvePath, out BinaryModule? module))
            return module;
        
        using Stream stream = moduleResolver.LoadModule(resolvePath);
        using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);
        module = new BinaryModule(this);

        // Verify the header
        string header = new string(reader.ReadChars(4));
        if (header != "SEQS")
            throw new Exception("Malformed module header!");
        
        // Read the version
        short major = reader.ReadInt16();
        short minor = reader.ReadInt16();
        short build = reader.ReadInt16();
        
        // Basic version checks
        if (major != SequenceRules.Version.Major)
            throw new Exception($"Incompatible module version {major}.{minor}.{build}");
        
        if(major == 0 && minor != SequenceRules.Version.Minor)
            throw new Exception($"Incompatible module version {major}.{minor}.{build}");
        
        if(minor > SequenceRules.Version.Minor)
            throw new Exception($"Incompatible module version {major}.{minor}.{build}");

        // It's valid - store it
        modules[resolvePath] = module;
        
        // Resolve metadata
        int metaSize = reader.ReadInt32();
        for (int i = 0; i < metaSize; i++)
        {
            module.SetMeta(reader.ReadString(), reader.ReadString());
        }
        
        // Resolve resolution indices
        int resolutionCount = reader.ReadInt32();
        for (int i = 0; i < resolutionCount; i++)
        {
            string key = reader.ReadString();
            int index = reader.ReadInt32();
            
            if(key == "__LOCAL")
                continue;
            if(key == "__STDLIB")
                continue;
            
            module.RegisterModuleMapping(index, LoadModule(key));
        }
        
        // Resolve globals
        int globalCount = reader.ReadInt32();
        for (int i = 0; i < globalCount; i++)
        {
            string key = reader.ReadString();
            ValueType globalType = (ValueType)reader.ReadByte();
            
            module.RegisterGlobal(key, globalType);
        }
        
        // Resolve functions
        int functionCount = reader.ReadInt32();
        for (int i = 0; i < functionCount; i++)
        {
            string key = reader.ReadString();
            int instructionCount = reader.ReadInt32();

            Instruction[] instructions = new Instruction[instructionCount];
            
            for (int j = 0; j < instructionCount; j++)
            {
                OpCode opCode = (OpCode)reader.ReadByte();
                DynValue? parameter = null;

                ValueType? expectedType = SequenceRules.GetOperandType(opCode);

                if (expectedType == ValueType.String)
                    parameter = new DynValue(reader.ReadString());
                else if (expectedType == ValueType.Boolean)
                    parameter = new DynValue(reader.ReadBoolean());
                else if (expectedType == ValueType.Floating)
                    parameter = new DynValue(reader.ReadSingle());
                else if (expectedType == ValueType.Integer)
                    parameter = new DynValue(reader.ReadInt32());

                Instruction instruction = new Instruction(opCode, parameter ?? new DynValue(ValueType.Bogus, null!));

                instructions[j] = instruction;
            }
            
            module.RegisterProcedure(key, instructions);
        }
        
        
        return module;
    }

    public ExecutionState CreateExecution(BinaryModule module, string function)
    {
        ExecutionState executionState = new ExecutionState();
        executionState.IsFinished = false;
        
        executionState.PushExecutionStack(module, function);

        return executionState;
    }
    
    private void RaiseInterrupt(string id, InterruptContext context)
    {
        if (interrupts.TryGetValue(id, out var handler))
        {
            context.InterruptID = id;
            handler(context);
            return;
        }

        if (id == InvalidInterrupt)
        {
            Console.Error.WriteLine("Please register " + InvalidInterrupt + ": Interrupt " + context.InvalidInterruptID + " uncaught!");
            return;
        }

        context.InvalidInterruptID = id;
        RaiseInterrupt(InvalidInterrupt, context);
    }

    public void Continue(ExecutionState executionState)
    {
        if(executionState.IsFinished)
            return;

        InterruptContext interruptContext = new InterruptContext(executionState);

        while (!interruptContext.PauseExecution && !executionState.IsFinished)
        {

            Instruction instruction = executionState.CurrentExecutionStackLevel.GetCurrentInstruction();
            
            //Console.WriteLine("# " + instruction.OpCode + " : " + instruction.Parameter.Raw);
            
            DynValue left;
            DynValue right;
            
            switch (instruction.OpCode)
            {
                
                case OpCode.PUSHB: 
                case OpCode.PUSHF: 
                case OpCode.PUSHI:
                case OpCode.PUSHS:
                    executionState.PushStack(instruction.Parameter);
                    executionState.Step();
                    break;
                
                case OpCode.PUSHN:
                    executionState.PushStack(new DynValue(ValueType.Handle, null!));
                    executionState.Step();
                    break;
                
                case OpCode.CALL:
                    executionState.Step();
                    string procedureName = executionState.PopStack().String;
                    int moduleIndex = executionState.PopStack().Int;

                    BinaryModule targetModule;
                    if (moduleIndex == 0)
                        targetModule = executionState.CurrentExecutionStackLevel.Module;
                    else
                        targetModule = executionState.CurrentExecutionStackLevel.Module.GetResolve(moduleIndex);
                    
                    executionState.PushExecutionStack(targetModule, procedureName);
                    break;
                
                case OpCode.RET:
                    executionState.PopExecutionStack();

                    if (executionState.ExecutionStackSize == 0)
                        executionState.IsFinished = true;
                    
                    break;
                
                case OpCode.INT:
                    string interruptName = executionState.PopStack().String;
                    RaiseInterrupt(interruptName, interruptContext);
                    executionState.Step();
                    break;

                case OpCode.JMP:
                    if(executionState.PopStack().Bool)
                        executionState.Jump(instruction.Parameter.Int);
                    else
                        executionState.Step();
                    break;

                case OpCode.POP:
                    executionState.PopStack();
                    executionState.Step();
                    break;
                    
                case OpCode.CLONE:
                    DynValue clonedValue = executionState.PopStack();
                    executionState.PushStack(clonedValue);
                    executionState.PushStack(clonedValue);
                    executionState.Step();
                    break;
                    
                case OpCode.STORE:
                    DynValue storeResolutionIndex = executionState.PopStack();
                    DynValue storedValue = executionState.PopStack();
                    executionState.SetValue(storeResolutionIndex.Int, instruction.Parameter.String, storedValue);
                    executionState.Step();
                    break;
                    
                case OpCode.LOAD:
                    DynValue loadResolutionIndex = executionState.PopStack();
                    DynValue loadedValue = executionState.GetValue(loadResolutionIndex.Int, instruction.Parameter.String);
                    executionState.PushStack(loadedValue);
                    executionState.Step();
                    break;
                    
                case OpCode.ADD:
                    right = executionState.PopStack();
                    left = executionState.PopStack();

                    if (left.Type == ValueType.Floating || right.Type == ValueType.Floating)
                    {
                        executionState.PushStack(new DynValue(left.AsFloat + right.AsFloat));
                    }
                    
                    executionState.PushStack(new DynValue(left.Int + right.Int));
                    executionState.Step();
                    break;
                    
                case OpCode.SUB:
                    right = executionState.PopStack();
                    left = executionState.PopStack();

                    if (left.Type == ValueType.Floating || right.Type == ValueType.Floating)
                    {
                        executionState.PushStack(new DynValue(left.AsFloat - right.AsFloat));
                    }
                    
                    executionState.PushStack(new DynValue(left.Int - right.Int));
                    executionState.Step();
                    break;
                
                case OpCode.MULT:
                    right = executionState.PopStack();
                    left = executionState.PopStack();

                    if (left.Type == ValueType.Floating || right.Type == ValueType.Floating)
                    {
                        executionState.PushStack(new DynValue(left.AsFloat * right.AsFloat));
                    }
                    
                    executionState.PushStack(new DynValue(left.Int * right.Int));
                    executionState.Step();
                    break;
                
                case OpCode.DIV:
                    right = executionState.PopStack();
                    left = executionState.PopStack();

                    if (left.Type == ValueType.Floating || right.Type == ValueType.Floating)
                    {
                        executionState.PushStack(new DynValue(left.AsFloat / right.AsFloat));
                    }
                    
                    executionState.PushStack(new DynValue(left.Int / right.Int));
                    executionState.Step();
                    break;
                    
                case OpCode.MOD:
                    right = executionState.PopStack();
                    left = executionState.PopStack();

                    if (left.Type == ValueType.Floating || right.Type == ValueType.Floating)
                    {
                        executionState.PushStack(new DynValue(left.AsFloat % right.AsFloat));
                    }
                    
                    executionState.PushStack(new DynValue(left.Int % right.Int));
                    executionState.Step();
                    break;
                    
                case OpCode.AND:
                    // TODO: Binary operator on integers
                    right = executionState.PopStack();
                    left = executionState.PopStack();
                    
                    executionState.PushStack(new DynValue(left.Bool && right.Bool));
                    executionState.Step();
                    break;
                    
                case OpCode.OR:
                    // TODO: Binary operator on integers
                    left = executionState.PopStack();
                    right = executionState.PopStack();
                    
                    executionState.PushStack(new DynValue(left.Bool || right.Bool));
                    executionState.Step();
                    break;
                
                case OpCode.NOT:
                    // TODO: Binary operator on integers
                    left = executionState.PopStack();
                    
                    executionState.PushStack(new DynValue(!left.Bool));
                    executionState.Step();
                    break;
                
                case OpCode.IGT:
                    right = executionState.PopStack();
                    left = executionState.PopStack();

                    if (left.Type == ValueType.Floating || right.Type == ValueType.Floating)
                    {
                        executionState.PushStack(new DynValue(left.AsFloat > right.AsFloat));
                    }
                    
                    executionState.PushStack(new DynValue(left.Int > right.Int));
                    executionState.Step();
                    break;
                    
                case OpCode.ILT:
                    right = executionState.PopStack();
                    left = executionState.PopStack();

                    if (left.Type == ValueType.Floating || right.Type == ValueType.Floating)
                    {
                        executionState.PushStack(new DynValue(left.AsFloat < right.AsFloat));
                    }
                    
                    executionState.PushStack(new DynValue(left.Int < right.Int));
                    executionState.Step();
                    break;
                    
                case OpCode.IEQ:
                    right = executionState.PopStack();
                    left = executionState.PopStack();

                    bool didEqual = Equals(left.Raw, right.Raw);
                    
                    executionState.PushStack(new DynValue(didEqual));
                    executionState.Step();
                    break;
                
                case OpCode.XOR:
                default:
                    interruptContext.InvalidOpCodeID = instruction.OpCode;
                    RaiseInterrupt(InvalidOpCode, interruptContext);
                    executionState.Step();
                    break;
            }

        }

        return;
    }


    public void RegisterInterrupt(string name, InterruptCallback callback)
    {
        interrupts[name] = callback;
    }
}
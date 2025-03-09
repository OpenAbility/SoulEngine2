using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Lexing;
using SoulEngine.SequenceScript.Machine;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;
using SoulEngine.SequenceScript.Utility;
using ValueType = SoulEngine.SequenceScript.Machine.ValueType;

namespace SoulEngine.SequenceScript.Emitter;

/// <summary>
/// Handles binding and emitting of SequenceScript files
/// </summary>
public class SequenceEmitter
{
    private readonly Dictionary<string, Instruction[]> functions = new Dictionary<string, Instruction[]>();
    private readonly Dictionary<string, int> moduleResolutionIndices = new Dictionary<string, int>();
    
    private readonly Dictionary<string, IdentifiablePrototype> prototypes = new Dictionary<string, IdentifiablePrototype>();

    private readonly CompilerContext context;
    private readonly string resolvePath;
    
    public SequenceEmitter(CompilerContext context, string resolvePath)
    {
        this.context = context;
        this.resolvePath = resolvePath;
    }

    public void Process(ProgramRootNode rootNode)
    {

        moduleResolutionIndices["__LOCAL"] = 0;

        foreach (SyntaxNode node in rootNode.Nodes)
        {
            if (node is ImportNode importNode)
                ResolveImport(importNode);
            if (node is GlobalStatement globalStatement)
                ResolveGlobal(globalStatement);
            if (node is ProcedureDefinitionNode procedureDefinition)
                ResolveProcedure(procedureDefinition);
        }
        
        foreach (SyntaxNode node in rootNode.Nodes)
        {
            if (node is ProcedureDefinitionNode procedureDefinition)
                ProcessProcedure(procedureDefinition);
        }
        
    }
    
    
    private void ResolveImport(ImportNode importNode)
    {

        CompilingFile? compilingFile = context.ResolveInclude(resolvePath, importNode.Target.Value);
        if (compilingFile == null)
        {
            context.Error(importNode.Target.Location, "SS3100", $"Could not resolve included module '{importNode.Target.Value}' imported from '{resolvePath}'");
            return;
        }

        moduleResolutionIndices[compilingFile.ResolvePath] = moduleResolutionIndices.Count;

        foreach (var function in compilingFile.functions)
        {
            
            if(function.Key.StartsWith("_"))
                continue;
            
            if (prototypes.ContainsKey(function.Key))
            {
                context.Error(importNode.Target.Location, "SS3010", $"Imported prototype '{function.Key}' already defined in global scope!");
                continue;
            }

            FunctionPrototype prototype = new FunctionPrototype(function.Key, compilingFile.ResolvePath, function.Value);
            prototypes[function.Key] = prototype;
        }
        
        foreach (var global in compilingFile.globals)
        {
            
            if(global.Key.StartsWith("_"))
                continue;
            
            if (prototypes.ContainsKey(global.Key))
            {
                context.Error(importNode.Target.Location, "SS3010", $"Imported prototype '{global.Key}' already defined in global scope!");
                continue;
            }

            VariablePrototype prototype = new VariablePrototype(global.Key, compilingFile.ResolvePath, global.Value);
            prototypes[global.Key] = prototype;
        }

    }
    
    private void ResolveGlobal(GlobalStatement globalStatement)
    {
        if (prototypes.ContainsKey(globalStatement.Identifier.Value))
        {
            context.Error(globalStatement.Identifier.Location, "SS3011", $"Defined prototype '{globalStatement.Identifier.Value}' already defined in global scope!");
            return;
        }

        VariablePrototype prototype = new VariablePrototype(globalStatement.Identifier.Value, "__LOCAL",
            SequenceRules.KeywordToValueType(globalStatement.Type.TokenType));
        prototypes[globalStatement.Identifier.Value] = prototype;
    }

    private void ResolveProcedure(ProcedureDefinitionNode definitionNode)
    {
        if (prototypes.ContainsKey(definitionNode.Identifier.Value))
        {
            context.Error(definitionNode.Identifier.Location, "SS3011", $"Defined prototype '{definitionNode.Identifier.Value}' already defined in global scope!");
            return;
        }

        CompilingFunction function = new CompilingFunction();
        function.ReturnType = SequenceRules.KeywordToReturnType(definitionNode.ReturnType.TokenType);
        function.Name = definitionNode.Identifier.Value;
        function.ParameterTypes = definitionNode.Parameters
            .Select(n => SequenceRules.KeywordToValueType(n.Type.TokenType)).ToArray();


        FunctionPrototype prototype = new FunctionPrototype(definitionNode.Identifier.Value, "__LOCAL",
            function);
        prototypes[function.Name] = prototype;
    }

    private void ProcessProcedure(ProcedureDefinitionNode definitionNode)
    {
        OpWriter writer = new OpWriter();
        Scope scope = new Scope();

        foreach (var prototype in prototypes)
        {
            scope.Prototypes[prototype.Key] = prototype.Value;
        }
        
        foreach (var statement in definitionNode.Body.Nodes)
        {
            ProcessStatement(writer, scope, statement);
        }
        

        functions[definitionNode.Identifier.Value] = writer.Build();
    }

    private void ProcessStatement(OpWriter writer, Scope scope, SyntaxNode statement)
    {
        
        if(statement is LocalVariableDefinition variableDefinition) ProcessVariableDefinition(scope, variableDefinition);
        else if (statement is ProcedureCallExpressionNode procedureCallExpressionNode) ProcessProcedureCall(writer, scope, procedureCallExpressionNode, false);

        else
        {
            context.Error(new CodeLocation(resolvePath, -1, -1), "SS3000", $"Unexpected or unhandled {statement} found!");
            return;
        }
        
    }

    private void ProcessProcedureCall(OpWriter writer, Scope scope, ProcedureCallExpressionNode procedureCall, bool expression)
    {
        if (!scope.Prototypes.TryGetValue(procedureCall.Identifier.Value, out var prototype))
        {
            context.Error(procedureCall.Identifier.Location, "SS3020", 
                $"Prototype '{procedureCall.Identifier.Value}' not defined in scope!");
            return;
        }
        
        if (prototype is not FunctionPrototype functionPrototype)
        {
            context.Error(procedureCall.Identifier.Location, "SS3021", 
                $"Prototype '{procedureCall.Identifier.Value}' is not a function!");
            return;
        }
        
        // Check parameter lineups... SIGH


        writer.Instruction(OpCode.PUSHI, new DynValue(moduleResolutionIndices[functionPrototype.PackageName]));
        writer.Instruction(OpCode.PUSHS, new DynValue(functionPrototype.Name));
        

    }
    
    private void ProcessVariableDefinition(Scope scope, LocalVariableDefinition variableDefinition) 
    {
        if (scope.Prototypes.ContainsKey(variableDefinition.Identifier.Value))
        {
            context.Error(variableDefinition.Identifier.Location, "SS3012", 
                $"Prototype '{variableDefinition.Identifier.Value}' already defined in scope!");
            return;
        }
        
        VariablePrototype prototype = new VariablePrototype(variableDefinition.Identifier.Value, "__SCOPE",
            SequenceRules.KeywordToValueType(variableDefinition.Type.TokenType));
        scope.Prototypes[variableDefinition.Identifier.Value] = prototype;
    }

    private ValueType? EvaluateExpressionType(ExpressionNode expressionNode)
    {
        if (expressionNode is ConstantNode constantNode)
        {
            if (constantNode.Value.TokenType == TokenType.String)
                return ValueType.String;
            if (constantNode.Value.TokenType == TokenType.TrueKw ||
                     constantNode.Value.TokenType == TokenType.FalseKw)
                return ValueType.Boolean;
            if (constantNode.Value.TokenType == TokenType.NullKw)
                return ValueType.Handle;

            if (constantNode.Value.TokenType == TokenType.Numeric)
            {
                if (constantNode.Value.Value.Contains("."))
                    return ValueType.Floating;
                return ValueType.Integer;
            }
            

        }
        
        return ValueType.Bogus;
    }
}
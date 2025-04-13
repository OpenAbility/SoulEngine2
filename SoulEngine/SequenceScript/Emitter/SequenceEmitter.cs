using System.Globalization;
using System.Security.Cryptography;
using System.Text;
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

    private readonly Dictionary<string, ValueType> globals = new Dictionary<string, ValueType>();
    
    private readonly Dictionary<string, IdentifiablePrototype> prototypes = new Dictionary<string, IdentifiablePrototype>();

    private readonly CompilerContext context;
    private readonly CompilingFile currentFile;
    
    public SequenceEmitter(CompilerContext context, CompilingFile compilingFile)
    {
        this.context = context;
        this.currentFile = compilingFile;
    }

    public void Process(ProgramRootNode rootNode)
    {

        moduleResolutionIndices["__LOCAL"] = 0;
        moduleResolutionIndices["__STDLIB"] = -1;

        Dictionary<string, string> metaData = new Dictionary<string, string>();

        metaData["_RESOLVE_PATH"] = currentFile.ResolvePath;
        metaData["_INPUT_PATH"] = currentFile.InputPath;

        ImportNode standardImportNode = new ImportNode();
        standardImportNode.Target = new Token(new CodeLocation("STANDARD LIBRARY INCLUDE", -20, -20), TokenType.String,
            "__STDLIB", false);
        
        ResolveImport(standardImportNode);
        

        foreach (SyntaxNode node in rootNode.Nodes)
        {
            if (node is ImportNode importNode)
                ResolveImport(importNode);
            if (node is GlobalStatement globalStatement)
                ResolveGlobal(globalStatement);
            if (node is ProcedureDefinitionNode procedureDefinition)
                ResolveProcedure(procedureDefinition);
            if (node is MetaStatement metaStatement)
            {
                metaData[metaStatement.Key.Value] = metaStatement.Value.Value;
            }
        }
        
        foreach (SyntaxNode node in rootNode.Nodes)
        {
            if (node is ProcedureDefinitionNode procedureDefinition)
                ProcessProcedure(procedureDefinition);
        }

        using FileStream outputStream = File.OpenWrite(currentFile.OutputPath);
        using BinaryWriter writer = new BinaryWriter(outputStream, Encoding.UTF8, true);
        
        writer.Write(['S', 'E', 'Q', 'S']);
        writer.Write((short)SequenceRules.Version.Major);
        writer.Write((short)SequenceRules.Version.Minor);
        writer.Write((short)SequenceRules.Version.Build);


        writer.Write(metaData.Count);
        foreach (var meta in metaData)
        {
            writer.Write(meta.Key);
            writer.Write(meta.Value);
        }
        
        writer.Write(moduleResolutionIndices.Count);

        foreach (var resolve in moduleResolutionIndices)
        {
            writer.Write(resolve.Key);
            writer.Write(resolve.Value);
        }
        
        writer.Write(globals.Count);
        foreach (var global in globals)
        {
            writer.Write(global.Key);
            writer.Write((byte)global.Value);
        }
        
        writer.Write(functions.Count);
        
        foreach (var function in functions)
        {
            writer.Write(function.Key);
            writer.Write(function.Value.Length);

            foreach (var instruction in function.Value)
            {
                writer.Write((byte)instruction.OpCode);
                
                // Thankfully the opcodes are typed, so we just write the raw parameter
                if (instruction.Parameter.Type == ValueType.Bogus)
                    continue;
                
                if(instruction.Parameter.Type == ValueType.Boolean)
                    writer.Write(instruction.Parameter.Bool);
                if(instruction.Parameter.Type == ValueType.Floating)
                    writer.Write(instruction.Parameter.Float);
                if(instruction.Parameter.Type == ValueType.Integer)
                    writer.Write(instruction.Parameter.Int);
                if(instruction.Parameter.Type == ValueType.String)
                    writer.Write(instruction.Parameter.String);
                if (instruction.Parameter.Type == ValueType.Handle)
                    throw new Exception("Cannot push handle to stack!");

            }
            
        }
    }
    
    
    private void ResolveImport(ImportNode importNode)
    {

        CompilingFile? compilingFile = context.ResolveInclude(currentFile.ResolvePath, importNode.Target.Value);
        if (compilingFile == null)
        {
            context.Error(importNode.Target.Location, "SS3100", $"Could not resolve included module '{importNode.Target.Value}' imported from '{currentFile.ResolvePath}'");
            return;
        }

        if(compilingFile.ResolvePath != "__STDLIB")
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

            VariablePrototype prototype = new VariablePrototype(global.Key, compilingFile.ResolvePath, global.Value, false);
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
            SequenceRules.KeywordToValueType(globalStatement.Type.TokenType), false);
        prototypes[globalStatement.Identifier.Value] = prototype;

        globals[globalStatement.Identifier.Value] = prototype.Type;

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
        
        ValueType? returnType = SequenceRules.KeywordToReturnType(definitionNode.ReturnType.TokenType);

        scope.ReturnType = returnType;


        foreach (var parameter in definitionNode.Parameters)
        {
            scope.Prototypes.Add(parameter.Identifier.Value, new VariablePrototype(parameter.Identifier.Value, "__LOCAL", SequenceRules.KeywordToValueType(parameter.Type.TokenType), true));
            writer.Instruction(OpCode.PUSHI, new DynValue(0));
            writer.Instruction(OpCode.STORE, new DynValue(parameter.Identifier.Value));
        }
        
        foreach (var prototype in prototypes)
        {
            scope.Prototypes[prototype.Key] = prototype.Value;
        }
        
        foreach (var statement in definitionNode.Body.Nodes)
        {
            ProcessStatement(writer, scope, statement);
        }
        
        // Backup return statement - might bloat the file a bit, but it makes our life easier
        
        if (returnType == ValueType.Boolean)
            writer.Instruction(OpCode.PUSHB, new DynValue(false));
        if (returnType == ValueType.Floating)
            writer.Instruction(OpCode.PUSHF, new DynValue(0f));
        if (returnType == ValueType.Integer)
            writer.Instruction(OpCode.PUSHI, new DynValue(0));
        if (returnType == ValueType.String)
            writer.Instruction(OpCode.PUSHS, new DynValue(""));
        if (returnType == ValueType.Handle)
            writer.Instruction(OpCode.PUSHN);

        writer.Instruction(OpCode.RET);
        

        functions[definitionNode.Identifier.Value] = writer.Build();
    }

    private void ProcessStatement(OpWriter writer, Scope scope, SyntaxNode statement)
    {
        
        if(statement is LocalVariableDefinition variableDefinition) ProcessVariableDefinition(writer, scope, variableDefinition);
        else if (statement is IfStatement ifStatement) ProcessIfStatement(writer, scope, ifStatement);
        else if (statement is WhileStatement whileStatement) ProcessWhileStatement(writer, scope, whileStatement);
        else if (statement is ForStatement forStatement) ProcessForStatement(writer, scope, forStatement);
        else if (statement is ReturnStatement returnStatement)
        {
            
            CheckExpression(scope, returnStatement.ExpressionNode, scope.ReturnType);
            if(returnStatement.ExpressionNode != null)
                ProcessExpression(writer, scope, returnStatement.ExpressionNode, true);
            writer.Instruction(OpCode.RET);

        }
        else if (statement is BodyNode bodyNode)
        {
            foreach (var st in bodyNode.Nodes)
            {
                ProcessStatement(writer, scope, st);
            }
        }
        
        else if (statement is ExpressionNode expressionNode) ProcessExpression(writer, scope, expressionNode, false);
        
        else
        {
            context.Error(new CodeLocation(currentFile.ResolvePath, -1, -1), "SS3000", $"Unexpected or unhandled statement type {statement} found!");
            return;
        }
        
    }

    private void ProcessForStatement(OpWriter writer, Scope scope, ForStatement forStatement)
    {

        Scope statementScope = scope.Clone();
        

        ProcessStatement(writer, statementScope, forStatement.Initializer);

        writer.Label(out var beginLoopLabel);
        
        ProcessExpression(writer, statementScope, forStatement.Comparison, true);
        writer.Instruction(OpCode.NOT);
        writer.Instruction(OpCode.JMP, out var endStatementLabel);

        ProcessStatement(writer, statementScope, forStatement.Body);
        
        ProcessStatement(writer, statementScope, forStatement.Incrementor);

        writer.Instruction(OpCode.PUSHB, new DynValue(true));
        writer.Instruction(OpCode.JMP, beginLoopLabel);
        
        writer.Label(endStatementLabel);

    }

    private void ProcessWhileStatement(OpWriter writer, Scope scope, WhileStatement whileStatement)
    {
        writer.Label(out var loopBeginning);
        ProcessExpression(writer, scope, whileStatement.Comparison, true);
        writer.Instruction(OpCode.NOT);
        writer.Instruction(OpCode.JMP, out var loopEnd);
        
        ProcessStatement(writer, scope.Clone(), whileStatement.Body);

        writer.Instruction(OpCode.PUSHB, new DynValue(true));
        writer.Instruction(OpCode.JMP, loopBeginning);
        writer.Label(loopEnd);
    }

    private void ProcessExpression(OpWriter writer, Scope scope, ExpressionNode expression, bool wantsValue)
    {

        if (expression is ProcedureCallExpressionNode procedureCallExpressionNode) ProcessProcedureCall(writer, scope, procedureCallExpressionNode, wantsValue);
        else if (expression is ConstantNode constantNode) ProcessConstant(writer, scope, constantNode, wantsValue);
        else if (expression is BinaryExpressionNode binaryExpressionNode)
            ProcessBinaryExpression(writer, scope, binaryExpressionNode, wantsValue);
        else if (expression is VariableExpression variableExpression)
            ProcessVariableExpression(writer, scope, variableExpression, wantsValue);
        else if (expression is VariableAssignationExpression assignationExpression)
            ProcessVariableAssignationExpression(writer, scope, assignationExpression, wantsValue);
        else if (expression is VariableEditorExpression editorExpression)
            ProcessVariableEditorExpression(writer, scope, editorExpression, wantsValue);
        else
        {
            context.Error(expression.GetLocation(), "SS3000", $"Unexpected or unhandled expression node {expression} found!");
        }
    }

    private void ProcessVariableEditorExpression(OpWriter writer, Scope scope, VariableEditorExpression editorExpression, bool wantsValue)
    {
        if (!scope.Prototypes.TryGetValue(editorExpression.Variable.Value, out var prototype))
        {
            context.Error(editorExpression.Variable.Location, "SS3020", 
                $"Prototype '{editorExpression.Variable.Value}' not defined in scope!");
            return;
        }
        
        if (prototype is not VariablePrototype variablePrototype)
        {
            context.Error(editorExpression.Variable.Location, "SS3021", 
                $"Prototype '{editorExpression.Variable.Value}' is not a variable!");
            return;
        }

        writer.Instruction(OpCode.PUSHI, new DynValue(moduleResolutionIndices[variablePrototype.PackageName]));
        writer.Instruction(OpCode.LOAD, new DynValue(variablePrototype.Name));

        if (editorExpression.Operator.TokenType == TokenType.Increment ||
            editorExpression.Operator.TokenType == TokenType.Decrement)
        {
            
            if(variablePrototype.Type != ValueType.Integer)
                context.Error(editorExpression.Operator.Location, "SS3020", 
                    $"Variable '{editorExpression.Variable.Value}' is not of int type!");

            writer.Instruction(OpCode.PUSHI, new DynValue(1));
            writer.Instruction(editorExpression.Operator.TokenType == TokenType.Increment ? OpCode.ADD : OpCode.SUB);
        }
        else
        {
            // +=, -= *= etc
            
            
            context.Error(editorExpression.Operator.Location, "SS3020", 
                $"Variable editor to variable '{editorExpression.Variable.Value}' is not of valid operand!");
        }

        if (wantsValue)
            writer.Instruction(OpCode.CLONE);
        writer.Instruction(OpCode.PUSHI, new DynValue(moduleResolutionIndices[variablePrototype.PackageName]));
        writer.Instruction(OpCode.STORE, new DynValue(variablePrototype.Name));
    }

    private void ProcessVariableAssignationExpression(OpWriter writer, Scope scope, VariableAssignationExpression assignationExpression, bool wantsValue)
    {
        if (!scope.Prototypes.TryGetValue(assignationExpression.Variable.Value, out var prototype))
        {
            context.Error(assignationExpression.Variable.Location, "SS3020", 
                $"Prototype '{assignationExpression.Variable.Value}' not defined in scope!");
            return;
        }
        
        if (prototype is not VariablePrototype variablePrototype)
        {
            context.Error(assignationExpression.Variable.Location, "SS3021", 
                $"Prototype '{assignationExpression.Variable.Value}' is not a variable!");
            return;
        }

        CheckExpression(scope, assignationExpression.Value, variablePrototype.Type);
        ProcessExpression(writer, scope, assignationExpression.Value, true);

        if (wantsValue)
            writer.Instruction(OpCode.CLONE);
        writer.Instruction(OpCode.PUSHI, new DynValue(moduleResolutionIndices[variablePrototype.PackageName]));
        writer.Instruction(OpCode.STORE, new DynValue(variablePrototype.Name));
    }

    private void ProcessVariableExpression(OpWriter writer, Scope scope, VariableExpression variableExpression, bool wantsValue)
    {
        if(!wantsValue)
            return;
        
        if (!scope.Prototypes.TryGetValue(variableExpression.Value.Value, out var prototype))
        {
            context.Error(variableExpression.Value.Location, "SS3020", 
                $"Prototype '{variableExpression.Value.Value}' not defined in scope!");
            return;
        }
        
        if (prototype is not VariablePrototype variablePrototype)
        {
            context.Error(variableExpression.Value.Location, "SS3021", 
                $"Prototype '{variableExpression.Value.Value}' is not a variable!");
            return;
        }

        writer.Instruction(OpCode.PUSHI, new DynValue(moduleResolutionIndices[variablePrototype.PackageName]));
        writer.Instruction(OpCode.LOAD, new DynValue(variablePrototype.Name));

    }

    private void ProcessBinaryExpression(OpWriter writer, Scope scope, BinaryExpressionNode expressionNode,
        bool wantsValue)
    {
        if (!wantsValue)
            return;

        TokenType operand = expressionNode.Operand.TokenType;

        OpCode operation = operand switch
        {
            TokenType.Star => OpCode.MULT,
            TokenType.Slash => OpCode.DIV,
            TokenType.Modulus => OpCode.MOD,

            TokenType.Plus => OpCode.ADD,
            TokenType.Minus => OpCode.SUB,

            TokenType.Equals => OpCode.IEQ,
            TokenType.NotEquals => OpCode.IEQ,
            TokenType.LessThan => OpCode.ILT,
            TokenType.GreaterThan => OpCode.IGT,
            TokenType.LessEquals => OpCode.ILT,
            TokenType.GreaterEquals => OpCode.IGT,

            TokenType.And => OpCode.AND,

            TokenType.Or => OpCode.OR,
            
            _ => throw new Exception("Invalid binary expression encountered!")
        };

        ProcessExpression(writer, scope, expressionNode.Left, true);
        ProcessExpression(writer, scope, expressionNode.Right, true);
        writer.Instruction(operation);

        if (operand == TokenType.NotEquals)
            writer.Instruction(OpCode.NOT);
        
        if (operand == TokenType.LessEquals || operand == TokenType.GreaterEquals)
            writer.Instruction(OpCode.IEQ).Instruction(OpCode.OR);

    }

    private void ProcessConstant(OpWriter writer, Scope scope, ConstantNode constantNode, bool wantsValue)
    {
        if(!wantsValue)
            return;

        if (constantNode.Value.TokenType == TokenType.Numeric)
        {
            if (constantNode.Value.Value.Contains('.'))
            {
                writer.Instruction(OpCode.PUSHF,
                    new DynValue(float.Parse(constantNode.Value.Value, CultureInfo.InvariantCulture)));
            }
            else
            {
                writer.Instruction(OpCode.PUSHI,
                    new DynValue(int.Parse(constantNode.Value.Value, CultureInfo.InvariantCulture)));
            }
        } 
        else if (constantNode.Value.TokenType == TokenType.TrueKw) 
            writer.Instruction(OpCode.PUSHB, new DynValue(true));
        else if (constantNode.Value.TokenType == TokenType.FalseKw) 
            writer.Instruction(OpCode.PUSHB, new DynValue(false));
        else if (constantNode.Value.TokenType == TokenType.String) 
            writer.Instruction(OpCode.PUSHS, new DynValue(constantNode.Value.Value));
        else if (constantNode.Value.TokenType == TokenType.NullKw) 
            writer.Instruction(OpCode.PUSHN);
        
    }

    private void ProcessIfStatement(OpWriter writer, Scope scope, IfStatement ifStatement)
    {
        Label statementEnd = new Label();
        Label? nextBranch = null;
        
        while (true)
        {
            // Resolve the next branch if it exists
            if (nextBranch != null)
                writer.Label(nextBranch);

            // It's an else statement - just emit the body
            if (ifStatement.Expression == null)
            {
                ProcessStatement(writer, scope.Clone(), ifStatement.Body);
                break;
            }
            // Otherwise it's conditional
            
            // Ensure it's boolean
            if (!CheckExpression(scope, ifStatement.Expression!, ValueType.Boolean))
                return;

            // Evaluate the if statement
            ProcessExpression(writer, scope, ifStatement.Expression!, true);
            // If it's NOT true, jump to the next branch
            writer.Instruction(OpCode.NOT);
            writer.Instruction(OpCode.JMP, out nextBranch);

            // Emit the statement
            ProcessStatement(writer, scope.Clone(), ifStatement.Body);
            
            // Then jump to the end (push true to ensure this always happens)
            writer.Instruction(OpCode.PUSHB, new DynValue(true));
            writer.Instruction(OpCode.JMP, statementEnd);

            ifStatement = ifStatement.Next!;
            if(ifStatement == null!)
                break;
        }

        // We reached the end
        writer.Label(statementEnd);
        // Resolve if needed
        if(nextBranch is { Location: -1 })
            writer.Label(nextBranch);


    }

    private void ProcessProcedureCall(OpWriter writer, Scope scope, ProcedureCallExpressionNode procedureCall, bool wantsValue)
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

        if (functionPrototype.Underlying.ParameterTypes.Length != procedureCall.Parameters.Length)
        {
            context.Error(procedureCall.Identifier.Location, "SS3022", 
                $"Parameter count mismatch for '{procedureCall.Identifier.Value}'! Expected {functionPrototype.Underlying.ParameterTypes.Length}, got {procedureCall.Parameters.Length}");
            return;
        }


        bool failed = false;
        for (int i = 0; i < procedureCall.Parameters.Length; i++)
        {
            if (!CheckExpression(scope, procedureCall.Parameters[i], functionPrototype.Underlying.ParameterTypes[i]))
                failed = true;
        }
        
        if(failed)
            return;

        for (int i = 0; i < procedureCall.Parameters.Length; i++)
        {
            ProcessExpression(writer, scope, procedureCall.Parameters[i], true);
        }

        // If the function is declared  with "extern"
        if (functionPrototype.Underlying.SystemFunction)
        {
            writer.Instruction(OpCode.PUSHS, new DynValue(functionPrototype.Name));
            writer.Instruction(OpCode.INT);
        }
        else
        {
            writer.Instruction(OpCode.PUSHI, new DynValue(moduleResolutionIndices[functionPrototype.PackageName]));
            writer.Instruction(OpCode.PUSHS, new DynValue(functionPrototype.Name));
            writer.Instruction(OpCode.CALL);
        }


        if (functionPrototype.Underlying.ReturnType != null && !wantsValue)
            writer.Instruction(OpCode.POP);


    }
    
    private void ProcessVariableDefinition(OpWriter writer, Scope scope, LocalVariableDefinition variableDefinition) 
    {
        if (scope.Prototypes.ContainsKey(variableDefinition.Identifier.Value))
        {
            context.Error(variableDefinition.Identifier.Location, "SS3012", 
                $"Prototype '{variableDefinition.Identifier.Value}' already defined in scope!");
            return;
        }
        
        VariablePrototype prototype = new VariablePrototype(variableDefinition.Identifier.Value, "__LOCAL",
            SequenceRules.KeywordToValueType(variableDefinition.Type.TokenType), true);
        scope.Prototypes[variableDefinition.Identifier.Value] = prototype;

        if (variableDefinition.DefaultValue != null)
        {
            CheckExpression(scope, variableDefinition.DefaultValue, prototype.Type);
            ProcessExpression(writer, scope, variableDefinition.DefaultValue, true);
            writer.Instruction(OpCode.PUSHI, new DynValue(0));
            writer.Instruction(OpCode.STORE, new DynValue(prototype.Name));
        }
        else
        {

            if (prototype.Type == ValueType.Boolean)
                writer.Instruction(OpCode.PUSHB, new DynValue(false));
            else if (prototype.Type == ValueType.Floating)
                writer.Instruction(OpCode.PUSHF, new DynValue(0.0f));
            else if (prototype.Type == ValueType.Integer)
                writer.Instruction(OpCode.PUSHI, new DynValue(0));
            else if (prototype.Type == ValueType.String)
                writer.Instruction(OpCode.PUSHS, new DynValue(""));
            else  if (prototype.Type == ValueType.Handle)
                writer.Instruction(OpCode.PUSHN);
            
            writer.Instruction(OpCode.PUSHI, new DynValue(0));
            writer.Instruction(OpCode.STORE, new DynValue(prototype.Name));
            
        }
    }

    private bool CheckExpression(Scope scope, ExpressionNode? expressionNode, ValueType? expected)
    {
        ValueType? evaluated = EvaluateExpressionType(scope, expressionNode);

        if (evaluated != expected)
        {
            context.Error(expressionNode.GetLocation(), "SS3030", 
                $"Expression type mismatch! Expected {expected?.ToString() ?? "void"}, got {evaluated.ToString() ?? "void"}");
            return false;
        }

        return true;
    }

    private ValueType? EvaluateExpressionType(Scope scope, ExpressionNode? expressionNode)
    {
        if (expressionNode == null)
            return null;
        
        if (expressionNode is ConstantNode constantNode)
        {
            return SequenceRules.TokenToValueType(constantNode.Value);
        }
        else if (expressionNode is BinaryExpressionNode binaryExpressionNode)
        {
            TokenType operand = binaryExpressionNode.Operand.TokenType;

            if (operand == TokenType.Equals || operand == TokenType.NotEquals || operand == TokenType.LessEquals ||
                operand == TokenType.GreaterEquals || operand == TokenType.LessThan || operand == TokenType.GreaterThan)
            {
                ValueType? leftValue = EvaluateExpressionType(scope, binaryExpressionNode.Left);
                ValueType? rightValue = EvaluateExpressionType(scope, binaryExpressionNode.Left);
                
                switch (leftValue)
                {
                    case ValueType.Boolean  when rightValue == ValueType.Boolean:
                    case ValueType.String   when rightValue == ValueType.String:
                    case ValueType.Handle   when rightValue == ValueType.Handle:
                    case ValueType.Integer  when rightValue == ValueType.Integer:
                    case ValueType.Floating  when rightValue == ValueType.Floating:
                    case ValueType.Integer  when rightValue == ValueType.Floating:
                    case ValueType.Floating when rightValue == ValueType.Integer:
                        return ValueType.Boolean;
                    
                    default:
                        context.Error(binaryExpressionNode.GetLocation(), "SS3000", $"Can't compare types {leftValue} and {rightValue}!");
                        return ValueType.Bogus;
                }
            } else if (operand == TokenType.Increment || operand == TokenType.Decrement)
            {
                // Wuh?
                throw new Exception("Got increment/decrement in binary expression??");
            }
            else {
                
                // It's +-*/ etc etc etc
                // We need to promote
                
                ValueType? leftType = EvaluateExpressionType(scope, binaryExpressionNode.Left);
                ValueType? rightType = EvaluateExpressionType(scope, binaryExpressionNode.Right);

                if (!leftType.IsNumericType())
                {
                    context.Error(binaryExpressionNode.Left.GetLocation(), "SS3000", $"Left part of numeric expression is of non-numeric type {leftType?.ToString() ?? "void"}!");
                    return ValueType.Bogus;
                }
                
                if (!rightType.IsNumericType())
                {
                    context.Error(binaryExpressionNode.Right.GetLocation(), "SS3000", $"Right part of numeric expression is of non-numeric type {rightType?.ToString() ?? "void"}!");
                    return ValueType.Bogus;
                }
                
                // Promote to largest
                return leftType == ValueType.Floating || rightType == ValueType.Floating
                    ? ValueType.Floating
                    : ValueType.Integer;
                
            }
            
            
        } else if (expressionNode is VariableExpression variableExpression)
        {
            if (!scope.Prototypes.TryGetValue(variableExpression.Value.Value, out var prototype))
            {
                context.Error(variableExpression.Value.Location, "SS3020", 
                    $"Prototype '{variableExpression.Value.Value}' not defined in scope!");
                return ValueType.Bogus;
            }
        
            if (prototype is not VariablePrototype variablePrototype)
            {
                context.Error(variableExpression.Value.Location, "SS3021", 
                    $"Prototype '{variableExpression.Value.Value}' is not a variable!");
                return ValueType.Bogus;
            }

            return variablePrototype.Type;
        } else if (expressionNode is ProcedureCallExpressionNode procedureCallExpressionNode)
        {
            if (!scope.Prototypes.TryGetValue(procedureCallExpressionNode.Identifier.Value, out var prototype))
            {
                context.Error(procedureCallExpressionNode.Identifier.Location, "SS3020", 
                    $"Prototype '{procedureCallExpressionNode.Identifier.Value}' not defined in scope!");
                return ValueType.Bogus;
            }
        
            if (prototype is not FunctionPrototype functionPrototype)
            {
                context.Error(procedureCallExpressionNode.Identifier.Location, "SS3021", 
                    $"Prototype '{procedureCallExpressionNode.Identifier.Value}' is not a function!");
                return ValueType.Bogus;
            }

            return functionPrototype.Underlying.ReturnType;
        }
        
        context.Error(expressionNode.GetLocation(), "SS3000", $"Unexpected or unhandled expression type {expressionNode} found!");
        return ValueType.Bogus;
    }
}
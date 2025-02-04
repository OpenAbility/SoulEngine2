using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Lexing;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;
using SoulEngine.SequenceScript.Utility;

namespace SoulEngine.SequenceScript.Parsing;

public class SequenceParser
{
    private readonly CompilerContext context;

    public SequenceParser(Token[] tokens, CompilerContext context)
    {
        this.context = context;
        this.tokens = tokens;
    }



    public ProgramRootNode Process()
    {
        index = 0;

        ProgramRootNode programRootNode = new ProgramRootNode();

        while (!EOF)
        {
            if(CurrentType == TokenType.ImportKw) programRootNode.Nodes.Add(ParseImport());
            else if(CurrentType == TokenType.GlobalKw) programRootNode.Nodes.Add(ParseGlobal());
            else if(CurrentType == TokenType.ProcKw) programRootNode.Nodes.Add(ParseProcedure());


            else
            {
                if (CurrentToken.Value != "")
                    context.Error(Location, "SS2010",
                        "Unexpected token '" + CurrentToken.Value + "' (" + CurrentType + ")!");
                else
                    context.Error(Location, "SS2002", "Unexpected token type " + CurrentType + "!");
                Step();
            }
        }        

        return programRootNode;
    }

    private ImportNode ParseImport()
    {
        Consume(TokenType.ImportKw);

        bool paren = TryConsume(TokenType.OpenParenthesis);


        ImportNode importNode = new ImportNode();
        importNode.Target = Consume(TokenType.String);


        if (paren)
            Consume(TokenType.CloseParenthesis);

        Consume(TokenType.EndStatement);

        return importNode;
    }

    private SyntaxNode ParseGlobal()
    {
        Consume(TokenType.GlobalKw);

        GlobalStatement globalStatement = new GlobalStatement();
        
        globalStatement.Type = Consume(TokenType.IntKw, TokenType.FloatKw, TokenType.BoolKw,
            TokenType.StringKw, TokenType.HandleKw);

        globalStatement.Identifier = Consume(TokenType.Identifier);

        if (CurrentType == TokenType.Assign)
        {
            Consume(TokenType.Assign);
            globalStatement.DefaultValue = ParseExpression();
        }

        Consume(TokenType.EndStatement);

        return globalStatement;
    }

    private SyntaxNode ParseProcedure()
    {
        Consume(TokenType.ProcKw);

        Token typeToken = Consume(TokenType.VoidKw, TokenType.IntKw, TokenType.FloatKw, TokenType.BoolKw,
            TokenType.StringKw, TokenType.HandleKw);

        Token identifierToken = Consume(TokenType.Identifier);

        Consume(TokenType.OpenParenthesis);

        List<ParameterDefinitionNode> parameterDefinitionNodes = new List<ParameterDefinitionNode>();

        if (CurrentType != TokenType.CloseParenthesis)
        {
            while (true)
            {
                ParameterDefinitionNode parameterDefinitionNode = new ParameterDefinitionNode();

                parameterDefinitionNode.Type = Consume(TokenType.IntKw, TokenType.FloatKw, TokenType.BoolKw,
                    TokenType.StringKw, TokenType.HandleKw);
                parameterDefinitionNode.Identifier = Consume(TokenType.Identifier);

                parameterDefinitionNodes.Add(parameterDefinitionNode);
                
                if(CurrentType == TokenType.CloseParenthesis)
                    break;

                Consume(TokenType.Comma);
            }
        }

        Consume(TokenType.CloseParenthesis);

        ProcedureDefinitionNode procedureDefinitionNode = new ProcedureDefinitionNode();
        procedureDefinitionNode.Identifier = identifierToken;
        procedureDefinitionNode.ReturnType = typeToken;
        procedureDefinitionNode.Parameters = parameterDefinitionNodes.ToArray();

        procedureDefinitionNode.Body = ParseBody();

        return procedureDefinitionNode;
    }


    private BodyNode ParseBody()
    {
        BodyNode bodyNode = new BodyNode();

        if (CurrentType != TokenType.OpenBraces)
        {
            bodyNode.Nodes = [ParseStatement(true)];
            return bodyNode;
        }

        Consume(TokenType.OpenBraces);

        List<SyntaxNode> content = new List<SyntaxNode>();
        
        while (CurrentType != TokenType.CloseBraces && !EOF)
        {
            content.Add(ParseStatement(true));
        }

        bodyNode.Nodes = content.ToArray();

        Consume(TokenType.CloseBraces);

        return bodyNode;
    }

    private SyntaxNode ParseStatement(bool requireEnd)
    {

        while (CurrentType == TokenType.EndStatement)
            Consume(TokenType.EndStatement);
        
        if (SequenceRules.AssignableTokenTypes.Contains(CurrentType)) return ParseVariableDefinition();

        if (CurrentType == TokenType.SwitchKw) return ParseSwitch();
        if (CurrentType == TokenType.IfKw) return ParseIf();
        if (CurrentType == TokenType.ReturnKw) return ParseReturn();
        if (CurrentType == TokenType.ForKw) return ParseFor();
        if (CurrentType == TokenType.WhileKw) return ParseWhile();

        ExpressionNode expressionNode = ParseExpression();
        if(requireEnd)
            Consume(TokenType.EndStatement);

        return expressionNode;
    }



    private ExpressionNode ParseExpression()
    {
        if (CurrentType == TokenType.Identifier && SequenceRules.GetBinaryOperatorPrecedence(PeekType(1)) != 0 &&
            !Peek(1).WhitespaceFollowed && PeekType(2) == TokenType.Assign)
        {
            // Variable editor
            return ParseVariableEditor();
        }
        
        if (CurrentType == TokenType.Identifier && 
            (PeekType(1) == TokenType.Increment || PeekType(1) == TokenType.Decrement))
        {
            Token id = Consume(CurrentType);
            Token op = Consume(CurrentType);

            ConstantNode constantNode = new ConstantNode();
            constantNode.Value = new Token(op.Location, TokenType.Numeric, "1", op.WhitespaceFollowed);
            
            VariableEditorExpression variableEditorExpression = new VariableEditorExpression();
            variableEditorExpression.Variable = id;
            variableEditorExpression.Operator = op;
            variableEditorExpression.Expression = constantNode;

            return variableEditorExpression;
        }

        if (CurrentType == TokenType.Identifier && PeekType(1) == TokenType.Assign)
        {
            return ParseVariableAssignation();
        }

        return ParseBinaryExpression(0);
    }

    private VariableEditorExpression ParseVariableEditor()
    {

        VariableEditorExpression variableEditorExpression = new VariableEditorExpression();

        variableEditorExpression.Variable = Consume(TokenType.Identifier);
        variableEditorExpression.Operator = Consume(CurrentType);
        Consume(TokenType.Assign);

        variableEditorExpression.Expression = ParseExpression();

        return variableEditorExpression;
    }

    private VariableAssignationExpression ParseVariableAssignation()
    {
        VariableAssignationExpression variableEditorExpression = new VariableAssignationExpression();

        variableEditorExpression.Variable = Consume(TokenType.Identifier);
        variableEditorExpression.Value = ParseExpression();

        return variableEditorExpression;
    }

    private ExpressionNode ParseBinaryExpression(int parentPrecedence)
    {
        int unaryPrecedence = SequenceRules.GetUnaryOperatorPrecedence(CurrentType);
        if (unaryPrecedence != 0 && unaryPrecedence > parentPrecedence)
        {
            UnaryExpressionNode unaryExpressionNode = new UnaryExpressionNode();
            
            unaryExpressionNode.Operation = Consume(CurrentType);
            unaryExpressionNode.Value = ParseBinaryExpression(unaryPrecedence);
            
            return unaryExpressionNode;
        }
		
        ExpressionNode left = ParsePrimaryExpression();

        while (true)
        {
            int precedence = SequenceRules.GetBinaryOperatorPrecedence(CurrentType);
			
            if(precedence == 0 || precedence < parentPrecedence)
                break;
			
            Token op = Consume(CurrentType);
            ExpressionNode right = ParseBinaryExpression(parentPrecedence);

            left = new BinaryExpressionNode(left, op, right);
        }

        return left;

    }

    private ExpressionNode ParsePrimaryExpression()
    {
        TokenType type = CurrentType;

        if (type == TokenType.String) return new ConstantNode(Consume(TokenType.String));
        if (type == TokenType.Numeric) return new ConstantNode(Consume(TokenType.Numeric));
        if (type == TokenType.TrueKw || type == TokenType.FalseKw) return new ConstantNode(Consume(CurrentType));

        if (type == TokenType.Identifier && PeekType(1) == TokenType.OpenParenthesis) return ParseCallExpression();
        if (type == TokenType.Identifier) return new VariableExpression(Consume(TokenType.Identifier));

        if (type == TokenType.OpenParenthesis) return ParseParenthesisedExpression();
        
        if (CurrentToken.Value != "")
            context.Error(Location, "SS2002",
                "Unexpected token '" + CurrentToken.Value + "' (" + CurrentType + ")!");
        else
            context.Error(Location, "SS2002", "Unexpected token type " + CurrentType + "!");
        Step(1);
        return new BogusExpression();
    }

    private ExpressionNode ParseParenthesisedExpression()
    {
        Consume(TokenType.OpenParenthesis);
        ExpressionNode expressionNode = ParseExpression();
        Consume(TokenType.CloseParenthesis);
        return expressionNode;
    }

    private ProcedureCallExpressionNode ParseCallExpression()
    {
        ProcedureCallExpressionNode expressionNode = new ProcedureCallExpressionNode();
        
        expressionNode.Identifier = Consume(TokenType.Identifier);

        Consume(TokenType.OpenParenthesis);

        List<ExpressionNode> parameters = new List<ExpressionNode>();
        
        if (CurrentType != TokenType.CloseParenthesis)
        {
            while (true)
            {
                parameters.Add(ParseExpression());
                
                if(CurrentType != TokenType.Comma)
                    break;

                Consume(TokenType.Comma);
            }
        }

        Consume(TokenType.CloseParenthesis);

        expressionNode.Parameters = parameters.ToArray();

        return expressionNode;
    }

    private LocalVariableDefinition ParseVariableDefinition()
    {
        LocalVariableDefinition variableDefinition = new LocalVariableDefinition();

        variableDefinition.Type = Consume(SequenceRules.AssignableTokenTypes);
        
        if (CurrentType == TokenType.OpenBrackets)
        {
            variableDefinition.IsArray = true;
            Consume(TokenType.CloseBrackets);
        }
        
        variableDefinition.Identifier = Consume(TokenType.Identifier);

        if (CurrentType == TokenType.Assign)
        {
            Consume(TokenType.Assign);
            
            
            if (variableDefinition.IsArray)
            {
                List<ExpressionNode> arrayValues = new List<ExpressionNode>();
                ArrayConstantNode arrayConstantNode = new ArrayConstantNode();

                Consume(TokenType.OpenBrackets);

                if (CurrentType != TokenType.CloseBrackets)
                {

                    while (true)
                    {
                        arrayValues.Add(ParseExpression());
                        
                        if(CurrentType != TokenType.Comma)
                            break;

                        Consume(TokenType.Comma);
                    }
                    
                }

                Consume(TokenType.CloseBrackets);

                arrayConstantNode.Values = arrayValues.ToArray();

                variableDefinition.DefaultValue = arrayConstantNode;

            }
            else
            {
                variableDefinition.DefaultValue = ParseExpression();
            }
        }

        Consume(TokenType.EndStatement);

        return variableDefinition;

    }


    private SwitchStatement ParseSwitch()
    {
        Consume(TokenType.SwitchKw);

        SwitchStatement switchStatement = new SwitchStatement();

        Consume(TokenType.OpenParenthesis);

        switchStatement.Expression = ParseExpression();

        Consume(TokenType.CloseParenthesis);

        Consume(TokenType.OpenBraces);

        List<SwitchCase> cases = new List<SwitchCase>();

        while (true)
        {
            if (CurrentType == TokenType.DefaultKw)
            {

                if (switchStatement.Default != null)
                {
                    context.Error(Location, "SS2007", "Switch statement contains more than one default case!");
                }
                
                Consume(TokenType.DefaultKw);

                switchStatement.Default = ParseBody();
            } else if (CurrentType == TokenType.CaseKw)
            {
                Consume(TokenType.CaseKw);
                
                Consume(TokenType.OpenParenthesis);
                
                SwitchCase switchCase = new SwitchCase();
                
                switchCase.Expression = ParseExpression();

                Consume(TokenType.CloseParenthesis);

                switchCase.Body = ParseBody();

                cases.Add(switchCase);
            } else if (CurrentType == TokenType.CloseBraces)
            {
                break;
            }
            else
            {
                if (CurrentToken.Value != "")
                    context.Error(Location, "SS2002",
                        "Unexpected token '" + CurrentToken.Value + "' (" + CurrentType + ")!");
                else
                    context.Error(Location, "SS2002", "Unexpected token type " + CurrentType + "!");
                Step();
            }
        }

        Consume(TokenType.CloseBraces);
        
        switchStatement.Cases = cases.ToArray();

        return switchStatement;
    }

    private IfStatement ParseIf()
    {
        IfStatement ifStatement = new IfStatement();

        if (CurrentType == TokenType.IfKw)
        {
            Consume(TokenType.IfKw);

            ifStatement.Expression = ParseParenthesisedExpression();

            ifStatement.Body = ParseBody();

            if (CurrentType == TokenType.ElseKw)
            {
                Consume(TokenType.ElseKw);
                ifStatement.Next = ParseIf();
            }
        }
        else
        {
            // It's an else statement
            
            ifStatement.Body = ParseBody();
        }

        return ifStatement;
    }

    private ReturnStatement ParseReturn()
    {
        ReturnStatement returnStatement = new ReturnStatement();
        Consume(TokenType.ReturnKw);

        returnStatement.ExpressionNode = ParseExpression();

        Consume(TokenType.EndStatement);

        return returnStatement;
    }

    private WhileStatement ParseWhile()
    {
        WhileStatement whileStatement = new WhileStatement();
        Consume(TokenType.WhileKw);

        whileStatement.Comparison = ParseParenthesisedExpression();

        whileStatement.Body = ParseBody();

        return whileStatement;
    }

    private ForStatement ParseFor()
    {
        ForStatement forStatement = new ForStatement();

        Consume(TokenType.ForKw);

        Consume(TokenType.OpenParenthesis);
        
        forStatement.Incrementor = ParseStatement(true);
        forStatement.Comparison = ParseExpression();
        Consume(TokenType.EndStatement);
        forStatement.Incrementor = ParseStatement(false);

        Consume(TokenType.CloseParenthesis);
        
        forStatement.Body = ParseBody();

        return forStatement;
    }
    
    
    
    #region ITERATION
    
    private readonly Token[] tokens;
    private int index;
    private Token CurrentToken => Peek(0);
    private TokenType CurrentType => PeekType(0);

    private CodeLocation Location => CurrentToken.Location;

    private bool EOF => index >= tokens.Length;

    private Token Peek(int amount)
    {
        if (tokens.Length <= index + amount)
            return new Token(new CodeLocation(), TokenType.Unknown, "EOF", false);
        return tokens[index + amount];
    }

    private TokenType PeekType(int amount)
    {
        return Peek(amount).TokenType;
    }

    private Token Consume(TokenType type)
    {
        Token token;
        
        if (CurrentType != type)
        {
            if (CurrentToken.Value != "")
                context.Error(Location, "SS2001",
                    "Unexpected token '" + CurrentToken.Value + "' (" + CurrentType + "): Expected " + type);
            else
                context.Error(Location, "SS2001", "Unexpected token type " + CurrentType + ": Expected " + type);
            token = new Token(Location, TokenType.Unknown, "INVALID", CurrentToken.WhitespaceFollowed);
            Step();
            return token;
        }

        token = CurrentToken;
        Step();
        return token;
    }

    private Token Consume(params TokenType[] types)
    {
        for (int i = 0; i < types.Length; i++)
        {
            if (CurrentType == types[i])
                return Consume(types[i]);
        }
        
        if (CurrentToken.Value != "")
            context.Error(Location, "SS2001",
                "Unexpected token '" + CurrentToken.Value + "' (" + CurrentType + "): Expected one of " + string.Join(", ", types));
        else
            context.Error(Location, "SS2001", "Unexpected token type " + CurrentType + ": Expected one of " + string.Join(", ", types));
        Token token = new Token(Location, TokenType.Unknown, "INVALID", CurrentToken.WhitespaceFollowed);
        Step(1);
        return token;
    }

    private bool TryConsume(TokenType type)
    {
        if (CurrentType == type)
        {
            Step();
            return true;
        }

        return false;
    }

    private void Step(int amount = 1)
    {
        index += amount;
    }
    
    #endregion
}
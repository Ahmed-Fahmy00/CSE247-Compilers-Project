using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        
        public string Name;
        public Node(string N) 
        { 
            this.Name = N;
        }
    }

    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public  Node root;
        
        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(P_Functions());
            root.Children.Add(P_Main_function());
            return root;
        }

        Node P_DataType()
        {
            //dataType -> int | float | string
            Node dataType = new Node("DataType");

            if (   TokenStream[InputPointer].token_type == Token_Class.Int_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.Float_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.String_DataType)
                dataType.Children.Add(match(TokenStream[InputPointer].token_type));

            return dataType;
        }
        Node P_Arithmatic_Operator()
        {
            //arithmatic operator -> plusOp |minusOp | multiplyOp | divideOp
            Node arithmaticOperator = new Node("ArithmaticOperator");
            if (TokenStream[InputPointer].token_type == Token_Class.Plus_Operator)
                arithmaticOperator.Children.Add(match(Token_Class.Plus_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Minus_Operator)
                arithmaticOperator.Children.Add(match(Token_Class.Minus_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Multiply_Operator)
                arithmaticOperator.Children.Add(match(Token_Class.Multiply_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Divide_Operator)
                arithmaticOperator.Children.Add(match(Token_Class.Divide_Operator));

            return arithmaticOperator;
        }
        Node P_Condition_Operator()
        {
            //condition operator -> notEqualOp | equalOp | lessThanOp | moreThanOp
            Node conditionOperator = new Node("ConditionOperator");

            if (TokenStream[InputPointer].token_type == Token_Class.Not_Equal_Operator)
                conditionOperator.Children.Add(match(Token_Class.Not_Equal_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Equal_Operator)
                conditionOperator.Children.Add(match(Token_Class.Equal_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Less_Than_Operator)
                conditionOperator.Children.Add(match(Token_Class.Less_Than_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.More_Than_Operator)
                conditionOperator.Children.Add(match(Token_Class.More_Than_Operator));

            return conditionOperator;
        }
        Node P_Boolean_Operator()
        {
            //boolean operator -> andOperator | orOperator
            Node booleanOperator = new Node("BooleanOperator");

            if (TokenStream[InputPointer].token_type == Token_Class.And_Operator)
                booleanOperator.Children.Add(match(Token_Class.And_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Or_Operator)
                booleanOperator.Children.Add(match(Token_Class.Or_Operator));
 
            return booleanOperator;
        }

        Node P_Term()
        {
            //Term → number | identifier | Function_call
            Node term = new Node("Term");
            if (TokenStream[InputPointer].token_type == Token_Class.Number)
                term.Children.Add(match(Token_Class.Number));
            else if (TokenStream[InputPointer].token_type == Token_Class.Identifier) 
            {
                if (TokenStream[InputPointer+1].token_type == Token_Class.Open_Parenthesis)
                    term.Children.Add(P_Function_call());
                else
                    term.Children.Add(match(Token_Class.Identifier));
            }
            return term; ;
        }
        Node P_Expression()
        {
            //Expression → stringLine | Term | Equation

            Node expression = new Node("Expression");

            if (InputPointer >= TokenStream.Count) return expression;
                
            Token_Class type = TokenStream[InputPointer].token_type;

            if (type == Token_Class.String)
                expression.Children.Add(match(Token_Class.String));
            else if (IsEquation())
                expression.Children.Add(P_Equation());
            else if (IsTerm())
                expression.Children.Add(P_Term());
            
            return expression;
        }
        Node P_Equation()
        {
            //Equation -> (Equation) Equation’ | Term Equation’

            Node equation = new Node("Equation");
            if (TokenStream[InputPointer].token_type == Token_Class.Open_Parenthesis)
            {
                equation.Children.Add(match(Token_Class.Open_Parenthesis));
                equation.Children.Add(P_Equation());
                equation.Children.Add(match(Token_Class.Close_Parenthesis));
                equation.Children.Add(P_Equation_D());
            }
            else
            {
                equation.Children.Add(P_Term());
                equation.Children.Add(P_Equation_D());
            }
            return equation;
        }
        Node P_Equation_D()
        {
            // Ops Equation Equation’ | epsilon
            Node equationD = new Node("EquationD");
            if(IsArithmaticOperator())            
            {
                equationD.Children.Add(P_Arithmatic_Operator());
                equationD.Children.Add(P_Equation());
                equationD.Children.Add(P_Equation_D());
                return equationD;
            }
            return null;
        }

        Node P_Condition()
        {
            //Condition → identifier ConditionOp Term
            Node condition = new Node("Condition");
            if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                condition.Children.Add(match(Token_Class.Identifier));
                condition.Children.Add(P_Condition_Operator());
                condition.Children.Add(P_Term());
            }
            return condition;
        }
        Node P_Condition_statement()
        {
            //Condition_statement → Condition Condition_statement
            Node conditionStatement = new Node("ConditionStatement");

            conditionStatement.Children.Add(P_Condition());
            conditionStatement.Children.Add(P_Condition_statement_D());

            return conditionStatement;
        }
        Node P_Condition_statement_D()
        {
            //Condition_statement_D → Boolean_Operator Condition Condition_statement' | epsilon
            Node conditionStatementD = new Node("ConditionStatementD");
            if (   TokenStream[InputPointer].token_type == Token_Class.And_Operator 
                || TokenStream[InputPointer].token_type == Token_Class.Or_Operator )
            {
                conditionStatementD.Children.Add(P_Boolean_Operator());
                conditionStatementD.Children.Add(P_Condition());
                conditionStatementD.Children.Add(P_Condition_statement_D());
            }
            else  
                return null; 

            return conditionStatementD;

        }

        Node P_Read_statement()
        {
            Node readStatement = new Node("ReadStatement");

            readStatement.Children.Add(match(Token_Class.Read_Keyword));
            readStatement.Children.Add(match(Token_Class.Identifier));
            readStatement.Children.Add(match(Token_Class.Semicolon_Symbol));

            return readStatement;
        }
        Node P_Write_statement()
        {
            // Write_statement → write Write_statement’
            Node writeStatement = new Node("WriteStatement");

            writeStatement.Children.Add(match(Token_Class.Write_Keyword));
            writeStatement.Children.Add(P_Write_statement_D());

            return writeStatement;

        }
        Node P_Write_statement_D()
        {
            // Write_statement’ → Expression ; | endl ;
            Node writeStatementD = new Node("WriteStatementD");

            if (TokenStream[InputPointer].token_type == Token_Class.String || IsEquation() || IsTerm())
            {
                writeStatementD.Children.Add(P_Expression());
                writeStatementD.Children.Add(match(Token_Class.Semicolon_Symbol));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Endl_Keyword)
            {
                writeStatementD.Children.Add(match(Token_Class.Endl_Keyword));
                writeStatementD.Children.Add(match(Token_Class.Semicolon_Symbol));
            }

            return writeStatementD;
        }

        Node P_Identifier()
        {
            Node identifier = new Node("Identifier");

            identifier.Children.Add(match(Token_Class.Identifier));
            identifier.Children.Add(P_Identifier_D());

            return identifier;
        }
        Node P_Identifier_D()
        {
            Node identifier_D = new Node("IdentifierD");

            if (TokenStream[InputPointer].token_type == Token_Class.Assignment_Operator)
            {
                identifier_D.Children.Add(match(Token_Class.Assignment_Operator));
                identifier_D.Children.Add(P_Expression());

                return identifier_D;
            }
            else
                return null;
        }
        Node P_Identifier_List()
        {
            Node identifier_list = new Node("IdentifierList");

            identifier_list.Children.Add(P_Identifier());
            identifier_list.Children.Add(P_Identifier_List_D());

            return identifier_list;
        }
        Node P_Identifier_List_D()
        {
            Node identifier_list_D = new Node("IdentifierListD");

            if (TokenStream[InputPointer].token_type == Token_Class.Comma_Symbol)
            {
                identifier_list_D.Children.Add(match(Token_Class.Comma_Symbol));
                identifier_list_D.Children.Add(P_Identifier());
                identifier_list_D.Children.Add(P_Identifier_List_D());

                return identifier_list_D;
            }
            else
                return null;
        }
        Node P_Declaration_statement()
        {
            Node declarationStatement = new Node("DeclarationStatement");

            declarationStatement.Children.Add(P_DataType());
            declarationStatement.Children.Add(P_Identifier_List());
            declarationStatement.Children.Add(match(Token_Class.Semicolon_Symbol));

            return declarationStatement;
        }
        Node P_Assignment_statement()
        {
            Node assignmentStatement = new Node("AssignmentStatement");

            assignmentStatement.Children.Add(match(Token_Class.Identifier));
            assignmentStatement.Children.Add(match(Token_Class.Assignment_Operator));
            assignmentStatement.Children.Add(P_Expression());
            assignmentStatement.Children.Add(match(Token_Class.Semicolon_Symbol));

            return assignmentStatement;
        }
        Node P_Repeat_statement()
        {
            Node repeatStatement = new Node("RepeatStatement");

            repeatStatement.Children.Add(match(Token_Class.Repeat_Keyword));
            repeatStatement.Children.Add(P_Statements());
            repeatStatement.Children.Add(match(Token_Class.Until_Keyword));
            repeatStatement.Children.Add(P_Condition_statement());

            return repeatStatement;
        }
       
        Node P_If_statement()
        {
            //If_statement → if Condition_statement then Statements Ret_statement Else_if_statement Else_statement end
            Node ifStatement = new Node("IfStatement");
            
            ifStatement.Children.Add(match(Token_Class.If_Keyword));
            ifStatement.Children.Add(P_Condition_statement());
            ifStatement.Children.Add(match(Token_Class.Then_Keyword));
            ifStatement.Children.Add(P_Statements());
            ifStatement.Children.Add(P_Else_if_statement());
            ifStatement.Children.Add(P_Else_statement());
            ifStatement.Children.Add(match(Token_Class.End_Keyword));

            return ifStatement;
        }
        Node P_Else_if_statement()
        {
            // Else_if_statement → else if Condition_statement then Statements Ret_statement Else_statement | epsilon
            Node elseIfStatement = new Node("ElseIfStatement");
            if (TokenStream[InputPointer].token_type == Token_Class.Else_If_Keyword)
            {
                elseIfStatement.Children.Add(match(Token_Class.Else_If_Keyword));
                elseIfStatement.Children.Add(P_Condition_statement());
                elseIfStatement.Children.Add(match(Token_Class.Then_Keyword));
                elseIfStatement.Children.Add(P_Statements());
                elseIfStatement.Children.Add(P_Else_statement());

                return elseIfStatement;
            }
            else
                return null;
        }
        Node P_Else_statement()
        {
            // Else_statement → else Statements | epsilon
            Node elseStatement = new Node("ElseStatement");

            if (TokenStream[InputPointer].token_type == Token_Class.Else_Keyword)
            {
                elseStatement.Children.Add(match(Token_Class.Else_Keyword));
                elseStatement.Children.Add(P_Statements());
                return elseStatement;
            }
            else
                return null;
        }

        Node P_Statement()
        {
            Node statement = new Node("Statement");

            if (TokenStream[InputPointer].token_type == Token_Class.Write_Keyword)
            {
                statement.Children.Add(P_Write_statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Read_Keyword)
            {
                statement.Children.Add(P_Read_statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Identifier &&
                     TokenStream[InputPointer + 1].token_type == Token_Class.Assignment_Operator)
            {
                statement.Children.Add(P_Assignment_statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Int_DataType ||
                     TokenStream[InputPointer].token_type == Token_Class.Float_DataType ||
                     TokenStream[InputPointer].token_type == Token_Class.String_DataType)
            {
                statement.Children.Add(P_Declaration_statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.If_Keyword)
            {
                statement.Children.Add(P_If_statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Repeat_Keyword)
            {
                statement.Children.Add(P_Repeat_statement());
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Identifier && TokenStream[InputPointer + 1].token_type == Token_Class.Open_Parenthesis)
                        
            {
                statement.Children.Add(P_Function_call());
                statement.Children.Add(match(Token_Class.Semicolon_Symbol));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected Statement and " + TokenStream[InputPointer].token_type.ToString() + " found\r\n");
                InputPointer++;
            }
                return statement;
        }
        Node P_Statements()
        {
            Node statements = new Node("Statements");

            statements.Children.Add(P_Statement());
            statements.Children.Add(P_Statements_D());
            return statements;
        }
        Node P_Statements_D()
        {
            Node statementsD = new Node("StatementsD");

            if (IsStartOfStatement() == true)
            {
                statementsD.Children.Add(P_Statement());
                statementsD.Children.Add(P_Statements_D());

                return statementsD;
            }
            return null;

        }

        Node P_Return_statement()
        {
            Node returnStatement = new Node("ReturnStatement");

            returnStatement.Children.Add(match(Token_Class.Return_Keyword));
            returnStatement.Children.Add(P_Expression());
            returnStatement.Children.Add(match(Token_Class.Semicolon_Symbol));

            return returnStatement;
        }

        Node P_Parameter()
        {
            Node parameter = new Node("Parameter");

            if (   TokenStream[InputPointer].token_type == Token_Class.Int_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.Float_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.String_DataType )
            {
                parameter.Children.Add(match(TokenStream[InputPointer].token_type));
                parameter.Children.Add(match(Token_Class.Identifier));
                return parameter;
            }
            else
                return null;
        }
        Node P_Parameters()
        {
            Node parameters = new Node("Parameters");

            parameters.Children.Add(P_Parameter());
            parameters.Children.Add(P_Parameters_D());

            return parameters;
        }
        Node P_Parameters_D()
        {
            Node parameterD = new Node("ParametersD");

            if (TokenStream[InputPointer].token_type == Token_Class.Comma_Symbol)
            {
                parameterD.Children.Add(match(Token_Class.Comma_Symbol));
                parameterD.Children.Add(P_Parameter());
                parameterD.Children.Add(P_Parameters_D());
            }
            return null;
        }

        Node P_Function_call()
        {
            Node functionCall = new Node("FunctionCall");

            functionCall.Children.Add(match(Token_Class.Identifier));
            functionCall.Children.Add(match(Token_Class.Open_Parenthesis));

            if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
                functionCall.Children.Add(match(Token_Class.Identifier));

            functionCall.Children.Add(match(Token_Class.Close_Parenthesis));
            return functionCall;

        }
        Node P_Function_declaration()
        {
            Node functionDeclaration = new Node("FunctionDeclaration");

            functionDeclaration.Children.Add(P_DataType());
            functionDeclaration.Children.Add(match(Token_Class.Identifier));
            functionDeclaration.Children.Add(match(Token_Class.Open_Parenthesis));
            functionDeclaration.Children.Add(P_Parameters());
            functionDeclaration.Children.Add(match(Token_Class.Close_Parenthesis));

            return functionDeclaration;
        }
        Node P_Function_body()
        {
            Node functionBody = new Node("FunctionBody");

            functionBody.Children.Add(match(Token_Class.Open_Brace));
            functionBody.Children.Add(P_Statements());
            functionBody.Children.Add(P_Return_statement());
            functionBody.Children.Add(match(Token_Class.Close_Brace));

            return functionBody;
        }
        Node P_Function()
        {
            Node functionStatement = new Node("Function");

            if ( ( TokenStream[InputPointer].token_type == Token_Class.Int_DataType
                || TokenStream[InputPointer].token_type == Token_Class.Float_DataType
                || TokenStream[InputPointer].token_type == Token_Class.String_DataType )
                && TokenStream[InputPointer + 1].token_type != Token_Class.Main_Keyword)
            {
                functionStatement.Children.Add(P_Function_declaration());
                functionStatement.Children.Add(P_Function_body());
                return functionStatement;
            }
            else
                return null;
        }
        Node P_Functions()
        {

            Node functionStatements = new Node("Functions");

            functionStatements.Children.Add(P_Function());
            //functionStatements.Children.Add(P_Functions_D());

            return functionStatements;
        }
        Node P_Functions_D()
        {

            Node functionStatementsD = new Node("FunctionsD");

            if ((  TokenStream[InputPointer].token_type == Token_Class.Int_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.Float_DataType  
                || TokenStream[InputPointer].token_type == Token_Class.String_DataType  )
                && TokenStream[InputPointer + 1].token_type != Token_Class.Main_Keyword )
            {

                functionStatementsD.Children.Add(P_Functions());
                return functionStatementsD;
            }
            return null;
        }

        Node P_Main_function()
        {
            Node mainFunction = new Node("MainFunction");

            mainFunction.Children.Add(match(Token_Class.Int_DataType));
            mainFunction.Children.Add(match(Token_Class.Main_Keyword));
            mainFunction.Children.Add(match(Token_Class.Open_Parenthesis));
            mainFunction.Children.Add(match(Token_Class.Close_Parenthesis));
            mainFunction.Children.Add(match(Token_Class.Open_Brace));
            mainFunction.Children.Add(P_Statements());
            mainFunction.Children.Add(P_Return_statement());
            mainFunction.Children.Add(match(Token_Class.Close_Brace));

            return mainFunction;
        }

        public Node match(Token_Class ExpectedToken)
        {
            if (InputPointer < TokenStream.Count)
            {
                if(TokenStream[InputPointer].token_type == Token_Class.Comment_Statement)
                    InputPointer++;
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());
                    return newNode;
                }
                else
                {
                    Errors.Error_List.Add( "Parsing Error: Expected " + ExpectedToken.ToString() + " and " +
                                            TokenStream[InputPointer].token_type.ToString() + "  found\r\n" );
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected " + ExpectedToken.ToString()  + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }

        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
        bool IsStartOfStatement()
        {
            if (InputPointer >= TokenStream.Count)
                return false;

            Token_Class currentToken = TokenStream[InputPointer].token_type;
            Token_Class nextToken = Token_Class.Unknown; // Assuming Token_Class.Null or a similar placeholder exists
            if (InputPointer + 1 < TokenStream.Count)
        {
                nextToken = TokenStream[InputPointer + 1].token_type;
            }

            if (currentToken == Token_Class.Write_Keyword) return true;
            if (currentToken == Token_Class.Read_Keyword) return true;
            if (currentToken == Token_Class.Identifier && nextToken == Token_Class.Assignment_Operator) return true;
            if (currentToken == Token_Class.Int_DataType ||
                currentToken == Token_Class.Float_DataType ||
                currentToken == Token_Class.String_DataType) return true; // Declaration
            if (currentToken == Token_Class.If_Keyword) return true;
            if (currentToken == Token_Class.Repeat_Keyword) return true;
            if (currentToken == Token_Class.Identifier && nextToken == Token_Class.Open_Parenthesis) return true; // Function call statement

            return false;
        }
        bool IsArithmaticOperator()
        {
            if (InputPointer >= TokenStream.Count)
                return false;

            Token_Class type = TokenStream[InputPointer].token_type;
            return type == Token_Class.Plus_Operator ||
                   type == Token_Class.Minus_Operator ||
                   type == Token_Class.Multiply_Operator ||
                   type == Token_Class.Divide_Operator;
        }
        bool IsTerm()
        {
            Token_Class tmp = TokenStream[InputPointer].token_type;
            if (tmp == Token_Class.Number || (tmp == Token_Class.Identifier && true && TokenStream[InputPointer + 1].token_type == Token_Class.Open_Parenthesis) ||
                tmp == Token_Class.Identifier)
            {
                return true;
            }
            return false;
        }
        bool IsEquation()
        {
            Token_Class tmp = TokenStream[InputPointer].token_type;
            if (tmp == Token_Class.Open_Parenthesis || IsTerm() == true)
            {
                return true;
            }
            return false;
        }

    }

}
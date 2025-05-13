using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            root.Children.Add(Program());
            return root;
        }

        Node Program()
        {
            Node program = new Node("Program");

            program.Children.Add(P_Functions());
            program.Children.Add(P_Main_function());

            return program;
        }


        Node P_DataType()
        {
            //dataType -> int | float | string
            Node dataType = new Node("DataType");

            if (TokenStream[InputPointer].token_type == Token_Class.Int_DataType || TokenStream[InputPointer].token_type == Token_Class.Float_DataType || TokenStream[InputPointer].token_type == Token_Class.String_DataType)
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
                arithmaticOperator.Children.Add(match(Token_Class.Divide_Operator);

            return arithmaticOperator;
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
                arithmaticOperator.Children.Add(match(Token_Class.Divide_Operator);

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
 
            // write your code here to check the boolean operator
            return booleanOperator;
        }
        Node P_Term()
        {
            //Term → number | identifier | Function_call
            Node term = new Node("Term");
            if (TokenStream[InputPointer].token_type == Token_Class.Number)
                term.Children.Add(match(Token_Class.Number));
            else if (TokenStream[InputPointer].token_type == Token_Class.Identifiers)
                term.Children.Add(match(Token_Class.Identifiers));
            else if (TokenStream[InputPointer].token_type == Token_Class.Function_Call)
                term.Children.Add(P_Function_call());
            return term; ;
        }

        Node P_Expression()
        {
            //Expression → stringLine | Term | Equation

            Node expression = new Node("Expression");

            if (InputPointer >= TokenStream.Count)
                return expression;

            Token_Class type = TokenStream[InputPointer].token_type;

            if (type == Token_Class.String)
            {
                expression.Children.Add(match(Token_Class.String));
            }
            else if (IsEquation())
            {
                expression.Children.Add(P_Equation());
            }
            else if (IsTerm())
            {
                expression.Children.Add(P_Term());
            }
     

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
            // write your code here to check the equation
            
        }
        Node P_Equation_D()
        {
            // Ops Equation Equation’ | epsilon
            Node equationD = new Node("EquationD");
               if(IsArithmaticOperator())            {
                equationD.Children.Add(P_Arithmatic_Operator());
                equationD.Children.Add(P_Equation());
                equationD.Children.Add(P_Equation_D());
                return equationD;
            }
            return null;
        }

            
                Node P_Condition()
        {
            Node condition = new Node("Condition");
            // write your code here to check the condition
            return condition;
        }


        Node P_Return_statement()
        {
            Node returnStatement = new Node("ReturnStatement");
            // write your code here to check the return statement
            return returnStatement;
        }

        Node P_Write_statement()
        {
            Node writeStatement = new Node("WriteStatement");
            // write your code here to check the write statement
            return writeStatement;
        }
        Node P_Write_statement_D()
        {
            Node writeStatementD = new Node("WriteStatementD");
            // write your code here to check the write statement D
            return writeStatementD;
        }


        Node P_Read_statement()
        {
            Node readStatement = new Node("ReadStatement");
            // write your code here to check the read statement
            return readStatement;
        }
        Node P_Assignment_statement()
        {
            Node assignmentStatement = new Node("AssignmentStatement");
            // write your code here to check the assignment statement
            return assignmentStatement;
        }
        Node P_Declaration_statement()
        {
            Node declarationStatement = new Node("DeclarationStatement");
            // write your code here to check the declaration statement
            return declarationStatement;
        }
        Node P_Repeat_statement()
        {
            Node repeatStatement = new Node("RepeatStatement");
            // write your code here to check the repeat statement
            return repeatStatement;
        }
        Node P_Condition_statement()
        {
            Node conditionStatement = new Node("ConditionStatement");
            // write your code here to check the condition statement
            return conditionStatement;
        }
        Node P_Condition_statement_D()
        {
            Node conditionStatementD = new Node("ConditionStatementD");
            // write your code here to check the condition statement D
            return conditionStatementD;
        }

        Node P_If_statement()
        {
            Node ifStatement = new Node("IfStatement");
            // write your code here to check the if statement
            return ifStatement;
        }
        Node P_Else_if_statement()
        {
            Node elseIfStatement = new Node("ElseIfStatement");
            // write your code here to check the else if statement
            return elseIfStatement;
        }
        Node P_Else_statement()
        {
            Node elseStatement = new Node("ElseStatement");
            // write your code here to check the else statement
            return elseStatement;
        }

        Node P_Statement()
        {
            Node statement = new Node("Statement");
            


            return statement;
        }
        Node P_Statements()
        {
            Node statements = new Node("Statements");

            

            return statements;
        }
        Node P_Statements_D()
        {
            Node statementsD = new Node("StatementsD");
            


            return statementsD;
        }

        Node P_Parameter()
        {
            Node parameter = new Node("Parameter");
            


            return parameter;
        }
        Node P_Parameters()
        {
            Node parameterD = new Node("Parameters");



            return parameterD;
        }
        Node P_Parameters_D()
        {
            Node parameterD = new Node("ParametersD");
            


            return parameterD;
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
            functionDeclaration.Children.Add(P_Parameter());
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
            functionStatements.Children.Add(P_Functions_D());

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
                functionStatementsD.Children.Add(P_Functions_D());
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
            mainFunction.Children.Add(match(Token_Class.Close_Brace));

            return mainFunction;
        }

        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
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
            if (tmp == Token_Class.Number || (tmp == Token_Class.Identifiers && true && TokenStream[InputPointer + 1].token_type == Token_Class.Open_Parenthesis) ||
                tmp == Token_Class.Identifiers)
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

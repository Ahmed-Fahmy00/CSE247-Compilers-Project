using System;
using System.Collections.Generic;
using System.Linq;
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

            program.Children.Add(P_Function_statements());
            program.Children.Add(P_Main_function());

            return program;
        }



        Node P_DataType()
        {
            Node dataType = new Node("DataType");

            if (TokenStream[InputPointer].token_type == Token_Class.Int_DataType || TokenStream[InputPointer].token_type == Token_Class.Float_DataType || TokenStream[InputPointer].token_type == Token_Class.String_DataType)
                dataType.Children.Add(match(TokenStream[InputPointer].token_type));
            else
                dataType.Children.Add(match(Token_Class.DataType));

            return dataType;
        }

        Node P_Arithmatic_Operator()
        {
            Node arithmaticOperator = new Node("ArithmaticOperator");
            
            return arithmaticOperator;
        }
        Node P_Assignment_Operator()
        {
            Node assignmentOperator = new Node("AssignmentOperator");
            // write your code here to check the assignment operator
            return assignmentOperator;
        }
        Node P_Condition_Operator()
        {
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
            Node booleanOperator = new Node("BooleanOperator");
            // write your code here to check the boolean operator
            return booleanOperator;
        }

        Node P_Identifiers()
        {
            Node identifiers = new Node("Identifiers");
            // write your code here to check the identifiers
            return identifiers;
        }

        Node P_Expression()
        {
            Node expression = new Node("Expression");
            // write your code here to check the expression
            return expression;
        }
        Node P_Equation()
        {
            Node equation = new Node("Equation");
            // write your code here to check the equation
            return equation;
        }
        Node P_Equation_D()
        {
            Node equationD = new Node("EquationD");
            // write your code here to check the equation D
            return equationD;
        }
        Node P_Condition()
        {
            Node condition = new Node("Condition");
            // write your code here to check the condition
            return condition;
        }

        Node P_Parameter()
        {
            Node parameter = new Node("Parameter");
            // write your code here to check the parameter
            return parameter;
        }
        Node P_Parameter_D()
        {
            Node parameterD = new Node("ParameterD");
            // write your code here to check the parameter D
            return parameterD;
        }
        Node P_Function_call()
        {
            Node functionCall = new Node("FunctionCall");
            // write your code here to check the function call
            return functionCall;
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
   `        Node repeatStatement = new Node("RepeatStatement");
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
            // write your code here to check the statement
            return statement;
        }
        Node P_Statements()
        {
            Node statements = new Node("Statements");
            // write your code here to check the statements
            return statements;
        }
        Node P_Statements_D()
        {
            Node statementsD = new Node("StatementsD");
            // write your code here to check the statements D
            return statementsD;
        }

        Node P_Function_declaration()
        {
            Node functionDeclaration = new Node("FunctionDeclaration");
            // write your code here to check the function declaration
            return functionDeclaration;
        }
        Node P_Function_body()
        {
            Node functionBody = new Node("FunctionBody");
            // write your code here to check the function body
            return functionBody;
        }
        Node P_Function_statement()
        {
            Node functionStatement = new Node("FunctionStatement");
            // write your code here to check the function statement
            return functionStatement;
        }
        Node P_Function_statements()
        {
            Node functionStatements = new Node("FunctionStatements");
            // write your code here to check the function statements
            return functionStatements;
        }
        Node P_Function_statements_D()
        {
            Node functionStatementsD = new Node("FunctionStatementsD");
            // write your code here to check the function statements D
            return functionStatementsD;
        }
        Node P_Main_function()
        {
            Node mainFunction = new Node("MainFunction");
            // write your code here to check the main function
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

    }

}

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
        private HashSet<Token_Class> SafeTokens;
        public Node StartParsing(List<Token> TokenStream)
        {
            this.SafeTokens = new HashSet<Token_Class>
            {
                Token_Class.Semicolon_Symbol,
                Token_Class.Close_Brace,     
                Token_Class.End_Keyword,     
                Token_Class.Write_Keyword,
                Token_Class.Read_Keyword,
                Token_Class.If_Keyword,
                Token_Class.Repeat_Keyword,
                Token_Class.Int_DataType,    
                Token_Class.Float_DataType,
                Token_Class.String_DataType,
                Token_Class.Identifier,      
                Token_Class.Comment_Statement, 
                Token_Class.Main_Keyword
            };
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(P_Functions());
            root.Children.Add(P_Main_function());
            return root;
        }
        void Recover()
        {
            Errors.Error_List.Add("Unexpected Token: Attempting to synchronize parser.");
            while (InputPointer < TokenStream.Count)
            {
                Token_Class currentToken = TokenStream[InputPointer].token_type;
                if (SafeTokens.Contains(currentToken))
                {
                    if (currentToken == Token_Class.Semicolon_Symbol)
                    {
                        InputPointer++; 
                    }
                    Errors.Error_List.Add($"Synchronization recovery: Resuming parse near token '{currentToken}'.");
                    return;
                }
                InputPointer++;
            }
            Errors.Error_List.Add("Synchronization recovery: Reached end of input while recovering.");
        }

        Node P_DataType()
        {

            //dataType -> int | float | string
            Node dataType = new Node("DataType");
            SkipComments();
            if (   TokenStream[InputPointer].token_type == Token_Class.Int_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.Float_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.String_DataType)
                dataType.Children.Add(match(TokenStream[InputPointer].token_type));
            else
                return null; 
            return dataType;
        }
        Node P_Arithmatic_Operator()
        {
            //arithmatic operator -> plusOp |minusOp | multiplyOp | divideOp
            Node arithmaticOperator = new Node("ArithmaticOperator");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Plus_Operator)
                arithmaticOperator.Children.Add(match(Token_Class.Plus_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Minus_Operator)
                arithmaticOperator.Children.Add(match(Token_Class.Minus_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Multiply_Operator)
                arithmaticOperator.Children.Add(match(Token_Class.Multiply_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Divide_Operator)
                arithmaticOperator.Children.Add(match(Token_Class.Divide_Operator));
            else
                return null; 

            return arithmaticOperator;
        }
        Node P_Condition_Operator()
        {
            //condition operator -> notEqualOp | equalOp | lessThanOp | moreThanOp
            Node conditionOperator = new Node("ConditionOperator");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Not_Equal_Operator)
                conditionOperator.Children.Add(match(Token_Class.Not_Equal_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Equal_Operator)
                conditionOperator.Children.Add(match(Token_Class.Equal_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Less_Than_Operator)
                conditionOperator.Children.Add(match(Token_Class.Less_Than_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.More_Than_Operator)
                conditionOperator.Children.Add(match(Token_Class.More_Than_Operator));
            else
                return null; 
            return conditionOperator;
        }
        Node P_Boolean_Operator()
        {
            //boolean operator -> andOperator | orOperator
            Node booleanOperator = new Node("BooleanOperator");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.And_Operator)
                booleanOperator.Children.Add(match(Token_Class.And_Operator));
            else if (TokenStream[InputPointer].token_type == Token_Class.Or_Operator)
                booleanOperator.Children.Add(match(Token_Class.Or_Operator));
            else return null; 
            return booleanOperator;
        }

        Node P_Term()
        {
            //Term → number | identifier | Function_call
            Node term = new Node("Term");
            SkipComments();
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
            SkipComments();
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
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Open_Parenthesis)
            {
                Node open = match(Token_Class.Open_Parenthesis);
                if (open != null)
                    equation.Children.Add(open);
                else
                    Errors.Error_List.Add("Parsing Error: Expected '(' in equation.");
                Node equa = P_Equation();
                if (equa != null)
                    equation.Children.Add(equa);
                else
                    Errors.Error_List.Add("Parsing Error: Expected equation inside parentheses.");
                Node close = match(Token_Class.Close_Parenthesis);
                if (close != null)
                    equation.Children.Add(close);
                else
                    Errors.Error_List.Add("Parsing Error: Expected ')' in equation.");
                Node node = P_Equation_D();
                if (node != null)
                    equation.Children.Add(node);
            }
            else
            {
                Node term = P_Term();
                if (term != null)
                    equation.Children.Add(term);
                else
                    Errors.Error_List.Add("Parsing Error: Expected term in equation.");
                Node equationD = P_Equation_D();
                if (equationD != null)
                    equation.Children.Add(equationD);
            }
            return equation;
        }
        Node P_Equation_D()
        {
            // Ops Equation Equation’ | epsilon
            Node equationD = new Node("Equation'");
            SkipComments();
            if (IsArithmaticOperator())
            {
                Node arithmaticOperator = P_Arithmatic_Operator();
                if (arithmaticOperator != null)
                    equationD.Children.Add(arithmaticOperator);
                else
                    Errors.Error_List.Add("Parsing Error: Expected arithmatic operator in equation.");
                Node equation = P_Equation();
                if (equation != null)
                    equationD.Children.Add(equation);
                else
                    Errors.Error_List.Add("Parsing Error: Expected equation after arithmatic operator.");
                Node equationD_ = P_Equation_D();
                if (equationD_ != null)
                    equationD.Children.Add(equationD_);
                return equationD;
            }
            else
                return null;
        }
        Node P_Condition()
        {
            //Condition → identifier ConditionOp Term
            Node condition = new Node("Condition");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                Node id = match(Token_Class.Identifier);
                if (id != null)
                    condition.Children.Add(id);
                else
                    Errors.Error_List.Add("Parsing Error: Expected identifier in condition.");
                Node conditionOperator = P_Condition_Operator();
                if (conditionOperator != null)
                    condition.Children.Add(conditionOperator);
                else
                    Errors.Error_List.Add("Parsing Error: Expected condition operator in condition.");
                Node term = P_Term();
                if (term != null)
                    condition.Children.Add(term);
                else
                    Errors.Error_List.Add("Parsing Error: Expected term in condition.");
            }
            return condition;
        }
        Node P_Condition_statement()
        {
            //Condition_statement → Condition Condition_statement
            Node conditionStatement = new Node("ConditionStatement");
            SkipComments();
            Node cond = P_Condition();
            if (cond != null)
                conditionStatement.Children.Add(cond);
            else
                Errors.Error_List.Add("Parsing Error: Expected condition in condition statement.");
            Node conditionStatementD = P_Condition_statement_D();
            if (conditionStatementD != null)
                conditionStatement.Children.Add(conditionStatementD);
            return conditionStatement;
        }
        Node P_Condition_statement_D()
        {
            //Condition_statement_D → Boolean_Operator Condition Condition_statement' | epsilon
            Node conditionStatementD = new Node("ConditionStatement'");
            SkipComments();
            if (   TokenStream[InputPointer].token_type == Token_Class.And_Operator 
                || TokenStream[InputPointer].token_type == Token_Class.Or_Operator )
            {
                Node bol = P_Boolean_Operator();
                if (bol != null)
                    conditionStatementD.Children.Add(bol);
                else
                    Errors.Error_List.Add("Parsing Error: Expected boolean operator in condition statement.");
                conditionStatementD.Children.Add(P_Condition());
                Node conditionStatementD_ = P_Condition_statement_D();
                if (conditionStatementD_ != null)
                    conditionStatementD.Children.Add(conditionStatementD_);
            }
            else  
                return null; 

            return conditionStatementD;

        }

        Node P_Read_statement()
        {
            Node readStatement = new Node("ReadStatement");
            SkipComments();
            Node read = match(Token_Class.Read_Keyword);
            if (read != null)
                readStatement.Children.Add(read);
            else
                Errors.Error_List.Add("Parsing Error: Expected 'read' keyword in read statement.");
            Node id = P_Identifier();
            if (id != null)
                readStatement.Children.Add(id);
            else
                Errors.Error_List.Add("Parsing Error: Expected identifier in read statement.");
            Node semi = match(Token_Class.Semicolon_Symbol);
            if (semi != null)
                readStatement.Children.Add(semi);
            else
                Errors.Error_List.Add("Parsing Error: Expected ';' in read statement.");
            return readStatement;
        }
        Node P_Write_statement()
        {
            // Write_statement → write Write_statement’
            Node writeStatement = new Node("WriteStatement");
            SkipComments();
            Node write = match(Token_Class.Write_Keyword);
            if (write != null)
                writeStatement.Children.Add(write);
            else
                Errors.Error_List.Add("Parsing Error: Expected 'write' keyword in write statement.");
            Node writeStatementD = P_Write_statement_D();
            if (writeStatementD != null)
                writeStatement.Children.Add(writeStatementD);
            else
                Errors.Error_List.Add("Parsing Error: Expected expression or endl in write statement.");
            return writeStatement;

        }
        Node P_Write_statement_D()
        {
            // Write_statement’ → Expression ; | endl ;
            Node writeStatementD = new Node("WriteStatement'");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.String || IsEquation() || IsTerm())
            {
                Node exp = P_Expression();
                if (exp != null)
                    writeStatementD.Children.Add(exp);
                else
                    Errors.Error_List.Add("Parsing Error: Expected expression in write statement.");
                Node semi = match(Token_Class.Semicolon_Symbol);
                if (semi != null)
                    writeStatementD.Children.Add(semi);
                else
                    Errors.Error_List.Add("Parsing Error: Expected ';' in write statement.");
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Endl_Keyword)
            {
                Node endl = match(Token_Class.Endl_Keyword);
                if (endl != null)
                    writeStatementD.Children.Add(endl);
                else
                    Errors.Error_List.Add("Parsing Error: Expected 'endl' in write statement.");
                Node semi = match(Token_Class.Semicolon_Symbol);
                if (semi != null)
                    writeStatementD.Children.Add(semi);
                else
                    Errors.Error_List.Add("Parsing Error: Expected ';' in write statement.");
            }

            return writeStatementD;
        }

        Node P_Identifier()
        {
            Node identifier = new Node("Identifier");
            SkipComments();
            Node id = match(Token_Class.Identifier);
            if (id != null)
                identifier.Children.Add(id);
            else
                Errors.Error_List.Add("Parsing Error: Expected identifier in identifier.");
            Node id_D = P_Identifier_D();
            if (id_D != null)
                identifier.Children.Add(id_D);
            return identifier;
        }
        Node P_Identifier_D()
        {
            Node identifier_D = new Node("Identifier'");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Assignment_Operator)
            {
                Node assign = match(Token_Class.Assignment_Operator);
                if (assign != null)
                    identifier_D.Children.Add(assign);
                else
                    Errors.Error_List.Add("Parsing Error: Expected assignment operator in identifier.");
                Node exp = P_Expression();
                if (exp != null)
                    identifier_D.Children.Add(exp);
                else
                    Errors.Error_List.Add("Parsing Error: Expected expression in identifier.");
                return identifier_D;
            }
            else
                return null;
        }
        Node P_Identifier_List()
        {
            Node identifier_list = new Node("IdentifierList");
            SkipComments();
            Node id = P_Identifier();
            if (id != null)
                identifier_list.Children.Add(id);
            else
                Errors.Error_List.Add("Parsing Error: Expected identifier in identifier list.");
            Node id_D = P_Identifier_List_D();
            if (id_D != null)
                identifier_list.Children.Add(id_D);
            return identifier_list;
        }
        Node P_Identifier_List_D()
        {
            Node identifier_list_D = new Node("IdentifierList'");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Comma_Symbol)
            {
                Node comma = match(Token_Class.Comma_Symbol);
                if (comma != null)
                    identifier_list_D.Children.Add(comma);
                else
                    Errors.Error_List.Add("Parsing Error: Expected ',' in identifier list.");
                Node id = match(Token_Class.Identifier);
                if (id != null)
                    identifier_list_D.Children.Add(id);
                else
                    Errors.Error_List.Add("Parsing Error: Expected identifier in identifier list.");
                Node identifier_list_D_ = P_Identifier_List_D();
                if (identifier_list_D_ != null)
                    identifier_list_D.Children.Add(identifier_list_D_);
                return identifier_list_D;
            }
            else
                return null;
        }
        Node P_Declaration_statement()
        {
            Node declarationStatement = new Node("DeclarationStatement");
            SkipComments();
            Node dataType = P_DataType();
            if (dataType != null)
                declarationStatement.Children.Add(dataType);
            else
                Errors.Error_List.Add("Parsing Error: Expected data type in declaration statement.");
            Node idl = P_Identifier_List();
            if (idl != null)
                declarationStatement.Children.Add(idl);
            else
                Errors.Error_List.Add("Parsing Error: Expected identifier list in declaration statement.");
            Node semi = match(Token_Class.Semicolon_Symbol);
            if (semi != null)
                declarationStatement.Children.Add(semi);
            else
                Errors.Error_List.Add("Parsing Error: Expected ';' in declaration statement.");
            return declarationStatement;
        }
        Node P_Assignment_statement()
        {
            Node assignmentStatement = new Node("AssignmentStatement");
            SkipComments();
            Node id = match(Token_Class.Identifier);
            if (id != null)
                assignmentStatement.Children.Add(id);
            else
                Errors.Error_List.Add("Parsing Error: Expected identifier in assignment statement.");
            Node assign = match(Token_Class.Assignment_Operator);
            if (assign != null)
                assignmentStatement.Children.Add(assign);
            else
                Errors.Error_List.Add("Parsing Error: Expected assignment operator in assignment statement.");
            Node exp = P_Expression();
            if (exp != null)
                assignmentStatement.Children.Add(exp);
            else
                Errors.Error_List.Add("Parsing Error: Expected expression in assignment statement.");
            Node semi = match(Token_Class.Semicolon_Symbol);
            if (semi != null)
                assignmentStatement.Children.Add(semi);
            else
                Errors.Error_List.Add("Parsing Error: Expected ';' in assignment statement.");
            return assignmentStatement;
        }
        Node P_Repeat_statement()
        {
            Node repeatStatement = new Node("RepeatStatement");
            SkipComments();
            Node rep  = match(Token_Class.Repeat_Keyword);
            if (rep != null)
                repeatStatement.Children.Add(rep);
            else
                Errors.Error_List.Add("Parsing Error: Expected 'repeat' keyword in repeat statement.");
            repeatStatement.Children.Add(P_Statements());
            Node until = match(Token_Class.Until_Keyword);
            if (until != null)
                repeatStatement.Children.Add(until);
            else
                Errors.Error_List.Add("Parsing Error: Expected 'until' keyword in repeat statement.");
            repeatStatement.Children.Add(P_Condition_statement());

            return repeatStatement;
        }
       
        Node P_If_statement()
        {
            //If_statement → if Condition_statement then Statements Ret_statement Else_if_statement Else_statement end
            Node ifStatement = new Node("IfStatement");
            SkipComments();
            Node ifk = match(Token_Class.If_Keyword);
            if (ifk != null)
                ifStatement.Children.Add(ifk);
            else
                Errors.Error_List.Add("Parsing Error: Expected 'if' keyword in if statement.");
            ifStatement.Children.Add(P_Condition_statement());
            Node then = match(Token_Class.Then_Keyword);
            if (then != null)
                ifStatement.Children.Add(then);
            else
                Errors.Error_List.Add("Parsing Error: Expected 'then' keyword in if statement.");
            Node stmts = P_Statements();
            if (stmts.Children.Count > 0)
                ifStatement.Children.Add(stmts);
            else
                Errors.Error_List.Add("Parsing Error: Expected statements in if statement.");
            Node Elif = P_Else_if_statement();
            if (Elif != null)
                ifStatement.Children.Add(Elif);
            Node els = P_Else_statement();
            if (els != null)
                ifStatement.Children.Add(els);
            Node end = match(Token_Class.End_Keyword);
            if (end != null)
                ifStatement.Children.Add(end);
            else
                Errors.Error_List.Add("Parsing Error: Expected 'end' keyword in if statement.");
            return ifStatement;
        }
        Node P_Else_if_statement()
        {
            // Else_if_statement → else if Condition_statement then Statements  Else_statement | epsilon
            Node elseIfStatement = new Node("ElseIfStatement");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Else_If_Keyword)
            {
                Node elif = match(Token_Class.Else_If_Keyword);
                if (elif != null)
                    elseIfStatement.Children.Add(elif);
                else
                    Errors.Error_List.Add("Parsing Error: Expected 'else if' keyword in else if statement.");
                elseIfStatement.Children.Add(P_Condition_statement());
                Node then = match(Token_Class.Then_Keyword);
                if (then != null)
                    elseIfStatement.Children.Add(then);
                else
                    Errors.Error_List.Add("Parsing Error: Expected 'then' keyword in else if statement.");
                Node stmts = P_Statements();
                if (stmts.Children.Count > 0)
                    elseIfStatement.Children.Add(stmts);
                else
                    Errors.Error_List.Add("Parsing Error: Expected statements in else if statement.");
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
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Else_Keyword)
            {
                Node elsek = match(Token_Class.Else_Keyword);
                if (elsek != null)
                    elseStatement.Children.Add(elsek);
                else
                    Errors.Error_List.Add("Parsing Error: Expected 'else' keyword in else statement.");
                Node stmts = P_Statements();
                if (stmts.Children.Count > 0)
                    elseStatement.Children.Add(stmts);
                else
                    Errors.Error_List.Add("Parsing Error: Expected statements in else statement.");
                return elseStatement;
            }
            else
                return null;
        }

        Node P_Statement()
        {
            Node statement = new Node("Statement");
            SkipComments();
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
            else if (TokenStream[InputPointer].token_type == Token_Class.Comment_Statement)
            {
                statement.Children.Add(match(Token_Class.Comment_Statement));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected a valid statement.");
                Recover();
                return null;
            }
            return statement;
        }
        Node P_Statements()
        {
            Node statements = new Node("Statements");
            SkipComments();
            Node stmt = P_Statement();
            if (stmt != null)
                statements.Children.Add(stmt);
            else
                Errors.Error_List.Add("Parsing Error: Expected statement in statements.");
            Node statementsD = P_Statements_D();
            if (statementsD != null)
                statements.Children.Add(statementsD);
            return statements;
        }
        Node P_Statements_D()
        {
            Node statementsD = new Node("Statements'");
            SkipComments();
            if (IsStartOfStatement() == true)
            {
                Node stmt = P_Statement();
                if (stmt != null)
                    statementsD.Children.Add(stmt);
                else
                    Errors.Error_List.Add("Parsing Error: Expected statement in statements.");
                Node statementsD_ = P_Statements_D();
                if (statementsD_ != null)
                    statementsD.Children.Add(statementsD_);
                return statementsD;
            }
            return null;
        }

        Node P_Return_statement()
        {
            Node returnStatement = new Node("ReturnStatement");
            SkipComments();
            Node returnKeyword = match(Token_Class.Return_Keyword);
            if (returnKeyword != null)
                returnStatement.Children.Add(returnKeyword);
            else
                return null;
            Node exp = P_Expression();
            if (exp != null)
                returnStatement.Children.Add(exp);
            else
                Errors.Error_List.Add("Parsing Error: Expected expression in return statement.");
            Node sm = match(Token_Class.Semicolon_Symbol);
            if (sm != null)
                returnStatement.Children.Add(sm);
            else
                Errors.Error_List.Add("Parsing Error: Expected ';' in return statement.");
            return returnStatement;
        }
        Node P_Parameter()
        {
            Node parameter = new Node("Parameter");
            SkipComments();
            if (   TokenStream[InputPointer].token_type == Token_Class.Int_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.Float_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.String_DataType )
            {
                Node node = match(TokenStream[InputPointer].token_type);
                if (node != null)
                    parameter.Children.Add(node);
                else
                    Errors.Error_List.Add("Parsing Error: Expected data type in parameter.");
                Node id = match(Token_Class.Identifier);
                if (id != null)
                    parameter.Children.Add(id);
                else
                    Errors.Error_List.Add("Parsing Error: Expected identifier in parameter.");
                return parameter;
            }
            else
                return null;
        }
        Node P_Parameters()
        {
            Node parameters = new Node("Parameters");
            SkipComments();
            Node parameter = P_Parameter();
            if (parameter != null)
                parameters.Children.Add(parameter);
            else
                return null;
            Node parameter_D = P_Parameters_D();
            if (parameter_D != null)
                parameters.Children.Add(parameter_D);
            return parameters;
        }
        Node P_Parameters_D()
        {
            Node parameterD = new Node("Parameters'");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Comma_Symbol)
            {
                Node comma = match(Token_Class.Comma_Symbol);
                if (comma != null)
                    parameterD.Children.Add(comma);
                else
                    Errors.Error_List.Add("Parsing Error: Expected ',' in parameters.");
                Node parameter = P_Parameter();
                if (parameter != null)
                    parameterD.Children.Add(parameter);
                else
                    Errors.Error_List.Add("Parsing Error: Expected parameter in parameters.");
                Node parameterD_ = P_Parameters_D();
                if (parameterD_ != null)
                    parameterD.Children.Add(parameterD_);
                return parameterD;
            }
            return null;
        }

        Node P_Function_call()
        {
            Node functionCall = new Node("FunctionCall");
            SkipComments();
            Node id = match(Token_Class.Identifier);
            if (id != null)
                functionCall.Children.Add(id);
            Node open = match(Token_Class.Open_Parenthesis);
            if (open != null)
                functionCall.Children.Add(open);
            else
                Errors.Error_List.Add("Parsing Error: Expected '(' in function call.");
            if (TokenStream[InputPointer].token_type == Token_Class.Identifier ||
                TokenStream[InputPointer].token_type == Token_Class.Number ||
                TokenStream[InputPointer].token_type == Token_Class.String)
            {
                Node expression = P_Expression();
                if (expression != null)
                    functionCall.Children.Add(expression);
                Node exp_D = P_Function_call_D();
                if (exp_D != null)
                    functionCall.Children.Add(exp_D);
            }
            Node node = match(Token_Class.Close_Parenthesis);
            if (node != null)
                functionCall.Children.Add(node);
            else
                Errors.Error_List.Add("Parsing Error: Expected ')' in function call.");
            return functionCall;
        }
        Node P_Function_call_D() {
            Node functionCallD = new Node("FunctionCall'");
            SkipComments();
            if (TokenStream[InputPointer].token_type == Token_Class.Comma_Symbol)
            {
                Node comma = match(Token_Class.Comma_Symbol);
                if (comma != null)
                    functionCallD.Children.Add(comma);
                else
                    Errors.Error_List.Add("Parsing Error: Expected ',' in function call.");
                Node expression = P_Expression();
                if (expression != null)
                    functionCallD.Children.Add(expression);
                else
                    Errors.Error_List.Add("Parsing Error: Expected expression in function call.");
                Node functionCallD_ = P_Function_call_D();
                if (functionCallD_ != null)
                    functionCallD.Children.Add(functionCallD_);
                return functionCallD;
            }
            return null;
        }
        Node P_Function_declaration()
        {
            Node functionDeclaration = new Node("FunctionDeclaration");
            SkipComments();
            Node dataType = P_DataType();
            if (dataType != null)
                functionDeclaration.Children.Add(dataType);
            else
                Errors.Error_List.Add("Parsing Error: Expected data type in function declaration.");
            Node id = match(Token_Class.Identifier);
            if (id != null)
                functionDeclaration.Children.Add(id);
            else
                Errors.Error_List.Add("Parsing Error: Expected identifier in function declaration.");
            Node open = match(Token_Class.Open_Parenthesis);
            if (open != null)
                functionDeclaration.Children.Add(open);
            else
                Errors.Error_List.Add("Parsing Error: Expected '(' in function declaration.");
            Node parameters = P_Parameters();
            if (parameters != null)
                functionDeclaration.Children.Add(parameters);
            Node node = match(Token_Class.Close_Parenthesis);
            if (node != null)
                functionDeclaration.Children.Add(node);
            else
                Errors.Error_List.Add("Parsing Error: Expected ')' in function declaration.");
            return functionDeclaration;
        }
        Node P_Function_body()
        {
            Node functionBody = new Node("FunctionBody");
            SkipComments();
            Node open = match(Token_Class.Open_Brace);
            if (open != null)
                functionBody.Children.Add(open);
            else
                Errors.Error_List.Add("Parsing Error: Expected '{' to start function body.");
            if (IsStartOfStatement())
            {
                Node statements = P_Statements();
                if (statements != null)
                    functionBody.Children.Add(statements);
                else
                    Errors.Error_List.Add("Parsing Error: Expected statements inside function body.");
            }

            Node returnStatement = P_Return_statement();
            if (returnStatement != null)
                functionBody.Children.Add(returnStatement);
            else
                Errors.Error_List.Add("Parsing Error: Expected return statement inside function body.");
            Node close = match(Token_Class.Close_Brace);
            if (close != null)
                functionBody.Children.Add(close);
            else
                Errors.Error_List.Add("Parsing Error: Expected '}' to end function body.");

            return functionBody;
        }
        Node P_Function()
        {
            Node functionStatement = new Node("Function");
            SkipComments();
            if ( ( TokenStream[InputPointer].token_type == Token_Class.Int_DataType
                || TokenStream[InputPointer].token_type == Token_Class.Float_DataType
                || TokenStream[InputPointer].token_type == Token_Class.String_DataType )
                && TokenStream[InputPointer + 1].token_type != Token_Class.Main_Keyword)
            {
                Node functionDeclaration = P_Function_declaration();
                if (functionDeclaration != null)
                    functionStatement.Children.Add(functionDeclaration);
                else
                    Errors.Error_List.Add("Parsing Error: Expected function declaration.");
                Node functionBody = P_Function_body();
                if (functionBody != null)
                    functionStatement.Children.Add(functionBody);
                else
                    Errors.Error_List.Add("Parsing Error: Expected function body.");
                return functionStatement;
            }
            else
                return null;
        }
        Node P_Functions()
        {

            Node functionStatements = new Node("Functions");
            SkipComments();
            Node functions = P_Function();
            if (functions == null)
                return null;
            else
                functionStatements.Children.Add(functions);
            Node functions_d = P_Functions_D();
            if (functions_d != null)
                functionStatements.Children.Add(functions_d);
            return functionStatements;
        }
        Node P_Functions_D()
        {

            Node functionStatementsD = new Node("Functions'");
            SkipComments();
            if ((  TokenStream[InputPointer].token_type == Token_Class.Int_DataType 
                || TokenStream[InputPointer].token_type == Token_Class.Float_DataType  
                || TokenStream[InputPointer].token_type == Token_Class.String_DataType  )
                && TokenStream[InputPointer + 1].token_type != Token_Class.Main_Keyword )
            {
                Node functionStatements = P_Function();
                if (functionStatements != null)
                    functionStatementsD.Children.Add(functionStatements);
                else
                    Errors.Error_List.Add("Parsing Error: Expected function declaration.");
                return functionStatementsD;
            }
            return null;
        }

        Node P_Main_function()
        {
            Node mainFunction = new Node("MainFunction");
            SkipComments();
            Node intaneger = match(Token_Class.Int_DataType);
            if (intaneger != null)
                mainFunction.Children.Add(intaneger);
            else
                Errors.Error_List.Add("Parsing Error: Expected 'int' data type in main function.");
            Node mani = match(Token_Class.Main_Keyword);
            if (mani != null)
                mainFunction.Children.Add(mani);
            else
                Errors.Error_List.Add("Parsing Error: Expected 'main' keyword in main function.");
            Node open = match(Token_Class.Open_Parenthesis);
            if (open != null)
                mainFunction.Children.Add(open);
            else
                Errors.Error_List.Add("Parsing Error: Expected '(' in main function.");
            Node close = match(Token_Class.Close_Parenthesis);
            if (close != null)
                mainFunction.Children.Add(close);
            else
                Errors.Error_List.Add("Parsing Error: Expected ')' in main function.");
            Node openBrace = match(Token_Class.Open_Brace);
            if (openBrace != null)
                mainFunction.Children.Add(openBrace);
            else
                Errors.Error_List.Add("Parsing Error: Expected '{' to start main function.");
            Node stmts = P_Statements();
            if (stmts != null)
                mainFunction.Children.Add(stmts);
            else
                Errors.Error_List.Add("Parsing Error: Expected statements inside main function.");
            Node returnStatement = P_Return_statement();
            if (returnStatement != null)
                mainFunction.Children.Add(returnStatement);
            else
                Errors.Error_List.Add("Parsing Error: Expected return statement inside main function.");
            Node closeBrace = match(Token_Class.Close_Brace);
            if (closeBrace != null)
                mainFunction.Children.Add(closeBrace);
            else
                Errors.Error_List.Add("Parsing Error: Expected '}' to end main function.");

            return mainFunction;
        }

        public Node match(Token_Class ExpectedToken)
        {
            SkipComments();
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
                Errors.Error_List.Add("Parsing Error: Expected " + ExpectedToken.ToString() + " but " +
                                        TokenStream[InputPointer].token_type.ToString() + " ('" + TokenStream[InputPointer].token_type + "') found at input position " + InputPointer + ".\r\n");
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
            if (InputPointer >= TokenStream.Count) return false;

            Token_Class currentToken = TokenStream[InputPointer].token_type;
            Token_Class nextToken = Token_Class.Unknown;
            if (InputPointer + 1 < TokenStream.Count) nextToken = TokenStream[InputPointer + 1].token_type;
            if (currentToken == Token_Class.Write_Keyword) return true;
            if (currentToken == Token_Class.Read_Keyword) return true;
            if (currentToken == Token_Class.Identifier && nextToken == Token_Class.Assignment_Operator) return true;
            if (currentToken == Token_Class.Int_DataType ||
                currentToken == Token_Class.Float_DataType ||
                currentToken == Token_Class.String_DataType) return true;
            if (currentToken == Token_Class.If_Keyword) return true;
            if (currentToken == Token_Class.Repeat_Keyword) return true;
            if (currentToken == Token_Class.Identifier && nextToken == Token_Class.Open_Parenthesis) return true; 


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
        private void SkipComments()
        {
            while (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comment_Statement)
            {
                InputPointer++;
            }
        }
    }

}
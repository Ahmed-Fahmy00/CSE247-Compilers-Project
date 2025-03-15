using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum Token_Class
{
    Number, String, Reserved_Keywords, Comment_Statement, Identifiers, Function_Call, Term,
    Arithmatic_Operator, Equation, Expression, Assignment_Statement, Datatype, Declaration_Statement,
    Write_Statement, Read_Statement, Return_Statement, Condition_Operator, Condition, Boolean_Operator,
    Condition_Statement, If_Statement, Else_If_Statement, Else_Statement, Repeat_Statement, FunctionName,
    Parameter, Function_Declaration, Function_Body, Function_Statement, Main_Function, Program, Assignment_Operator,
    Symbol



}
namespace JASON_Compiler
{
    

    public class Token
    {
       public string lex;
       public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();
        //Dictionary<string, Token_Class> Symbols = new Dictionary<string, Token_Class>();
        public Scanner()
        {
            ReservedWords.Add("INT", Token_Class.Datatype);
            ReservedWords.Add("FLOAT", Token_Class.Datatype);
            ReservedWords.Add("STRING", Token_Class.Datatype);

            ReservedWords.Add("READ", Token_Class.Reserved_Keywords);
            ReservedWords.Add("WRITE", Token_Class.Reserved_Keywords);
            ReservedWords.Add("REPEAT", Token_Class.Reserved_Keywords);
            ReservedWords.Add("UNTIL", Token_Class.Reserved_Keywords);
            ReservedWords.Add("IF", Token_Class.Reserved_Keywords);
            ReservedWords.Add("ELSEIF", Token_Class.Reserved_Keywords);
            ReservedWords.Add("ELSE", Token_Class.Reserved_Keywords);
            ReservedWords.Add("THEN", Token_Class.Reserved_Keywords);
            ReservedWords.Add("RETURN", Token_Class.Reserved_Keywords);
            ReservedWords.Add("MAIN", Token_Class.Reserved_Keywords);
            ReservedWords.Add("END", Token_Class.Reserved_Keywords);
            ReservedWords.Add("ENDL", Token_Class.Reserved_Keywords);

            Operators.Add("+", Token_Class.Arithmatic_Operator);
            Operators.Add("-", Token_Class.Arithmatic_Operator);
            Operators.Add("*", Token_Class.Arithmatic_Operator);
            Operators.Add("/", Token_Class.Arithmatic_Operator);

            Operators.Add(":=", Token_Class.Assignment_Operator);

            Operators.Add("<", Token_Class.Condition_Operator);
            Operators.Add(">", Token_Class.Condition_Operator);
            Operators.Add("=", Token_Class.Condition_Operator);
            Operators.Add("<>", Token_Class.Condition_Operator);

            Operators.Add("&&", Token_Class.Boolean_Operator);
            Operators.Add("||", Token_Class.Boolean_Operator);


            //Operators.Add(".", Token_Class.Symbol);

            //Symbols.Add(";", Token_Class.Symbol);
            //Symbols.Add(",", Token_Class.Symbol);
            //Symbols.Add("(", Token_Class.Symbol);
            //Symbols.Add(")", Token_Class.Symbol);
            //Symbols.Add("{", Token_Class.Symbol);
            //Symbols.Add("}", Token_Class.Symbol);



        }

        public void StartScanning(string SourceCode)
        {
            for(int i=0; i<SourceCode.Length;i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;

                if (CurrentChar >= 'A' && CurrentChar <= 'z') //if you read a character
                {
                   
                }

                else if(CurrentChar >= '0' && CurrentChar <= '9')
                {
                   
                }
                else if(CurrentChar == '{')
                {
                }
                else
                {
                   
                }
            }
            
            JASON_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            //Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            if (isReservedWord(Lex))
                Tok.token_type = ReservedWords[Lex.ToUpper()];

            //Is it an identifier?
            if (isIdentifier(Lex))
                Tok.token_type = Token_Class.Identifiers;

            //Is it a Constant?
            if (isConstant(Lex))
                Tok.token_type = Token_Class.Number;
            if (isString(Lex))
                Tok.token_type = Token_Class.String;
            //Is it an operator?
            if (IsOperator(Lex))
                Tok.token_type = Operators[Lex];
            if (IsSymbol(Lex))
                Tok.token_type = Token_Class.Symbol;
            if (IsComment(Lex))
                Tok.token_type = Token_Class.Comment_Statement;
            
            //Is it an undefined?
            Tokens.Add(Tok);
        }


        bool isString (string lex)
        {
            // Check if the lex is a string or not.
            Regex RS = new Regex(@"^\"".*\""$"); 
            return RS.IsMatch(lex);
        }
        bool isIdentifier(string lex)
        {
            // Check if the lex is an identifier or not.
            Regex RId = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$");
            return RId.IsMatch(lex);
        }
        bool isConstant(string lex)
        {
            // (\+|\-)? (E (\+|\-)?[0-9]+)?
            // Check if the lex is a constant (Number) or not.
            Regex RC = new Regex(@"^[0-9]+(\.[0-9]+)?$");
            return RC.IsMatch(lex);
        }
        bool IsOperator(string lex)
        {
            // Check if the lex is a constant (Number) or not.
            //return RO.IsMatch(lex);
            //Regex RO = new Regex(@"^\:\=|\=|\<|\>|<>|\+|\-|\*|\/|\&\&|\|\|$");
            return Operators.ContainsKey(lex.ToUpper());
        }
        bool IsSymbol(string lex)
        {
            // Check if the lex is a constant (Number) or not.
            Regex RSymbol = new Regex(@"^;|,|\(|\)|\{|\}$");
            return RSymbol.IsMatch(lex);
        }
        bool IsComment(string lex)
        {
            // Check if the lex is a constant (Number) or not.
            Regex RComment = new Regex(@"^/\*.*\*/$");
            return RComment.IsMatch(lex);
        }
        bool isReservedWord(string lex)
        {
            // Check if the lex is a reserved word or not.
            return ReservedWords.ContainsKey(lex.ToUpper());
        }
    }
}

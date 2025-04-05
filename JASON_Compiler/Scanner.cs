using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum Token_Class
{
    Unknown, Number, Reserved_Keywords, Comment_Statement, Identifiers, Symbol

    , Int_DataType, Float_DataType, String_DataType
    , Read_Keyword, Write_Keyword, Repeat_Keyword, Until_Keyword
    , If_Keyword, Else_If_Keyword, Else_Keyword, Then_Keyword
    , Return_Keyword, Main_Keyword, End_Keyword

    , Arithmatic_Operator, Assignment_Operator, Condition_Operator, Boolean_Operator

    , Plus_Operator,Minus_Operator,Multiply_Operator,Divide_Operator
        
    , Less_Than_Operator,More_Than_Operator,Equal_Operator,Not_Equal_Operator
        
    , And_Operator,Or_Operator

    , Semicolon_Symbol,Comma_Symbol,Open_Parenthesis,Close_Parenthesis,Open_Brace,Close_Brace

    , Declaration_Statement, Function_Call, Term, Equation, Expression, FunctionName
    , Parameter, Function_Declaration, Function_Body, Function_Statement, Program

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
        Dictionary<string, Token_Class> Symbols = new Dictionary<string, Token_Class>();
        public Scanner()
        {

            ReservedWords.Add("int", Token_Class.Int_DataType);
            ReservedWords.Add("float", Token_Class.Float_DataType);
            ReservedWords.Add("string", Token_Class.String_DataType);
            ReservedWords.Add("read", Token_Class.Read_Keyword);
            ReservedWords.Add("write", Token_Class.Write_Keyword);
            ReservedWords.Add("repeat", Token_Class.Repeat_Keyword);
            ReservedWords.Add("until", Token_Class.Until_Keyword);
            ReservedWords.Add("if", Token_Class.If_Keyword);
            ReservedWords.Add("elseif", Token_Class.Else_If_Keyword);
            ReservedWords.Add("else", Token_Class.Else_Keyword);
            ReservedWords.Add("then", Token_Class.Then_Keyword);
            ReservedWords.Add("return", Token_Class.Return_Keyword);
            ReservedWords.Add("main", Token_Class.Main_Keyword);
            ReservedWords.Add("end", Token_Class.End_Keyword);

            Operators.Add("+", Token_Class.Plus_Operator);
            Operators.Add("-", Token_Class.Minus_Operator);
            Operators.Add("*", Token_Class.Multiply_Operator);
            Operators.Add("/", Token_Class.Divide_Operator);

            Operators.Add(":=", Token_Class.Assignment_Operator);

            Operators.Add("<", Token_Class.Less_Than_Operator);
            Operators.Add(">", Token_Class.More_Than_Operator);
            Operators.Add("=", Token_Class.Equal_Operator);
            Operators.Add("<>", Token_Class.Not_Equal_Operator);

            Operators.Add("&&", Token_Class.And_Operator);
            Operators.Add("||", Token_Class.Or_Operator);

            Symbols.Add(";", Token_Class.Semicolon_Symbol);
            Symbols.Add(",", Token_Class.Comma_Symbol);
            Symbols.Add("(", Token_Class.Open_Parenthesis);
            Symbols.Add(")", Token_Class.Close_Parenthesis);
            Symbols.Add("{", Token_Class.Open_Brace);
            Symbols.Add("}", Token_Class.Close_Brace);

        }

        public void StartScanning(string SourceCode)
        {
            for (int l = 0; l < SourceCode.Length; l++)
            {
                int r = l;
                char CurrentChar = SourceCode[r];
                string CurrentLexeme = "";
                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n' || CurrentChar == '\0')
                    continue;

                if ((CurrentChar >= 'A' && CurrentChar <= 'Z') || (CurrentChar >= 'a' && CurrentChar <= 'z')) //if you read a character
                {
                    while (r < SourceCode.Length && ((CurrentChar >= 'A' && CurrentChar <= 'Z') || (CurrentChar >= 'a' && CurrentChar <= 'z')
                    || CurrentChar >= '0' && CurrentChar <= '9'))
                    {
                        CurrentLexeme += CurrentChar;
                        r++;
                        if (r >= SourceCode.Length)
                            break;
                        CurrentChar = SourceCode[r];
                    }
                    if (r < SourceCode.Length && !(CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n' || CurrentChar == '\0'
                        || CurrentChar == '\t'|| CurrentChar == ' '))
                    {
                        Errors.Error_List.Add("Invalid Character in Identifier: " + CurrentChar);
                        //l = r; // to ignore the invalid identifier
                        //continue;
                    }
                    FindTokenClass(CurrentLexeme);
                    l = r;
                }
                else if ((CurrentChar == '/') && (r + 1 < SourceCode.Length && SourceCode[r + 1] == '*'))
                {

                        CurrentLexeme += CurrentChar;
                        r++;
                        if (r >= SourceCode.Length)
                            break;
                        CurrentChar = SourceCode[r];
                        CurrentLexeme += CurrentChar;
                        while (r + 1 < SourceCode.Length && !(CurrentChar == '*' && SourceCode[r + 1] == '/'))
                        {
                            r++;
                            if (r >= SourceCode.Length)
                                break;
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                        if (r + 1 < SourceCode.Length) // "/* ahmed fahmy"
                    {
                            r++;
                            if (r >= SourceCode.Length)
                                break;
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                        else
                        {
                            //Error: Comment not closed
                            Errors.Error_List.Add("Comment not closed: \n" + CurrentLexeme);
                            break;
                        }

                    FindTokenClass(CurrentLexeme);
                    l = r;
                }
                else if (CurrentChar == '\"') //if you read a string
                {
                    r++;
                    if (r >= SourceCode.Length)
                        break;
                    CurrentChar = SourceCode[r];
                    while (r < SourceCode.Length && CurrentChar != '\"')
                    {
                        CurrentLexeme += CurrentChar;
                        r++;
                        if (r >= SourceCode.Length)
                            break;
                        CurrentChar = SourceCode[r];
                    }
                    FindTokenClass('\"' + CurrentLexeme + '\"');
                    l = r;
                }
                else if (CurrentChar >= '0' && CurrentChar <= '9')
                {
                    while (r < SourceCode.Length && CurrentChar >= '0' && CurrentChar <= '9')
                    {
                        CurrentLexeme += CurrentChar;
                        r++;
                        if (r >= SourceCode.Length)
                            break;
                        CurrentChar = SourceCode[r];
                    }
                    if (r < SourceCode.Length && CurrentChar == '.')
                    {
                        CurrentLexeme += CurrentChar;
                        r++;
                        if (r >= SourceCode.Length)
                            break;
                        CurrentChar = SourceCode[r];
                        if (CurrentChar == '.')
                        {
                            //Error: Invalid number
                            Errors.Error_List.Add("Extra Decimal Point: " + CurrentLexeme);
                            continue;
                        }
                        while (r < SourceCode.Length && CurrentChar >= '0' && CurrentChar <= '9')
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length)
                                break;
                            CurrentChar = SourceCode[r];
                        }
                    }
                    FindTokenClass(CurrentLexeme);
                    l = r - 1;
                }
                else if (Symbols.ContainsKey(CurrentChar.ToString()))
                {
                    FindTokenClass(CurrentChar.ToString());
                }
                else if (Operators.ContainsKey(CurrentChar.ToString())
                    || CurrentChar == ':' || CurrentChar == '&' || CurrentChar == '|')
                {
                    if (CurrentChar == ':')
                    {
                        if (r + 1 < SourceCode.Length && SourceCode[r + 1] == '=')
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length)
                                break;
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                    }
                    else if (CurrentChar == '<')
                    {
                        if (r + 1 < SourceCode.Length && SourceCode[r + 1] == '>')
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length)
                                break;
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                    }
                    else if (CurrentChar == '&')
                    {
                        if (r + 1 < SourceCode.Length && SourceCode[r + 1] == '&')
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length)
                                break;
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                    }
                    else if (CurrentChar == '|')
                    {
                        if (r + 1 < SourceCode.Length && SourceCode[r + 1] == '|')
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length)
                                break;
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                    }
                    else
                    {
                        CurrentLexeme += CurrentChar;
                        FindTokenClass(CurrentLexeme);
                        l = r;
                    }
                }
                else
                {
                    CurrentLexeme += CurrentChar;
                    FindTokenClass(CurrentLexeme);
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
                Tok.token_type = ReservedWords[Lex.ToLower()];

            //Is it an identifier?
            else if (isIdentifier(Lex))
                Tok.token_type = Token_Class.Identifiers;

            //Is it a Constant?
            else if (isConstant(Lex))
                Tok.token_type = Token_Class.Number;
            else if (isString(Lex))
                Tok.token_type = Token_Class.String_DataType;
            //Is it an operator?
            else if (IsOperator(Lex))
                Tok.token_type = Operators[Lex];
            else if (IsSymbol(Lex))
                Tok.token_type = Symbols[Lex];
            else if (IsComment(Lex))
                Tok.token_type = Token_Class.Comment_Statement;
            else
                Tok.token_type = Token_Class.Unknown;
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
            return Operators.ContainsKey(lex);
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
            Regex RComment = new Regex(@"/\*[\s\S]*?\*/");
            return RComment.IsMatch(lex);
        }
        bool isReservedWord(string lex)
        {
            // Check if the lex is a reserved word or not.
            return ReservedWords.ContainsKey(lex.ToLower());
        }
    }
}

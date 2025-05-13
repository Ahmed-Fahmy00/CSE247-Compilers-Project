using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum Token_Class
{
    Unknown, Number, Reserved_Keywords, String, Comment_Statement, Identifier, Symbol

    , Int_DataType, Float_DataType, String_DataType
    , Read_Keyword, Write_Keyword, Repeat_Keyword, Until_Keyword
    , If_Keyword, Else_If_Keyword, Else_Keyword, Then_Keyword, Return_Keyword, Main_Keyword, End_Keyword, Endl_Keyword

    , Arithmatic_Operator, Assignment_Operator, Condition_Operator, Boolean_Operator

    , Plus_Operator,Minus_Operator, Multiply_Operator, Divide_Operator
    , Less_Than_Operator, More_Than_Operator, Equal_Operator, Not_Equal_Operator
    , And_Operator,Or_Operator

    , Semicolon_Symbol, Comma_Symbol, Open_Parenthesis, Close_Parenthesis, Open_Brace, Close_Brace
    
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
            ReservedWords.Add("endl", Token_Class.Endl_Keyword);


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
            int Line = 1;
            for (int l = 0; l < SourceCode.Length; l++)
            {
                int r = l;
                char CurrentChar = SourceCode[r];
                string CurrentLexeme = "";

                if (isWhiteSpace(CurrentChar))
                { // For Skiping whitespaces and newlines
                    if (CurrentChar == '\n') Line++;
                    continue;
                }

                if (isLetter(CurrentChar))
                { // For Identifiers or Reserved Words
                    while (r < SourceCode.Length && (isLetter(CurrentChar) || isDigit(CurrentChar)))
                    {
                        CurrentLexeme += CurrentChar;
                        r++;
                        if (r >= SourceCode.Length) break;

                        CurrentChar = SourceCode[r];
                    }
                    if (r < SourceCode.Length && !(isWhiteSpace(CurrentChar) || isOperator(CurrentChar) || isSymbol(CurrentChar) || CurrentChar == ':'
                    || CurrentChar == '&' || CurrentChar == '|'))
                    { // Error For Invalid Characters
                        Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Character in Identifier: \" " + CurrentLexeme + " \" that is : \" " + CurrentChar + " \" \n");
                    }
                    FindTokenClass(CurrentLexeme);
                    l = r - 1;
                }

                else if ((CurrentChar == '/') && (r + 1 < SourceCode.Length && SourceCode[r + 1] == '*'))
                { // For Comments
                    CurrentLexeme += "/*";
                    r++;
                    while (r + 1 < SourceCode.Length && !(CurrentChar == '*' && SourceCode[r + 1] == '/'))
                    {
                        r++;
                        if (r >= SourceCode.Length) break;

                        CurrentChar = SourceCode[r];
                        CurrentLexeme += CurrentChar;
                    }
                    if (r + 1 < SourceCode.Length)
                    {
                        r++;
                        if (r >= SourceCode.Length) break;

                        CurrentChar = SourceCode[r];
                        CurrentLexeme += CurrentChar;
                    }
                    else
                    { //Error For Comment not closed
                        Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Comment not closed: \" " + CurrentLexeme + " \" \n");
                        break;
                    }
                    FindTokenClass(CurrentLexeme);
                    l = r;
                }

                else if (CurrentChar == '\"')
                { // For strings
                    r++;
                    if (r >= SourceCode.Length) break;

                    CurrentChar = SourceCode[r];
                    while (r < SourceCode.Length && CurrentChar != '\"')
                    {
                        CurrentLexeme += CurrentChar;
                        r++;
                        if (r >= SourceCode.Length) break;

                        CurrentChar = SourceCode[r];
                    }
                    if (CurrentChar != '\"')
                    { //Error For String not closed
                        Errors.Error_List.Add("Scanning Error Line: " + Line + ", String not closed: \" " + CurrentLexeme + " \" \n");
                        break;
                    }
                    FindTokenClass('\"' + CurrentLexeme + '\"');
                    l = r;
                }

                else if (isDigit(CurrentChar))
                { // For Numbers
                    int dotCount = 0;
                    while (r < SourceCode.Length && isDigit(CurrentChar))
                    {
                        CurrentLexeme += CurrentChar;
                        r++;
                        if (r >= SourceCode.Length) break;

                        CurrentChar = SourceCode[r];
                    } 
                    if (!isWhiteSpace(CurrentChar) && !isDigit(CurrentChar) && !isOperator(CurrentChar) && !isSymbol(CurrentChar) && CurrentChar != '.')
                    { // Error For Invalid Characters
                        while (r < SourceCode.Length && !isWhiteSpace(CurrentChar) && !isSymbol(CurrentChar) && !isOperator(CurrentChar))
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length) break;

                            CurrentChar = SourceCode[r];
                        }
                        Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Number: " + CurrentLexeme);
                        l = r - 1;
                        continue;
                    }
                    if (r < SourceCode.Length && CurrentChar == '.')
                    {
                        dotCount++;
                        CurrentLexeme += CurrentChar;
                        r++;
                        if (r >= SourceCode.Length)
                        {
                            Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Number: \"" + CurrentLexeme + "\", Number cannot end with a dot.\n");
                            break;
                        }
                        CurrentChar = SourceCode[r];

                        if (r >= SourceCode.Length || isWhiteSpace(CurrentChar))
                        { // Error Decimal no Number "2."
                            Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Number: \"" + CurrentLexeme + "\", Number cannot end with a dot.\n");
                            l = r - 1;
                            continue;
                        }
                        else if (CurrentChar == '.')
                        { //Error: Invalid number 
                            while (!isWhiteSpace(CurrentChar) && !isSymbol(CurrentChar) && !isOperator(CurrentChar) && r < SourceCode.Length)
                            {
                                CurrentLexeme += CurrentChar;
                                r++;
                                if (r >= SourceCode.Length) break;

                                CurrentChar = SourceCode[r];
                            }
                            Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Number: \"" + CurrentLexeme + CurrentChar + "\", Extra decimal point.\n");
                            l = r - 1;
                            continue;
                        }
                        else if (!isDigit(CurrentChar))
                        {
                            while (!isWhiteSpace(CurrentChar) && !isSymbol(CurrentChar) && !isOperator(CurrentChar) && r < SourceCode.Length)
                            {
                                CurrentLexeme += CurrentChar;
                                r++;
                                if (r >= SourceCode.Length) break;

                                CurrentChar = SourceCode[r];
                            }
                            Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Number: \"" + CurrentLexeme + "\", Expected digit after decimal point.\n");

                            l = r - 1;
                            continue;
                        }
                        while (r < SourceCode.Length && (isDigit(CurrentChar) || CurrentChar == '.'))
                        {
                            if (CurrentChar == '.')
                            {
                                while (r < SourceCode.Length && !isWhiteSpace(CurrentChar) && !isSymbol(CurrentChar) && !isOperator(CurrentChar))
                                {
                                    CurrentLexeme += CurrentChar;
                                    r++;
                                    if (r >= SourceCode.Length) break;

                                    CurrentChar = SourceCode[r];
                                }
                                Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Number: \"" + CurrentLexeme + "\", Multiple decimal points.\n");
                                l = r;
                                break;
                            }
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length) break;
                                
                            CurrentChar = SourceCode[r];
                        }
                        if (!isWhiteSpace(CurrentChar) && !isSymbol(CurrentChar) && !isOperator(CurrentChar) && r < SourceCode.Length){
                            while (r < SourceCode.Length && !isWhiteSpace(CurrentChar) && !isSymbol(CurrentChar) && !isOperator(CurrentChar))
                            {
                                CurrentLexeme += CurrentChar;
                                r++;
                                if (r >= SourceCode.Length) break;

                                CurrentChar = SourceCode[r];
                            }
                            Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Number: \"" + CurrentLexeme + "\"\n");
                            l = r - 1;
                            continue;
                        }
                    }
                    FindTokenClass(CurrentLexeme);
                    l = r - 1;
                }

                else if (isOperator(CurrentChar) || CurrentChar == ':' || CurrentChar == '&' || CurrentChar == '|')
                { // For Operators
                    if (CurrentChar == ':')
                    {
                        if (r + 1 < SourceCode.Length && SourceCode[r + 1] == '=')
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length) break;
                                
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                        else
                        {
                            Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Operator: \"" + CurrentChar + "\"\n");
                            continue;

                        }
                    }
                    else if (CurrentChar == '<')
                    {
                        if (r + 1 < SourceCode.Length && SourceCode[r + 1] == '>')
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length) break;
                                
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                        else
                        {
                            CurrentLexeme += CurrentChar;
                            FindTokenClass(CurrentLexeme);
                            l = r;
                            continue;
                        }
                    }
                    else if (CurrentChar == '&')
                    {
                        if (r + 1 < SourceCode.Length && SourceCode[r + 1] == '&')
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length) break;
                                
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                        else
                        {
                            Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Operator: \"" + CurrentChar + "\"\n");
                            continue;
                        }
                    }
                    else if (CurrentChar == '|')
                    {
                        if (r + 1 < SourceCode.Length && SourceCode[r + 1] == '|')
                        {
                            CurrentLexeme += CurrentChar;
                            r++;
                            if (r >= SourceCode.Length) break;
                                
                            CurrentChar = SourceCode[r];
                            CurrentLexeme += CurrentChar;
                        }
                        else
                        {
                            Errors.Error_List.Add("Scanning Error Line: " + Line + " ,Invalid Operator: \"" + CurrentChar + "\"\n");
                            continue;
                        }
                    }
                    else
                        CurrentLexeme += CurrentChar;

                    FindTokenClass(CurrentLexeme);
                    l = r;
                }

                else if (isSymbol(CurrentChar))// For Symbols
                    FindTokenClass(CurrentChar.ToString());

                else
                    Errors.Error_List.Add("Scanning Error Line: " + Line + ", Invalid Character: " + CurrentChar);
            }
            if (!IsValidParentheses(SourceCode))// FOR Validating parentheses after scanning
                Errors.Error_List.Add("Error Line: " + Line + ", Unmatched parentheses in the source code.");

            JASON_Compiler.TokenStream = Tokens;

        }

        bool isWhiteSpace(char CurrentChar)
        {// Check if it is a whitespace
            return CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n' || CurrentChar == '\0' || CurrentChar == '\t'|| CurrentChar == ' ';
        }
        bool isLetter(char CurrentChar)
        {// Check if it is a letter
            return (CurrentChar >= 'A' && CurrentChar <= 'Z') || (CurrentChar >= 'a' && CurrentChar <= 'z');
        }
        bool isDigit(char CurrentChar)
        {// Check if it is a digit
            return CurrentChar >= '0' && CurrentChar <= '9';
        }
        bool isOperator(char CurrentChar)
        {// Check if it is an operator
            return Operators.ContainsKey(CurrentChar.ToString());
        }
        bool isSymbol(char CurrentChar)
        {// Check if it is a symbol
            return Symbols.ContainsKey(CurrentChar.ToString());
        }

        bool IsValidParentheses(string s)
        {// FOR THE NUMBER OF PARENTHESIS
            Stack<char> stack = new Stack<char>();
            Dictionary<char, char> close = new Dictionary<char, char> { { '}', '{' }, { ')', '(' } };
            foreach (char c in s)
            {
                if (close.ContainsKey(c))
                {
                    if (stack.Count > 0 && stack.Peek() == close[c])
                        stack.Pop();
                    else
                        return false;
                }
                else if (close.ContainsValue(c))
                    stack.Push(c);
            }
            return stack.Count == 0;
        }

        void FindTokenClass(string Lex)
        {//Token_Class TC;
            
            Token Tok = new Token();
            Tok.lex = Lex;
            if (IsComment(Lex))//Is it a comment?
                Tok.token_type = Token_Class.Comment_Statement;

            else if (IsString(Lex))//Is it a string?
                Tok.token_type = Token_Class.String;

            else if (IsReservedWord(Lex))//Is it a reserved word?
                Tok.token_type = ReservedWords[Lex.ToLower()];

            else if (IsIdentifier(Lex))//Is it an identifier?
                Tok.token_type = Token_Class.Identifier;

            else if (IsConstant(Lex))//Is it a Constant?
                Tok.token_type = Token_Class.Number;

            else if (IsOperator(Lex))//Is it an operator?
                Tok.token_type = Operators[Lex];

            else if (IsSymbol(Lex))//Is it a symbol?
                Tok.token_type = Symbols[Lex];

            else//Is it an undefined?
                Tok.token_type = Token_Class.Unknown;

            Tokens.Add(Tok);
        }

        bool IsComment(string lex)
        {// Check if the lex is a comment or not.
            Regex RComment = new Regex(@"/\*[\s\S]*?\*/");
            return RComment.IsMatch(lex);
        }
        bool IsString(string lex)
        {// Check if the lex is a string or not.
            Regex RS = new Regex(@"^\"".*\""$");
            return RS.IsMatch(lex);
        } 
        bool IsReservedWord(string lex)
        {// Check if the lex is a reserved word or not.
            return ReservedWords.ContainsKey(lex.ToLower());
        }
        bool IsIdentifier(string lex)
        {// Check if the lex is an identifier or not.
            Regex RId = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$");
            return RId.IsMatch(lex);
        }
        bool IsConstant(string lex)
        {// Check if the lex is a constant (Number) or not.
            Regex RC = new Regex(@"^[0-9]+(\.[0-9]+)?$");
            return RC.IsMatch(lex);
        }
        bool IsOperator(string lex)
        {// Check if the lex is an operator or not.
            return Operators.ContainsKey(lex);
        }
        bool IsSymbol(string lex)
        {// Check if the lex is a Symbol or not.
            Regex RSymbol = new Regex(@"^(;|,|\(|\)|\{|\})$");
            return RSymbol.IsMatch(lex);
        }

    }

}

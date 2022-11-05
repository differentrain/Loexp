using Loexp.Internals.Tokens;
using Loexp.Internals.Tokens.Operators;
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Loexp.Internals
{
    internal static class ExpParseHelper
    {
        [SkipLocalsInit]
        public static string Parse(ExpReader ar)
        {
            ParseContent content = new(ar);
            while (content.Peek())
            {
                switch (content.State)
                {
                    case ParseState.Start:
                        Parse_Start(ref content);
                        break;
                    case ParseState.Argument:
                        Parse_Argument(ref content);
                        break;
                    case ParseState.Argumnet_IntSize:
                        Parse_Argument_IntSize(ref content);
                        break;
                    case ParseState.Argumnet_CodePage:
                        Parse_Argument_CodePage(ref content);
                        break;
                    default: //Argument Error
                        return Utils.GetInvalidArgs();
                }
                if (content.IsBreak)
                    break;
            }
            content.IsBreak = false;
            return InitParse(ref content);
        }

        private static string InitParse(ref ParseContent content)
        {
            return content.State switch
            {
                ParseState.Start => Utils.GetUsages(),
                ParseState.Float => SetNumberTypeAndParse<float>(ref content, OperatorParseAvailabilities.Float),
                ParseState.Double => SetNumberTypeAndParse<double>(ref content, OperatorParseAvailabilities.Float),
                ParseState.INT8 => SetNumberTypeAndParse<sbyte>(ref content, OperatorParseAvailabilities.Signed),
                ParseState.INT16 => SetNumberTypeAndParse<short>(ref content, OperatorParseAvailabilities.Signed),
                ParseState.INT32 => SetNumberTypeAndParse<int>(ref content, OperatorParseAvailabilities.Signed),
                ParseState.INT64 => SetNumberTypeAndParse<long>(ref content, OperatorParseAvailabilities.Signed),
                ParseState.UINT8 => SetNumberTypeAndParse<byte>(ref content, OperatorParseAvailabilities.Unsigned),
                ParseState.UINT16 => SetNumberTypeAndParse<ushort>(ref content, OperatorParseAvailabilities.Unsigned),
                ParseState.UINT32 => SetNumberTypeAndParse<uint>(ref content, OperatorParseAvailabilities.Unsigned),
                ParseState.UINT64 => SetNumberTypeAndParse<ulong>(ref content, OperatorParseAvailabilities.Unsigned),
                ParseState.Bytes => content.Reader.HexBytesToString(content.Encoding),
                ParseState.String => content.Reader.StringToHexBytes(content.Encoding),
                ParseState.Argument or ParseState.Argumnet_IntSize or ParseState.Argumnet_CodePage or ParseState.ArgumentError => Utils.GetInvalidArgs(),
                // ParseState.CodepageError
                _ => "invalid code page.",
            };

            static string SetNumberTypeAndParse<T>(ref ParseContent ct, OperatorParseAvailabilities ava)
                 where T :
                unmanaged,
                IComparable,
                IConvertible,
                ISpanFormattable,
                IComparable<T>,
                IEquatable<T>,
                IBinaryNumber<T>,
                IMinMaxValue<T>
            {
                ct.Available = ava;
                return ParseCore<T>(ref ct);
            }
        }

        private static void Parse_Start(ref ParseContent content)
        {
            switch (content.CharValue)
            {
                case ' ':
                case '\0':
                    ++content.Reader.Position;
                    break;
                case '\\':
                    content.State = ParseState.Argument;
                    ++content.Reader.Position;
                    break;
                default:
                    content.IsSigned = true;
                    content.SetBreak(ParseState.INT64);
                    break;
            }
        }

        private static void Parse_Argument(ref ParseContent content)
        {
            switch (content.CharValue)
            {
                case 'f':
                    content.SetBreak(ParseState.Float);
                    break;
                case 'd':
                    content.SetBreak(ParseState.Double);
                    break;
                case 'i':
                    content.IsSigned = true;
                    content.State = ParseState.Argumnet_IntSize;
                    break;
                case 'u':
                    content.IsSigned = false;
                    content.State = ParseState.Argumnet_IntSize;
                    break;
                case 's':
                    content.IsString = true;
                    content.State = ParseState.Argumnet_CodePage;
                    break;
                case 'b':
                    content.IsString = false;
                    content.State = ParseState.Argumnet_CodePage;
                    break;
                default:
                    content.SetBreak(ParseState.ArgumentError);
                    break;
            }
            ++content.Reader.Position;
        }

        private static void Parse_Argument_IntSize(ref ParseContent content)
        {
            var size = content.CharValue switch
            {
                '1' => content.IsSigned ? ParseState.INT8 : ParseState.UINT8,
                '2' => content.IsSigned ? ParseState.INT16 : ParseState.UINT16,
                '4' => content.IsSigned ? ParseState.INT32 : ParseState.UINT32,
                '8' or '\0' => content.IsSigned ? ParseState.INT64 : ParseState.UINT64,
                _ => ParseState.ArgumentError,
            };
            ++content.Reader.Position;
            content.SetBreak(size);
        }

        private static void Parse_Argument_CodePage(ref ParseContent content)
        {
            if (content.CharValue == '\0')
            {
                SetCodePage(ref content, Encoding.UTF8.CodePage);
                ++content.Reader.Position;
            }
            else if (content.Reader.TryReadInt(out int codepage))
            {
                SetCodePage(ref content, codepage);
            }
            else
                content.SetBreak(ParseState.ArgumentError);

            static void SetCodePage(ref ParseContent ct, int cp)
            {
                try
                {
                    //TODO: if try to impl interactive mode, CodePagesEncodingProvider must be registered just once.
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    ct.Encoding = Encoding.GetEncoding(cp);
                    ct.SetBreak(ct.IsString ? ParseState.String : ParseState.Bytes);
                }
                catch
                {
                    ct.SetBreak(ParseState.CodepageError);
                }
            }
        }


        private static string ParseCore<T>(ref ParseContent content)
             where T :
            unmanaged,
            IComparable,
            IConvertible,
            ISpanFormattable,
            IComparable<T>,
            IEquatable<T>,
            IBinaryNumber<T>,
            IMinMaxValue<T>
        {
            content.State = ParseState.NumberOrUnaryOrFunctionOrConstant;
            while (content.Read())
            {
                switch (content.State)
                {
                    case ParseState.NumberOrUnaryOrFunctionOrConstant:
                        Parse_NumberOrUnaryOrFunctionOrConstant<T>(ref content);
                        break;
                    case ParseState.ZeroOrPrefix:
                        Parse_ZeroOrPrefix<T>(ref content);
                        break;
                    case ParseState.NegOrNumber:
                        Parse_NegOrNumber<T>(ref content);
                        break;
                    case ParseState.BinaryOrBracketOrComma:
                        Parse_BinaryOrBracketOrComma<T>(ref content);
                        break;
                    case ParseState.FunctionLeftBracket:
                        Parse_FunctionLeftBracket(ref content);
                        break;
                    case ParseState.BracketsError:
                        return Utils.GetInvalidBrackets();
                    case ParseState.FunctionError:
                        return Utils.GetUsages("invalid function.\n\n");
                    case ParseState.NotSupportNEG:
                        return Utils.GetUsages("number does not support NEG(-) operator.\n\n");
                    case ParseState.NumberError:
                        return Utils.GetUsages("invalid number.\n\n");
                    default: // ExpressionError
                        return Utils.GetInvalidExp();
                }
            }

            return Calc<T>(ref content);
        }

        [SkipLocalsInit]
        private static string Calc<T>(ref ParseContent content)
            where T :
            unmanaged,
            IComparable,
            IConvertible,
            ISpanFormattable,
            IComparable<T>,
            IEquatable<T>,
            IBinaryNumber<T>,
            IMinMaxValue<T>
        {
            switch (content.State)
            {
                case ParseState.NumberOrUnaryOrFunctionOrConstant: //number
                case ParseState.BinaryOrBracketOrComma: //after number
                    break;
                case ParseState.ZeroOrPrefix: //zero
                    content.Queue.Enqueue(new ExpNumber<T>(T.Zero));
                    break;
                default: // Expression Errors
                    return Utils.GetInvalidExp();
            }

            while (content.Stack.TryPop(out IExpToken? token))
            {
                if (token.TokenType == ExpTokenType.LeftBracket)
                    return Utils.GetInvalidBrackets();
                content.Queue.Enqueue(token);
            }

            if (content.Queue.Count == 0)
                return Utils.GetUsages();

            IExpOperator op;
            ExpNumber<T> result;
            while (content.Queue.TryDequeue(out IExpToken? token))
            {
                if (token.TokenType == ExpTokenType.Number)
                {
                    if (token is ExpNumber<T> num && num.ErrorMesssage != null)
                        return num.ErrorMesssage;
                    content.Stack.Push(token);
                }
                else
                {
                    op = (token as IExpOperator)!;
                    if (content.Stack.Count < op.OperandCount)
                        return Utils.GetInvalidExp();

                    result = op.Calc(CreateNumArgs(content.Stack, op.OperandCount));
                    if (result.ErrorMesssage != null)
                        return result.ErrorMesssage;
                    content.Stack.Push(result);
                }
            }

            return content.Stack.Count == 1 ? (content.Stack.Pop() as ExpNumber<T>)!.ToString() : Utils.GetInvalidExp();

            static ExpNumber<T>[] CreateNumArgs(Stack<IExpToken> t, int count)
            {
                ExpNumber<T>[] args = new ExpNumber<T>[count];
                for (int i = count - 1; i >= 0; i--)
                    args[i] = (t.Pop() as ExpNumber<T>)!;
                return args;
            }

        }

        private static void Parse_NumberOrUnaryOrFunctionOrConstant<T>(ref ParseContent content)
            where T :
            unmanaged,
            IComparable,
            IConvertible,
            ISpanFormattable,
            IComparable<T>,
            IEquatable<T>,
            IBinaryNumber<T>,
            IMinMaxValue<T>
        {
            switch (content.CharValue)
            {
                case ' ':
                case '\0':
                    return;
                case '-':
                    content.State = typeof(T) != typeof(float) &&
                                    typeof(T) != typeof(double) &&
                                    !content.IsSigned ?
                                        ParseState.NotSupportNEG :
                                        ParseState.NegOrNumber;
                    return;
                case '0':
                    content.SetZeroOrPerfix(maybeNeg: false);
                    return;
                case '.':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    content.TrySetNumber<T>(10, -1);
                    return;
                case '(':
                    content.AddLeftBracket();
                    return;
                default:
                    content.TryAddOperator<T>(
                        -1,
                        OperatorParseOptions.Constant |
                        OperatorParseOptions.UnaryOperator |
                        OperatorParseOptions.Function);
                    return;
            }
        }

        private static void Parse_NegOrNumber<T>(ref ParseContent content)
             where T :
            unmanaged,
            IComparable,
            IConvertible,
            ISpanFormattable,
            IComparable<T>,
            IEquatable<T>,
            IBinaryNumber<T>,
            IMinMaxValue<T>
        {
            switch (content.CharValue)
            {
                case ' ':
                case '\0':
                    content.AddUnaryOperator(ExpOperatorNeg.Instance);
                    return;
                case '-':
                    content.AddUnaryOperator(ExpOperatorNeg.Instance);
                    content.State = ParseState.NegOrNumber;
                    return;
                case '(':
                    content.AddUnaryOperator(ExpOperatorNeg.Instance);
                    content.AddLeftBracket();
                    return;
                case '0':
                    content.SetZeroOrPerfix(maybeNeg: true);
                    return;
                case '.':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    content.TrySetNumber<T>(10, -2);
                    return;
                default:
                    content.AddUnaryOperator(ExpOperatorNeg.Instance);
                    content.TryAddOperator<T>(
                     -1,
                     OperatorParseOptions.Constant |
                     OperatorParseOptions.UnaryOperator |
                     OperatorParseOptions.Function);
                    return;
            }
        }

        private static void Parse_FunctionLeftBracket(ref ParseContent content)
        {
            if (content.CharValue == ' ')
                return;
            else if (content.CharValue == '(')
                content.AddLeftBracket();
            else
                content.State = ParseState.FunctionError;
        }

        private static void Parse_ZeroOrPrefix<T>(ref ParseContent content)
             where T :
            unmanaged,
            IComparable,
            IConvertible,
            ISpanFormattable,
            IComparable<T>,
            IEquatable<T>,
            IBinaryNumber<T>,
            IMinMaxValue<T>
        {
            switch (content.CharValue)
            {
                case 'x':
                    ProcessOtherBase(ref content, 16);
                    break;
                case 'o':
                    ProcessOtherBase(ref content, 8);
                    break;
                case 'b':
                    ProcessOtherBase(ref content, 2);
                    break;
                default:
                    content.TrySetNumber<T>(10, content.MayBeNeg ? -3 : -1);
                    return;
            }

            static void ProcessOtherBase(ref ParseContent ct, int bas)
            {
                if (ct.MayBeNeg)
                    ct.Stack.Push(ExpOperatorNeg.Instance);
                ct.TrySetNumber<T>(bas, 0);
            }
        }

        private static void Parse_BinaryOrBracketOrComma<T>(ref ParseContent content)
            where T :
            unmanaged,
            IComparable,
            IConvertible,
            ISpanFormattable,
            IComparable<T>,
            IEquatable<T>,
            IBinaryNumber<T>,
            IMinMaxValue<T>
        {
            switch (content.CharValue)
            {
                case ' ':
                case '\0':
                    return;
                case '(':
                    content.AddBinaryOperator(ExpOperatorMul.Instance);
                    content.AddLeftBracket();
                    return;
                case ',':
                    content.ProcessComma();
                    break;
                case ')':
                    content.ProcessRightBracket();
                    break;
                default:
                    content.TryAddOperator<T>(
                       -1,
                       OperatorParseOptions.BinaryOperator);
                    return;
            }
        }

        private enum ParseState
        {
            #region argument
            /*--------------------------------------------------*/
            Start,
            Argument,
            Argumnet_IntSize,
            Argumnet_CodePage,
            /*--------------------------------------------------*/
            #region exp type
            /*--------------------------------------------------*/
            Float,
            Double,
            INT8,
            INT16,
            INT32,
            INT64,
            UINT8,
            UINT16,
            UINT32,
            UINT64,
            Bytes,
            String,
            /*--------------------------------------------------*/
            #endregion

            #endregion
            #region expression
            /*--------------------------------------------------*/
            NumberOrUnaryOrFunctionOrConstant,
            ZeroOrPrefix,
            NegOrNumber,
            BinaryOrBracketOrComma,
            FunctionLeftBracket,
            /*--------------------------------------------------*/
            #endregion
            #region error
            /*--------------------------------------------------*/
            ArgumentError,
            CodepageError,
            /*--------------------------------------------------*/
            NumberError,
            NotSupportNEG,
            FunctionError,
            BracketsError,
            ExpressionError,
            /*--------------------------------------------------*/
            #endregion

        }


        private ref struct ParseContent
        {
            public readonly ExpReader Reader;
            public int CharValue;

            public ParseState State = ParseState.Start;

            public bool IsSigned = false;
            public bool IsString = false;
            public Encoding Encoding = Encoding.UTF8;
            public OperatorParseAvailabilities Available;


            public bool IsBreak = false;

            public bool MayBeNeg;

            public ParseContent(ExpReader ar) => Reader = ar;

            public Stack<IExpToken> Stack = new(1024);
            public Queue<IExpToken> Queue = new(1024);


            public void SetBreak(ParseState state)
            {
                State = state;
                IsBreak = true;
            }

            public void SetZeroOrPerfix(bool maybeNeg)
            {
                State = ParseState.ZeroOrPrefix;
                MayBeNeg = maybeNeg;
            }

            public void AddConstant<T>(IExpOperator op)
               where T :
               unmanaged,
               IComparable,
               IConvertible,
               ISpanFormattable,
               IComparable<T>,
               IEquatable<T>,
               IBinaryNumber<T>,
               IMinMaxValue<T>
            {
                Queue.Enqueue(op.Calc<T>());
                State = ParseState.BinaryOrBracketOrComma;
            }

            public void AddLeftBracket()
            {
                Stack.Push(ExpOperatorLeftBracket.Instance);
                State = ParseState.NumberOrUnaryOrFunctionOrConstant;
            }

            public void AddUnaryOperator(IExpOperator op)
            {
                Stack.Push(op);
                State = ParseState.NumberOrUnaryOrFunctionOrConstant;
            }

            public void AddFunction(IExpOperator op)
            {
                Stack.Push(op);
                State = ParseState.FunctionLeftBracket;
            }

            public void AddBinaryOperator(IExpOperator op)
            {
                while (Stack.TryPeek(out IExpToken? token))
                {
                    if (token.TokenType != ExpTokenType.Operator ||
                        token is not IExpOperator t ||
                        t.ParseOption != OperatorParseOptions.BinaryOperator ||
                        op.Order > t.Order)
                        break;
                    Queue.Enqueue(Stack.Pop());
                }
                Stack.Push(op);
                State = ParseState.NumberOrUnaryOrFunctionOrConstant;
            }

            public void AddOperator<T>(IExpOperator op)
                where T :
                unmanaged,
                IComparable,
                IConvertible,
                ISpanFormattable,
                IComparable<T>,
                IEquatable<T>,
                IBinaryNumber<T>,
                IMinMaxValue<T>
            {
                switch (op.ParseOption)
                {
                    case OperatorParseOptions.BinaryOperator:
                        AddBinaryOperator(op);
                        break;
                    case OperatorParseOptions.UnaryOperator:
                        AddUnaryOperator(op);
                        break;
                    case OperatorParseOptions.Function:
                        AddFunction(op);
                        break;
                    default: //OperatorParseOptions.Constant:
                        AddConstant<T>(op);
                        break;
                }
            }

            public void TrySetNumber<T>(int fromBase, int indexOffset)
                where T :
                unmanaged,
                IComparable,
                IConvertible,
                ISpanFormattable,
                IComparable<T>,
                IEquatable<T>,
                IBinaryNumber<T>,
                IMinMaxValue<T>
            {
                if (Reader.TryReadNumber(fromBase, indexOffset, out ExpNumber<T> num))
                {
                    Queue.Enqueue(num);
                    State = ParseState.BinaryOrBracketOrComma;
                }
                else
                    State = ParseState.NumberError;
            }

            public void TryAddOperator<T>(int offset, OperatorParseOptions options)
                where T :
                unmanaged,
                IComparable,
                IConvertible,
                ISpanFormattable,
                IComparable<T>,
                IEquatable<T>,
                IBinaryNumber<T>,
                IMinMaxValue<T>
            {
                if (Reader.TryReadOperator(offset, Available, options, out IExpOperator op))
                    AddOperator<T>(op);
                else
                    State = ParseState.ExpressionError;
            }

            public void ProcessComma()
            {
                while (Stack.TryPeek(out IExpToken? token))
                {
                    if (token.TokenType == ExpTokenType.LeftBracket)
                    {
                        State = ParseState.NumberOrUnaryOrFunctionOrConstant;
                        return;
                    }
                    Queue.Enqueue(Stack.Pop());
                }
                State = ParseState.FunctionError;
            }

            public void ProcessRightBracket()
            {
                while (Stack.TryPop(out IExpToken? token))
                {
                    if (token.TokenType == ExpTokenType.LeftBracket)
                    {
                        if (Stack.TryPeek(out token) &&
                            token is IExpOperator op &&
                            op.ParseOption == OperatorParseOptions.Function)
                        {
                            Queue.Enqueue(Stack.Pop());
                            State = ParseState.BinaryOrBracketOrComma;
                        }
                        return;
                    }
                    Queue.Enqueue(token);
                }
                State = ParseState.BracketsError;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Peek() => (CharValue = Reader.Peek()) >= 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Read() => (CharValue = Reader.Read()) >= 0;
        }


    }
}

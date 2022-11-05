using Loexp.Internals.Tokens;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Loexp.Internals
{

    internal sealed class ExpReader : TextReader
    {
        private static readonly List<IExpOperator> s_operators = new();

        internal static void RegisterOperator(IExpOperator op) => s_operators.Add(op);


        private readonly string _arguments;

        public ExpReader(params string[] args)
        {
            StringBuilder sb = new(4096);

            for (int i = 0; i < args.Length; i++)
                _ = sb.Append(args[i]).Append('\0');
            --sb.Length;
            _arguments = sb.ToString();
            Position = 0;
        }

        public int Position { get; set; }


        public override int Peek() => Position < _arguments.Length ? char.ToLower(_arguments[Position]) : -1;


        public override int Read()
        {
            int ret = Peek();
            ++Position;
            return ret;
        }

        [SkipLocalsInit]
        public override string ReadToEnd()
        {
            StringBuilder sb = new(_arguments.Length - Position);
            char ch;
            while (Position < _arguments.Length)
            {
                ch = _arguments[Position++];
                if (ch != '\0')
                    sb.Append(ch);
            }
            return sb.ToString();
        }


        [SkipLocalsInit]
        public bool TryReadInt(out int result)
        {
            ReadOnlySpan<char> span;
            int length = _arguments.Length - Position;
            while (length > 0)
            {
                span = _arguments.AsSpan(Position, length);
                if (int.TryParse(span, NumberStyles.None, null, out result))
                {
                    Position += length;
                    return true;
                }
                --length;
            }
            result = 0;
            return false;
        }

        [SkipLocalsInit]
        public bool TryReadNumber<T>(int fromBase, int posOffset, out ExpNumber<T> result)
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
            ReadOnlySpan<char> span;
            Position += posOffset;
            int length = _arguments.Length - Position;
            while (length > 0)
            {
                span = _arguments.AsSpan(Position, length);
                if (ExpNumber<T>.TryParse(span, fromBase, out result))
                {
                    Position += length;
                    return true;
                }
                --length;
            }
            result = default(T);
            return false;
        }

        [SkipLocalsInit]
        public bool TryReadOperator(int posOffset,
                                    OperatorParseAvailabilities available,
                                    OperatorParseOptions options,
                                    out IExpOperator result)
        {

            result = ExpOperatorEmpty.Instance;
            Position += posOffset;
            if (Position >= _arguments.Length)
                return false;

            List<IExpOperator> foundList = InitOpParseList(available, options);
            int index = Position + 1, charIndex = 1;
            IExpOperator op;
            char ch;
            while (index < _arguments.Length && foundList.Count > 0)
            {
                ch = char.ToLower(_arguments[index]);

                for (int i = 0; i < foundList.Count; i++)
                {
                    op = foundList[i];
                    if (charIndex < op.Pattern.Length)
                    {
                        if (char.ToLower(op.Pattern[charIndex]) != ch)
                            foundList.RemoveAt(i--);
                    }
                    else if (op.Pattern.Length > result.Pattern.Length)
                    {
                        result = op;
                    }
                }
                ++index;
                ++charIndex;
            }
            if (result.Pattern != string.Empty)
            {
                Position += result.Pattern.Length;
                return true;
            }
            if (foundList.Count != 0)
            {
                result = foundList.MaxBy(mOp => mOp.Pattern.Length)!;
                Position += result.Pattern.Length;
                return true;
            }
            return false;
        }

        [SkipLocalsInit]
        private List<IExpOperator> InitOpParseList(OperatorParseAvailabilities available, OperatorParseOptions options)
        {
            IExpOperator op;
            char ch = char.ToLower(_arguments[Position]);
            List<IExpOperator> foundList = new(capacity: s_operators.Count);
            for (int i = 0; i < s_operators.Count; i++)
            {
                op = s_operators[i];
                if (op.Pattern.Length > 0 &&
                    char.ToLower(op.Pattern[0]) == ch &&
                    op.Availabilities.HasFlag(available) &&
                    options.HasFlag(op.ParseOption))
                {
                    foundList.Add(op);
                }
            }
            return foundList;
        }


        public string HexBytesToString(Encoding encoding)
        {
            try
            {
                return encoding.WebName + ": " + encoding.GetString(Convert.FromHexString(ReadToEnd()));
            }
            catch (FormatException)
            {
                return Utils.GetUsages("invalid hex bytes.\n\n");
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string StringToHexBytes(Encoding encoding)
        {
            try
            {
                return encoding.WebName + ": " + Convert.ToHexString(encoding.GetBytes(ReadToEnd()));
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }





    }
}

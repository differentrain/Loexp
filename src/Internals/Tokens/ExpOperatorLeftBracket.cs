using System.Numerics;

namespace Loexp.Internals.Tokens
{
    internal class ExpOperatorLeftBracket : IExpOperator
    {
        public static readonly ExpOperatorLeftBracket Instance = new();

        public OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.None;

        public OperatorParseOptions ParseOption => OperatorParseOptions.None;

        public int OperandCount => 0;

        public int Order => -1;

        public string Pattern => "(";

        public ExpTokenType TokenType => ExpTokenType.LeftBracket;

        public ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) where T : unmanaged, IComparable, IConvertible, ISpanFormattable, IComparable<T>, IEquatable<T>, IBinaryNumber<T>, IMinMaxValue<T>
        {
            return default(T);
        }

        public override string ToString() => Pattern.ToString();

    }
}

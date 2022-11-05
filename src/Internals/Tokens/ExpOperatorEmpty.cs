using System.Numerics;

namespace Loexp.Internals.Tokens
{
    internal class ExpOperatorEmpty : IExpOperator
    {
        public static readonly ExpOperatorEmpty Instance = new();

        public OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.None;

        public OperatorParseOptions ParseOption => OperatorParseOptions.None;

        public int OperandCount => 0;

        public int Order => -1;

        public string Pattern => string.Empty;

        public ExpTokenType TokenType => ExpTokenType.Operator;

        public ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) where T : unmanaged, IComparable, IConvertible, ISpanFormattable, IComparable<T>, IEquatable<T>, IBinaryNumber<T>, IMinMaxValue<T>
        {
            return default(T);
        }
    }
}

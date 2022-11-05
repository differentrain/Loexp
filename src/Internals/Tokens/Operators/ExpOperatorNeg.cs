using System.Numerics;

namespace Loexp.Internals.Tokens.Operators
{
    // TODO: know about Neg
    /* ExpOperatorNeg class did not derived from OperatorUnary class,
     * besause this operator may be the signed prefixed of a dex number.
     * So this type should not be automatic registered with ExpReader,
     * and process it by ourself.
     */
    internal sealed class ExpOperatorNeg : IExpOperator
    {
        public static readonly ExpOperatorNeg Instance = new();

        public ExpTokenType TokenType => ExpTokenType.Operator;

        public string Pattern => "-";

        public int OperandCount => 1;

        public OperatorParseOptions ParseOption => OperatorParseOptions.UnaryOperator;

        public OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Signed | OperatorParseAvailabilities.Float;

        /*
         -, ~        700
         *, /, %     600
         +, -        500
         <<, >>, >>> 400
         &           300
         ^           200
         |           100
         function or constant is 0
         */
        public int Order => 700;

        public ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) where T : unmanaged, IComparable, IConvertible, ISpanFormattable, IComparable<T>, IEquatable<T>, IBinaryNumber<T>, IMinMaxValue<T>
        {
            return -args[0].Value;
        }

        public override string ToString() => Pattern.ToString();
    }
}

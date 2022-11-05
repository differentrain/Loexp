using System.Numerics;

namespace Loexp.Internals
{
    internal interface IExpOperator : IExpToken
    {
        public OperatorParseAvailabilities Availabilities { get; }

        public OperatorParseOptions ParseOption { get; }

        public int OperandCount { get; }

        public int Order { get; }

        public string Pattern { get; }

        public ExpNumber<T> Calc<T>(params ExpNumber<T>[] args)
            where T :
               unmanaged,
               IComparable,
               IConvertible,
               ISpanFormattable,
               IComparable<T>,
               IEquatable<T>,
               IBinaryNumber<T>,
               IMinMaxValue<T>;
    }
}

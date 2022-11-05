using System.Numerics;
using System.Runtime.CompilerServices;

namespace Loexp.Internals
{
    internal abstract class OperatorBase<TSelf> : IExpOperator, ISingleton<TSelf>
        where TSelf : OperatorBase<TSelf>, new()
    {
        public static TSelf Instance => new();

        static OperatorBase() => ExpReader.RegisterOperator(Instance);

        public abstract OperatorParseAvailabilities Availabilities { get; }
        public abstract OperatorParseOptions ParseOption { get; }
        public abstract int OperandCount { get; }
        public abstract int Order { get; }
        public abstract string Pattern { get; }

        public ExpTokenType TokenType => ExpTokenType.Operator;
        public abstract ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) where T : unmanaged, IComparable, IConvertible, ISpanFormattable, IComparable<T>, IEquatable<T>, IBinaryNumber<T>, IMinMaxValue<T>;

        public override string ToString() => Pattern.ToString();

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void Register() { }

    }
}

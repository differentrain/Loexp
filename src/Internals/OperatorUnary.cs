namespace Loexp.Internals
{
    internal abstract class OperatorUnary<TSelf> : OperatorBase<TSelf>
        where TSelf : OperatorUnary<TSelf>, new()
    {
        public override int OperandCount => 1;

        public override OperatorParseOptions ParseOption => OperatorParseOptions.UnaryOperator;

    }
}

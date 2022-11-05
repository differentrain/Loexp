namespace Loexp.Internals
{
    internal abstract class OperatorBinary<TSelf> : OperatorBase<TSelf>
        where TSelf : OperatorBinary<TSelf>, new()
    {
        public override int OperandCount => 2;

        public override OperatorParseOptions ParseOption => OperatorParseOptions.BinaryOperator;
    }
}

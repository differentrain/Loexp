namespace Loexp.Internals
{
    internal abstract class OperatorConstant<TSelf> : OperatorBase<TSelf>
        where TSelf : OperatorConstant<TSelf>, new()
    {
        public override int OperandCount => 0;

        public override OperatorParseOptions ParseOption => OperatorParseOptions.Constant;

        public override int Order => 0;


    }
}

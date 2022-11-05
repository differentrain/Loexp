namespace Loexp.Internals
{
    internal abstract class OperatorFunction<TSelf> : OperatorBase<TSelf>
        where TSelf : OperatorFunction<TSelf>, new()
    {
        public override OperatorParseOptions ParseOption => OperatorParseOptions.Function;

        public override int Order => 0;

    }
}

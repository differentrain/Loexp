namespace Loexp.Internals.Tokens.Operators
{
    internal sealed class ExpOperatorNot : OperatorUnary<ExpOperatorNot>
    {
        public override string Pattern => "~";

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int;

        public override int Order => 700;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) => ~args[0].Value;

    }
}

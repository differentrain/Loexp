namespace Loexp.Internals.Tokens.Operators
{
    internal sealed class ExpOperatorXor : OperatorBinary<ExpOperatorXor>
    {
        public override string Pattern => "^";

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int;

        public override int Order => 200;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) => args[0].Value ^ args[1].Value;

    }
}

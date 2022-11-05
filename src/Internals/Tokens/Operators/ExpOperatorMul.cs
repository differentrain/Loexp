namespace Loexp.Internals.Tokens.Operators
{
    internal sealed class ExpOperatorMul : OperatorBinary<ExpOperatorMul>
    {
        public override string Pattern => "*";

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int | OperatorParseAvailabilities.Float;

        public override int Order => 600;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) => args[0].Value * args[1].Value;

    }
}

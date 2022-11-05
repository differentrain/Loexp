namespace Loexp.Internals.Tokens.Operators
{
    internal sealed class ExpOperatorOr : OperatorBinary<ExpOperatorOr>
    {
        public override string Pattern => "|";

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int | OperatorParseAvailabilities.Float;

        public override int Order => 100;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) => args[0].Value | args[1].Value;

    }
}

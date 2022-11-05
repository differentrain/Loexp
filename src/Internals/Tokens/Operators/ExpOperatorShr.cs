namespace Loexp.Internals.Tokens.Operators
{
    internal sealed class ExpOperatorShr : OperatorBinary<ExpOperatorShr>
    {
        public override string Pattern => ">>>";

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int;

        public override int Order => 400;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) => args[0] >>> args[1];


    }
}

namespace Loexp.Internals.Tokens.Functions
{
    internal sealed class ExpFunctionRor : OperatorFunction<ExpFunctionRor>
    {
        public override string Pattern => "ror";

        public override int OperandCount => 2;

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) => args[0].Ror(args[1]);

    }
}

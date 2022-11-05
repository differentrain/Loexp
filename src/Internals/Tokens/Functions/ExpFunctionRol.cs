namespace Loexp.Internals.Tokens.Functions
{
    internal sealed class ExpFunctionRol : OperatorFunction<ExpFunctionRol>
    {
        public override string Pattern => "rol";

        public override int OperandCount => 2;

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) => args[0].Rol(args[1]);

    }
}

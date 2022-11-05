namespace Loexp.Internals.Tokens.Constants
{
    internal sealed class ExpConstantMin : OperatorConstant<ExpConstantMin>
    {
        public override string Pattern => "min";

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int | OperatorParseAvailabilities.Float;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) => T.MinValue;

    }
}

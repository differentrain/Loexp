namespace Loexp.Internals.Tokens.Constants
{
    internal sealed class ExpConstantMax : OperatorConstant<ExpConstantMax>
    {
        public override string Pattern => "max";

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int | OperatorParseAvailabilities.Float;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args) => T.MaxValue;

    }
}

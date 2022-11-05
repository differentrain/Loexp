namespace Loexp.Internals.Tokens.Operators
{
    internal sealed class ExpOperatorDiv : OperatorBinary<ExpOperatorDiv>
    {
        public override string Pattern => "/";

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int | OperatorParseAvailabilities.Float;

        public override int Order => 600;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args)
        {
            ExpNumber<T> b = args[1];
            return b.Value == T.Zero ?
                new ExpNumber<T>("division by zero") :
                args[0].Value / b.Value;
        }
    }
}

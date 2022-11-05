namespace Loexp.Internals.Tokens
{
    // TODO: implement an operator
    /*
        A class derived from abstract class OperatorBinary, OperatorConstant, OperatorFunction, OperatorUnary or OperatorBase,
        will be automatic registered with ExpReader by invoking OperatorBase<T>.Register() method.
     */
    internal sealed class OperatorImplSample : OperatorConstant<OperatorImplSample>
    {
        public override string Pattern => "sample";

        public override OperatorParseAvailabilities Availabilities => OperatorParseAvailabilities.Int | OperatorParseAvailabilities.Float;

        public override ExpNumber<T> Calc<T>(params ExpNumber<T>[] args)
        {
            return new ExpNumber<T>("Sample:\n" +
                                  "if the ErrorMesssage property of the instance of the ExpNumber<T> is not null,\n" +
                                  "the calculation will be be suspend.");
        }
    }
}

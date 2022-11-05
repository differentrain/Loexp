namespace Loexp.Internals
{
    [Flags]
    internal enum OperatorParseOptions
    {
        None = 0x0000,
        UnaryOperator = 0x0001,
        BinaryOperator = 0x0010,
        Function = 0x0100,
        Constant = 0x1000
    }
}

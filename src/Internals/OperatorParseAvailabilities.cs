namespace Loexp.Internals
{
    [Flags]
    internal enum OperatorParseAvailabilities
    {
        None = 0x0000,
        Signed = 0x0001,
        Unsigned = 0x0010,
        Int = 0x0011,
        Float = 0x0100,
    }
}

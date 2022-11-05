using Loexp.Internals.Tokens;
using Loexp.Internals.Tokens.Constants;
using Loexp.Internals.Tokens.Functions;
using Loexp.Internals.Tokens.Operators;

namespace Loexp.Internals
{
    internal static class Utils
    {

        public static TTo Read<TFrom, TTo>(this TFrom value)
             where TFrom : unmanaged
             where TTo : unmanaged
        {
            ulong buf = 0;
            unsafe
            {
                *(TFrom*)&buf = value;
                return *(TTo*)&buf;
            }
        }


        public static void WriteValue<TSource, TValue>(ref TSource source, TValue value)
             where TSource : unmanaged
             where TValue : unmanaged
        {
            ulong buf = 0;
            unsafe
            {
                *(TValue*)&buf = value;
                source = *(TSource*)&buf;
            }
        }

        public static void RegisterOperators()
        {
            ExpOperatorAdd.Register();
            ExpOperatorAnd.Register();
            ExpOperatorDiv.Register();
            ExpOperatorMod.Register();
            ExpOperatorMul.Register();
            ExpOperatorNot.Register();
            ExpOperatorOr.Register();
            ExpOperatorSar.Register();
            ExpOperatorShl.Register();
            ExpOperatorShr.Register();
            ExpOperatorSub.Register();
            ExpOperatorXor.Register();

            ExpFunctionRol.Register();
            ExpFunctionRor.Register();

            ExpConstantMax.Register();
            ExpConstantMin.Register();

            OperatorImplSample.Register();
        }

        public static string GetInvalidExp() => GetUsages("invalid expression.\n\n");

        public static string GetInvalidArgs() => GetUsages("invalid argument.\n\n");

        public static string GetInvalidBrackets() => GetUsages("mismatching brackets.\n\n");

        public static string GetUsages(string? s = null)
        {
            return $"{s}loexp [expression]\r\nloexp [options] [expression]\r\n----------------------------\r\nIgnores case and all white-spaces between each element will be ignored.\r\n----------------------------\r\nOptions：\r\n  \\f              Using single floating-point number.\r\n  \\d              Using double floating-point number.\r\n  \\i[1/2/4/8]     Using signed 8/16/32/64-bit integer. Deafult value is set to Int64.\r\n  \\u[1/2/4/8]     Using unsigned 8/16/32/64-bit integer. Deafult value is set to UInt64.\r\n  \\b[codepage]    Convert bytes to string by using specified code page. Deafult value is set to utf-8.\r\n  \\s[codepage]    Convert string to bytes by using specified code page.  Deafult value is set to utf-8.\r\n----------------------------\r\nExpression:\r\n  operators       +, -, *, /, %, &, |, ~, ^, <<, >>, >>>\r\n  functions       rol(v,shift), ror(v,shift)\r\n  constants       max, min\r\n\r\nFloating numbers can be in exponent notion.\r\nNumbers with non-decimal bases must be have 0x/0o/0b prefixes.";
        }

    }
}

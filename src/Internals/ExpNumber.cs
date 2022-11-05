using System.Collections.Specialized;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;


namespace Loexp.Internals
{

    internal class ExpNumber<T> : IExpToken, IShiftOperators<ExpNumber<T>, ExpNumber<T>, ExpNumber<T>>
         where T :
            unmanaged,
            IComparable,
            IConvertible,
            ISpanFormattable,
            IComparable<T>,
            IEquatable<T>,
            IBinaryNumber<T>,
            IMinMaxValue<T>
    {

        public ExpTokenType TokenType => ExpTokenType.Number;

        public ExpNumber(T value) => Value = value;

        public ExpNumber(string errorMsg)
        {
            ErrorMesssage = errorMsg;
        }

        public T Value { get; }

        public string? ErrorMesssage;
   
        public ExpNumber<T> Rol(ExpNumber<T> shift)
        {
            T t = Value;
            int sh = shift.Value.Read<T, int>() & (~(Unsafe.SizeOf<T>() << 3));
            switch (Unsafe.SizeOf<T>())
            {
                case 1:
                    RolCore<byte>(ref t, sh);
                    break;
                case 2:
                    RolCore<ushort>(ref t, sh);
                    break;
                case 4:
                    Utils.WriteValue(ref t, BitOperations.RotateLeft(t.Read<T, uint>(), sh));
                    break;
                default:
                    Utils.WriteValue(ref t, BitOperations.RotateLeft(t.Read<T, ulong>(), sh));
                    break;
            }
            return new ExpNumber<T>(t);

            static void RolCore<U>(ref T value, int msh) where U : unmanaged
            {
                uint src = value.Read<T, uint>();
                Utils.WriteValue(ref value, ((src << msh) | (src >> ((Unsafe.SizeOf<T>() << 3) - msh))).Read<uint, U>());
            }
        }

        public ExpNumber<T> Ror(ExpNumber<T> shift)
        {
            int size = Unsafe.SizeOf<T>() << 3;
            T t = Value;
            int sh = shift.Value.Read<T, int>() & (~size);
            switch (Unsafe.SizeOf<T>())
            {
                case 1:
                    RorCore<byte>(ref t, sh);
                    break;
                case 2:
                    RorCore<ushort>(ref t, sh);
                    break;
                case 4:
                    Utils.WriteValue(ref t, BitOperations.RotateRight(t.Read<T, uint>(), sh));
                    break;
                default:
                    Utils.WriteValue(ref t, BitOperations.RotateRight(t.Read<T, ulong>(), sh));
                    break;
            }
            return new ExpNumber<T>(t);

            static void RorCore<U>(ref T value, int msh) where U : unmanaged
            {
                uint src = value.Read<T, uint>();
                Utils.WriteValue(ref value, ((src >> msh) | (src << ((Unsafe.SizeOf<T>() << 3) - msh))).Read<uint, U>());

            }
        }

        public override string ToString()
        {
            if (ErrorMesssage != null)
                return ErrorMesssage;
            StringBuilder sb = new(1024);
            return AppendOutput(sb).ToString();
        }

        [SkipLocalsInit]
        public StringBuilder AppendOutput(StringBuilder sb)
        {
            OrderedDictionary outputDict = new(7)
            {
                { "HEX     ", Value.Read<T, long>().ToString("X", null) }
            };

            switch (Unsafe.SizeOf<T>())
            {
                case 1:
                    InitDict<sbyte, byte>(outputDict, Value);
                    break;
                case 2:
                    InitDict<short, ushort>(outputDict, Value);
                    break;
                case 4:
                    InitDict<int, uint>(outputDict, Value);
                    break;
                default:
                    InitDict<long, ulong>(outputDict, Value);
                    break;
            }


            outputDict.Add("FLOAT   ", Value.Read<T, float>().ToString());
            outputDict.Add("DOUBLE  ", Value.Read<T, double>().ToString());
            outputDict.Add("OCT     ", Convert.ToString(Value.Read<T, long>(), 8));
            outputDict.Add("BIN     ", Convert.ToString(Value.Read<T, long>(), 2));

            string numType = typeof(T) == typeof(float)
                ? "FLOAT   "
                : typeof(T) == typeof(double)
                ? "DOUBLE  "
                : T.MinValue == T.Zero ? $"UINT{Unsafe.SizeOf<T>() << 3:D2}  " : $"INT{Unsafe.SizeOf<T>() << 3:D2}   ";
            _ = sb.Append("------------------------\n")
              .Append(numType)
              .AppendLine(outputDict[numType] as string)
              .AppendLine("------------------------");

            outputDict.Remove(numType);

            foreach (string item in outputDict.Keys)
                _ = sb.Append(item).AppendLine(outputDict[item] as string);

            return sb;

            static void InitDict<TI, TU>(OrderedDictionary dict, T value) where TI : unmanaged where TU : unmanaged
            {
                dict.Add($"INT{Unsafe.SizeOf<T>() << 3:D2}   ", value.Read<T, TI>().ToString());
                dict.Add($"UINT{Unsafe.SizeOf<T>() << 3:D2}  ", value.Read<T, TU>().ToString());
            }

        }

        public static bool TryParse(in ReadOnlySpan<char> s, int fromBase, out ExpNumber<T> result)
        {
            if (fromBase == 10)
                return TryParse(s, out result);
            try
            {
                ulong buf = Unsafe.SizeOf<T>() switch
                {
                    1 => Convert.ToByte(s.ToString(), fromBase),
                    2 => Convert.ToUInt16(s.ToString(), fromBase),
                    4 => Convert.ToUInt32(s.ToString(), fromBase),
                    _ => Convert.ToUInt64(s.ToString(), fromBase),
                };
                unsafe
                {
                    result = Unsafe.Read<T>(&buf);
                    return true;
                }
            }
            catch
            {
                result = default!;
                return false;
            }
        }

        public static bool TryParse(in ReadOnlySpan<char> s, out ExpNumber<T> result)
        {
            bool ret = typeof(T) == typeof(float) || typeof(T) == typeof(double) ?
                        T.TryParse(
                            s,
                            NumberStyles.AllowExponent |
                            NumberStyles.AllowDecimalPoint |
                            NumberStyles.AllowLeadingSign,
                            null,
                            out T r) :
                        T.TryParse(
                            s,
                            T.MinValue != T.Zero ? NumberStyles.AllowLeadingSign : NumberStyles.None,
                            null,
                            out r);
            result = r;
            return ret;
        }

        public static implicit operator ExpNumber<T>(T left) => new(left);

        public static implicit operator T(ExpNumber<T> left) => left.Value;


        public static ExpNumber<T> operator <<(ExpNumber<T> value, ExpNumber<T> shiftAmount)
        {
            return typeof(T) == typeof(double) || typeof(T) == typeof(long) || typeof(T) == typeof(ulong)
                ? (ExpNumber<T>)(value.Value.Read<T, long>() << shiftAmount.Value.Read<T, int>()).Read<long, T>()
                : (ExpNumber<T>)(value.Value.Read<T, int>() << shiftAmount.Value.Read<T, int>()).Read<int, T>();
        }

        public static ExpNumber<T> operator >>(ExpNumber<T> value, ExpNumber<T> shiftAmount)
        {
            return typeof(T) == typeof(double) || typeof(T) == typeof(ulong)
                ? (ExpNumber<T>)(value.Value.Read<T, ulong>() >> shiftAmount.Value.Read<T, int>()).Read<ulong, T>()
                : typeof(T) == typeof(uint) || typeof(T) == typeof(byte) || typeof(T) == typeof(ushort) || typeof(T) == typeof(float)
                ? (ExpNumber<T>)(value.Value.Read<T, long>() >> shiftAmount.Value.Read<T, int>()).Read<long, T>()
                : (ExpNumber<T>)(value.Value.Read<T, int>() >> shiftAmount.Value.Read<T, int>()).Read<int, T>();
        }

        public static ExpNumber<T> operator >>>(ExpNumber<T> value, ExpNumber<T> shiftAmount)
        {
            return typeof(T) == typeof(double) || typeof(T) == typeof(long) || typeof(T) == typeof(ulong)
                ? (ExpNumber<T>)(value.Value.Read<T, long>() >>> shiftAmount.Value.Read<T, int>()).Read<long, T>()
                : (ExpNumber<T>)(value.Value.Read<T, int>() >>> shiftAmount.Value.Read<T, int>()).Read<int, T>();
        }
    }
}

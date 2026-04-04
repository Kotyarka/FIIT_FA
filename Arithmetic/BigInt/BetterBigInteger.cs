using Arithmetic.BigInt.Interfaces;
using Arithmetic.BigInt.MultiplyStrategy;

namespace Arithmetic.BigInt;

public sealed class BetterBigInteger : IBigInteger
{
    private int _signBit;
    
    private uint _smallValue; // Если число маленькое, храним его прямо в этом поле, а _data == null.
    private uint[]? _data;
    
    public bool IsNegative => _signBit == 1;

    private void ListConstructor(uint[] digits, bool isNegative)
    {
        if (digits == null)
        {
            throw new ArgumentNullException(nameof(digits));
        }

        int NonZeroIndex = digits.Length - 1;
        while (NonZeroIndex >= 0 && digits[NonZeroIndex] == 0)
            NonZeroIndex--;

        if (NonZeroIndex < 0) // посчитали сколько нулевых чисел в конце (начале в литл эндиан)
        {
            _signBit = 0;
            _smallValue = 0;
            _data = null;
            return;
        }

        var normalized = new uint[NonZeroIndex + 1];
        Array.Copy(digits, normalized, normalized.Length);

        if (normalized.Length == 1) // если слишком маленькое
        {
            _smallValue = normalized[0];
            _data = null;
            _signBit = (isNegative && normalized[0] != 0) ? 1 : 0;
            return;
        }
        else
        {
            _data = normalized;
            _smallValue = 0;
            _signBit = isNegative ? 1 : 0;
        }
    }

    /// От массива цифр (little endian)
    public BetterBigInteger(uint[] digits, bool isNegative = false)
    {
        ListConstructor(digits, isNegative);
    }
    public BetterBigInteger(IEnumerable<uint> digits, bool isNegative = false) 
        : this(digits?.ToArray() ?? throw new ArgumentNullException(nameof(digits)), isNegative)
    {
    }
    
    public BetterBigInteger(string value, int radix)
    {
        if (value == null || radix == null)
        {
            throw new ArgumentNullException();
        }

        if (radix < 2 || radix > 36)
        {
            throw new ArgumentOutOfRangeException();
        }

         value = value.Trim();
         
        if (value.Length == 0)
        {
            throw new ArgumentException();
        }

        bool isNegative = false;
        if (value[0] == '-')
        {
            isNegative = true;
            value = value.Substring(1);
        } else if (value[0] == '+')
        {
            isNegative = false;
            value = value.Substring(1);
        }

        if (value.Length == 0)
        {
            throw new ArgumentException();
        }

        var result = new uint[] { 0 };
        foreach (char c in value)
        {
            int digit = ChatToDigit(c, radix);
            if (digit < 0 || digit >= radix)
                throw new FormatException();
            result = Gorner(result, radix, digit);
        }

        ListConstructor(result, isNegative);
    }

    private static int ChatToDigit(char c, int? radix)
    {
        if (c >= '0' && c <= '9') 
            return c - '0';
        if (c >= 'A' && c <= 'Z')
            return c - 'A' + 10;
        if (c >= 'a' && c <= 'z')
            return c - 'a' + 10;
        return -1;
    }
    
    private static uint[] Gorner(uint[] digits, int multiplier, int digit)
    {
        digits = MultiplyGorner(digits, multiplier);
        digits = AddGorner(digits, (uint)digit);
        return digits;
    }
    private static uint[] MultiplyGorner(uint[] digits, int multiplier)
    {
        if (multiplier == 0)
        {
            return new uint[] { 0 };
        } else if (multiplier == 1)
        {
            return digits;
        }

        ulong carry = 0;
        var result = new uint[digits.Length + 1]; // заранее еще один разряд

        for (int i = 0; i < digits.Length; i++)
        {
            ulong product = (ulong)digits[i] * (ulong)multiplier + carry;
            result[i] = (uint)(product & 0xFFFFFFFF); // побитовоо отсекам то, что больше 2**32
            carry = product >> 32;
        }
        if (carry > 0)
            result[digits.Length] = (uint)carry; // записываем остаток
        else
            Array.Resize(ref result, digits.Length + 1);

        return result;       
    }

   private static uint[] AddGorner(uint[] digits, uint addend)
    {
        if (addend == 0)
        {
            return digits;
        }

        ulong carry = addend;
        var result = new uint[digits.Length];      
        for (int i = 0; i < digits.Length; i++)
        {
            ulong sum = (uint)digits[i] + carry;
            carry = sum >> 32;
            result[i] = (uint)(sum & 0xFFFFFFFF);         
        }
        if (carry > 0) {
            Array.Resize(ref result, digits.Length + 1);
            result[digits.Length] = (uint)carry;
        }
        return result;
    } 
    public ReadOnlySpan<uint> GetDigits()
    {
        return _data ?? [_smallValue];
    }
    
    public int CompareTo(IBigInteger? other) => throw new NotImplementedException();
    public bool Equals(IBigInteger? other) => throw new NotImplementedException();
    public override bool Equals(object? obj) => obj is IBigInteger other && Equals(other);
    public override int GetHashCode() => throw new NotImplementedException();
    
    
    public static BetterBigInteger operator +(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator -(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator -(BetterBigInteger a) => throw new NotImplementedException();
    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    
    
    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
       => throw new NotImplementedException("Умножение делегируется стратегии, выбирать необходимо в зависимости от размеров чисел");
    
    public static BetterBigInteger operator ~(BetterBigInteger a) => throw new NotImplementedException();
    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator <<(BetterBigInteger a, int shift) => throw new NotImplementedException();
    public static BetterBigInteger operator >> (BetterBigInteger a, int shift) => throw new NotImplementedException();
    
    public static bool operator ==(BetterBigInteger a, BetterBigInteger b) => Equals(a, b);
    public static bool operator !=(BetterBigInteger a, BetterBigInteger b) => !Equals(a, b);
    public static bool operator <(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) < 0;
    public static bool operator >(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) > 0;
    public static bool operator <=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) >= 0;
    
    public override string ToString() => ToString(10);
    public string ToString(int radix) => throw new NotImplementedException();
    
}
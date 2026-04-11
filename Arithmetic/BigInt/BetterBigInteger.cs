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
    
    public int CompareTo(IBigInteger? other)
    {
        if (other == null) return 1;
        if (other is not BetterBigInteger BBIOther) 
            throw new ArgumentException($"wrong type");
        if (IsNegative != BBIOther.IsNegative) 
            return IsNegative ? -1 : 1;
        
        ReadOnlySpan<uint> ThisSpan = GetDigits();
        ReadOnlySpan<uint> OtherSpan = BBIOther.GetDigits();
        
        if (ThisSpan.Length != OtherSpan.Length)
            return IsNegative 
                ? OtherSpan.Length.CompareTo(ThisSpan.Length) 
                : ThisSpan.Length.CompareTo(OtherSpan.Length);
        
        for (int i = ThisSpan.Length - 1; i >= 0; i--)
        {
            if (ThisSpan[i] != OtherSpan[i])
                return IsNegative 
                    ? OtherSpan[i].CompareTo(ThisSpan[i])
                    : ThisSpan[i].CompareTo(OtherSpan[i]);
        }
        return 0;
    }
    public bool Equals(IBigInteger? other)
    {
        return this.CompareTo(other) == 0;
    }
    public override bool Equals(object? obj) => obj is IBigInteger other && Equals(other);
    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        
        hash.Add(_signBit);
        
        if (_data == null)
        {
            hash.Add(_smallValue);
        }
        else
        {
            hash.Add(_data.Length);
            
            int maxDigitsToHash = Math.Min(_data.Length, 16);
            for (int i = 0; i < maxDigitsToHash; i++)
            {
                hash.Add(_data[i]);
            }
            
            if (_data.Length > 16)
            {
                hash.Add(_data[_data.Length - 1]); 
                hash.Add(_data[_data.Length - 2]); 
            }
        }
        
        return hash.ToHashCode(); // может быть коллизия, но зато не будет долгим при огромных числах
    }
    
    
    public static BetterBigInteger operator +(BetterBigInteger a, BetterBigInteger b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        
        if (a.IsNegative == b.IsNegative)
        {
            var result = AddMagnitudes(a.GetDigits(), b.GetDigits());
            return new BetterBigInteger(result, a.IsNegative);
        }
        else
        {
            int compare = CompareMagnitudes(a.GetDigits(), b.GetDigits());
            if (compare == 0)
                return new BetterBigInteger(new uint[] { 0 }, false);
            
            bool resultNegative = compare < 0 ? b.IsNegative : a.IsNegative;
            var result = SubtractMagnitudes(
                compare >= 0 ? a.GetDigits() : b.GetDigits(),
                compare >= 0 ? b.GetDigits() : a.GetDigits()
            );
            return new BetterBigInteger(result, resultNegative);
        }
    }

    public static BetterBigInteger operator -(BetterBigInteger a, BetterBigInteger b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        
        var negatedB = new BetterBigInteger(b.GetDigits().ToArray(), !b.IsNegative);
        return a + negatedB;
    }

    public static BetterBigInteger operator -(BetterBigInteger a)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        
        var digits = a.GetDigits().ToArray();
        bool isNegative = !a.IsNegative && !IsZero(digits);
        return new BetterBigInteger(digits, isNegative);
    }

    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        
        var bDigits = b.GetDigits();
        if (IsZero(bDigits))
            throw new DivideByZeroException();
        
        var aDigits = a.GetDigits();
        if (CompareMagnitudes(aDigits, bDigits) < 0)
            return new BetterBigInteger(new uint[] { 0 }, false);
        
        var result = DivideMagnitudes(aDigits, bDigits, out _);
        bool resultNegative = a.IsNegative != b.IsNegative;
        return new BetterBigInteger(result, resultNegative && !IsZero(result));
    }

    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        
        var bDigits = b.GetDigits();
        if (IsZero(bDigits))
            throw new DivideByZeroException();
        
        var aDigits = a.GetDigits();
        if (CompareMagnitudes(aDigits, bDigits) < 0)
            return new BetterBigInteger(aDigits.ToArray(), a.IsNegative);
        
        DivideMagnitudes(aDigits, bDigits, out uint[] remainder);
        bool remainderZero = IsZero(remainder);
        return new BetterBigInteger(remainder, a.IsNegative && !remainderZero);
    }

    private static uint[] AddMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        int maxLen = Math.Max(a.Length, b.Length);
        var result = new uint[maxLen + 1];
        ulong carry = 0;
        
        for (int i = 0; i < maxLen; i++)
        {
            ulong sum = carry;
            if (i < a.Length) sum += a[i];
            if (i < b.Length) sum += b[i];
            
            result[i] = (uint)(sum & 0xFFFFFFFF);
            carry = sum >> 32;
        }
        
        if (carry > 0)
            result[maxLen] = (uint)carry;
        else
            Array.Resize(ref result, maxLen);
        
        return result;
    }

    private static uint[] SubtractMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        var result = new uint[a.Length];
        ulong borrow = 0;
        
        for (int i = 0; i < a.Length; i++)
        {
            ulong sub = (ulong)a[i] - borrow;
            if (i < b.Length) sub -= b[i];
            
            if ((long)sub < 0)
            {
                sub += 0x100000000UL;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }
            
            result[i] = (uint)sub;
        }
        
        int nonZeroIndex = result.Length - 1;
        while (nonZeroIndex > 0 && result[nonZeroIndex] == 0)
            nonZeroIndex--;
        
        if (nonZeroIndex + 1 < result.Length)
            Array.Resize(ref result, nonZeroIndex + 1);
        
        return result;
    }

    private static int CompareMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        if (a.Length != b.Length)
            return a.Length.CompareTo(b.Length);
        
        for (int i = a.Length - 1; i >= 0; i--)
        {
            if (a[i] != b[i])
                return a[i].CompareTo(b[i]);
        }
        return 0;
    }

    private static bool IsZero(ReadOnlySpan<uint> digits)
    {
        return digits.Length == 1 && digits[0] == 0;
    }

private static uint[] DivideMagnitudes(ReadOnlySpan<uint> dividend, ReadOnlySpan<uint> divisorSpan, out uint[] remainder)
{
    var divisor = divisorSpan.ToArray();
    
    if (CompareMagnitudes(dividend, divisor) < 0)
    {
        remainder = dividend.ToArray();
        return new uint[] { 0 };
    }
    
    int n = divisor.Length;
    int m = dividend.Length - n;
    
    var quotient = new uint[m + 1];
    var remainderArray = new uint[dividend.Length];
    dividend.CopyTo(remainderArray);
    
    uint d = (uint)(0x100000000UL / (divisor[n - 1] + 1));
    var normalizedDividend = MultiplyGorner(remainderArray, (int)d);
    var normalizedDivisor = MultiplyGorner(divisor, (int)d);
    
    if (normalizedDividend.Length <= n + m)
    {
        var temp = new uint[m + n + 1];
        normalizedDividend.CopyTo(temp, 0);
        normalizedDividend = temp;
    }
    
    for (int j = m; j >= 0; j--)
    {
        ulong dividendPart = ((ulong)normalizedDividend[j + n] << 32) + normalizedDividend[j + n - 1];
        ulong qhat = dividendPart / normalizedDivisor[n - 1];
        ulong rhat = dividendPart % normalizedDivisor[n - 1];
        
        if (qhat > 0xFFFFFFFF)
            qhat = 0xFFFFFFFF;
        

        while (n > 1 && qhat * normalizedDivisor[n - 2] > ((rhat << 32) + normalizedDividend[j + n - 2]))
        {
            qhat--;
            rhat += normalizedDivisor[n - 1];
            if (rhat >= 0x100000000UL)
                break;
        }
        
        ulong borrow = 0;
        ulong carry = 0;
        
        for (int i = 0; i < n; i++)
        {
            ulong product = qhat * normalizedDivisor[i] + carry;
            carry = product >> 32;
            
            long diff = (long)normalizedDividend[j + i] - (long)(product & 0xFFFFFFFF) - (long)borrow;
            
            if (diff < 0)
            {
                diff += 0x100000000L;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }
            
            normalizedDividend[j + i] = (uint)diff;
        }
        
        long finalDiff = (long)normalizedDividend[j + n] - (long)borrow;
        normalizedDividend[j + n] = (uint)finalDiff;
        
        quotient[j] = (uint)qhat;
        
        if (finalDiff < 0)
        {
            quotient[j]--;
            carry = 0;
            
            for (int i = 0; i < n; i++)
            {
                ulong sum = (ulong)normalizedDividend[j + i] + normalizedDivisor[i] + carry;
                normalizedDividend[j + i] = (uint)sum;
                carry = sum >> 32;
            }
            
            normalizedDividend[j + n] += (uint)carry;
        }
    }
    
    int qi = quotient.Length - 1;
    while (qi > 0 && quotient[qi] == 0)
        qi--;
    
    if (qi + 1 < quotient.Length)
        Array.Resize(ref quotient, qi + 1);
    
    int lastNonZero = normalizedDividend.Length - 1;
    while (lastNonZero >= 0 && normalizedDividend[lastNonZero] == 0)
        lastNonZero--;
    
    if (lastNonZero < 0)
    {
        remainder = new uint[] { 0 };
    }
    else
    {
        var remainderNormalized = new uint[lastNonZero + 1];
        Array.Copy(normalizedDividend, 0, remainderNormalized, 0, lastNonZero + 1);
        remainder = DivideByDigit(remainderNormalized, d);
        
        int ri = remainder.Length - 1;
        while (ri > 0 && remainder[ri] == 0)
            ri--;
        
        if (ri + 1 < remainder.Length)
            Array.Resize(ref remainder, ri + 1);
    }
    
    return quotient;
}

    private static uint[] DivideByDigit(uint[] digits, uint divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException();
        
        if (digits.Length == 1 && digits[0] == 0)
            return new uint[] { 0 };
        
        var result = new uint[digits.Length];
        ulong remainder = 0;
        
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            ulong value = (remainder << 32) + digits[i];
            result[i] = (uint)(value / divisor);
            remainder = value % divisor;
        }
        
        int nonZeroIndex = result.Length - 1;
        while (nonZeroIndex > 0 && result[nonZeroIndex] == 0)
            nonZeroIndex--;
        
        if (nonZeroIndex + 1 < result.Length)
            Array.Resize(ref result, nonZeroIndex + 1);
        
        return result;
    }
    
    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
    {
        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();
        if ((aDigits.Length + bDigits.Length) < 20)
        {
            SimpleMultiplier mult = new();
            return mult.Multiply(a, b);
        } else if ((aDigits.Length + bDigits.Length) <= 40)
        {
            KaratsubaMultiplier mult = new();
            return mult.Multiply(a, b);
        } else
        {
            FftMultiplier mult = new();
            return mult.Multiply(a, b);
        }
    }
    private uint[] ToTwosComplement(ReadOnlySpan<uint> a, bool isNegative)
    {
        uint[] magnitude = a.ToArray();
        int length = magnitude.Length;
        uint[] result = new uint[length];
        ulong carry = 1;
        for (int i = 0; i < length, i++)
        {
            result[i] = ~magnitude[i];
            ulong res = result[i] + carry;
            result[i] = (uint)res;
            carry = res >> 32;
        }
        return result;
    }
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
    public string ToString(int radix)
{
    if (radix < 2 || radix > 36)
        throw new ArgumentOutOfRangeException(nameof(radix), "Radix must be between 2 and 36");
    
    var digits = GetDigits();
    
    if (digits.Length == 1 && digits[0] == 0)
        return "0";
    
    var result = new List<char>();
    var workingCopy = digits.ToArray();
    
    while (!(workingCopy.Length == 1 && workingCopy[0] == 0))
    {
        var quotient = DivideByDigit(workingCopy, (uint)radix);
        
        ulong remainder = 0;
        for (int i = workingCopy.Length - 1; i >= 0; i--)
        {
            ulong value = (remainder << 32) | workingCopy[i];
            remainder = value % (uint)radix;
        }
        
        result.Add(DigitToChar((int)remainder));
        workingCopy = quotient;
    }
    
    if (result.Count == 0)
        result.Add('0');
    
    result.Reverse();
    
    return (IsNegative ? "-" : "") + new string(result.ToArray());
}

private static char DigitToChar(int digit)
{
    if (digit < 10)
        return (char)('0' + digit);
    else
        return (char)('A' + digit - 10);
}
    
}
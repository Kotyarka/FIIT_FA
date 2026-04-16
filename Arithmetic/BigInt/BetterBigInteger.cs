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

        int nonZeroIndex = digits.Length - 1;
        while (nonZeroIndex >= 0 && digits[nonZeroIndex] == 0)
            nonZeroIndex--;

        if (nonZeroIndex < 0) // посчитали сколько нулевых чисел в конце (начале в литл эндиан)
        {
            _signBit = 0;
            _smallValue = 0;
            _data = null;
            return;
        }

        var normalized = new uint[nonZeroIndex + 1];
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
        if (value == null)
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
        }
        else if (value[0] == '+')
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
        }
        else if (multiplier == 1)
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

        if (carry > 0)
        {
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
        if (other is not BetterBigInteger bbiOther)
            throw new ArgumentException($"wrong type");

        if (IsNegative != bbiOther.IsNegative)
            return IsNegative ? -1 : 1;

        ReadOnlySpan<uint> thisSpan = GetDigits();
        ReadOnlySpan<uint> otherSpan = bbiOther.GetDigits();

        if (thisSpan.Length != otherSpan.Length)
            return IsNegative
                ? otherSpan.Length.CompareTo(thisSpan.Length)
                : thisSpan.Length.CompareTo(otherSpan.Length);

        for (int i = thisSpan.Length - 1; i >= 0; i--)
        {
            if (thisSpan[i] != otherSpan[i])
                return IsNegative
                    ? otherSpan[i].CompareTo(thisSpan[i])
                    : thisSpan[i].CompareTo(otherSpan[i]);
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


        var negatedB = new BetterBigInteger(b.GetDigits().ToArray(), !b.IsNegative);
        return a + negatedB;
    }

    public static BetterBigInteger operator -(BetterBigInteger a)
    {

        var digits = a.GetDigits().ToArray();
        bool isNegative = !a.IsNegative && !IsZero(digits);
        return new BetterBigInteger(digits, isNegative);
    }

    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b)
    {

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


        return DivideLongDivision(dividend, divisorSpan, out remainder);
    }

/* I call it Varenik's algorithm
                По сути это простое деление столбиком, но вместо умных прикидок Кнута
                здесь используется поиск подходящей цифры в частное бинарным поиском.
                Конечно, это менее оптимизированно, зато достаточно просто в реализации.
                                                                                            */
    private static uint[] DivideLongDivision(ReadOnlySpan<uint> dividend, ReadOnlySpan<uint> divisor, out uint[] remainder)
    {
        var dividendBI = new BetterBigInteger(dividend.ToArray(), false);
        var divisorBI = new BetterBigInteger(divisor.ToArray(), false);

        string dividendStr = dividendBI.ToString();
        string divisorStr = divisorBI.ToString();

        if (dividendStr.Length < divisorStr.Length)
        {
            remainder = dividend.ToArray();
            return new uint[] { 0 };
        }

        var quotientDigits = new List<uint>();
        var currentDividend = new BetterBigInteger(new uint[] { 0 }, false);

        for (int i = 0; i < dividendStr.Length; i++)
        {
            int digit = dividendStr[i] - '0';
            currentDividend = currentDividend * new BetterBigInteger(new uint[] { 10 }, false) + new BetterBigInteger(new uint[] { (uint)digit }, false);

            if (currentDividend < divisorBI)
            {
                if (quotientDigits.Count > 0)
                    quotientDigits.Add(0);
                continue;
            }

            uint qDigit = BinarySearchDigit(currentDividend, divisorBI);
            quotientDigits.Add(qDigit);

            var product = new BetterBigInteger(new uint[] { qDigit }, false) * divisorBI;
            currentDividend = currentDividend - product;
        }

        var quotient = new BetterBigInteger(new uint[] { 0 }, false);
        foreach (var qd in quotientDigits)
        {
            quotient = quotient * new BetterBigInteger(new uint[] { 10 }, false) + new BetterBigInteger(new uint[] { qd }, false);
        }

        var remainderBI = currentDividend;
        remainder = remainderBI.GetDigits().ToArray();

        return quotient.GetDigits().ToArray();
    }

    private static uint BinarySearchDigit(BetterBigInteger target, BetterBigInteger divisor)
    {
        if (target < divisor)
            return 0;

        uint left = 0;
        uint right = 9;
        uint result = 0;

        while (left <= right)
        {
            uint mid = (left + right) / 2;
            var product = new BetterBigInteger(new uint[] { mid }, false) * divisor;

            if (product <= target)
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return result;
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
        }
        else if ((aDigits.Length + bDigits.Length) <= 40)
        {
            KaratsubaMultiplier mult = new();
            return mult.Multiply(a, b);
        }
        else
        {
            FftMultiplier mult = new();
            return mult.Multiply(a, b);
        }
    }

    private static uint[] ToTwosComplement(ReadOnlySpan<uint> a, bool isNegative, int length)
    {
        uint[] magnitude = a.ToArray();
        if (magnitude.Length < length)
        {
            Array.Resize(ref magnitude, length);
        }

        if (isNegative == false)
        {
            return magnitude;
        }

        uint[] result = new uint[length];
        ulong carry = 1;

        for (int i = 0; i < length; i++)
        {
            result[i] = ~magnitude[i];
            ulong res = result[i] + carry;
            result[i] = (uint)res;
            carry = res >> 32;
        }

        return result;
    }

    private static (uint[] magnitude, bool negative) FromTwosComplement(uint[] binary)
    {
        int last = binary.Length - 1;
        bool negative = (binary[last] & 0x80000000) != 0;

        if (!negative)
        {
            Normalize(ref binary);
            return (binary, false);
        }
        else
        {
            uint[] mag = new uint[binary.Length];
            for (int i = 0; i < binary.Length; i++)
                mag[i] = ~binary[i];

            ulong carry = 1;
            for (int i = 0; i < mag.Length && carry != 0; i++)
            {
                ulong sum = mag[i] + carry;
                mag[i] = (uint)sum;
                carry = sum >> 32;
            }

            Normalize(ref mag);
            return (mag, true);
        }
    }

    public static void Normalize(ref uint[] magnitude)
    {
        int lastNotZeroIndex = magnitude.Length - 1;
        while (lastNotZeroIndex > 0 && magnitude[lastNotZeroIndex] == 0)
        {
            lastNotZeroIndex--;
        }

        if (lastNotZeroIndex != magnitude.Length - 1)
        {
            Array.Resize(ref magnitude, lastNotZeroIndex + 1);
        }
    }

    public static BetterBigInteger operator ~(BetterBigInteger a)
    {
        var aDigits = a.GetDigits();
        var aTwosComplement = ToTwosComplement(aDigits, a.IsNegative, aDigits.Length + 1);

        for (int i = 0; i < aDigits.Length + 1; i++)
        {
            aTwosComplement[i] = ~aTwosComplement[i];
        }

        (uint[] result, bool sign) = FromTwosComplement(aTwosComplement);
        return new BetterBigInteger(result, sign);
    }

    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b)
    {
        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();
        int maxLength = Math.Max(aDigits.Length, bDigits.Length) + 1;

        var aTwosComplement = ToTwosComplement(aDigits, a.IsNegative, maxLength);
        var bTwosComplement = ToTwosComplement(bDigits, b.IsNegative, maxLength);

        uint[] result = new uint[maxLength];
        for (int i = 0; i < maxLength; i++)
        {
            result[i] = aTwosComplement[i] & bTwosComplement[i];
        }

        (var mag, bool sign) = FromTwosComplement(result);
        Normalize(ref mag);
        return new BetterBigInteger(mag, sign);
    }

    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b)
    {
        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();
        int maxLength = Math.Max(aDigits.Length, bDigits.Length) + 1;

        var aTwosComplement = ToTwosComplement(aDigits, a.IsNegative, maxLength);
        var bTwosComplement = ToTwosComplement(bDigits, b.IsNegative, maxLength);

        uint[] result = new uint[maxLength];
        for (int i = 0; i < maxLength; i++)
        {
            result[i] = aTwosComplement[i] | bTwosComplement[i];
        }

        (var mag, bool sign) = FromTwosComplement(result);
        Normalize(ref mag);
        return new BetterBigInteger(mag, sign);
    }

    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b)
    {
        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();
        int maxLength = Math.Max(aDigits.Length, bDigits.Length) + 1;

        var aTwosComplement = ToTwosComplement(aDigits, a.IsNegative, maxLength);
        var bTwosComplement = ToTwosComplement(bDigits, b.IsNegative, maxLength);

        uint[] result = new uint[maxLength];
        for (int i = 0; i < maxLength; i++)
        {
            result[i] = aTwosComplement[i] ^ bTwosComplement[i];
        }

        (var mag, bool sign) = FromTwosComplement(result);
        Normalize(ref mag);
        return new BetterBigInteger(mag, sign);
    }

    public static BetterBigInteger operator <<(BetterBigInteger a, int shift)
    {
        if (a is null)
        {
            throw new ArgumentNullException(nameof(a));
        }

        if (shift < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(shift), "shift must be >1");
        }

        if (shift == 0)
        {
            return a;
        }

        var aDigits = a.GetDigits();
        int wordShift = shift / 32;
        int bitShift = shift % 32;
        int newLength = aDigits.Length + wordShift + 1;
        uint[] result = new uint[newLength];

        for (int i = 0; i < aDigits.Length; i++)
        {
            int pos = i + wordShift;
            ulong val = (ulong)aDigits[i] << bitShift;
            result[pos] |= (uint)val;
            if (pos + 1 < newLength)
                result[pos + 1] |= (uint)(val >> 32);
        }

        Normalize(ref result);
        var aResult = new BetterBigInteger(result, a.IsNegative);
        return aResult;
    }

    public static BetterBigInteger operator >>(BetterBigInteger a, int shift)
    {
        if (a is null)
        {
            throw new ArgumentNullException(nameof(a));
        }

        if (shift < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(shift), "shift must be >= 0");
        }

        if (shift == 0)
        {
            return a;
        }

        if (!a.IsNegative)
        {
            var aDigits = a.GetDigits().ToArray();
            int wordShift = shift / 32;
            int bitShift = shift % 32;

            if (wordShift >= aDigits.Length)
            {
                return new BetterBigInteger(new uint[] { 0 }, false);
            }

            int newLength = aDigits.Length - wordShift;
            uint[] result = new uint[newLength];

            for (int i = 0; i < newLength; i++)
            {
                int pos = i + wordShift;
                ulong val = (ulong)aDigits[pos];
                if (bitShift > 0 && pos + 1 < aDigits.Length)
                    val |= (ulong)aDigits[pos + 1] << 32;
                result[i] = (uint)(val >> bitShift);
            }

            Normalize(ref result);
            return new BetterBigInteger(result, false);
        }
        else
        {
            var magnA = new BetterBigInteger(a.GetDigits().ToArray(), false);
            var one = new BetterBigInteger(new uint[] { 1 }, false);
            var pow = one << shift;
            var mask = pow - one;

            if (magnA <= mask)
            {
                return new BetterBigInteger(new uint[] { 1 }, true); // Return -1
            }

            var numerator = magnA + mask;
            var shifted = numerator >> shift;
            return -shifted;
        }
    }

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
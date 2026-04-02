using Arithmetic.BigInt.Interfaces;
using Arithmetic.BigInt.MultiplyStrategy;

namespace Arithmetic.BigInt;

public sealed class BetterBigInteger : IBigInteger
{
    private int _signBit;
    
    private uint _smallValue; // Если число маленькое, храним его прямо в этом поле, а _data == null.
    private uint[]? _data;
    
    public bool IsNegative => _signBit == 1;
    
    /// От массива цифр (little endian)
    public BetterBigInteger(uint[] digits, bool isNegative = false)
    {
        if (digits == null)
        {
            throw new ArgumentNullException();
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

        var normalized =  new uint[NonZeroIndex + 1];
        Array.Copy(digits, normalized, normalized.Length);

        if (normalized.Length == 1) // если слишком маленбкое
        {
            _smallValue = normalized[0];
            _data = null;
            _signBit = isNegative ? 1 : 0;

            return;
        }
        else
        {
            _data = normalized;
            _smallValue = 0;
            _signBit = isNegative ? 1 : 0;
        }
    
    public BetterBigInteger(IEnumerable<uint> digits, bool isNegative = false)
    {
        throw new NotImplementedException();
    }
    
    public BetterBigInteger(string value, int radix)
    {
        throw new NotImplementedException();
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
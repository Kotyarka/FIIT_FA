using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class KaratsubaMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
{
    var ADigits = a.GetDigits();
    var BDigits = b.GetDigits();
    int lenMax = Math.Max(ADigits.Length, BDigits.Length);
    
    if (ADigits.Length <= 2 || BDigits.Length <= 2)
    {
        SimpleMultiplier mult = new();
        return mult.Multiply(a, b);
    }

    int half1 = lenMax / 2;

    uint[] aPadded = new uint[lenMax];
    uint[] bPadded = new uint[lenMax];
    ADigits.CopyTo(aPadded);
    BDigits.CopyTo(bPadded);

    BetterBigInteger a0 = new(aPadded[..half1].ToArray(), false);
    BetterBigInteger a1 = new(aPadded[half1..].ToArray(), false);
    BetterBigInteger b0 = new(bPadded[..half1].ToArray(), false);
    BetterBigInteger b1 = new(bPadded[half1..].ToArray(), false);

    BetterBigInteger D1 = Multiply(a0, b0);
    BetterBigInteger D3 = Multiply(a1, b1);
    BetterBigInteger D2 = Multiply(a0 + a1, b0 + b1);
    BetterBigInteger D4 = D2 - D1 - D3;
    
    var x = new BetterBigInteger(Enumerable.Repeat(0u, half1).Concat(new uint[] { 1 }).ToArray(), false);
    var x2 = new BetterBigInteger(Enumerable.Repeat(0u, half1 * 2).Concat(new uint[] { 1 }).ToArray(), false);

    BetterBigInteger result = D1 + D4 * x + D3 * x2;
    
    if (a.IsNegative ^ b.IsNegative)
        result = -result;
    return result;
}
}

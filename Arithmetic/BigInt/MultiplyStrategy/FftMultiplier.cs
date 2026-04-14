using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class FftMultiplier : IMultiplier
{
    private static void FFT(Complex[] number, bool IsInvert)
    { // FFT based on Cooley-Tukey algorithm
        int N = number.Length;
        if (N == 1)
        {
            return;
        }   

        Complex[] even  = new Complex[N / 2];
        Complex[] odd = new Complex[N / 2];

        for (int i = 0; i < N / 2; i++)
        {
            even[i] = number[2 * i];
            odd[i] = number[2 * i + 1];
        }

        FFT(even, IsInvert);
        FFT(odd, IsInvert);

        double angle = 2 * Math.PI / N * (IsInvert ? -1 : 1);
        Complex w = new Complex(1, 0);
        Complex pow = new Complex(Math.Cos(angle), Math.Sin(angle));

        for (int i = 0; i < N / 2; i++)
        {
            number[i] = even[i] + w * odd[i];
            number[i + N/2] = even[i] - w * odd[i];
            w = w * pow;
        }

        if (IsInvert)
        {
            for (int i = 0; i < N; i++)
            {
                number[i] /= 2;
            }
        }
    }
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        bool negative = (a.IsNegative) ^ (b.IsNegative);
        var aDigits = a.GetDigits().ToArray();
        var bDigits = b.GetDigits().ToArray();

        int n = 1;
        int sumLen = aDigits.Length + bDigits.Length;
        while (n < sumLen) n <<= 1; // we need power of two in vector to make good divides by 2


        var aComplex = new Complex[n];
        var bComplex = new Complex[n];

        for (int i = 0; i < aDigits.Length; i++)
            aComplex[i] = new Complex(aDigits[i], 0);
        for (int i = 0; i < bDigits.Length; i++)
            bComplex[i] = new Complex(bDigits[i], 0);

        FFT(aComplex, false);
        FFT(bComplex, false);

        for (int i = 0; i < n; i++)
            aComplex[i] *= bComplex[i];

        FFT(aComplex, true);

        uint[] resultDigits = new uint[n + 64];
        ulong carry = 0;
        int idx = 0;

        for (int i = 0; i < n; i++)
        {
            ulong val = (ulong)Math.Round(aComplex[i].Real);
            carry += val;
            resultDigits[idx++] = (uint)(carry & 0xFFFFFFFFUL);
            carry >>= 32;
        }

        while (carry > 0)
        {
            resultDigits[idx++] = (uint)(carry & 0xFFFFFFFFUL);
            carry >>= 32;
        }

        uint[] finalDigits = new uint[idx];
        Array.Copy(resultDigits, 0, finalDigits, 0, idx);

        if (finalDigits.Length == 0)
            finalDigits = new uint[] { 0 };
        
        return new BetterBigInteger(finalDigits, negative);
    }
}
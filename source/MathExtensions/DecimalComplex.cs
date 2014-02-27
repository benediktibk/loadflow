using System;
using System.Numerics;

namespace MathExtensions
{
    public struct DecimalComplex : IEquatable<DecimalComplex>, IFormattable
    {
        public decimal Real { get; set; }
        public decimal Imaginary { get; set; }

        public DecimalComplex(decimal real, decimal imaginary) : this()
        {
            Real = real;
            Imaginary = imaginary;
        }

        public DecimalComplex(Complex value) : this()
        {
            Real = (decimal) value.Real;
            Imaginary = (decimal) value.Imaginary;
        }

        public bool Equals(DecimalComplex other)
        {
            return Real.Equals(other.Real) && Imaginary.Equals(other.Imaginary);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return "(" + Real.ToString(format, formatProvider) + ", j" + Imaginary.ToString(format, formatProvider) + ")";
        }
    }
}

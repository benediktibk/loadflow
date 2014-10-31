using System;

namespace Misc
{
    public class Angle
    {
        #region variables

        private readonly double _value;

        #endregion

        #region constructor

        public Angle(double radiant)
        {
            if (radiant > 0)
            {
                var factor = (int)(radiant / (2 * Math.PI));
                _value = radiant - factor*2*Math.PI;
            }
            else
            {
                var factor = (int)((-1) * radiant / (2 * Math.PI)) + 1;
                _value = radiant + factor*2*Math.PI;
            }
        }

        public Angle()
        {
            _value = 0;
        }

        public static Angle FromDegree(double degree)
        {
            return new Angle(degree*Math.PI/180);
        }

        #endregion

        #region properties

        public double Radiant
        {
            get { return _value; }
        }

        public double Degree
        {
            get { return _value*180/Math.PI; }
        }

        public double DegreeAroundZero
        {
            get
            {
                var degree = Degree;
                return degree < 180 ? degree : degree - 360;
            }
        }

        #endregion

        #region operators

        public static Angle operator +(Angle lhs, Angle rhs)
        {
            return new Angle(lhs.Radiant + rhs.Radiant);
        }

        public static Angle operator -(Angle lhs, Angle rhs)
        {
            return new Angle(lhs.Radiant - rhs.Radiant);
        }

        public static Angle operator *(Angle angle, double factor)
        {
            return new Angle(angle.Radiant * factor);
        }

        public static Angle operator *(double factor, Angle angle)
        {
            return new Angle(angle.Radiant * factor);
        }

        public static bool Equal(Angle one, Angle two, double delta)
        {
            var difference = one.Radiant - two.Radiant;
            var absoluteDifference = Math.Abs(difference);

            if (absoluteDifference > Math.PI)
                absoluteDifference -= 2*Math.PI;

            return Math.Abs(absoluteDifference) < delta;
        }

        #endregion
    }
}

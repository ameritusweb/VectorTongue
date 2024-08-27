using System;

public class IntersectionTimesCalculator
{
    public double CalculateFirstIntersectionTime(
        double t_offset, double C, double D, double F,
        double P0, double s1, double A, double E,
        double G, double Q2, double s2,
        double theta_10, double theta_20, double B)
    {
        // Condition 1: Eq(12*P0*s1*t_offset**4/(A) - E, 1/3)
        double leftSide = 12 * P0 * s1 * Math.Pow(t_offset, 4) / A - E;
        if (Math.Abs(leftSide - 1.0 / 3.0) < 1e-9)  // checking if leftSide is approximately 1/3
        {
            double sqrtTerm1 = Math.Pow(4.0 / 3.0 - 2 * C - D, 1.0 / 3.0);
            double sqrtTerm2 = Math.Pow(2 * C - D, 1.0 / 3.0);

            double result = (-t_offset - sqrtTerm1) / 2.0 - sqrtTerm2 + 8.0 / 3.0 + F / sqrtTerm1;
            return result / 2.0;
        }
        else
        {
            double innerTerm1 = (12 * P0 * s1 * Math.Pow(t_offset, 4) / A - E - 1.0 / 3.0);
            double innerTerm2 = (3 * G - 4 * Q2 * s2 * Math.Pow(t_offset, 4) / A + 4 * Q2 * s2 * Math.Pow(t_offset, 2) / A
                + Math.Pow(t_offset, 4) - 2 * Math.Pow(t_offset, 2) / 3
                + 8 * theta_10 / (3 * A) - 8 * theta_20 / (3 * A)
                + Math.Pow(innerTerm1, 3) / 27.0 + C - D);

            double sqrtTerm1 = Math.Pow(-2 * innerTerm1 / innerTerm2 + Math.Pow(B, 2) / 16.0 - 1.0 / 27.0, 1.0 / 3.0);
            double sqrtTerm2 = Math.Pow(2 * innerTerm1 / innerTerm2 + Math.Pow(B, 2) / 16.0 - 1.0 / 27.0, 1.0 / 3.0);

            double result = (-t_offset - sqrtTerm1 + 2 * G - 4 * Q2 * s2 * Math.Pow(t_offset, 4) / A + 4 * Q2 * s2 * Math.Pow(t_offset, 2) / A
                + Math.Pow(t_offset, 4) - 2 * Math.Pow(t_offset, 2) / 3
                + 8 * theta_10 / (3 * A) - 8 * theta_20 / (3 * A) + sqrtTerm2 + 8.0 / 3.0 + F / sqrtTerm1) / 2.0;

            return result / 2.0;
        }
    }

    public double CalculateSecondIntersectionTime(
        double t_offset, double C, double D, double F,
        double P0, double s1, double A, double E,
        double G, double Q2, double s2,
        double theta_10, double theta_20, double B)
    {
        // Condition 1: Eq(12*P0*s1*t_offset**4/(A) - E, 1/3)
        double leftSide = 12 * P0 * s1 * Math.Pow(t_offset, 4) / A - E;
        if (Math.Abs(leftSide - 1.0 / 3.0) < 1e-9)  // checking if leftSide is approximately 1/3
        {
            double sqrtTerm1 = Math.Pow(4.0 / 3.0 - 2 * C - D, 1.0 / 3.0);
            double sqrtTerm2 = Math.Pow(2 * C - D, 1.0 / 3.0);

            double result = (-t_offset - sqrtTerm1) / 2.0 + sqrtTerm2 + 8.0 / 3.0 + F / sqrtTerm1;
            return result / 2.0;
        }
        else
        {
            double innerTerm1 = (12 * P0 * s1 * Math.Pow(t_offset, 4) / A - E - 1.0 / 3.0);
            double innerTerm2 = (3 * G - 4 * Q2 * s2 * Math.Pow(t_offset, 4) / A + 4 * Q2 * s2 * Math.Pow(t_offset, 2) / A
                + Math.Pow(t_offset, 4) - 2 * Math.Pow(t_offset, 2) / 3
                + 8 * theta_10 / (3 * A) - 8 * theta_20 / (3 * A)
                + Math.Pow(innerTerm1, 3) / 27.0 + C - D);

            double sqrtTerm1 = Math.Pow(-2 * innerTerm1 / innerTerm2 + Math.Pow(B, 2) / 16.0 - 1.0 / 27.0, 1.0 / 3.0);
            double sqrtTerm2 = Math.Pow(2 * innerTerm1 / innerTerm2 + Math.Pow(B, 2) / 16.0 - 1.0 / 27.0, 1.0 / 3.0);

            double result = (-t_offset - sqrtTerm1 + 2 * G - 4 * Q2 * s2 * Math.Pow(t_offset, 4) / A + 4 * Q2 * s2 * Math.Pow(t_offset, 2) / A
                + Math.Pow(t_offset, 4) - 2 * Math.Pow(t_offset, 2) / 3
                + 8 * theta_10 / (3 * A) - 8 * theta_20 / (3 * A) + sqrtTerm2 + 8.0 / 3.0 + F / sqrtTerm1) / 2.0;

            return result / 2.0;
        }
    }
}

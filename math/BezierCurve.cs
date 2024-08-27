using System;

public class BezierCurve
{
    // Function to compute the value of the Bezier curve y(t)
    public static double Bezier(double t, double s0, double P0, double P1, double P2, double P3)
    {
        double u = 1 - t;
        return s0 * (u * u * u * P0 + 3 * u * u * t * P1 + 3 * u * t * t * P2 + t * t * t * P3);
    }

    // Function to compute the first derivative of the Bezier curve with respect to t
    public static double BezierDerivative(double t, double s0, double P0, double P1, double P2, double P3)
    {
        double u = 1 - t;
        return s0 * (-3 * u * u * P0 + 3 * (u * u - 2 * u * t) * P1 + 3 * (2 * u * t - t * t) * P2 + 3 * t * t * P3);
    }

    // Function to compute the second derivative of the Bezier curve with respect to t
    public static double BezierSecondDerivative(double t, double s0, double P0, double P1, double P2, double P3)
    {
        double u = 1 - t;
        return s0 * (6 * u * P0 - 6 * (2 * u - t) * P1 + 6 * (1 - 2 * t) * P2 + 6 * t * P3);
    }

    public static void Main()
    {
        // Define the control points and initial spin rate
        double s0 = 1.0;
        double P0 = 1.0, P1 = 1.5, P2 = 2.0, P3 = 1.0;

        // Compute y(0)
        double y0 = Bezier(0, s0, P0, P1, P2, P3);

        // Compute y'(0)
        double y0_prime = BezierDerivative(0, s0, P0, P1, P2, P3);

        // Compute y''(0)
        double y0_double_prime = BezierSecondDerivative(0, s0, P0, P1, P2, P3);

        // First term: 1 / y(0)
        double term1 = 1.0 / y0;

        // Second term: -y'(0) / y(0)^2 * t integrated from 0 to 1
        double term2 = -y0_prime / (y0 * y0) * 0.5;

        // Third term: ((2*y'(0)^2 - y(0)*y''(0)) / 2*y(0)^3) * (t^2/2) integrated from 0 to 1
        double term3 = ((2 * y0_prime * y0_prime) - (y0 * y0_double_prime)) / (2 * y0 * y0 * y0) * (1.0 / 3.0);

        // Fourth term: 1 / 6*y(0)^4 * (t^4/4) integrated from 0 to 1
        double term4 = 1.0 / (6 * y0 * y0 * y0 * y0) * (1.0 / 4.0);

        // Sum the terms to approximate the total time
        double approximateTotalTime = term1 + term2 + term3 + term4;

        // Print the approximate total time
        Console.WriteLine("Approximate Total Time to Traverse the Curve: " + approximateTotalTime);
    }
}

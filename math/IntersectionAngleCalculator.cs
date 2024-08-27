using System;

public class BWVIntersectionCalculator
{
    // Method to calculate the angle at a given intersection time t
    public static double CalculateAngle1(double t, double P0, double P2, double s1, double theta_10)
    {
        double term1 = P0 * s1 * t;
        double term2 = Math.Pow(t, 4) * (3 * P0 * s1 / 8.0 - 3 * P2 * s1 / 8.0);
        double term3 = Math.Pow(t, 2) * (-3 * P0 * s1 / 4.0 + 3 * P2 * s1 / 4.0);
        return term1 + term2 + term3 + theta_10;
    }

    public static double CalculateAngle2(double t, double Q0, double Q2, double s2, double theta_20)
    {
        double term1 = Q0 * s2 * t;
        double term2 = Math.Pow(t, 4) * (3 * Q0 * s2 / 8.0 - 3 * Q2 * s2 / 8.0);
        double term3 = Math.Pow(t, 2) * (-3 * Q0 * s2 / 4.0 + 3 * Q2 * s2 / 4.0);
        return term1 + term2 + term3 + theta_20;
    }
}

class Program
{
    static void Main()
    {
        // Example values for P0, P2, s1, theta_10 for BWV1
        double P0_val = 1.0;
        double P2_val = 2.0;
        double s1_val = 5.0;
        double theta_10_val = 0.0;

        // Example values for Q0, Q2, s2, theta_20 for BWV2
        double Q0_val = 1.5;
        double Q2_val = 2.5;
        double s2_val = 0.8;
        double theta_20_val = Math.PI / 4;

        // Example intersection time
        double t_intersection = 1.2;

        // Calculate the angles at the intersection time
        double angle1 = BWVIntersectionCalculator.CalculateAngle1(t_intersection, P0_val, P2_val, s1_val, theta_10_val);
        double angle2 = BWVIntersectionCalculator.CalculateAngle2(t_intersection, Q0_val, Q2_val, s2_val, theta_20_val);

        Console.WriteLine($"Intersection angle for BWV1: {angle1}");
        Console.WriteLine($"Intersection angle for BWV2: {angle2}");
    }
}

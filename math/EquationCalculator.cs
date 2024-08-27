using System;

public class EquationCalculator
{
    public double CalculateA(double P0, double P2, double Q0, double Q2, double s1, double s2)
    {
        return 3 * P0 * s1 - 3 * P2 * s1 - 3 * Q0 * s2 + 3 * Q2 * s2;
    }

    public double CalculateB(double t_offset, double A, double P0, double P2, double Q0, double Q2, double s1, double s2)
    {
        return 12 * P0 * s1 * Math.Pow(t_offset, 3) / A - 12 * P0 * s1 * t_offset / A + 8 * P0 * s1 / A 
             - 12 * P2 * s1 * Math.Pow(t_offset, 3) / A + 12 * P2 * s1 * t_offset / A 
             - 12 * Q0 * s2 * Math.Pow(t_offset, 3) / A + 12 * Q0 * s2 * t_offset / A - 8 * Q0 * s2 / A 
             + 12 * Q2 * s2 * Math.Pow(t_offset, 3) / A - 12 * Q2 * s2 * t_offset / A 
             - 4 * Math.Pow(t_offset, 3) + 4 * t_offset;
    }

    public double CalculateC(double t_offset, double A, double P0, double s1)
    {
        return 8 * P0 * s1 * Math.Pow(t_offset, 4) / A - 8 * P0 * s1 * Math.Pow(t_offset, 2) / A 
             + 16 * P0 * s1 * t_offset / (3 * A);
    }

    public double CalculateD(double t_offset, double A, double P2, double Q0, double Q2, double s1, double s2, double theta_10, double theta_20, double B)
    {
        return 8 * P2 * s1 * Math.Pow(t_offset, 4) / A + 8 * P2 * s1 * Math.Pow(t_offset, 2) / A 
             - 8 * Q0 * s2 * Math.Pow(t_offset, 4) / A + 8 * Q0 * s2 * Math.Pow(t_offset, 2) / A 
             - 16 * Q0 * s2 * t_offset / (3 * A) + 8 * Q2 * s2 * Math.Pow(t_offset, 4) / A 
             - 8 * Q2 * s2 * Math.Pow(t_offset, 2) / A - 2 * Math.Pow(t_offset, 4) 
             + 4 * Math.Pow(t_offset, 2) / 3 - 16 * theta_10 / (3 * A) 
             + 16 * theta_20 / (3 * A) - Math.Pow(B, 2) / 8 + 2.0 / 27.0;
    }

    public double CalculateE(double t_offset, double A, double P0, double P2, double Q0, double Q2, double s1, double s2, double theta_10, double theta_20)
    {
        return 12 * P0 * s1 * Math.Pow(t_offset, 2) / A + 8 * P0 * s1 * t_offset / A 
             - 12 * P2 * s1 * Math.Pow(t_offset, 4) / A + 12 * P2 * s1 * Math.Pow(t_offset, 2) / A 
             - 12 * Q0 * s2 * Math.Pow(t_offset, 4) / A + 12 * Q0 * s2 * Math.Pow(t_offset, 2) / A 
             - 8 * Q0 * s2 * t_offset / A + 12 * Q2 * s2 * Math.Pow(t_offset, 4) / A 
             - 12 * Q2 * s2 * Math.Pow(t_offset, 2) / A - 3 * Math.Pow(t_offset, 4) 
             + 2 * Math.Pow(t_offset, 2) - 8 * theta_10 / A + 8 * theta_20 / A;
    }

    public double CalculateF(double t_offset, double A, double P0, double P2, double Q0, double Q2, double s1, double s2)
    {
        return 24 * P0 * s1 * Math.Pow(t_offset, 3) / A - 24 * P0 * s1 * t_offset / A 
             + 16 * P0 * s1 / A - 24 * P2 * s1 * Math.Pow(t_offset, 3) / A 
             + 24 * P2 * s1 * t_offset / A - 24 * Q0 * s2 * Math.Pow(t_offset, 3) / A 
             + 24 * Q0 * s2 * t_offset / A - 16 * Q0 * s2 / A 
             + 24 * Q2 * s2 * Math.Pow(t_offset, 3) / A - 24 * Q2 * s2 * t_offset / A 
             - 8 * Math.Pow(t_offset, 3) + 8 * t_offset;
    }

    public double CalculateG(double t_offset, double A, double P0, double P2, double Q0, double s1, double s2)
    {
        return -4 * P0 * s1 * Math.Pow(t_offset, 4) / A + 4 * P0 * s1 * Math.Pow(t_offset, 2) / A 
             - 8 * P0 * s1 * t_offset / (3 * A) + 4 * P2 * s1 * Math.Pow(t_offset, 4) / A 
             - 4 * P2 * s1 * Math.Pow(t_offset, 2) / A + 4 * Q0 * s2 * Math.Pow(t_offset, 4) / A 
             - 4 * Q0 * s2 * Math.Pow(t_offset, 2) / A + 8 * Q0 * s2 * t_offset / (3 * A);
    }
}

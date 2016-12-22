using UnityEngine;
using System;
using System.Collections;

public class Term1D
{
    private double m_scalar = 0;
    private double m_power = 0;

    public Term1D()
    {

    }

    public Term1D(double power, double scalar)
    {
        m_power = power;
        m_scalar = scalar;
    }

    public double Calculate(double x)
    {
        return m_scalar * Math.Pow(x, m_power);
    }

    #region Operators
    public static Term1D operator *(Term1D a, Term1D b)
    {
        return new Term1D(a.m_power + b.m_power, a.m_scalar * b.m_scalar);
    }

    public static Term1D operator *(double scalar, Term1D term)
    {
        return new Term1D(term.power, term.scalar * scalar);
    }

    public static Term1D operator *(Term1D term, double scalar)
    {
        return new Term1D(term.power, term.scalar * scalar);
    }

    public static Term1D operator +(Term1D term, double scalar)
    {
        return new Term1D(term.power, term.scalar + scalar);
    }

    #endregion

    #region Accesseurs

    public double power
    {
        get { return m_power; }
    }

    public double scalar
    {
        get { return m_scalar; }
    }

    #endregion
}

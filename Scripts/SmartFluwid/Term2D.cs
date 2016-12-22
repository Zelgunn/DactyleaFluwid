using UnityEngine;
using System;

public class Term2D
{
    private double m_scalar = 0;
    private double m_xPower = 0, m_yPower = 0;

    public Term2D()
    {

    }

    public Term2D(double xPower, double yPower, double scalar)
    {
        m_xPower = xPower;
        m_yPower = yPower;
        m_scalar = scalar;
    }

    public double Calculate(double x, double y)
    {
        return m_scalar * Math.Pow(x, m_xPower) * Math.Pow(y, m_yPower);
    }

    static public Term2D MultiplyXYTerms(Term1D xPolygon, Term1D yPolygon)
    {
        Term2D result = new Term2D();

        result.m_xPower = xPolygon.power;
        result.m_yPower = yPolygon.power;
        result.m_scalar = xPolygon.scalar * yPolygon.scalar;

        return result;
    }

    #region Operators
    public static Term2D operator *(Term2D a, Term2D b)
    {
        Term2D result = new Term2D();

        result.m_scalar = a.m_scalar * b.m_scalar;
        result.m_xPower = a.m_xPower + b.m_xPower;
        result.m_yPower = a.m_yPower + b.m_yPower;

        return result;
    }

    public static Term2D operator *(double scalar, Term2D term)
    {
        term.m_scalar *= scalar;
        return term;
    }

    public static Term2D operator *(Term2D term, double scalar)
    {
        term.m_scalar *= scalar;
        return term;
    }

    public static Term2D operator +(Term2D term, double scalar)
    {
        term.m_scalar += scalar;
        return term;
    }
    #endregion

    #region Accesseurs

    public double xPower
    {
        get { return m_xPower; }
    }

    public double yPower
    {
        get { return m_yPower; }
    }

    public double scalar
    {
        get { return m_scalar; }
    }

    #endregion
}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Polynome2D
{
    private double m_k = 0;
    private List<Term1D> m_xTerms = new List<Term1D>();
    private List<Term1D> m_yTerms = new List<Term1D>();
    private List<Term2D> m_xyTerms = new List<Term2D>();

    public Polynome2D()
    {
        m_k = 0;
    }

    public Polynome2D(double k)
    {
        m_k = k;
    }

    public double Calculate(double x, double y)
    {
        double z = m_k;

        foreach (Term1D xTerm in m_xTerms)
        {
            z += xTerm.Calculate(x);
        }

        foreach (Term1D yTerm in m_yTerms)
        {
            z += yTerm.Calculate(y);
        }

        foreach (Term2D xyTerm in m_xyTerms)
        {
            z += xyTerm.Calculate(x, y);
        }

        return z;
    }

    public void AddXTerm(Term1D term)
    {
        for(int i = 0; i < m_xTerms.Count ; i++)
        {
            Term1D xTerm = m_xTerms[i];
            if(xTerm.power == term.power)
            {
                xTerm += term.scalar;
                m_xTerms[i] = xTerm;
                return;
            }
        }

        m_xTerms.Add(term);
    }

    public void AddYTerm(Term1D term)
    {
        for (int i = 0; i < m_yTerms.Count; i++)
        {
            Term1D yTerm = m_yTerms[i];
            if (yTerm.power == term.power)
            {
                yTerm += term.scalar;
                m_yTerms[i] = yTerm;
                return;
            }
        }

        m_yTerms.Add(term);
    }

    public void AddXYTerm(Term2D term)
    {
        for (int i = 0; i < m_xyTerms.Count; i++)
        {
            Term2D xyTerm = m_xyTerms[i];
            if ((xyTerm.xPower == term.xPower) && (xyTerm.yPower == term.yPower))
            {
                xyTerm += term.scalar;
                m_xyTerms[i] = xyTerm;
                return;
            }
        }

        m_xyTerms.Add(term);
    }

    static public Polynome2D MultiplyXYPolygones(Polynome1D xPolygon, Polynome1D yPolygon)
    {
        Polynome2D result = new Polynome2D();

        for (int x = 0; x < xPolygon.termCount; x++ )
        {
            Term1D xTerm = xPolygon.terms[x];

            for (int y = 0; y < yPolygon.termCount; y++)
            {
                Term1D yTerm = yPolygon.terms[y];

                Term2D xyTerm = Term2D.MultiplyXYTerms(xTerm, yTerm);

                result += xyTerm;
            }

            result.AddXTerm(xTerm * yPolygon.constant);
        }

        for (int y = 0; y < yPolygon.termCount; y++)
        {
            Term1D yTerm = yPolygon.terms[y];

            result.AddYTerm(yTerm * xPolygon.constant);
        }

        result.m_k = xPolygon.constant * yPolygon.constant;

        return result;
    }

    static public Polynome2D operator*(Polynome2D a, double scalar)
    {
        Polynome2D result = new Polynome2D();
        result.m_k = scalar * a.m_k;

        foreach (Term1D xTerm in a.m_xTerms)
        {
            result.m_xTerms.Add(xTerm * scalar);
        }

        foreach (Term1D yTerm in a.m_yTerms)
        {
            result.m_yTerms.Add(yTerm * scalar);
        }

        foreach (Term2D xyTerm in a.m_xyTerms)
        {
            result.m_xyTerms.Add(xyTerm * scalar);
        }

        return result;
    }

    static public Polynome2D operator *(double scalar, Polynome2D a)
    {
        return a * scalar;
    }

    static public Polynome2D operator +(Polynome2D polynome, Term2D term)
    {
        for (int i = 0; i < polynome.m_xyTerms.Count; i++)
        {
            Term2D xyTerm = polynome.m_xyTerms[i];
            if ((xyTerm.xPower == term.xPower) && (xyTerm.yPower == term.yPower))
            {
                xyTerm += term.scalar;
                polynome.m_xyTerms[i] = xyTerm;
                return polynome;
            }
        }

        polynome.m_xyTerms.Add(term);
        return polynome;
    }

    static public Polynome2D operator +(Polynome2D a, Polynome2D b)
    {
        Polynome2D result = new Polynome2D();

        foreach (Term2D xyTerm in a.m_xyTerms) result += xyTerm;
        foreach (Term1D xTerm in a.m_xTerms) result.AddXTerm(xTerm);
        foreach (Term1D yTerm in a.m_yTerms) result.AddYTerm(yTerm);

        foreach (Term2D xyTerm in b.m_xyTerms) result += xyTerm;
        foreach (Term1D xTerm in b.m_xTerms) result.AddXTerm(xTerm);
        foreach (Term1D yTerm in b.m_yTerms) result.AddYTerm(yTerm);

        result.m_k = a.m_k + b.m_k;

        return result;
    }

    override public string ToString()
    {
        string res = "";

        foreach (Term2D term in m_xyTerms)
        {
            if (term.scalar == 0)
            {
                continue;
            }

            if (term.xPower == 1)
            {
                res += term.scalar + "(x * ";
            }
            else if (term.xPower == 2)
            {
                res += term.scalar + "(x² * ";
            }
            else
            {
                res += term.scalar + "(x^ * " + term.xPower;
            }

            if (term.yPower == 1)
            {
                res += "y) + ";
            }
            else if (term.yPower == 2)
            {
                res += "y²) + ";
            }
            else
            {
                res += "y^" + term.yPower + ") + ";
            }
        }

        foreach (Term1D term in m_xTerms)
        {
            if (term.scalar == 0)
            {
                continue;
            }

            if (term.power == 1)
            {
                res += term.scalar + "x + ";
            }
            else if (term.power == 2)
            {
                res += term.scalar + "x² + ";
            }
            else
            {
                res += term.scalar + "x^" + term.power + " + ";
            }
        }

        foreach (Term1D term in m_yTerms)
        {
            if (term.scalar == 0)
            {
                continue;
            }

            if (term.power == 1)
            {
                res += term.scalar + "y + ";
            }
            else if (term.power == 2)
            {
                res += term.scalar + "y² + ";
            }
            else
            {
                res += term.scalar + "y^" + term.power + " + ";
            }
        }

        res += m_k;

        return res;
    }

    public double k
    {
        get { return m_k; }
    }

    public List<Term1D> xTerms
    {
        get { return m_xTerms; }
    }

    public List<Term1D> yTerms
    {
        get { return m_yTerms; }
    }

    public List<Term2D> xyTerms
    {
        get { return m_xyTerms; }
    }
}

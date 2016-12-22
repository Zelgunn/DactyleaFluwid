using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Polynome1D
{
    private double m_k = 0;
    private List<Term1D> m_xTerms = new List<Term1D>();

    public Polynome1D(double k = 0)
    {
        m_k = k;
    }

    public Polynome1D(List<Term1D> xTerms, double k)
    {
        m_k = k;
        m_xTerms = new List<Term1D>(xTerms);
    }

    public void Add(Term1D term)
    {
        for (int i = 0; i < m_xTerms.Count; i++)
        {
            Term1D xTerm = m_xTerms[i];
            if (xTerm.power == term.power)
            {
                xTerm += term.scalar;
                m_xTerms[i] = xTerm;
                return;
            }
        }

        m_xTerms.Add(term);
    }

    public double Calcultate(double x)
    {
        double z = m_k;

        foreach (Term1D xTerm in m_xTerms)
        {
            z += xTerm.Calculate(x);
        }
        return z;
    }

    #region Operators
    public static Polynome1D operator +(Polynome1D a, Polynome1D b)
    {
        Debug.Log("NOT IMPLEMENTED : Polynome1D.Optimize()");
        Polynome1D result = new Polynome1D();

        result.m_xTerms = new List<Term1D>(a.m_xTerms);
        result.m_k = a.m_k + b.m_k;
        foreach (Term1D xTerm in b.m_xTerms)
        {
            result.Add(xTerm);
        }

        return result;
    }

    public static Polynome1D operator *(Polynome1D a, Polynome1D b)
    {
        Polynome1D result = new Polynome1D();

        foreach (Term1D aTerm in a.m_xTerms)
        {
            foreach (Term1D bTerm in b.m_xTerms)
            {
                result.Add(aTerm * bTerm);
            }

            result.Add(aTerm * b.m_k);
        }

        foreach (Term1D bTerm in b.m_xTerms)
        {
            result.Add(a.m_k * bTerm);
        }

        result.m_k = a.m_k * b.m_k;

        return result;
    }

    override public string ToString()
    {
        return ToString('x');
    }

    public string ToString(char symbol)
    {
        string res = "";

        foreach (Term1D term in m_xTerms)
        {
            if (term.scalar == 0)
            {
                continue;
            }

            if (term.power == 1)
            {
                res += term.scalar + symbol.ToString() + " + ";
            }
            else if (term.power == 2)
            {
                res += term.scalar + symbol.ToString() + "² + ";
            }
            else
            {
                res += term.scalar + symbol.ToString() + "^" + term.power + " + ";
            }
        }

        res += m_k;

        return res;
    }
    #endregion

    #region Accesseurs

    public double constant
    {
        get { return m_k; }
    }

    public int termCount
    {
        get { return m_xTerms.Count; }
    }

    public List<Term1D> terms
    {
        get { return m_xTerms; }
    }

    #endregion
}

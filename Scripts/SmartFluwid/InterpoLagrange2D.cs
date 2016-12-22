using UnityEngine;
using System;
using System.Collections.Generic;

public class InterpoLagrange2D
{
    private double[] m_xValues;
    private double[] m_yValues;
    private double[][] m_zValues;

    private Polynome2D m_polynome2D;

    public InterpoLagrange2D(double[] xValues, double[] yValues, double[][] zValues)
    {
        m_xValues = xValues;
        m_yValues = yValues;
        m_zValues = zValues;

        CalculateFunction();
    }

    /*
    public float Confidence()
    {
        return 1 - MeanError();
    }

    public float MeanError()
    {
        int xCount = m_xValues.Length;
        int yCount = m_yValues.Length;
        //double[] deltas = new double[xCount * yCount];
        double averageError = 0;

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 1; j < yCount; j++)
            {
                double originalValue = m_zValues[i][j];
                double interpolatedValue = m_polynome2D.Calculate(m_xValues[i], m_yValues[j]);

                double delta = originalValue - interpolatedValue;
                if (interpolatedValue == 0)
                {
                    if (originalValue != 0) averageError += Math.Abs(delta / originalValue);
                }
                else
                {
                    averageError += Math.Abs(delta / interpolatedValue);
                }


                //deltas[i * yCount + j] = delta;
            }
        }

        averageError /= xCount * yCount;
        return (float)averageError;
    }

    public float MaxError()
    {
        int xCount = m_xValues.Length;
        int yCount = m_yValues.Length;
        //double[] deltas = new double[xCount * yCount];
        double maxError = 0;

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 1; j < yCount; j++)
            {
                double originalValue = m_zValues[i][j];
                double interpolatedValue = m_polynome2D.Calculate(m_xValues[i], m_yValues[j]);

                double delta = originalValue - interpolatedValue;
                double error = 0;

                if (interpolatedValue == 0)
                {
                    if (originalValue != 0) error = Math.Abs(delta / originalValue);
                    Debug.Log("0");
                }
                else
                {
                    error = Math.Abs(delta / interpolatedValue);
                }

                maxError = Math.Max(error, error);
                //deltas[i * yCount + j] = delta;
            }
        }

        return (float)maxError;
    }
    */

    #region Private Functions
    private void CalculateFunction()
    {
        m_polynome2D = new Polynome2D();

        int xCount = m_xValues.Length;
        int yCount = m_yValues.Length;

        // Somme de i = 0 à n de...
        for (int i = 0; i < xCount; i++)
        {
            // Somme de j = 0 à m de...
            for (int j = 0; j < yCount; j++)
            {
                // f(xi,yj)
                double z = m_zValues[i][j];
                // * Li(x) * Lj(y)
                Polynome1D lix = LagrangePolynome(m_xValues, i);
                Polynome1D ljy = LagrangePolynome(m_yValues, j);

                m_polynome2D += z * Polynome2D.MultiplyXYPolygones(lix, ljy);
            }
        }
    }
    #endregion

    #region LagrangePolynome
    static public Polynome1D LagrangePolynome(double[] points, int i)
    {
        return LagrangePolynome(new List<double>(points), i);
    }

    static public Polynome1D LagrangePolynome(List<double> points, int i)
    {
        Polynome1D result = new Polynome1D(1);
        int n = points.Count;

        for (int j = 0; j < n; j++)
        {
            if (j == i) continue;

            double denom = points[i] - points[j];

            Term1D term = new Term1D (1 , 1 / denom);
            List<Term1D> terms = new List<Term1D>();
            terms.Add(term);
            Polynome1D polynome = new Polynome1D (terms, -points[j] / denom);

            result *= polynome;
        }

        return result;
    }
    #endregion

    #region Accesseurs
    public Polynome2D polynome2D
    {
        get { return m_polynome2D; }
    }
    #endregion
}

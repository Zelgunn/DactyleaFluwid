using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshVolumeFormula
{
    public enum MeshID
    {
        Erlenmeyer = 1,
        Becher = 2,
        TestTube = 4,
        Sink = 8,
        GraduatedPipette = 16,
        Cup,
        Undefined = 64
    }

    private Polynome2D m_formula;
    private MeshID m_id;
    private double m_relativeVolume;

    public MeshVolumeFormula(Polynome2D formula, MeshID id, double relativeVolume)
    {
        m_formula = new Polynome2D(formula.k);

        foreach (Term1D term in formula.xTerms) m_formula.AddXTerm(term);
        foreach (Term1D term in formula.yTerms) m_formula.AddYTerm(term);
        foreach (Term2D term in formula.xyTerms) m_formula.AddXYTerm(term);

        m_id = id;
        m_relativeVolume = relativeVolume;
    }

    public MeshVolumeFormula(List<Term1D> xTerms, List<Term1D> yTerms, List<Term2D> xyTerms, double k, MeshID id, double relativeVolume)
    {
        m_formula = new Polynome2D(k);

        foreach (Term1D term in xTerms) m_formula.AddXTerm(term);
        foreach (Term1D term in yTerms) m_formula.AddYTerm(term);
        foreach (Term2D term in xyTerms) m_formula.AddXYTerm(term);

        m_id = id;
        m_relativeVolume = relativeVolume;
    }

    public Polynome2D formula
    {
        get { return m_formula; }
    }

    public MeshID meshID
    {
        get { return m_id; }
    }

    public double relativeVolume
    {
        get { return m_relativeVolume; }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class XMLTerm1D
{
    public double m_power = 0;
    public double m_scalar = 0;

    public XMLTerm1D()
    {

    }

    public XMLTerm1D(Term1D term)
    {
        m_scalar = term.scalar;
        m_power = term.power;
    }

    public Term1D ToTerm1D()
    {
        return new Term1D(m_power, m_scalar);
    }
}

public class XMLTerm2D
{
    public double m_xPower = 0, m_yPower = 0;
    public double m_scalar = 0;

    public XMLTerm2D()
    {

    }

    public XMLTerm2D(Term2D term)
    {
        m_scalar = term.scalar;
        m_xPower = term.xPower;
        m_yPower = term.yPower;
    }

    public Term2D ToTerm2D()
    {
        return new Term2D(m_xPower, m_yPower, m_scalar);
    }
}

public class XmlMeshFormula
{
    static private string s_dataFilePath = @"Assets\MeshVolume.xml";

    static private List<MeshVolumeFormula> s_loadedDatas = null;
    static private Dictionary<MeshVolumeFormula.MeshID, MeshVolumeFormula> s_sortedDatas = null;

    public MeshVolumeFormula.MeshID m_id;
    public double m_k = 0;
    public double m_relativeVolume = 0;
    public List<XMLTerm1D> m_xTerms = new List<XMLTerm1D>();
    public List<XMLTerm1D> m_yTerms = new List<XMLTerm1D>();
    public List<XMLTerm2D> m_xyTerms = new List<XMLTerm2D>();


    public XmlMeshFormula()
    {

    }

    public XmlMeshFormula(MeshVolumeFormula meshVolumeFormula)
    {
        m_id = meshVolumeFormula.meshID;
        m_relativeVolume = meshVolumeFormula.relativeVolume;

        Polynome2D formula = meshVolumeFormula.formula;
        m_k = formula.k;

        m_xTerms = new List<XMLTerm1D>(formula.xTerms.Count);
        foreach (Term1D term in formula.xTerms) m_xTerms.Add(new XMLTerm1D(term));
        m_yTerms = new List<XMLTerm1D>(formula.yTerms.Count);
        foreach (Term1D term in formula.yTerms) m_yTerms.Add(new XMLTerm1D(term));
        m_xyTerms = new List<XMLTerm2D>(formula.xyTerms.Count);
        foreach (Term2D term in formula.xyTerms) m_xyTerms.Add(new XMLTerm2D(term));

    }

    public MeshVolumeFormula FormulaCopy()
    {
        List<Term1D> xTerms = new List<Term1D>(m_xTerms.Count);
        foreach (XMLTerm1D term in m_xTerms) xTerms.Add(term.ToTerm1D());
        List<Term1D> yTerms = new List<Term1D>(m_yTerms.Count);
        foreach (XMLTerm1D term in m_yTerms) yTerms.Add(term.ToTerm1D());
        List<Term2D> xyTerms = new List<Term2D>(m_xyTerms.Count);
        foreach (XMLTerm2D term in m_xyTerms) xyTerms.Add(term.ToTerm2D());

        MeshVolumeFormula formula = new MeshVolumeFormula(xTerms, yTerms, xyTerms, m_k, m_id, m_relativeVolume);

        return formula;
    }

    static public void SaveFormula(MeshVolumeFormula formula, bool forceUpdate = false)
    {
        // Reload
        List<MeshVolumeFormula> existingData = LoadData();

        if(!forceUpdate)
        {
            for(int i = 0; i < existingData.Count; i++)
            {
                MeshVolumeFormula existingFormula = existingData[i];
                if (existingFormula.meshID == formula.meshID) return;
            }
        }
        else
        {
            for(int i = 0; i < existingData.Count; i++)
            {
                MeshVolumeFormula existingFormula = existingData[i];
                if (existingFormula.meshID == formula.meshID)
                {
                    existingData.RemoveAt(i);
                    break;
                }
            }
        }

        List<XmlMeshFormula> xmlFormulas = new List<XmlMeshFormula>(existingData.Count + 1);
        foreach (MeshVolumeFormula existingFormula in existingData) xmlFormulas.Add(new XmlMeshFormula(existingFormula));

        // Add
        xmlFormulas.Add(new XmlMeshFormula(formula));

        // Save

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<XmlMeshFormula>));

        using (StreamWriter streamWriter = new StreamWriter(s_dataFilePath))
        {
            xmlSerializer.Serialize(streamWriter, xmlFormulas);
        }
    }

    static public List<MeshVolumeFormula> LoadData()
    {
        s_loadedDatas = new List<MeshVolumeFormula>();
        s_sortedDatas = new Dictionary<MeshVolumeFormula.MeshID, MeshVolumeFormula>();
        if (!System.IO.File.Exists(s_dataFilePath)) return s_loadedDatas;

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<XmlMeshFormula>));

        using (StreamReader streamReader = new StreamReader(s_dataFilePath))
        {
            List<XmlMeshFormula> rawData = xmlSerializer.Deserialize(streamReader) as List<XmlMeshFormula>;

            foreach (XmlMeshFormula rawPolygone in rawData)
            {
                s_loadedDatas.Add(rawPolygone.FormulaCopy());
                s_sortedDatas.Add(rawPolygone.m_id, rawPolygone.FormulaCopy());
            }
        }

        return s_loadedDatas;
    }

    static public List<MeshVolumeFormula> LoadedDatas()
    {
        if (s_loadedDatas == null) LoadData();
        return s_loadedDatas;
    }

    static public MeshVolumeFormula FormulaForMesh(MeshVolumeFormula.MeshID meshId)
    {
        if (s_loadedDatas == null) LoadData();
        if(!s_sortedDatas.ContainsKey(meshId))
        {
            Debug.LogError(meshId + " ne dispose pas encore de données précalculées");
            return null;
        }
        return s_sortedDatas[meshId];
    }
}

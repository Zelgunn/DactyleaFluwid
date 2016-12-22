using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmartFluwidBehaviour : MonoBehaviour
{
    [SerializeField]
    protected MeshVolumeFormula.MeshID m_meshID = MeshVolumeFormula.MeshID.Erlenmeyer;

    protected double m_fullVolume;
    protected double m_currentVolume;
    [SerializeField]
    protected double m_volumePercentage;

    protected double m_height;
    protected Polynome2D m_heightFormula;
    protected float m_scaleFactor;

    protected void Awake()
    {
        LoadMeshFormula();
    }

    protected void LoadMeshFormula()
    {
        MeshVolumeFormula xmlMeshFormula = XmlMeshFormula.FormulaForMesh(m_meshID);

        if (xmlMeshFormula == null)
        {
            this.enabled = false;
            return;
        }

        m_heightFormula = xmlMeshFormula.formula;
        m_scaleFactor = transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z;
        m_fullVolume = xmlMeshFormula.relativeVolume * m_scaleFactor;
        m_currentVolume = m_fullVolume * m_volumePercentage;
    }

    protected void UpdateFluwidHeight()
    {
        float angle = Vector3.Angle(transform.forward, Vector3.up);
        m_height = m_heightFormula.Calculate(angle, m_currentVolume / m_scaleFactor) * transform.lossyScale.y;
    }

    virtual protected void Update()
    {
        UpdateFluwidHeight();
    }
}
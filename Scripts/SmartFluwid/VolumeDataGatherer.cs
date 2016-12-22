using UnityEngine;
using System;
using System.Collections;

public class VolumeDataGatherer : MonoBehaviour
{
    [SerializeField] MeshVolumeFormula.MeshID m_meshID = MeshVolumeFormula.MeshID.Erlenmeyer;

    private MeshRenderer m_meshRenderer;
    private MeshFilter m_meshFilter;
    private Mesh m_mesh;
    private Vector3[] m_vertices;
    private int[] m_triangles;
    private double m_fullVolume;
    private Quaternion m_baseRotation;

    [SerializeField] private bool m_forceUpdate = false;

    [Header("Interpolation")]
    [SerializeField] [Range(3, 25)] private int m_anglesCount = 10;
    [SerializeField] [Range(3, 25)] private int m_volumesCount = 10;
    [SerializeField] [Range(1, 1000)] private int m_heightAccuracy = 10;

    private double[] m_angles;
    private double[] m_volumes;
    private double[][] m_datas;

    [Header("Validation")]
    [SerializeField] [Range(10, 100)] private int m_validationAnglesCount = 25;
    [SerializeField] [Range(10, 100)] private int m_validationVolumesCount = 25;
    [SerializeField] [Range(10, 1000)] private int m_validationHeightAccuracy = 15;

    private double[] m_validationAngles;
    private double[] m_validationVolumes;
    private double[][] m_validationDatas;

    private void Awake ()
    {
        Application.runInBackground = true;
        m_baseRotation = transform.rotation;

        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();

        m_mesh = m_meshFilter.sharedMesh;

        m_vertices = m_mesh.vertices;
        m_triangles = m_mesh.triangles;

        Vector3[] transformedVertices = TransformedVertices(m_vertices);
        m_fullVolume = CalculateVolume(transformedVertices, m_triangles);

        MainProcess();
	}

    [ContextMenu("Reprocess")]
    private void MainProcess()
    {
        StartCoroutine(MainProcessCoroutine());
    }

    private IEnumerator MainProcessCoroutine()
    {
        yield return ComputeInterpolationData();
        InterpoLagrange2D interpoLagrange2D = Interpolate();
        EvaluateInterpolation(interpoLagrange2D, m_angles, m_volumes, m_datas);

        yield return ComputeValidationData();
        EvaluateInterpolation(interpoLagrange2D, m_validationAngles, m_validationVolumes, m_validationDatas);
    }

    private double ZeroOfChebychev(double minValue, double maxValue, int step, int step_count)
    {
        step_count--;
        return (minValue + maxValue) / 2 + (minValue - maxValue) * Math.Cos(step * Math.PI / step_count) / 2;
    }

    private IEnumerator ComputeInterpolationData()
    {
        transform.rotation = m_baseRotation;

        //double anglePace = 180 / (m_anglesCount - 1);
        //double volumePace = m_fullVolume / (m_volumesCount - 1);

        double minH = 9999, maxH = -9999;

        m_angles = new double[m_anglesCount];
        m_volumes = new double[m_volumesCount];
        m_datas = new double[m_anglesCount][];

        for (int i = 0; i < m_anglesCount; i++)
        {
            m_datas[i] = new double[m_volumesCount];
            m_angles[i] = ZeroOfChebychev(0, 180, i, m_anglesCount);
            //m_angles[i] = anglePace * i;
        }

        for (int i = 0; i < m_volumesCount; i++)
        {
            m_volumes[i] = ZeroOfChebychev(0, m_fullVolume, i, m_volumesCount);
            //m_volumes[i] = volumePace * i;
        }

        // 1) Pour chaque orientation
        for (int angleStep = 0; angleStep < m_anglesCount; angleStep++)
        {
            transform.rotation = m_baseRotation;
            transform.Rotate(Vector3.right, (float)m_angles[angleStep]);
            double minHeight, maxHeight;
            // Get MinH et MaxH
            GetMinMaxHighnessOfMesh(out minHeight, out maxHeight);

            //  2) Pour chaque hauteur (entre MinH et MaxH)
            for (int volumeStep = 0; volumeStep < m_volumesCount; volumeStep++)
            {
                double volumeToFind = m_volumes[volumeStep];
                // Trouver meilleure hauteur
                double height = HeightFromVolumeDichotomy(volumeToFind, m_heightAccuracy, minHeight, maxHeight);

                minH = Math.Min(height, minH);
                maxH = Math.Max(height, maxH);

                m_datas[angleStep][volumeStep] = height;

                yield return null;
                //      Supprimer les parties
            }
        }
    }

    private InterpoLagrange2D Interpolate()
    {
        InterpoLagrange2D interpoLagrange2D = new InterpoLagrange2D(m_angles, m_volumes, m_datas);

        double relativeVolume = m_fullVolume / (transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z);

        XmlMeshFormula.SaveFormula(new MeshVolumeFormula(interpoLagrange2D.polynome2D, m_meshID, relativeVolume), m_forceUpdate);

        return interpoLagrange2D;
    }

    private IEnumerator ComputeValidationData()
    {
        transform.rotation = m_baseRotation;

        double anglePace = 180 / (m_validationAnglesCount - 1);
        double volumePace = m_fullVolume / (m_validationVolumesCount - 1);

        double minH = 9999, maxH = -9999;

        m_validationAngles = new double[m_validationAnglesCount];
        m_validationVolumes = new double[m_validationVolumesCount];
        m_validationDatas = new double[m_validationAnglesCount][];

        for (int i = 0; i < m_validationAnglesCount; i++)
        {
            m_validationDatas[i] = new double[m_validationVolumesCount];
            m_validationAngles[i] = anglePace * i;
        }

        for (int i = 0; i < m_validationVolumesCount; i++)
        {
            m_validationVolumes[i] = volumePace * i;
        }

        // 1) Pour chaque orientation
        for (int angleStep = 0; angleStep < m_validationAnglesCount; angleStep++)
        {
            transform.rotation = m_baseRotation;
            transform.Rotate(Vector3.right, (float)m_validationAngles[angleStep]);

            double minHeight, maxHeight;
            // Get MinH et MaxH
            GetMinMaxHighnessOfMesh(out minHeight, out maxHeight);
            //  2) Pour chaque hauteur (entre MinH et MaxH)
            for (int volumeStep = 0; volumeStep < m_validationVolumesCount; volumeStep++)
            {
                double volumeToFind = m_validationVolumes[volumeStep];
                // Trouver meilleure hauteur
                double height = HeightFromVolumeDichotomy(volumeToFind, m_validationHeightAccuracy, minHeight, maxHeight);

                minH = Math.Min(height, minH);
                maxH = Math.Max(height, maxH);

                m_validationDatas[angleStep][volumeStep] = height;

                yield return null;
                //      Supprimer les parties
            }
        }
    }

    private void EvaluateInterpolation(InterpoLagrange2D interpoLagrange2D, double[] xValues, double[] yValues, double[][] zValues)
    {
        Polynome2D interpolationPolynome = interpoLagrange2D.polynome2D;
        int xCount = xValues.Length;
        int yCount = yValues.Length;

        double meanRelativeError = 0;
        double maxRelativeError = 0;

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 1; j < yCount; j++)
            {
                double originalValue = zValues[i][j];
                double interpolatedValue = interpolationPolynome.Calculate(xValues[i], yValues[j]);

                double delta = originalValue - interpolatedValue;


                double relativeError = 0;
                if (interpolatedValue == 0)
                {
                    if (originalValue != 0)
                    {
                        relativeError = Math.Abs(delta / originalValue);
                    }
                    // else error = 0 (because delta = 0)
                }
                else
                {
                    relativeError = Math.Abs(delta / interpolatedValue);
                }

                meanRelativeError += relativeError;
                maxRelativeError = Math.Max(maxRelativeError, relativeError);
            }
        }

        meanRelativeError /= xCount * yCount;

        Debug.Log(m_meshID.ToString() + " : Erreur relative moyenne : " + (meanRelativeError * 100).ToString() + "%");
        Debug.Log(m_meshID.ToString() + " : Erreur relative max : " + (maxRelativeError * 100).ToString() + "%");
    }

    private double CalculateVolume(Vector3[] vertices, int[] triangles)
    {
        if ((vertices == null) || (triangles == null)) return 0;

        Vector3 v1, v2, v3;

        double volume = 0.0f;

        for (int j = 0; j < triangles.Length; j += 3)
        {
            v1 = vertices[triangles[j]];
            v2 = vertices[triangles[j + 1]];
            v3 = vertices[triangles[j + 2]];

            volume += ((v2.y - v1.y) * (v3.z - v1.z) - (v2.z - v1.z) * (v3.y - v1.y)) * (v1.x + v2.x + v3.x);
        }

        return volume / 6.0f;
    }

    private double HeightFromVolumeDichotomy(double volume, int accuracy, double min, double max)
    {
        double previousHeight = 0, height = 0;
        double volumeAtPreviousHeight = 0, volumeAtHeight = 0, tmp;

        for (int i = 0; i < accuracy; i++)
        {
            height = (max + min) / 2;

            tmp = MeshVolumeAtHeight(height);

            volumeAtHeight = tmp;

            if ((previousHeight < height) && (volumeAtPreviousHeight > volumeAtHeight))
            {
                volumeAtHeight = volumeAtPreviousHeight;
            }

            if (volumeAtHeight == volume) return height;

            if (volumeAtHeight > volume)
            {
                max = height;
            }
            else
            {
                min = height;
            }

            if (i < (accuracy - 1))
            {
                previousHeight = height;
                volumeAtPreviousHeight = volumeAtHeight;
            }
        }

        if (volumeAtPreviousHeight == volumeAtHeight)
        {
            return height;
        }

        double ratio = (volumeAtPreviousHeight - volume) / (volumeAtPreviousHeight - volumeAtHeight);

        height = ratio * height + (1 - ratio) * previousHeight;

        return height;
    }

    private double MeshVolumeAtHeight(double height)
    {   
        //      Duppliquer Mesh
        GameObject copy = new GameObject();
        MeshFilter meshFilter = copy.AddComponent<MeshFilter>();
        meshFilter.mesh = m_mesh;
        MeshRenderer meshRenderer = copy.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = m_meshRenderer.sharedMaterials;

        copy.transform.localScale = transform.localScale;
        copy.transform.rotation = transform.rotation;

        //      Couper duplicatat
        GameObject[] slices = BLINDED_AM_ME.MeshCut.Cut(copy, Vector3.up * (float)height, Vector3.up, m_meshRenderer.material);
        
        if(!slices[0])
        {
            Debug.Log("Erreur : Impossible de calculer le mesh à cette hauteur : " + height);
            return -1;
        }

        //      Calculer volume partie "gauche"
        Mesh resultingMesh = slices[0].GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = TransformedVertices(resultingMesh.vertices);
        int[]triangles = resultingMesh.triangles;

        double volume = CalculateVolume(vertices, triangles);

        Destroy(slices[0]);
        if(slices[1]) Destroy(slices[1]);

        return volume;
    }

    private void GetMinMaxHighnessOfMesh(out double min, out double max)
    {
        Vector3[] transformedVertices = TransformedVertices(m_vertices);
        min = max = 0;

        for (int i = 0; i < transformedVertices.Length; i++)
        {
            Vector3 point = transformedVertices[i];
            if (i == 0)
            {
                min = point.y;
                max = point.y;
            }
            else if (point.y < min)
            {
                min = point.y;
            }
            else if (point.y > max)
            {
                max = point.y;
            }
        }
    }

    private Vector3[] TransformedVertices(Vector3[] vertices)
    {
        Vector3[] result = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            result[i] = TransformedVertex(vertices[i]);
        }

        return result;
    }

    private Vector3 TransformedVertex(Vector3 vertex)
    {
        vertex.Scale(transform.lossyScale);
        vertex = transform.rotation * vertex;

        return vertex;
    }
}

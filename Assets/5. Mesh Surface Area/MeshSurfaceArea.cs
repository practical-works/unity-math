using Sirenix.OdinInspector;
using UnityEngine;

public class MeshSurfaceArea : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;

    //----------------------------------------------------------------------------------------------------

    [ShowInInspector, DisplayAsString, EnableGUI]
    public Mesh Mesh => _meshFilter != null ? _meshFilter.sharedMesh : null;

    [ShowInInspector, BoxGroup("Infos"), PropertyRange(0, 0, MaxGetter = nameof(TrianglesCount))]
    public int DisplayTriangleIndex { get; set; }

    [ShowInInspector, BoxGroup("Infos"), DisplayAsString, EnableGUI]
    public int TrianglesCount => Mesh != null ? Mesh.triangles.Length / 3 : 0;

    [ShowInInspector, BoxGroup("Infos"), DisplayAsString, EnableGUI]
    public int VerticesCount => Mesh != null ? Mesh.vertexCount : 0;

    [ShowInInspector, BoxGroup("Infos"), DisplayAsString, EnableGUI]
    public int NormalsCount => Mesh != null ? Mesh.normals.Length : 0;

    [ShowInInspector, BoxGroup("Infos"), DisplayAsString, EnableGUI]
    public int TangentsCount => Mesh != null ? Mesh.tangents.Length : 0;

    [ShowInInspector, DisplayAsString, EnableGUI, SuffixLabel("unit x unit")]
    public float SurfaceArea => GetMeshSurfaceArea(_meshFilter.sharedMesh);

    [ShowInInspector, DisplayAsString, EnableGUI, SuffixLabel("unit x unit x unit")]
    public float Volume => GetMeshVolume(_meshFilter.sharedMesh);

    //----------------------------------------------------------------------------------------------------

    public static float GetMeshSurfaceArea(Mesh mesh)
    {
        if (mesh == null) return 0;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        float totalSurfaceArea = 0;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 vertex0 = vertices[triangles[i]];
            Vector3 vertex1 = vertices[triangles[i + 1]];
            Vector3 vertex2 = vertices[triangles[i + 2]];
            Vector3 edge0 = vertex0 - vertex1;
            Vector3 edge1 = vertex0 - vertex2;
            totalSurfaceArea += Vector3.Cross(edge0, edge1).magnitude;
        }
        totalSurfaceArea *= 0.5f;
        return totalSurfaceArea;
    }

    public static float GetMeshVolume(Mesh mesh)
    {
        if (mesh == null) return 0;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        float totalVolume = 0;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 vertex0 = vertices[triangles[i]];
            Vector3 vertex1 = vertices[triangles[i + 1]];
            Vector3 vertex2 = vertices[triangles[i + 2]];
            Vector3 edge0 = vertex0 - vertex1;
            Vector3 edge1 = vertex0 - vertex2;
            totalVolume += Vector3.Dot(vertex0, Vector3.Cross(vertex1, vertex2));
        }
        totalVolume *= 1f / 6f;
        return totalVolume;
    }

    //----------------------------------------------------------------------------------------------------

    private void OnValidate()
    {
        DisplayTriangleIndex = (int)Mathf.Clamp(DisplayTriangleIndex, 0f, TrianglesCount - 1f);
    }

    private void OnDrawGizmos()
    {
        if (_meshFilter == null) return;
        Vector3 origin = _meshFilter.transform.position;
        Mesh mesh = _meshFilter.sharedMesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Gizmos.color = Color.red;
        int i = 3 * DisplayTriangleIndex;
        Gizmos.DrawLine(from: origin + vertices[triangles[i]], to: origin + vertices[triangles[i + 1]]);
        Gizmos.DrawLine(from: origin + vertices[triangles[i + 1]], to: origin + vertices[triangles[i + 2]]);
        Gizmos.DrawLine(from: origin + vertices[triangles[i + 2]], to: origin + vertices[triangles[i]]);
    }
}
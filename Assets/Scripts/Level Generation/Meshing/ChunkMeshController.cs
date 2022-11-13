using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkMeshController : MonoBehaviour
{
    public LevelMeshController levelMeshController;
    public Vector3Int ChunkAddress { get; private set; }
    public Vector3 ChunkSizeInMeters { get; private set; }

    public Material TerrainMaterial
    {
        get
        {
            return levelMeshController.terrainMaterial;
        }
    }

    protected Mesh mesh;
    protected MeshFilter meshFilter;
    protected MeshCollider meshCollider;

    protected List<Vector3> vertices;
    protected List<int> triangles;
    protected List<Vector3> normals;

    private void Awake()
    {
        mesh = new Mesh();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.enabled = false;
        //meshCollider.sharedMesh = mesh;

        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
    }

    public void SetLayer(int layer)
    {
        gameObject.layer = layer;
    }

    public void SetChunkAddress(Vector3Int chunkAddress)
    {
        ChunkAddress = chunkAddress;
    }

    public void SetChunkSizeInMeters(Vector3 chunkSizeInMeters)
    {
        ChunkSizeInMeters = chunkSizeInMeters;
    }

    public abstract void Generate(Grid grid);

    protected void ClearData()
    {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
    }

    public void UpdateMesh()
    {
        if (ShouldUpdateMesh())
        {
            mesh.Clear();

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            //mesh.RecalculateNormals();

            GetComponent<MeshRenderer>().material = TerrainMaterial;

            //Debug.Log(gameObject.name);
            if (vertices.Count > 30)
            {
                // TODO - Safa: [Physics.PhysX] cleaning the mesh failed çöz. 3 0 16ta oluyor mesela.
                meshCollider.sharedMesh = mesh;
                meshCollider.enabled = true;
            }

            ClearData();
        }
    }

    public virtual bool ShouldUpdateMesh()
    {
        return true;
    }
}

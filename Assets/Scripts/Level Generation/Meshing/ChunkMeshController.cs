using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkMeshController: MonoBehaviour
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

    protected List<Vector3> vertices;
    protected List<int> triangles;
    protected List<Vector3> normals;

    private void Awake()
    {
        mesh = new Mesh();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
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
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        //mesh.RecalculateNormals();

        GetComponent<MeshRenderer>().material = TerrainMaterial;

        ClearData();
    }
}

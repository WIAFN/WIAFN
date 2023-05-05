using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;

public abstract class ChunkMeshController : MonoBehaviour
{
    [HideInInspector]
    public LevelMeshController levelMeshController;

    public Vector3Int ChunkAddress { get; private set; }
    public Vector3 ChunkSizeInMeters { get; private set; }

    //public Material TerrainMaterial
    //{
    //    get
    //    {
    //        return levelMeshController.terrainMaterial;
    //    }
    //}

    protected Mesh mesh;
    protected MeshFilter meshFilter;
    protected MeshCollider meshCollider;

    protected Task meshGenerateTask;

    public Task MeshGenerationTask => meshGenerateTask;

    private Vector3[] _verticesArr;
    private int[] _trianglesArr;
    private Vector3[] _normalsArr;
    //private List<Vector2> uvs;

    protected List<Vector3> vertices;
    protected List<int> triangles;
    protected List<Vector3> normals;
    //protected List<Vector2> _tempUVs;

    private bool _meshUpdateRequested;

    private void Awake()
    {
        mesh = new Mesh();
        //meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter = gameObject.GetComponent<MeshFilter>();
        //meshFilter.mesh = mesh;
        meshFilter.sharedMesh = mesh;

        //meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.enabled = false;
        //meshCollider.sharedMesh = mesh;

        _verticesArr = null;
        _trianglesArr = null;
        _normalsArr = null;

        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
        //uvs = new List<Vector2>();
    }

    private void Start()
    {
        mesh.name = $"Chunk {ChunkAddress} Mesh";
    }

    private void Update()
    {
        if (_meshUpdateRequested && IsReadyToUpdateMesh())
        {
            //mesh.Clear();
            _meshUpdateRequested = false;

            mesh.vertices = _verticesArr;
            mesh.triangles = _trianglesArr;
            mesh.normals = _normalsArr;
            //mesh.RecalculateNormals();
            //mesh.uv = uvs.ToArray();

            //GetComponent<MeshRenderer>().material = TerrainMaterial;

            //Debug.Log(gameObject.name);
            Vector3 size = mesh.bounds.size;
            int zeroCount = 0;
            zeroCount += (size.x == 0) ? 1 : 0;
            zeroCount += (size.y == 0) ? 1 : 0;
            zeroCount += (size.z == 0) ? 1 : 0;

            if (zeroCount < 2)
            {
                meshCollider.sharedMesh = mesh;
                meshCollider.enabled = true;
            }

            levelMeshController.OnChunkMeshUpdated(this);
            ClearData();
        }
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

    public abstract void Generate(Grid grid, bool multithreaded = true, Task taskIfMultithreaded = null);

    protected void ClearData()
    {
        _verticesArr = null;
        _trianglesArr = null;
        _normalsArr = null;
    }

    public bool MarkMeshToUpdate()
    {
        bool result = ShouldUpdateMesh();
        if (result)
        {
            _meshUpdateRequested = true;
        }

        return result;
    }

    public virtual bool ShouldUpdateMesh()
    {
        return true;
    }

    public virtual bool IsReadyToUpdateMesh()
    {
        //return true;
        return (meshGenerateTask == null || meshGenerateTask.Status != TaskStatus.Running) && levelMeshController.CheckUpdateMesh(this);
    }

    protected void ValidateAndFlushMeshData()
    {
        _verticesArr = vertices.ToArray();
        _trianglesArr = triangles.ToArray();
        _normalsArr = normals.ToArray();
        FlushMeshData();
    }

    protected void FlushMeshData()
    {
        vertices.Clear();
        vertices.Capacity = 100;
        triangles.Clear();
        triangles.Capacity = 100;
        normals.Clear();
        normals.Capacity = 100;
    }

    //public bool HasNeighbour(Vector3Int direction)
    //{
    //    Debug.Assert(direction.sqrMagnitude <= 2);

    //    JunkyardLevelGenerator junkyardLevelGenerator = (JunkyardLevelGenerator)levelMeshController.LevelGenerator;
    //    return junkyardLevelGenerator.CurrentGrid.CheckIfInGrid(ChunkAddress + direction);
    //}

    // Not tested.
    public Vector3Int[] GetNeighbours()
    {
        List<Vector3Int> neighbourAddresses = new List<Vector3Int>();
        Vector3Int[] directions = new Vector3Int[] { Vector3Int.up, Vector3Int.right, Vector3Int.forward, Vector3Int.down, Vector3Int.left, Vector3Int.back };

        foreach (var direction in directions)
        {
            //if (HasNeighbour(direction))
            //{
                neighbourAddresses.Add(ChunkAddress + direction);
            //}
        }

        return neighbourAddresses.ToArray();
    }
}

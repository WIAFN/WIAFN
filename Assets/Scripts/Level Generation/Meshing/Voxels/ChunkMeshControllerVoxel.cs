using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ChunkMeshControllerVoxel : ChunkMeshController
{
    public override void Generate(Grid grid)
    {
        Vector3 gridCellSize = new Vector3(ChunkSizeInMeters.x / grid.Size.x, ChunkSizeInMeters.y / grid.Size.y, ChunkSizeInMeters.z / grid.Size.z);
        ClearData();

        int vertexIndex = 0;
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                for (int z = 0; z < grid.Width; z++)
                {
                    Vector3Int address = new Vector3Int(x, y, z);

                    if (grid.GetCell(address).Empty)
                    {
                        continue;
                    }

                    Vector3 pos = new Vector3(x * gridCellSize.x, y * gridCellSize.y, z * gridCellSize.z);

                    for (int p = 0; p < 6; p++)
                    {
                        if (grid.GetNeighbour(address, VoxelData.faceChecks[p]).Empty)
                        {
                            vertices.Add(pos + Vector3.Scale(VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]], gridCellSize));
                            vertices.Add(pos + Vector3.Scale(VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]], gridCellSize));
                            vertices.Add(pos + Vector3.Scale(VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]], gridCellSize));
                            vertices.Add(pos + Vector3.Scale(VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]], gridCellSize));

                            triangles.Add(vertexIndex);
                            triangles.Add(vertexIndex + 1);
                            triangles.Add(vertexIndex + 2);
                            triangles.Add(vertexIndex + 2);
                            triangles.Add(vertexIndex + 1);
                            triangles.Add(vertexIndex + 3);

                            for (int i = 0; i < 4; i++)
                            {
                                normals.Add(VoxelData.faceChecks[p]);
                            }

                            vertexIndex += 4;
                        }

                    }
                }
            }
        }
    }
}

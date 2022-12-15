using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(MeshRenderer))]
public class ChunkMeshControllerSmooth : ChunkMeshController
{
    public override void Generate(Grid grid)
    {
        Vector3 gridCellSize = new Vector3(ChunkSizeInMeters.x / grid.Size.x, ChunkSizeInMeters.y / grid.Size.y, ChunkSizeInMeters.z / grid.Size.z);
        ClearData();

        int xEnd = grid.Width - 1;
        if (ChunkAddress.x == levelMeshController.levelSizeInChunks.x - 1)
        {
            xEnd += 1;
        }

        int yEnd = grid.Height - 1;
        if (ChunkAddress.y == levelMeshController.levelSizeInChunks.y - 1)
        {
            yEnd += 1;
        }

        int zEnd = grid.Width - 1;
        if (ChunkAddress.z == levelMeshController.levelSizeInChunks.z - 1)
        {
            zEnd += 1;
        }

        for (int x = -1; x < xEnd; x++)
        {
            for (int y = -1; y < yEnd; y++)
            {
                for (int z = -1; z < zEnd; z++)
                {
                    Vector3Int address = new Vector3Int(x, y, z);
                    ProcessCube(grid, address, gridCellSize);
                }
            }
        }
    }

    public void ProcessCube(Grid grid, Vector3Int address, Vector3 gridCellSize)
    {
        // Calculate coordinates of each corner of the current cube
        Vector3Int[] cornerCoords = new Vector3Int[8];
        cornerCoords[0] = address + new Vector3Int(0, 0, 0);
        cornerCoords[1] = address + new Vector3Int(1, 0, 0);
        cornerCoords[2] = address + new Vector3Int(1, 0, 1);
        cornerCoords[3] = address + new Vector3Int(0, 0, 1);
        cornerCoords[4] = address + new Vector3Int(0, 1, 0);
        cornerCoords[5] = address + new Vector3Int(1, 1, 0);
        cornerCoords[6] = address + new Vector3Int(1, 1, 1);
        cornerCoords[7] = address + new Vector3Int(0, 1, 1);

        // Calculate unique index for each cube configuration.
        // There are 256 possible values (cube has 8 corners, so 2^8 possibilites).
        // A value of 0 means cube is entirely inside the surface; 255 entirely outside.
        // The value is used to look up the edge table, which indicates which edges of the cube the surface passes through.
        int cubeConfiguration = 0;
        for (int i = 0; i < 8; i++)
        {
            // Think of the configuration as an 8-bit binary number (each bit represents the state of a corner point).
            // The state of each corner point is either 0: above the surface, or 1: below the surface.
            // The code below sets the corresponding bit to 1, if the point is below the surface.
            if (grid.GetCell(cornerCoords[i]).Filled)
            {
                cubeConfiguration |= (1 << i);
            }
        }

        // Get array of the edges of the cube that the surface passes through.

        // Create triangles for the current cube configuration
        for (int i = 0; i < 16; i += 3)
        {
            int edgeIndexA = MarchTables.Triangulation[cubeConfiguration, i];
            // If edge index is -1, then no further vertices exist in this configuration
            if (edgeIndexA == -1) { break; }

            // Get indices of the two corner points defining the edge that the surface passes through.
            // (Do this for each of the three edges we're currently looking at).
            int a0 = MarchTables.CornerIndexAFromEdge[edgeIndexA];
            int a1 = MarchTables.CornerIndexBFromEdge[edgeIndexA];

            int edgeIndexB = MarchTables.Triangulation[cubeConfiguration, i + 1];
            int b0 = MarchTables.CornerIndexAFromEdge[edgeIndexB];
            int b1 = MarchTables.CornerIndexBFromEdge[edgeIndexB];

            int edgeIndexC = MarchTables.Triangulation[cubeConfiguration, i + 2];
            int c0 = MarchTables.CornerIndexAFromEdge[edgeIndexC];
            int c1 = MarchTables.CornerIndexBFromEdge[edgeIndexC];

            int vertexIndex = vertices.Count;

            // Calculate positions of each vertex.
            CreateVertexData(grid, gridCellSize, cornerCoords[a0], cornerCoords[a1]);
            CreateVertexData(grid, gridCellSize, cornerCoords[b0], cornerCoords[b1]);
            CreateVertexData(grid, gridCellSize, cornerCoords[c0], cornerCoords[c1]);


            // Normal:
            Vector3 normal = CalculateNormal(vertices[vertexIndex], vertices[vertexIndex+1], vertices[vertexIndex+2]);
            for (int k = 0; k < 3; k++)
            {
                normals.Add(normal);
            }


            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }
    }

    private void CreateVertexData(Grid grid, Vector3 gridCellSize, Vector3Int coordA, Vector3Int coordB)
    {
        // Interpolate between the two corner points based on the density
        Vector3 posA = new Vector3(coordA.x * gridCellSize.x, coordA.y * gridCellSize.y, coordA.z * gridCellSize.z);
        Vector3 posB = new Vector3(coordB.x * gridCellSize.x, coordB.y * gridCellSize.y, coordB.z * gridCellSize.z);

        float aDensity = grid.GetCell(coordA).CellRawValue - 0.5f;
        float bDensity = grid.GetCell(coordB).CellRawValue - 0.5f;

        float t;
        if (bDensity == aDensity)
        {
            t = 0.5f;
        }
        else
        {
            t = Mathf.Clamp((0f - aDensity) / (bDensity - aDensity), 0f, 1f);
        }

        Vector3 position = posA + t * (posB - posA);

        vertices.Add(position);
    }

    private Vector3 CalculateNormal(Vector3 pos1, Vector3 pos2, Vector3 pos3)
    {
        Vector3 normal = Vector3.Cross(pos3 - pos2, pos1 - pos2);

        return Vector3.Normalize(normal);
    }

    public override bool ShouldUpdateMesh()
    {
        // Check if chunk is worth drawing.
        bool notWorth = true;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            if (vertex.y > 0.01f)
            {
                notWorth = false;
                break;
            }
        }

        return !notWorth;
    }
}

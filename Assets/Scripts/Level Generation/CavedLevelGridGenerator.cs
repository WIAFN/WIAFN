using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelMeshController))]
public class CavedLevelGridGenerator : MonoBehaviour
{
    public int width, height;
    private LevelMeshController levelMeshController;

    // Start is called before the first frame update
    void Start()
    {
        levelMeshController = GetComponent<LevelMeshController>();

        GenerateGrid();
    }

    void GenerateGrid()
    {
        Grid grid = new Grid(width, height);
        Perlin terrainPerlin = new Perlin(20);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                float noiseValue = terrainPerlin.Noise3D(x, 0, z, 0.15f, 0.8f, 2);
                int terrainHeight = Mathf.FloorToInt(noiseValue * height);

                for (int y = 5; y > 3; y--)
                {
                    grid.GetCell(x, terrainHeight - y, z).SetValue(terrainPerlin.Noise3D(x, -y, z, 0.15f, 0.8f, 2));
                }

                for (int y = 3; y > 0; y--)
                {
                    grid.GetCell(x, terrainHeight - y, z).SetValue(terrainPerlin.Noise3D(x, -y, z, 0.15f, 0.8f, 2))/*.SetFilled(true)*/;
                }

                grid.GetCell(x, terrainHeight, z).SetValue(noiseValue)/*.SetFilled(true)*/;

                for (int y = -1; y > -3; y--)
                {
                    grid.GetCell(x, terrainHeight - y, z).SetValue(terrainPerlin.Noise3D(x, -y, z, 0.15f, 0.8f, 2));
                }
            }
        }

        Perlin undergroundPerlin = new Perlin(10);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height / 3; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    float noiseValue = undergroundPerlin.Noise3D(x, y, z, 0.7f, 1, 1);
                    GridCell cell = grid.GetCell(x, y, z);
                    if (cell.Empty)
                    {
                        cell.SetValue(noiseValue);

                        //if (noiseValue > 0.5f)
                        //{
                        //    cell.SetFilled(true);
                        //}
                    }
                }
            }
        }

        levelMeshController.Generate(grid);
        levelMeshController.UpdateMeshes();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelMeshController))]
public class RoomedLevelGenerator : MonoBehaviour
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
        Perlin terrainPerlin = new Perlin(31);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                float noiseValue = terrainPerlin.Noise2D(x, z, 0.16f, 0.8f, 2);

                if (noiseValue <= 0.5f)
                {
                    noiseValue = 0f;
                }
                else
                {
                    noiseValue = RangeUtilities.map(noiseValue, 0.5f, 1f, 0f, 1f);
                }

                int terrainHeight = Mathf.FloorToInt(noiseValue * height);
                for (int y = 0; y < terrainHeight; y++)
                {
                    grid.GetCell(x, y, z).SetValue(noiseValue).SetFilled(true);
                }
            }
        }

        levelMeshController.Generate(grid);
        levelMeshController.UpdateMeshes();
    }
}

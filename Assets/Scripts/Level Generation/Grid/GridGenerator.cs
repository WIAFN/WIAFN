using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WIAFN.LevelGeneration
{
    public class GridGenerator
    {
        private LevelGeneratorBase _levelGeneratorBase;
        public int widthInMeters, heightInMeters;
        public Vector2Int resolution;
        public Vector3 startPos;

        public Grid grid;

        public GridGenerator(LevelGeneratorBase levelGeneratorBase, int width, int height, Vector2Int resolution, Vector3 startPos)
        {
            _levelGeneratorBase = levelGeneratorBase;
            this.widthInMeters = width;
            this.heightInMeters = height;
            this.resolution = resolution;
            this.startPos = startPos;

            this.grid = null;
        }

        public GridGenerator(LevelGeneratorBase levelGeneratorBase, int width, int height, Vector2Int resolution) : this(levelGeneratorBase, width, height, resolution, Vector3.zero)
        {
        }

        public void GenerateGrid()
        {
            Grid grid = new Grid(resolution.x, resolution.y);
            int width = resolution.x;
            int height = resolution.y;
            float unitWidth = (float)widthInMeters / width;
            //float unitHeight = heightInMeters / height;

            for (int x = 0; x < width; x++)
            {
                float xPos = x * unitWidth;
                for (int z = 0; z < width; z++)
                {
                    float zPos = z * unitWidth;
                    float noiseValue = _levelGeneratorBase.GetNoiseValueAt(startPos.x + xPos, startPos.z + zPos);

                    int terrainHeight = Mathf.FloorToInt(noiseValue * height);
                    for (int y = 0; y < terrainHeight; y++)
                    {
                        //grid.GetCell(x, y, z).SetValue(noiseValue).SetFilled(true);
                        float heightValue = (float)(height - y) / height;
                        grid.GetCell(x, y, z).SetValue(RangeUtilities.map(heightValue, 0f, 1f, 0.5f, 1f));
                    }

                    int heightDiff = height - terrainHeight;
                    for (int y = 0; y < heightDiff; y++)
                    {
                        float heightValue = (float)(heightDiff - y) / heightDiff;
                        grid.GetCell(x, terrainHeight + y, z).SetValue(RangeUtilities.map(heightValue, 0f, 1f, 0f, 0.5f));
                    }
                }
            }

            this.grid = grid;
        }
    }
}

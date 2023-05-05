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
        public int width, height;
        public Vector3Int startPos;

        public Grid grid;

        public GridGenerator(LevelGeneratorBase levelGeneratorBase, int width, int height, Vector3Int startPos)
        {
            _levelGeneratorBase = levelGeneratorBase;
            this.width = width;
            this.height = height;
            this.startPos = startPos;


            this.grid = null;
        }

        public GridGenerator(LevelGeneratorBase levelGeneratorBase, int width, int height) : this(levelGeneratorBase, width, height, Vector3Int.zero)
        {
        }

        public void GenerateGrid()
        {
            Grid grid = new Grid(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < width; z++)
                {
                    float noiseValue = _levelGeneratorBase.GetNoiseValueAt(startPos.x + x, startPos.z + z);

                    int terrainHeight = Mathf.FloorToInt(noiseValue * height);
                    for (int y = 0; y < terrainHeight; y++)
                    {
                        //grid.GetCell(x, y, z).SetValue(noiseValue).SetFilled(true);
                        float heightValue = (height - y) / height;
                        grid.GetCell(x, y, z).SetValue(RangeUtilities.map(heightValue, 0f, 1f, 0.5f, 1f));
                    }

                    float heightDiff = height - terrainHeight;
                    for (int y = 0; y < heightDiff; y++)
                    {
                        float heightValue = (heightDiff - y) / heightDiff;
                        grid.GetCell(x, terrainHeight + y, z).SetValue(RangeUtilities.map(heightValue, 0f, 1f, 0f, 0.5f));
                    }
                }
            }

            this.grid = grid;
        }
    }
}

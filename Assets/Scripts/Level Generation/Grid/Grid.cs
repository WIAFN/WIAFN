using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Grid
{
    private Grid parentGrid;
    private Vector3Int originOnParentGrid;

    private int width, height;
    private GridCell[,,] gridMap;
    
    public Grid (Grid parentGrid, Vector3Int originOnParentGrid, int width, int height): this(width, height)
    {
        this.parentGrid = parentGrid;
        this.originOnParentGrid = originOnParentGrid;
    }

    /// <summary>
    /// Initializes a grid with empty cells.
    /// </summary>
    public Grid(int width, int height)
    {
        this.width = width;
        this.height = height;
        gridMap = new GridCell[width, height, width];

        // Fills grid with empty.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    gridMap[x, y, z] = new GridCell();
                }
            }
        }
    }

    public GridCell GetCell(Vector3Int address)
    {
        return GetCell(address.x, address.y, address.z);
    }

    public GridCell GetCell(int x, int y, int z)
    {
        if (!CheckIfInGrid(x, y, z))
        {
            if (parentGrid != null)
            {
                return parentGrid.GetCell(GetAddressOnParentGrid(new Vector3Int(x, y, z)));
            }
            else
            {
                GridCell newCell = new GridCell();
                return newCell;
            }
        }

        return gridMap[x, y, z];
    }

    public GridCell GetNeighbour(Vector3Int address, Vector3 direction)
    {
        direction.Normalize();
        int x = (int)Math.Round(direction.x, MidpointRounding.AwayFromZero);
        int y = (int)Math.Round(direction.y, MidpointRounding.AwayFromZero);
        int z = (int)Math.Round(direction.z, MidpointRounding.AwayFromZero);
        Vector3Int directionInt = new Vector3Int(x, y, z);

        return GetCell(address + directionInt);
    }

    public void SetCell(Vector3Int address, GridCell cell)
    {
        SetCell(address.x, address.y, address.z, cell);
    }

    public void SetCell(int x, int y, int z, GridCell cell)
    {
        Assert.IsTrue(CheckIfInGrid(x, y, z));
        gridMap[x, y, z] = cell;
    }


    public List<GridCell> GetFilledNeighbours(Vector3Int address)
    {
        List<GridCell> cells = new List<GridCell>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {

                    if (!CheckIfInGrid(address.x + x, address.y + y, address.z + z))
                    {
                        continue;
                    }

                    GridCell neighbour = GetCell(address.x + x, address.y + y, address.z + z);
                    if (neighbour.Filled)
                    {
                        cells.Add(neighbour);
                    }
                }
            }
        }

        return cells;
    }


    /// <summary>
    /// Gets the subgrid within addresses (inclusive).
    /// </summary>
    public Grid GetSubGrid(Vector3Int minAddress, Vector3Int maxAddress)
    {
        int width = Mathf.Max(maxAddress.x - minAddress.x, maxAddress.z - minAddress.z) + 1;
        int height = maxAddress.y - minAddress.y + 1;
        Grid subGrid = new Grid(this, minAddress, width, height);

        for (int x = minAddress.x; x <= maxAddress.x; x++)
        {
            for (int y = minAddress.y; y <= maxAddress.y; y++)
            {
                for (int z = minAddress.z; z <= maxAddress.z; z++)
                {
                    subGrid.SetCell(x - minAddress.x, y - minAddress.y, z - minAddress.z, this.GetCell(x, y, z));
                }
            }
        }

        return subGrid;
    }


    public bool CheckIfInGrid(Vector3Int address)
    {
        return CheckIfInGrid(address.x, address.y, address.z);
    }

    public bool CheckIfInGrid(int x, int y, int z)
    {
        return CheckIfInWidth(x) && CheckIfInHeight(y) && CheckIfInWidth(z);
    }

    public bool CheckIfInWidth(int address)
    {
        return address >= 0 && address < Width;
    }

    public bool CheckIfInHeight(int address)
    {
        return address >= 0 && address < Height;
    }

    public bool CheckIfOnBorder(Vector3Int address)
    {
        return CheckIfOnBorder(address.x, address.y, address.z);
    }

    public bool CheckIfOnBorder(int x, int y, int z)
    {
        return CheckIfOnBorderHorizontal(x) || CheckIfOnBorderVertical(y) || CheckIfOnBorderHorizontal(z);
    }

    public bool CheckIfOnBorderVertical(int y)
    {
        return y == 0 || y == Height - 1;
    }

    public bool CheckIfOnBorderHorizontal(int x)
    {
        return x == 0 || x == Width - 1;
    }

    private Vector3Int GetAddressOnParentGrid(Vector3Int address)
    {
        return originOnParentGrid + address;
    }


    public int Width { get { return width; } }

    public int GridVolume 
    {
        get
        {
            return width * width * height;
        } 
    }

    public int Height
    {
        get
        {
            return height;
        }
    }

    public Vector3Int Size
    {
        get
        {
            return new Vector3Int(width, height, width);
        }
    }

    public bool IsSubGrid
    {
        get
        {
            return parentGrid != null;
        }
    }

    public bool BorderGridOnParentGrid
    {
        get
        {
            return IsSubGrid && (parentGrid.CheckIfOnBorder(originOnParentGrid) || parentGrid.CheckIfOnBorder(originOnParentGrid + new Vector3Int(Width, Height, Width)));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public float CellRawValue { get; private set; }
    public Vector3Int Address {get; private set;}

    public GridCell(int x, int y, int z): this(new Vector3Int(x, y, z)) { }
    
    public GridCell(Vector3Int address) { 
        Address = address;
        CellRawValue = -1f;
     }

    public void SetValue(float cellValue) {
        CellRawValue = cellValue;
    }

    public bool HasValue
    {
        get
        {
            return CellRawValue != -1f;
        }
    }

    public bool Filled
    {
        get
        {
            return CellRawValue > 0.5f;
        }
    }

    public bool IsEmpty
    { 
        get
        {
            return !Filled;
        } 
    }
}

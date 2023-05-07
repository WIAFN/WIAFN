using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class GridCell
{
    public float CellRawValue { get; private set; }
    //public Vector3Int Address { get; private set; }
    public bool Filled => CellRawValue >= 0.5f;

    //public GridCell(int x, int y, int z): this(new Vector3Int(x, y, z)) { }

    public GridCell(/*Vector3Int address*/) : this(/*address, */-1f) { }

    //public GridCell(Vector3Int address, float cellRawValue, bool filled) : this(address, cellRawValue)
    //{
    //    Filled = filled;
    //}

    public GridCell(/*Vector3Int address, */float cellRawValue)
    {
        //Address = address;
        CellRawValue = cellRawValue;
    }

    //public GridCell SetFilled(bool filled)
    //{
    //    Filled = filled;
    //    return this;
    //}

    //public GridCell SetAddress(int x, int y, int z)
    //{
    //    return this.SetAddress(new Vector3Int(x, y, z));
    //}

    //public GridCell SetAddress(Vector3Int address)
    //{
    //    Address = address;
    //    return this;
    //}

    public GridCell SetValue(float cellValue) {
        CellRawValue = cellValue;
        return this;
    }

    public bool HasValue
    {
        get
        {
            return CellRawValue != -1f;
        }
    }

    public bool Empty
    { 
        get
        {
            return !Filled;
        } 
    }
}

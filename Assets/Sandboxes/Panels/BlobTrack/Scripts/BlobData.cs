using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobData
{
    /*public:*/
    public Vector2 Position { get; set; }
    public Vector2 OldPosition { get; set; }
    public bool WasReleased { get; set; }
    public bool IsWiping { get; set; }
    public bool IsDragging { get; set; }
    public bool IsGrabbing { get; set; }
    public ref GameObject GetObjRef { get { return ref objectRef; } }
    /****/

    /*private:*/
    private int grabRef = 0;
    private int idx;
    private GameObject objectRef;
    /****/

    public BlobData(Vector2 _pos, int idx)
    {
        Position = _pos;
        OldPosition = Position;
    }

    public void SetGrabRef(ref GameObject _obj)
    {
        objectRef = _obj;
    }
}

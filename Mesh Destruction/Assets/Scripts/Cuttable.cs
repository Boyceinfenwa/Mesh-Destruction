using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

public class Cuttable : MonoBehaviour
{
    [SerializeField]
    private bool _Solid = true;

    [SerializeField]
    private bool _reverseWindTriangles = false;

    [SerializeField]
    private bool _Gravity = false;

    [SerializeField]
    private bool _shareVerts = false;

    [SerializeField]
    private bool _smoothVerts = false;

    public bool Solid
    {
        get
        {
            return _Solid;
        }
        set
        {
            _Solid = value;
        }
    }

            public bool ReverseWireTriangles
    {
        get
        {
            return _reverseWindTriangles;
        }
        set
        {
            _reverseWindTriangles = value;
        }
    }

    public bool Gravity
    {
        get
        {
            return _Gravity;
        }
        set
        {
            _Gravity = value;
        }
    }

    public bool ShareVerts
    {
        get
        {
            return _shareVerts;
        }
        set
        {
            _shareVerts = value;
        }
    }

    public bool SmoothVerts
    {
        get
        {
            return _smoothVerts;
        }
        set
        {
            _smoothVerts = value;
        }
    }
}

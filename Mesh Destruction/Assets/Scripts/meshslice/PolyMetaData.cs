using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

namespace MeshSlicing
{
    public class PolyMetaData
    {
        private List<Vector3> verts3D = new List<Vector3>();
        private List<Vector2> verts2d = new List<Vector2>();
        private Vector3 normal;
        private List<Vector2> uv = new List<Vector2>();
        private int parent, id;

        public int Parent { get => parent; }
        public int ID { get => id; }
        public List<Vector3> Verts3D { get => verts3D; }
        public List<Vector2> Verts2D { get => verts2d; }
        public List<Vector2> UV { get => uv; }
        public Vector3 Normal { get => normal; }

        public PolyMetaData(List<Vector3> _verts3d, List<Vector2> verts2d, List<Vector2> uv, Vector3 normal, int id, int parentIndex = -1)
        {
            this.verts3D = _verts3d;
            this.verts2d = verts2d;
            this.uv = uv;
            this.normal = normal;
            this.parent = parentIndex;
            this.id = id;
        }
        public PolyMetaData(PolyMetaData _poly, int parentIndex = -1)
        {
            this.verts3D = _poly.Verts3D;
            this.verts2d = _poly.Verts2D;
            this.uv = _poly.UV;
            this.normal = _poly.Normal;
            this.parent = parentIndex;
            this.id = _poly.ID;
        }

        public void SetParent(int parent)
        {
            this.parent = parent;
        }

        public bool PointInPoly(Vector2 point)
        {
            bool inside = false;

            int PolyLength = verts2d.Count;
            for (int i = 0; i < verts2d.Count; i++)
            {
                int j = (i + 1) % PolyLength;
                
                if ((verts2d[i].y < point.y && verts2d[j].y >= point.y) || (verts2d[i].y >= point.y && verts2d[j].y < point.y))
                {
                    Vector2 dir = verts2d[j] - verts2d[i];
                    Vector2 newPoint = point - verts2d[i];
                    float factor = (newPoint.y / dir.y);
                    if (newPoint.x < dir.x * factor && dir.y != 0)
                        inside = !inside;
                }
            }
            return inside;
        }
    }

    public class FinishedPoly
    {
        public List<Vector3> verts = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector2> uv = new List<Vector2>();
        public List<int> tris = new List<int>();
    }

    public class EarClippingPoly
    {
        PolyMetaData outerPoly;
        List<PolyMetaData> holes = new List<PolyMetaData>();
        Vector3 normal;

        public PolyMetaData Outline { get => outerPoly; }
        public List<PolyMetaData> Holes { get => holes; }
        public Vector3 Normal { get => normal; }

        public EarClippingPoly(PolyMetaData outsidePoly)
        {
            this.outerPoly = outsidePoly;
        }

        public void AddHole(PolyMetaData hole)
        {
            this.holes.Add(hole);
        }
    }
}


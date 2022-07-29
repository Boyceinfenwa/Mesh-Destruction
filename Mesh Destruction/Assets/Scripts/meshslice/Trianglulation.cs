
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSlicing

{
    
    public static class Triangulation
    {
       
        public static FinishedPoly Triangulate(PolyCreator polygon, int verticesCount)
        {
            FinishedPoly finishedpoly = new FinishedPoly();

            for (int i = 0; i < polygon.Outlines.Count; i++)
            {
                EarClippingPoly earClipPoly = new EarClippingPoly(new PolyMetaData(polygon.Outlines[i]));
                for (int j = 0; j < polygon.Holes.Count; j++)
                {
                    if (polygon.Holes[j].Parent == polygon.Outlines[i].ID)
                    {
                        earClipPoly.AddHole(new PolyMetaData(polygon.Holes[j]));
                    }
                }
                
                int triangleOffsetIndex = verticesCount + finishedpoly.verts.Count;

                EarclIpping earClipper = new EarclIpping(earClipPoly, triangleOffsetIndex);
                earClipper.ClipEars();

                finishedpoly.tris.AddRange(earClipper.Tris);
                finishedpoly.uv.AddRange(earClipper.UV);
                finishedpoly.verts.AddRange(earClipper.Verts);
                for (int n = 0; n < earClipper.Verts.Count; n++)
                {
                    finishedpoly.normals.Add(polygon.Normal);
                }
            }
            return finishedpoly;
        }
    }
}


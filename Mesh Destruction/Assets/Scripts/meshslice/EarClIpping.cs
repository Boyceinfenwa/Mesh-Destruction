using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MeshSlicing
{
    public class EarclIpping
    {
        List<Vector2> verts2D;
        List<Vector3> verts3D;
        List<Vector2> uvs;
        List<int> convexVerts = new List<int>();
        List<int> reflexVerts = new List<int>();
        List<int> earVerts = new List<int>();
        List<int> vertsToEmpty = new List<int>();
        bool positiveSide;
        List<int> tris = new List<int>();
        private int triOffset; 

        public List<Vector3> Verts { get => verts3D; }
        public List<Vector2> UV { get => uvs; }
        public List<int> Tris { get => tris; }

        public EarclIpping(EarClippingPoly poly, int triOffset = 0)
        {
            this.triOffset = triOffset;

            if (poly.Holes.Count > 0)
            {
                verts3D = poly.Outline.Verts3D;
                verts2D = poly.Outline.Verts2D;
                uvs = poly.Outline.UV;
                ConnectHoles(poly.Holes);
            }
            else
            {
                verts3D = poly.Outline.Verts3D;
                verts2D = poly.Outline.Verts2D;
                uvs = poly.Outline.UV;
            }

            FillInitialTriangulationData();
        }

        private void FillInitialTriangulationData()
        {
            int vertsCount = verts2D.Count;

           
            for (int i = 0; i < vertsCount; i++)
            {
                vertsToEmpty.Add(i);
            }

            for (int i = 0; i < vertsCount; i++)
            {
                if (IsConvexVtx(verts2D[(vertsCount + i - 1) % vertsCount], verts2D[i], verts2D[(i + 1) % vertsCount]))
                {
                    convexVerts.Add(i);
                }
                else
                {
                    reflexVerts.Add(i);
                }
            }
          
            for (int i = 0; i < convexVerts.Count; i++)
            {
                int vM = convexVerts[i];
                int vL = IdxxBerfore(vM);
                int vR = IdxAfter(vM);

                if (IsEar(verts2D[vL], verts2D[vM], verts2D[vR]))
                {
                    earVerts.Add(convexVerts[i]);
                }
                else continue;
            }
        }

        public void ClipEars()
        {
            while (vertsToEmpty.Count > 3)
            {

                if (earVerts.Count == 0)
                {
                    Debug.LogError("NOPE!!! no EarVerts found here yep thats 0 also there are still " + vertsToEmpty.Count + " VertsToEmpty");
                }

              
                int vM = earVerts.First();
                int vL = IdxxBerfore(vM);
                int vR = IdxAfter(vM);
         
                tris.AddRange(new List<int>() { vM + triOffset, vL + triOffset, vR + triOffset });
              
                earVerts.Remove(vM);
                convexVerts.Remove(vM);
                vertsToEmpty.Remove(vM);

                int vT;
                vT = IdxxBerfore(vL);
                UpdateVerts(vT, vL, vR);
  
                vT = IdxAfter(vR);
                UpdateVerts(vL, vR, vT);

            }
 
            if (vertsToEmpty.Count == 3)
            {
                int vL = vertsToEmpty[0]; ;
                int vM = vertsToEmpty[1];
                int vR = vertsToEmpty[2];

                
                tris.AddRange(new List<int>() { vM + triOffset, vL + triOffset, vR + triOffset });
               
                earVerts.Remove(vM);
                vertsToEmpty.Clear();
            }
        }

        private bool IsConvexVtx(Vector2 vL, Vector2 vM, Vector2 vR)
        {
            if ((vM - vL).normalized == (vR - vM).normalized) return true;
            Plane plane = new Plane(vL, vM, Vector3.back);
            return plane.GetSide(vR);
        }
  
        private bool IsEar(Vector2 vL, Vector2 vM, Vector2 vR)
        {
            for (int i = 0; i < reflexVerts.Count; i++)
            {
                Vector2 P = verts2D[reflexVerts[i]];

                if (P == vL || P == vR) continue;

                double s1 = vR.y - vM.y;
                double s2 = vR.x - vM.x;
                double s3 = vL.y - vM.y;
                double s4 = P.y - vM.y;

                double w1 = (vM.x * s1 + s4 * s2 - P.x * s1) / (s3 * s2 - (vL.x - vM.x) * s1);
                double w2 = (s4 - w1 * s3) / s1;
                if (w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1) return false;
                else continue;
            }
            return true;
        }

        private void ConnectHoles(List<PolyMetaData> holes)
        {
            List<PolyMetaData> holesToEmpty = new List<PolyMetaData>(holes);

            while (holesToEmpty.Count > 0)
            {
                Vector2 maxPoint2D = Vector2.zero;
                Vector3 maxPoint3D = Vector3.zero;
                float maxX = holesToEmpty[0].Verts2D[0].x;
                int vertexIndex = 0, holeIndex = 0;
                for (int i = 0; i < holesToEmpty.Count; i++)
                {
                    for (int j = 0; j < holesToEmpty[i].Verts2D.Count; j++)
                    {
                        float tempX = holesToEmpty[i].Verts2D[j].x;
                        if (tempX >= maxX)
                        {
                            maxPoint3D = holesToEmpty[i].Verts3D[j];
                            maxPoint2D = holesToEmpty[i].Verts2D[j];
                            maxX = tempX;
                            vertexIndex = j;
                            holeIndex = i;
                        }
                    }
                }
                
                int connectingVtxIdx = FindVertsToConnect(maxPoint2D);
                List<Vector3> newVerts3D = new List<Vector3>();
                List<Vector2> newVerts2D = new List<Vector2>();
                List<Vector2> connectionUV = new List<Vector2>();
                
                int holeSize = holesToEmpty[holeIndex].Verts2D.Count;
                for (int i = 0; i < holeSize; i++)
                {
                    newVerts3D.Add(holesToEmpty[holeIndex].Verts3D[(vertexIndex + i) % holeSize]);
                    newVerts2D.Add(holesToEmpty[holeIndex].Verts2D[(vertexIndex + i) % holeSize]);
                    connectionUV.Add(holesToEmpty[holeIndex].UV[(vertexIndex + i) % holeSize]);
                }
                newVerts3D.Add(maxPoint3D);  
                newVerts3D.Add(verts3D[connectingVtxIdx]);   
                newVerts2D.Add(maxPoint2D);  
                newVerts2D.Add(verts2D[connectingVtxIdx]);   
                connectionUV.Add(holesToEmpty[holeIndex].UV[vertexIndex]);  
                connectionUV.Add(uvs[connectingVtxIdx]);   
                
                verts3D.InsertRange(connectingVtxIdx + 1, newVerts3D);   
                verts2D.InsertRange(connectingVtxIdx + 1, newVerts2D);   
                uvs.InsertRange(connectingVtxIdx + 1, connectionUV);   

                holesToEmpty.RemoveAt(holeIndex);
            }

        }
        
        private int FindVertsToConnect(Vector2 hV)
        {
            List<Edge2D> possibleEdges = new List<Edge2D>();
            Vector2 possiblePoint = Vector2.zero;
            int possibleIdxx = 0;
            int vertsCount = verts2D.Count;

            bool connectingEdgeFound = false;
            
            for (int i = 0; i < vertsCount; i++)
            {        
                Vector2 v1 = verts2D[i];
                Vector2 v2 = verts2D[(i + 1) % vertsCount];
                if (v1.x > hV.x || v2.x > hV.x)
                {
                    possibleEdges.Add(new Edge2D(v1, v2, i, (i + 1) % vertsCount));
                }
            }

           
            for (int i = 0; i < possibleEdges.Count; i++)
            {
                
                Vector2 sP = possibleEdges[i].StartPos;
                Vector2 eP = possibleEdges[i].EndPos;
                
                if ((sP.y < hV.y && eP.y >= hV.y) || (sP.y >= hV.y && eP.y < hV.y))
                {
                    bool sPb = Mathf.Max(sP.x, eP.x) == sP.x;
                    possiblePoint = sPb ? sP : eP;
                    possibleIdxx = sPb ? possibleEdges[i].StartIdx : possibleEdges[i].EndIdx;
                    possibleEdges.RemoveAt(i);
                    break;
                }
                else continue;
            }
            
            while (!connectingEdgeFound)
            {
                
                Edge2D cE = new Edge2D(hV, possiblePoint);

                
                for (int i = 0; i < possibleEdges.Count; i++)
                {
                    Vector2 sP = possibleEdges[i].StartPos;
                    Vector2 eP = possibleEdges[i].EndPos;

                   

                    if (sP != cE.EndPos && eP != cE.EndPos && cE.Intersect(possibleEdges[i]))
                    {
                       
                        connectingEdgeFound = false;
                        bool sPb = Mathf.Max(sP.x, eP.x) == sP.x;
                        possiblePoint = sPb ? sP : eP;
                        possibleIdxx = sPb ? possibleEdges[i].StartIdx : possibleEdges[i].EndIdx;
                        possibleEdges.RemoveAt(i);
                        break;
                    }
                    else
                    {
                        
                        connectingEdgeFound = true;
                        continue;
                    }
                }
            }
            return possibleIdxx;
        }

       
        private int IdxxBerfore(int vM)
        {
            int index = vertsToEmpty.IndexOf(vM);
            int count = vertsToEmpty.Count;
            return vertsToEmpty[(count + index - 1) % count];
        }
      
        private int IdxAfter(int vM)
        {
            int index = vertsToEmpty.IndexOf(vM);
            int count = vertsToEmpty.Count;
            return vertsToEmpty[(count + index + 1) % count];
        }

     
        
        private void UpdateVerts(int iL, int iM, int iR)
        {
            Vector2 vL = verts2D[iL];
            Vector2 vM = verts2D[iM];
            Vector2 vR = verts2D[iR];

            if (reflexVerts.Contains(iM))
            {
               
                if (IsConvexVtx(vL, vM, vR))
                {
                    convexVerts.Add(iM);
                    reflexVerts.Remove(iM);
                    
                    if (IsEar(vL, vM, vR))
                        earVerts.Insert(0, iM);
                }
            }
            else
            {
                bool wasEar = earVerts.Contains(iM);
                if (IsEar(vL, vM, vR))
                {
                    if (!wasEar)
                        earVerts.Insert(0, iM);
                }
                else
                {
                    if (wasEar)
                        earVerts.Remove(iM);
                }
            }
        }
    }
}


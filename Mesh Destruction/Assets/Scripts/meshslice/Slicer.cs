using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using MeshSlicing;


public class Slicer : MonoBehaviour
{
    public float rayLength = 0, castAngle = 0, lines = 0;
    public LayerMask sliceLayer;
    public Material sliceMat;
    public float force = 0;

    public bool concave = false;
    public bool correctData = true;
    public bool drawRays = false;
    private bool setEdge = false;

    private Vector3 edgeVtx = Vector3.zero;
    private Vector2 edgeUV = Vector2.zero;
    private Plane edgePlane = new Plane();

    void Update()
    {
     
        if (drawRays)
            DrawRays();
    }

    private void OnTriggerExit(Collider other)
    {
        SliceMesh();
    }

    public void SliceMesh()
    {
        
        Ray ray = new Ray(transform.position, transform.forward);
        List<RaycastHit> _hits = new List<RaycastHit>();

        float halfAngle = castAngle;
        halfAngle /= 2;

        float minAngle = -halfAngle;
        float increment = (castAngle / (lines - 1));
        for (float i = minAngle; i <= halfAngle; i += increment)
        {
            Quaternion rayDir = Quaternion.AngleAxis(i, transform.up);
            Ray _ray = new Ray(transform.position, (rayDir * transform.forward).normalized);
            _hits.AddRange(Physics.RaycastAll(_ray, rayLength, sliceLayer).ToList());
        }
        
        RaycastHit[] hits = _hits.GroupBy(g => g.transform.gameObject).Select(f => f.First()).ToArray();

       
        for (int x = 0; x < hits.Length; x++)  
        {
            Mesh hitMesh = hits[x].transform.GetComponent<MeshFilter>().mesh;
            List<CutMesh> slices = new List<CutMesh>();

            CutMesh unCutMesh = new CutMesh()
            {
                uv = hitMesh.uv,
                verts = hitMesh.vertices,
                normals = hitMesh.normals,
                tris = new int[hitMesh.subMeshCount][],
            };
            for (int i = 0; i < hitMesh.subMeshCount; i++)
            {
                unCutMesh.tris[i] = hitMesh.GetTriangles(i);
            }

            Plane plane = new Plane(hits[x].transform.InverseTransformDirection(transform.up), hits[x].transform.InverseTransformPoint(hits[x].point));

            slices.AddRange(GenMesh(unCutMesh, plane, true, hits[x].transform));

            for (int s = 0; s < slices.Count; s++)
            {
                slices[s].MakeGameObject(hits[x].transform.gameObject, sliceMat, (s + 1).ToString(), transform.forward * force);
            }
            slices.Clear();
            Destroy(hits[x].transform.gameObject);
        }
    }

    private List<CutMesh> GenMesh(CutMesh orgin, Plane plane, bool posSide, Transform objTrans)
    {
        CutMesh slicePos = new CutMesh();
        CutMesh sliceNeg = new CutMesh();
        
       
        PolyCreator polyPos = new PolyCreator(objTrans, plane.normal * -1);
        PolyCreator polyNeg = new PolyCreator(objTrans, plane.normal);
        bool matPositiveAdded = false;
        bool matNegativeAdded = false;

        
        for (int submesh = 0; submesh < orgin.tris.Length; submesh++)
        {
            int[] orginTris = orgin.tris[submesh];
            setEdge = false;

            
            for (int t = 0; t < orginTris.Length; t += 3)
            {
                int t1 = t, t2 = t + 1, t3 = t + 2;

                bool sA = plane.GetSide(orgin.verts[orginTris[t1]]) == posSide;
                bool sB = plane.GetSide(orgin.verts[orginTris[t2]]) == posSide;
                bool sC = plane.GetSide(orgin.verts[orginTris[t3]]) == posSide;

                int sCount = (sA ? 1 : 0) +
                                (sB ? 1 : 0) +
                                (sC ? 1 : 0);
               
                if (sCount == 0)
                {
                    sliceNeg.AddTri(submesh, orgin.verts[orginTris[t1]], orgin.verts[orginTris[t2]], orgin.verts[orginTris[t3]],
                                      orgin.normals[orginTris[t1]], orgin.normals[orginTris[t2]], orgin.normals[orginTris[t3]],
                                      orgin.uv[orginTris[t1]], orgin.uv[orginTris[t2]], orgin.uv[orginTris[t3]]);
                    if (!matNegativeAdded)
                    {
                        matNegativeAdded = true;
                        sliceNeg.matIdx.Add(submesh);
                    }

                    continue;
                }
                else if (sCount == 3)
                {
                    slicePos.AddTri(submesh, orgin.verts[orginTris[t1]], orgin.verts[orginTris[t2]], orgin.verts[orginTris[t3]],
                                      orgin.normals[orginTris[t1]], orgin.normals[orginTris[t2]], orgin.normals[orginTris[t3]],
                                      orgin.uv[orginTris[t1]], orgin.uv[orginTris[t2]], orgin.uv[orginTris[t3]]);
                    if (!matPositiveAdded)
                    {
                        matPositiveAdded = true;
                        slicePos.matIdx.Add(submesh);
                    }

                    continue;
                }

                if (!matNegativeAdded)
                {
                    matNegativeAdded = true;
                    sliceNeg.matIdx.Add(submesh);
                }
                if (!matPositiveAdded)
                {
                    matPositiveAdded = true;
                    slicePos.matIdx.Add(submesh);
                }

                int soleIdx = sB == sC ? 0 : sA == sC ? 1 : 2;
                int idxB = t + ((soleIdx + 1) % 3), indexC = t + ((soleIdx + 2) % 3);
                soleIdx += t;

                Vector3 soleVertex = orgin.verts[orginTris[soleIdx]],
                        vertexB = orgin.verts[orginTris[idxB]],                 
                        vertexC = orgin.verts[orginTris[indexC]];                 

                Vector3 soleNormal = orgin.normals[orginTris[soleIdx]],
                        normalB = orgin.normals[orginTris[idxB]],
                        normalC = orgin.normals[orginTris[indexC]];

                Vector2 soleUv = orgin.uv[orginTris[soleIdx]],
                        uvB = orgin.uv[orginTris[idxB]],
                        uvC = orgin.uv[orginTris[indexC]];

                float lerpB;
                float lerpC;

                Vector3 newVertexB = PointOnPlane(plane, soleVertex, vertexB, out lerpB),     
                        newVertexC = PointOnPlane(plane, soleVertex, vertexC, out lerpC);     
                Vector3 newNormalB = Vector3.Lerp(soleNormal, normalB, lerpB),                
                        newNormalC = Vector3.Lerp(soleNormal, normalC, lerpC);
                Vector2 newUvB = Vector2.Lerp(soleUv, uvB, lerpB),
                        newUvC = Vector2.Lerp(soleUv, uvC, lerpC);

                if (!concave)
                {
                    AddSliceTri(submesh, slicePos, newVertexB, newVertexC,
                                         plane.normal * -1,
                                         newUvB, newUvC);
                    AddSliceTri(submesh, sliceNeg, newVertexB, newVertexC,
                                         plane.normal,
                                         newUvB, newUvC);
                }

                if (sCount == 1)
                {
                    slicePos.AddTri(submesh, soleVertex, newVertexB, newVertexC, soleNormal, newNormalB, newNormalC, soleUv, newUvB, newUvC);
                    sliceNeg.AddTri(submesh, newVertexB, vertexB, vertexC, newNormalB, normalB, normalC, newUvB, uvB, uvC);
                    sliceNeg.AddTri(submesh, newVertexB, vertexC, newVertexC, newNormalB, normalC, newNormalC, newUvB, uvC, newUvC);

                    if (concave)
                    {
                        edge edgePositive = new edge(newVertexB, newVertexC, plane.normal * -1, newUvB, newUvC);
                        polyPos.AddEdge(edgePositive);
                        
                        edge edgeNegative = new edge(newVertexC, newVertexB, plane.normal, newUvC, newUvB);
                        polyNeg.AddEdge(edgeNegative);
                    }
                    continue;
                }
                else if (sCount == 2)
                {
                   
                    slicePos.AddTri(submesh, newVertexB, vertexB, vertexC, newNormalB, normalB, normalC, newUvB, uvB, uvC);
                    slicePos.AddTri(submesh, newVertexB, vertexC, newVertexC, newNormalB, normalC, newNormalC, newUvB, uvC, newUvC);
                    
                    sliceNeg.AddTri(submesh, soleVertex, newVertexB, newVertexC, soleNormal, newNormalB, newNormalC, soleUv, newUvB, newUvC);
                    if (concave)
                    {
                        
                        edge edgePositive = new edge(newVertexC, newVertexB, plane.normal * -1, newUvC, newUvB);
                        polyPos.AddEdge(edgePositive);
                        
                        edge edgeNegative = new edge(newVertexB, newVertexC, plane.normal, newUvB, newUvC);
                        polyNeg.AddEdge(edgeNegative);
                    }
                    continue;
                }
            }
        }

        if (concave)
        {
            polyPos.JoinEdges();
            polyNeg.JoinEdges();

            FinishedPoly polyPositiveFinished = Triangulation.Triangulate(polyPos, slicePos.VertexCount);
            FinishedPoly polyNegativeFinished = Triangulation.Triangulate(polyNeg, sliceNeg.VertexCount);

            slicePos.AddPoly(polyPositiveFinished);
            sliceNeg.AddPoly(polyNegativeFinished);
        }

        slicePos.FillArray(correctData);
        sliceNeg.FillArray(correctData);

        return new List<CutMesh>() { slicePos, sliceNeg };
    }

    private void AddSliceTri(int subMesh, CutMesh slice, Vector3 vtx1, Vector3 vtx2, Vector3 normal, Vector2 uv1, Vector2 uv2)
    {
        if (!setEdge)
        {
            setEdge = true;
            edgeVtx = vtx1;
            edgeUV = uv1;
        }
        else
        {
            edgePlane.Set3Points(edgeVtx, vtx1, vtx2);

            slice.AddTri(subMesh,
                                edgeVtx,
                                edgePlane.GetSide(edgeVtx + normal) ? vtx1 : vtx2,
                                edgePlane.GetSide(edgeVtx + normal) ? vtx2 : vtx1,
                                normal,
                                normal,
                                normal,
                                edgeUV,
                                uv1,
                                uv2);
        }
    }

    private edge AddEdge(bool positiveSide, Vector3 vtx1, Vector3 vtx2, Vector3 normal, Vector2 uv1, Vector2 uv2)
    {
        return new edge(positiveSide ? vtx1 : vtx2,
                        positiveSide ? vtx2 : vtx1,
                        normal,
                        positiveSide ? uv1 : uv2,
                        positiveSide ? uv2 : uv1);
    }

    
    private Vector3 PointOnPlane(Plane plane, Vector3 vtx1, Vector3 vtx2, out float lerp)
    {
        Vector3 dir = (vtx2 - vtx1);
        Ray ray = new Ray(vtx1, dir.normalized);
        float dist;
        plane.Raycast(ray, out dist);
        Vector3 vtx3 = vtx1 + (dir.normalized * dist);
        lerp = dist / dir.magnitude;
        return vtx3;
    }

    private void DrawRays()
    {
        float halfAngle = castAngle;
        halfAngle /= 2;
        float minAngle = -halfAngle;
        float increment = (castAngle / (lines - 1));

        for (float i = minAngle; i <= halfAngle; i += increment)
        {
            Quaternion rayDir = Quaternion.AngleAxis(i, transform.up);
            Debug.DrawRay(transform.position, (rayDir * transform.forward).normalized * rayLength, Color.green);
        }
    }
}

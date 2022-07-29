using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshSlicing
{
    public class PolyCreator
    {

        private Vector3 polyNormal;
        private Transform parentTransform;
        public Vector3 Normal { get => polyNormal; }

       
        private List<edge> rawEdges = new List<edge>();    
        private List<List<edge>> orderedEdges = new List<List<edge>>();
        private List<PolyMetaData> indiPolys = new List<PolyMetaData>();
        private List<PolyMetaData> outlines = new List<PolyMetaData>();
        private List<PolyMetaData> holes = new List<PolyMetaData>();

        public List<PolyMetaData> Outlines { get => outlines; }
        public List<PolyMetaData> Holes { get => holes; }

        public PolyCreator(Transform transform, Vector3 normal)
        {
            this.polyNormal = normal;
            this.parentTransform = transform;
        }

        public void AddEdge(edge edge)
        {
            rawEdges.Add(edge);
        }

        public void JoinEdges()
        {
            int currentList = 0;
            List<edge> listToEmpy = new List<edge>(rawEdges);

            while (listToEmpy.Count > 0)
            {
                if (orderedEdges.Count - 1 < currentList)
                    orderedEdges.Add(new List<edge>());

                if (orderedEdges[currentList].Count == 0)
                {
                    orderedEdges[currentList].Add(listToEmpy[0]);
                    listToEmpy.RemoveAt(0);
                }

                edge nextEdge = orderedEdges[currentList][orderedEdges[currentList].Count - 1];

                while (true)
                {
                    bool NextedgeLocated = false;
                    for (int i = 0; i < listToEmpy.Count; i++)
                    {
                        if (nextEdge.EndPos == listToEmpy[i].StartPos)
                        {
                            NextedgeLocated = true;
                            orderedEdges[currentList].Add(listToEmpy[i]);
                            listToEmpy.RemoveAt(i);
                            nextEdge = orderedEdges[currentList].Last();
                        }
                    }
                    if (!NextedgeLocated)
                    {
                        currentList++;
                        break;
                    }
                }
            }
            MakePolys();
            FilterHolesAndOutlines();
        }

        private void MakePolys()
        {
            for (int i = 0; i < orderedEdges.Count; i++)
            {
                List<Vector3> tempVerts3D = new List<Vector3>();
                List<Vector2> tempVerts2D = new List<Vector2>();
                List<Vector2> tempUV = new List<Vector2>();

                for (int j = 0; j < orderedEdges[i].Count; j++)
                {
                    tempVerts3D.Add(orderedEdges[i][j].StartPos);
                    tempVerts2D.Add(Project3DTo2D(orderedEdges[i][j].StartPos, polyNormal));
                    tempUV.Add(orderedEdges[i][j].StartUV);
                }
                indiPolys.Add(new PolyMetaData(tempVerts3D, tempVerts2D, tempUV, polyNormal, i));
            }
            orderedEdges.Clear();
        }

        private void FilterHolesAndOutlines()
        {
            outlines = new List<PolyMetaData>(indiPolys);
            indiPolys.Clear();

            bool noChanges = false;
            bool noNewOutlines = false;
            bool  noNewHoles = false;

            while (!noChanges)
            {
                noChanges = true;
                if (!noNewOutlines && outlines.Count > 1)
                {
                    noNewOutlines = true;
                    List<PolyMetaData> tempOutlines = new List<PolyMetaData>();
                    List<PolyMetaData> tempHoles = new List<PolyMetaData>();

                    for (int i = 0; i < outlines.Count; i++)
                    {
                        bool isInside = false;
                        for (int j = 0; j < outlines.Count; j++)
                        {
                            if (i != j && outlines[i].Parent == outlines[j].Parent)
                            {
                                if (outlines[j].PointInPoly(outlines[i].Verts2D[UnityEngine.Random.Range(0, outlines[i].Verts2D.Count - 1)]))
                                {
                                    noChanges = false;
                                    noNewHoles = false;
                                    PolyMetaData tempPoly = new PolyMetaData(outlines[i], outlines[j].ID);
                                    tempHoles.Add(tempPoly);
                                    isInside = true;
                                    break;
                                }
                                else continue;
                            }
                        }

                        if (!isInside)
                        {
                            tempOutlines.Add(outlines[i]);
                        }
                    }
                    outlines = new List<PolyMetaData>(tempOutlines);
                    holes.AddRange(tempHoles);
                    tempHoles.Clear();
                    tempOutlines.Clear();
                }

                if (!noNewHoles && holes.Count > 1)
                {
                    noNewHoles = true;
                    List<PolyMetaData> tempOutlines = new List<PolyMetaData>();
                    List<PolyMetaData> tempHoles = new List<PolyMetaData>();
                    
                    for (int i = 0; i < holes.Count; i++)
                    {
                        bool isInside = false;
                        for (int j = 0; j < holes.Count; j++)
                        {
                            if (i != j && holes[i].Parent == holes[j].Parent)
                            {
                                if (holes[j].PointInPoly(holes[i].Verts2D[UnityEngine.Random.Range(0, holes[i].Verts2D.Count - 1)]))
                                {
                                    noChanges = false;
                                    noNewOutlines = false;
                                    PolyMetaData tempPolygon = new PolyMetaData(holes[i], holes[i].Parent);
                                    tempOutlines.Add(tempPolygon);
                                    isInside = true;
                                    break;
                                }
                                else continue;
                            }
                        }
                        if (!isInside)
                        {
                            tempHoles.Add(holes[i]);
                        }
                    }
                    holes = new List<PolyMetaData>(tempHoles);
                    outlines.AddRange(tempOutlines);
                    tempHoles.Clear();
                    tempOutlines.Clear();
                }
            }
            indiPolys.Clear();
        }

        private Vector2 Project3DTo2D(Vector3 point, Vector3 normal)
        {
            Vector3 u;
            if (Mathf.Abs(Vector3.Dot(parentTransform.forward, normal)) < 0.2f)
                u = Vector3.ProjectOnPlane(parentTransform.right, normal);
            else
                u = Vector3.ProjectOnPlane(parentTransform.forward, normal);

            Vector3 v = Vector3.Cross(u, normal).normalized;
            return new Vector2(Vector3.Dot(point, u), Vector3.Dot(point, v));
        }
    }
}
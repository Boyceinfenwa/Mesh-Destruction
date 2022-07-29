using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MeshSlicing;
using System.Linq;

namespace MeshSlicing
{
    public class CutMesh
    {
        List<Vector3> vertexList = new List<Vector3>();
        public int VertexCount { get => vertexList.Count; }
        List<Vector3> normalList = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();
        List<List<int>> triList = new List<List<int>>();
        public Vector3[] verts;
        public Vector3[] normals;
        public Vector2[] uv;
        public int[][] tris;

        private List<Vector3> correctedVerts = new List<Vector3>();
        private List<Vector3> correctedNormals = new List<Vector3>();
        private List<Vector2> correctedUvs = new List<Vector2>();
        private List<List<int>> correctedTris = new List<List<int>>();

        public PolyCreator poly;
        public List<int> matIdx = new List<int>();
        private int newSubmesh = -1;

        public CutMesh() { }

        public void AddTri(int submesh, Vector3 vtx1, Vector3 vtx2, Vector3 vtx3, Vector3 norm1, Vector3 norm2, Vector3 norm3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            int triListIdx = triList.Count - 1;
            if (triListIdx < submesh && submesh > newSubmesh)
            {
                triList.Add(new List<int>());
                newSubmesh = submesh;
                triListIdx += 1;
            }

            triList[triListIdx].Add(vertexList.Count);
            vertexList.Add(vtx1);

            triList[triListIdx].Add(vertexList.Count);
            vertexList.Add(vtx2);

            triList[triListIdx].Add(vertexList.Count);
            vertexList.Add(vtx3);

            normalList.Add(norm1);
            normalList.Add(norm2);
            normalList.Add(norm3);
            uvList.Add(uv1);

            uvList.Add(uv2);
            uvList.Add(uv3);
        }
        public void AddPoly(FinishedPoly _poly)
        {
            vertexList.AddRange(_poly.verts);
            normalList.AddRange(_poly.normals);
            uvList.AddRange(_poly.uv);
            triList.Add(new List<int>());
            triList[triList.Count - 1].AddRange(_poly.tris);
        }

        private void CorrectData()
        {
            for (int submesh = 0; submesh < triList.Count; submesh++)
            {
                if (correctedTris.Count - 1 < submesh)
                    correctedTris.Add(new List<int>());

                for (int t = 0; t < triList[submesh].Count; t++)
                {
                    bool corrected = false;
                    if (correctedVerts.Count == 0)
                    {
                        correctedTris[submesh].Add(correctedVerts.Count);
                        correctedVerts.Add(vertexList[triList[submesh][t]]);
                        correctedNormals.Add(normalList[triList[submesh][t]]);
                        correctedUvs.Add(uvList[triList[submesh][t]]);
                    }
                   
                    else
                    {
                        for (int i = 0; i < correctedVerts.Count; i++)
                        {
                            if (correctedVerts[i] == vertexList[triList[submesh][t]] && correctedNormals[i] == normalList[triList[submesh][t]])
                            {
                                correctedTris[submesh].Add(i);
                                corrected = true;
                                break;
                            }
                        }
              
                        if (!corrected)
                        {
                            correctedTris[submesh].Add(correctedVerts.Count);
                            correctedVerts.Add(vertexList[triList[submesh][t]]);
                            correctedNormals.Add(normalList[triList[submesh][t]]);
                            correctedUvs.Add(uvList[triList[submesh][t]]);
                        }
                    }
                }
            }
        }

        private void FillArray()
        {
            CorrectData();

            verts = correctedVerts.ToArray();
            normals = correctedNormals.ToArray();
            uv = correctedUvs.ToArray();
            tris = new int[correctedTris.Count][];
            for (int i = 0; i < correctedTris.Count; i++)
                tris[i] = correctedTris[i].ToArray();

            vertexList.Clear();
            normalList.Clear();
            uvList.Clear();
            triList.Clear();
            correctedVerts.Clear();
            correctedNormals.Clear();
            correctedUvs.Clear();
            correctedTris.Clear();
        }
        private void FillArrayUncorrected()
        {
            verts = vertexList.ToArray();
            normals = normalList.ToArray();
            uv = uvList.ToArray();
            tris = new int[triList.Count][];
            for (int i = 0; i < triList.Count; i++)
                tris[i] = triList[i].ToArray();
        }
        public void FillArray(bool corrected = true)
        {
            if (corrected) FillArray();
            else FillArrayUncorrected();
        }


        public void MakeGameObject(GameObject orgin, Material sliceMat, string name, Vector3 force)
        {
            GameObject slice;
            slice = new GameObject(orgin.name + name);
            slice.transform.position = orgin.transform.position;
            slice.transform.rotation = orgin.transform.rotation;
            slice.transform.localScale = orgin.transform.localScale;
            slice.layer = orgin.layer;

            Mesh mesh = new Mesh();
            mesh.name = orgin.name;
            mesh.vertices = verts;
            mesh.normals = mesh.normals;
            mesh.uv = uv;
            mesh.subMeshCount = tris.Length;
           
            for (int t = 0; t < tris.Length; t++)
                mesh.SetTriangles(tris[t], t, true);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            slice.AddComponent<MeshFilter>().mesh = mesh;

            if (sliceMat == null) sliceMat = orgin.GetComponent<MeshRenderer>().material;
            MeshRenderer renderer = slice.AddComponent<MeshRenderer>();
            Material[] newMats = new Material[mesh.subMeshCount];
            Material[] orginMats = orgin.GetComponent<MeshRenderer>().materials;
            for (int i = 0; i < newMats.Length; i++)
            {
                if (i == newMats.Length - 1 && newMats.Length > 1)
                    newMats[i] = sliceMat;
                else
                    newMats[i] = orginMats[i];
            }
            renderer.materials = newMats;

            slice.AddComponent<MeshCollider>().convex = true;

            Rigidbody rigid = slice.AddComponent<Rigidbody>();
            if (force != Vector3.zero) rigid.AddForce(force, ForceMode.Impulse);
            UnityEngine.GameObject.Destroy(slice,10);
            
        }

       
    }
}


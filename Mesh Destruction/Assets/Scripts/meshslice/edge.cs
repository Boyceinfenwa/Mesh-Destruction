using System.Collections;
using System.Collections.Generic;
using UnityEngine;





namespace MeshSlicing
{
    public class edge : MonoBehaviour
    {
        Vector3 startPos;
        Vector3 endPos;
        Vector3 normal;
        private Vector2 startUV, endUV;

        public Vector3 StartPos { get => startPos; }
        public Vector3 EndPos { get => endPos; }
        public Vector3 Normal { get => normal; }
        public Vector2 StartUV { get => startUV; }
        public Vector2 EndUV { get => endUV; }

        public edge(Vector3 _startPos, Vector3 _endPos, Vector3 normal, Vector2 startUV, Vector2 endUV)
        {
            this.startPos = _startPos;
            this.endPos = _endPos;
            this.normal = normal;
            this.startUV = startUV;
            this.endUV = endUV;
        }
    }

    public class Edge2D
    {
        private Vector2 startPos;
        private Vector2 endPos;
        private int startIdx;
        private int endIdx;
        public Vector2 StartPos { get => startPos; }
        public Vector2 EndPos { get => endPos; }
        public int StartIdx { get => startIdx; }
        public int EndIdx { get => endIdx; }

        public Edge2D(Vector2 _startPos, Vector2 _endPos, int _startIdx, int _endIdx)
        {
            this.startPos = _startPos;
            this.endPos = _endPos;
            this.startIdx = _startIdx;
            this.endIdx = _endIdx;
        }
        public Edge2D(Vector2 _startPos, Vector2 _endPos)
        {
            this.startPos = _startPos;
            this.endPos = _endPos;
        }

        public bool Intersect(Edge2D other)
        {
            Vector2 p1 = startPos;
            Vector2 p2 = endPos;
            Vector2 p3 = other.startPos;
            Vector2 p4 = other.endPos;

            var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

            if (d == 0.0f)
            {
                return false;
            }

            var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            {
                return false;
            }
            return true;
        }
    }
}





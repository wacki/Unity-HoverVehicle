using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{
    public class HeightProbe : MonoBehaviour
    {
        public ProbeTriangle[] probeTriangles;
        public float rayLength = 5;

        public bool useCustomProbeDirection = false;
        public Vector3 propeDirection;

        public Vector3 currentNormal { get { return _currentNormal; } }
        private Vector3 _currentNormal;

        private bool _isGrounded;
        public bool isGrounded { get { return _isGrounded; } }

        [System.Serializable]
        public class ProbeTriangle
        {
            public Transform[] points;
        }
        

        private void Update()
        {
            _isGrounded = false;
            _currentNormal = Vector3.zero;
            int triangleCount = 0;
            for (int i = 0; i < probeTriangles.Length; i++)
            {
                var triangle = probeTriangles[i];
                Vector3[] hitPoints = new Vector3[3];
                bool shouldSkip = false;

                for (int j = 0; j < triangle.points.Length; j++)
                {
                    var curPoint = triangle.points[j];
                    Vector3 raycastDir = -curPoint.up;
                    if (useCustomProbeDirection)
                        raycastDir = propeDirection;
                    RaycastHit hitInfo;
                    if (!Physics.Raycast(curPoint.position, raycastDir, out hitInfo, rayLength))
                    {
                        shouldSkip = true;
                        break;
                    }

                    Debug.DrawLine(curPoint.position, hitInfo.point, Color.yellow);

                    hitPoints[j] = hitInfo.point;
                }

                if(!shouldSkip)
                {
                    // calculate normal
                    var AB = hitPoints[1] - hitPoints[0];
                    var AC = hitPoints[2] - hitPoints[0];
                    var normal = Vector3.Cross(AC, AB);
                    normal.Normalize();

                    Debug.DrawLine(hitPoints[0], hitPoints[1], Color.black);
                    Debug.DrawLine(hitPoints[0], hitPoints[2], Color.black);
                    Debug.DrawLine(hitPoints[1], hitPoints[2], Color.black);

                    // debug draw normal
                    var pos = hitPoints[0] + hitPoints[1] + hitPoints[2];
                    pos /= 3;
                    Debug.DrawLine(pos, pos + normal, Color.blue);

                    _currentNormal += normal;
                    _isGrounded = true;

                    triangleCount++;
                }
            }

            _currentNormal /= triangleCount;
            _currentNormal.Normalize();
            


            Debug.DrawLine(transform.position, transform.position + _currentNormal, new Color(0, 0.5f, 1, 1));
        }
        
        
    }

}
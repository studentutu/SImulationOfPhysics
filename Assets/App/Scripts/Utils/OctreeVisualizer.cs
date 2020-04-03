using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Utils
{
    public class OctreeVisualizer : MonoBehaviour
    {
        [SerializeField] private Transform objectToFind = default;

        public bool UpdateGizmo = false;

        public float size = 5f;
        public int depth = 2;

        private Octree<int> tree;

        private void OnDrawGizmos() // OnValidate
        {
            tree = new Octree<int>(this.transform.position, size, depth);
            DrawNode(tree.GetRoot());

            if (!UpdateGizmo)
                return;
            if (objectToFind != null)
            {
                DrawFoundNode(tree.FindNode(objectToFind.position));

            }
        }

        private void DrawNode(Octree<int>.OctreeNode<int> node)
        {
            if (node.IsLeaf())
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.blue;
                foreach (var subnode in node.subnodes)
                {
                    DrawNode(subnode);
                }
            }
            Gizmos.DrawWireCube(node.position, Vector3.one * node.size);
        }

        private void DrawFoundNode(Octree<int>.OctreeNode<int> node)
        {
            Gizmos.color = Color.red;
            if (node.size >= 0)
            {
                Gizmos.DrawCube(node.position, Vector3.one * node.size);
                Gizmos.DrawCube(node.position, Vector3.one * node.size);
                Gizmos.DrawCube(node.position, Vector3.one * node.size);
            }
        }
    }


    public struct Octree<T>
    {
        private OctreeNode<T> node;
        private int depth;

        public OctreeNode<T> GetRoot()
        {
            return node;
        }

        public Octree(Vector3 position, float size, int _depth)
        {
            node = new OctreeNode<T>(position, size);
            this.depth = _depth;
            node.Subdivide(this.depth);
        }

        public OctreeNode<T> FindNode(Vector3 lookUpPosition)
        {
            return node.findNode(lookUpPosition, OctreeNode<T>.GetIndexOfPos(lookUpPosition, node), this.depth);
        }

        // xx0 - Front
        // xx1 - Back
        // x0x - Left
        // x1x - Right
        // 0xx - Bottom
        // 1xx - Top
        // Above 100 4
        // Right 010 2
        // Back  001 1
        private enum OctreeIndex
        {
            BottomLeftFront = 0,    // 000,  // 0
            BottomRightFront = 2,   // 010,  // 2
            BottomRightBack = 3,    // 011,  // 3
            BottomLeftBack = 1,     // 001,  // 1
            TopLeftFront = 4,       // 100,  // 4
            TopRightFront = 6,      // 110,  // 6
            TopRightBack = 7,       // 111,  // 7
            TopLeftBack = 5         // 101   // 5
        }

        public struct OctreeNode<W>
        {
            public OctreeNode<W>[] subnodes;
            public Vector3 position;
            public float size;

            public OctreeNode(Vector3 pos, float _size)
            {
                position = pos;
                size = _size;
                subnodes = null;
            }

            public static int GetIndexOfPos(Vector3 lookUpPosition, OctreeNode<W> node)
            {
                int index = 0;
                index |= lookUpPosition.y > (node.position.y) ? 4 : 0;  // Is Above?
                index |= lookUpPosition.x > (node.position.x) ? 2 : 0;  // Is Right?
                index |= lookUpPosition.z > (node.position.z) ? 1 : 0;  // Is Behind?

                // xx1 - Back
                // x1x - Right
                // 1xx - Top
                return index;
            }

            public void Subdivide(int depth)
            {
                subnodes = new OctreeNode<W>[8];
                Vector3 newpos;
                float newSize = size * 0.25f;
                for (int i = 0; i < subnodes.Length; i++)
                {
                    newpos = position;
                    if ((i & 4) == 4) //  100
                    {
                        // Top
                        newpos.y += newSize;
                    }
                    else
                    {
                        // Bottom
                        newpos.y -= newSize;
                    }

                    if ((i & 2) == 2) //  010
                    {
                        // Right
                        newpos.x += newSize;
                    }
                    else
                    {
                        // Left
                        newpos.x -= newSize;
                    }

                    if ((i & 1) == 1)  //  001
                    {
                        // Back
                        newpos.z += newSize;
                    }
                    else
                    {
                        // Front
                        newpos.z -= newSize;
                    }

                    subnodes[i] = new OctreeNode<W>(newpos, size * 0.5f);
                    if (depth > 0)
                    {
                        subnodes[i].Subdivide(depth - 1);
                    }
                }
            }

            public bool IsLeaf()
            {
                return subnodes == null;
            }

            public OctreeNode<W> findNode(Vector3 lookUpPosition, int lookUpDirection, int depth)
            {

                if (IsLeaf())
                    return IsInsideBoundingBox(lookUpPosition) ? this : new OctreeNode<W>(Vector3.zero, -1);

                if (depth < 0)
                    return new OctreeNode<W>(Vector3.zero, -1);

                return subnodes[lookUpDirection].findNode(lookUpPosition, GetIndexOfPos(lookUpPosition, subnodes[lookUpDirection]), depth - 1);
            }

            private bool IsInsideBoundingBox(Vector3 lookUpPos)
            {

                int index = 1;
                // inside - is Not A
                float halfSize = this.size * 0.5f;
                index &= lookUpPos.y > (this.position.y - halfSize) ? 1 : 0;  // Is Above?
                index &= lookUpPos.x > (this.position.x - halfSize) ? 1 : 0;  // Is Right?
                index &= lookUpPos.z > (this.position.z - halfSize) ? 1 : 0;  // Is Behind?

                index &= lookUpPos.y < (this.position.y + halfSize) ? 1 : 0;  // Is Above?
                index &= lookUpPos.x < (this.position.x + halfSize) ? 1 : 0;  // Is Right?
                index &= lookUpPos.z < (this.position.z + halfSize) ? 1 : 0;  // Is Behind?

                return index == 1;
            }
        }
    }
}
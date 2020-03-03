using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Gravel3D.Vectors;
using Gravel3D.Transforms;
using Gravel3D.Lighting;

namespace Gravel3D.Renderers
{

    public class Camera
    {
        public float focalLength;   //How far ahead of the camera the screen is
        public Graphics graphics;   //The graphics object that everything should be drawn to
        public Vector2 screenScale; //The size of the screen

        public PointTransform transform = new PointTransform(Vector3.Zero, Vector3.Zero);

        private float clippingPlane = 0.01f;

        private Color[,] colorMap;
        private bool[,] drawnMap;

        private Bitmap image;

        public Camera(Form screen, float focalLength)
        {
            this.focalLength = focalLength;
            this.graphics = screen.CreateGraphics();
            this.screenScale = new Vector2(screen.Size.Width, -screen.Size.Width);
            //Y is negative here because on the screen, y goes from 0 at the top to MAX at the bottom.

            colorMap = new Color[screen.Size.Width + 1, screen.Size.Height + 1];
            drawnMap = new bool[screen.Size.Width + 1, screen.Size.Height + 1];

            image = new Bitmap(screen.Size.Width + 1, screen.Size.Height + 1);
        }

        public Vector2 Project(Vector3 v)
        {

            float newX = focalLength * v.x / v.z;
            float newY = focalLength * v.y / v.z;
            Vector2 tmp = new Vector2(newX, newY);
            tmp = tmp + new Vector2(1f / 2, -1f / 2);
            return tmp.Scale(screenScale);
        }

        public void Render(Transform[] contents, Graphics g)
        {

            List<Transform> sortedContents = new List<Transform>();
            foreach(Transform transform in contents)
            {
                sortedContents.Add(transform.ToSpace(this.transform));
            }
            sortedContents.Sort(Transform.SortByZ);

            List<ILightSource> lightSources = new List<ILightSource>();
            foreach(Transform transform in sortedContents)
            {
                if (transform is ILightSource) lightSources.Add(transform as ILightSource);
            }

            //sortedContents.Reverse();
            foreach(Transform transform in sortedContents)
            {
                foreach(Triangle3 triangle in transform.Triangles)
                {
                    foreach (Triangle3 clippedTriangle in ClipTriangle(triangle))
                    {
                        RenderTriangle(clippedTriangle, g, lightSources.ToArray());
                    }
                }
            }
        }

        public void RenderTriangle(Triangle3 triangle, Graphics g, ILightSource[] lightSources)
        {
            //Gets whether the triangle's normal is pointing away from the camera (positive z)
            //If it isn't, and the triangle isn't double-normalled, then doesn't do anything.
            if (Vector3.Dot(triangle.Normal, triangle.vertices[0]) < 0 && !triangle.doubleNormaled) return;

            PointF[] points = new PointF[3];
            for(int i = 0; i < 3; i++)
            {
                points[i] = Project(triangle.vertices[i]).ToPoint;
            }

            g.FillPolygon(new SolidBrush(triangle.GetColor(lightSources)), points);
            
        }

        private float ULerp(float x1, float y1, float x2, float y2, float x)
        {
            float m = (y1 - y2) / (x1 - x2);
            return m * (x - x1) + y1;
        }

        private float ULerp(Vector2 v1, Vector2 v2, float x)
        {
            return ULerp(v1.x, v1.y, v2.x, v2.y, x);
        }

        ////Given an array of Vector3s representing the vertices of a polygon,
        ////Returns another array of Vector3s represent the vertices of a 'clipped' polygon.
        //public Vector3[] clip(Vector3[] vertices)
        //{
        //    List<Vector3> newVertices = new List<Vector3>();
        //    for(int i = 0; i < vertices.Length; i++)
        //    {
        //        Vector3 vertex = vertices[i];
        //        //If the vertex is behind the clipping plane
        //        if (vertex.z < this.clippingPlane)
        //        {
        //            //If the vertex behind this one is in front of the clipping plane
        //            if (vertices[Form1.wrap(i - 1, vertices.Length)].z > this.clippingPlane) 
        //            {
        //                //Add the newVertices the intersection between the line and the clipping plane
        //                Vector3 vertex2 = vertices[Form1.wrap(i - 1, vertices.Length)];
        //                float newX = (vertex.x - vertex2.x) * (this.clippingPlane - vertex.z) / (vertex.z - vertex2.z) + vertex.x;
        //                float newY = (vertex.y - vertex2.y) * (this.clippingPlane - vertex.z) / (vertex.z - vertex2.z) + vertex.y;
        //                newVertices.Add(new Vector3(newX, newY, this.clippingPlane));
        //            }
        //            //If the vertex ahead of this one is in front of the clipping plane
        //            if (vertices[Form1.wrap(i + 1, vertices.Length)].z > this.clippingPlane)
        //            {
        //                if (!(vertices.Length <= 2))
        //                {

        //                    //Add the newVertices the intersection between the line and the clipping plane
        //                    Vector3 vertex2 = vertices[Form1.wrap(i + 1, vertices.Length)];
        //                    float newX = (vertex.x - vertex2.x) * (this.clippingPlane - vertex.z) / (vertex.z - vertex2.z) + vertex.x;
        //                    float newY = (vertex.y - vertex2.y) * (this.clippingPlane - vertex.z) / (vertex.z - vertex2.z) + vertex.y;
        //                    newVertices.Add(new Vector3(newX, newY, this.clippingPlane));
        //                }
        //            }
        //        }
        //        else
        //        {
        //            newVertices.Add(vertex);
        //        }
        //    }

        //    return newVertices.ToArray();
        //}

        private Triangle3[] ClipTriangle(Triangle3 triangle)
        {
            //Goes through and determines how many and which vertices are clipped
            int clippedCount = 0;
            bool[] isClipped = new bool[3];
            for(int i = 0; i < 3; i++)
            {
                if(triangle.vertices[i].z < clippingPlane)
                {
                    clippedCount++;
                    isClipped[i] = true;
                }
                else
                {
                    isClipped[i] = false;
                }
            }

            //If no vertices were clipped, return the original
            if(clippedCount == 0)
            {
                return new Triangle3[] { triangle };
            }
            
            //If all vertices were clipped, return nothing
            if(clippedCount == 3)
            {
                return new Triangle3[0];
            }

            //If two vertices were clipped
            else if(clippedCount == 2)
            {
                //Finds the unclipped vertex
                Vector3 unclippedVertex = null;
                foreach(Vector3 vertex in triangle.vertices)
                {
                    if(vertex.z > clippingPlane)
                    {
                        unclippedVertex = vertex;
                        break;
                    }
                }

                //Finds the new vertices
                Vector3[] newVertices = new Vector3[3];
                for(int i = 0; i < 3; i++)
                {
                    if (!isClipped[i])
                    {
                        newVertices[i] = triangle.vertices[i];
                        continue;
                    }

                    Vector3 clippingVertex = triangle.vertices[i];

                    float s = (clippingPlane - unclippedVertex.z) / (clippingVertex.z - unclippedVertex.z);
                    newVertices[i] = (clippingVertex - unclippedVertex).Scale(s) + unclippedVertex;
                }
                return new Triangle3[] { new Triangle3(newVertices, triangle) };
            }

            //If one vertex was clipped
            else
            {
                int clippedIndex = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (isClipped[i])
                    {
                        clippedIndex = i;
                        break;
                    }
                }

                Vector3 clippingVertex = triangle.vertices[clippedIndex];
                Vector3 vertexBefore = clippedIndex > 0 ? triangle.vertices[clippedIndex - 1] : triangle.vertices[2];
                Vector3 vertexAfter = clippedIndex < 2 ? triangle.vertices[clippedIndex + 1] : triangle.vertices[0];

                //Takes care of the vertexBefore - clippedVertex clipping
                float s1 = (clippingPlane - vertexBefore.z) / (clippingVertex.z - vertexBefore.z);
                Vector3 clippedVertex1 = (clippingVertex - vertexBefore).Scale(s1) + vertexBefore;

                //Takes care of the vertexAfter - clippedVertex clipping
                float s2 = (clippingPlane - vertexAfter.z) / (clippingVertex.z - vertexAfter.z);
                Vector3 clippedVertex2 = (clippingVertex - vertexAfter).Scale(s2) + vertexAfter;

                //Finally, returns the triangles
                return new Triangle3[] { new Triangle3(new Vector3[] { vertexBefore, clippedVertex1, clippedVertex2}, triangle),
                        new Triangle3(new Vector3[] { clippedVertex2, vertexAfter, vertexBefore}, triangle)};

            }

        }

        //public bool[] isClipped(Vector3[] vertices)
        //{
        //    bool[] returnArray = new bool[vertices.Length];
        //    for (int i = 0; i < vertices.Length; i++)
        //    {
        //        returnArray[i] = vertices[i].z < this.clippingPlane;
        //    }
        //    return returnArray;
        //}

        //public bool justOneClipped(Vector3[] vertices)
        //{
        //    int clippedCount = 0;
        //    foreach(bool b in isClipped(vertices)){
        //        if(b) clippedCount++;
        //    }
        //    return clippedCount == 1;
        //}

        //public int[] indicesClipped(Vector3[] vertices)
        //{
        //    List<int> returnData = new List<int>();
        //    bool[] clippedVertices = isClipped(vertices);
        //    for(int i = 0; i < clippedVertices.Length; i++)
        //    {
        //        if (clippedVertices[i]) returnData.Add(i);
        //    }
        //    return returnData.ToArray();
        //}

        private Bitmap ColorMapToBitMap()
        {
            Bitmap bitmap = new Bitmap(colorMap.GetLength(0), colorMap.GetLength(1));
            for(int i = 0; i < bitmap.Width; i++)
            {
                for(int j = 0; j < bitmap.Height; j++)
                {
                    bitmap.SetPixel(i, j, colorMap[i, j]);
                }
            }
            return bitmap;
        }
    }

    public class Triangle3
    {

        public Vector3[] vertices;

        public Color albedo;

        public bool doubleNormaled = false;

        public ITriangleShader shader;

        public Vector3 Normal
        {
            get
            {
                return Vector3.Cross(vertices[0] - vertices[1], vertices[0] - vertices[2]).Normalized;
            }
        }

        public Triangle3(Vector3[] vertices, Color albedo, ITriangleShader shader)
        {
            this.vertices = vertices;
            this.albedo = albedo;
            this.shader = shader;
        }

        public Triangle3(Vector3[] vertices, Triangle3 from)
        {
            this.vertices = vertices;
            this.albedo = from.albedo;
            this.shader = from.shader;
            this.doubleNormaled = from.doubleNormaled;
        }

        public Color GetColor(ILightSource[] lightSources)
        {
            if (lightSources.Length == 0) return albedo;

            return shader.NormalShading(Normal, lightSources[0].GetLightRay(this), albedo);
        }

        public Vector2[] ProjectedVertices(Camera camera)
        {
            Vector2[] projectedVertices = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                projectedVertices[i] = camera.Project(vertices[i]);
            }
            return projectedVertices;
        }
    }

    public interface ITriangleShader
    {
        Color NormalShading(Vector3 triangleNormal, LightRay lightDirection, Color triangleAlbedo);
    }

    public class NormalShader : ITriangleShader
    {
        public float weight; //How much of an impact the dot product has on the returned color
            //1 means that the lerp is from black to albedo
            //0 means that the lerp is from albedo to albedo (it's really just albedo)

        public NormalShader(float weight)
        {
            this.weight = weight;
        }

        public virtual Color NormalShading(Vector3 triangleNormal, LightRay lightRay, Color triangleAlbedo)
        {
            float dotProduct = Clamp(0f, 1f, Vector3.Dot(triangleNormal, lightRay.direction)) + Scene.Instance.baseLighting;
            return Lerp(triangleAlbedo, Color.Black,  weight * (1 - dotProduct));
        }

        protected Color Lerp(Color c1, Color c2, float t)
        {
            int r = (int)Lerp(c1.R, c2.R, t);
            int b = (int)Lerp(c1.B, c2.B, t);
            int g = (int)Lerp(c1.G, c2.G, t);

            return Color.FromArgb(r, g, b);
        }

        protected float Lerp(float y1, float y2, float t)
        {
            return (y2 - y1) * Clamp(0, 1, t) + y1;
        }

        protected float Clamp(float min, float max, float t)
        {
            if (t > max) return max;
            if (t < min) return min;
            return t;
        }
    }

    public class FakeNormalShader : NormalShader
    {

        public Vector3 normal;

        public FakeNormalShader(float weight, Vector3 normal) : base(weight)
        {
            this.normal = normal;
        }

        public override Color NormalShading(Vector3 triangleNormal, LightRay lightRay, Color triangleAlbedo)
        {
            float dotProduct = Clamp(0f, 1f, Vector3.Dot(-normal, lightRay.direction)) + Scene.Instance.baseLighting;
            return Lerp(triangleAlbedo, Color.Black, weight * (1 - dotProduct));
        }
    }
}
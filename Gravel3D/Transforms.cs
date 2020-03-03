using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Gravel3D.Vectors;
using Gravel3D.Renderers;

namespace Gravel3D.Transforms
{
    public abstract class Transform
    {
        public const float maxSortableZ = 10000;
        
        //Transformation properties
        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public Vector3 scaleFactors = new Vector3(1, 1, 1);

        //The event that is called every frame update
        public delegate void updateDelegate(int deltaTime, Transform parent);
        public event updateDelegate OnUpdate;

        public Transform[] children;

        public static Transform Origin
        {
            get
            {
                return PointTransform.Origin;
            }
        }

        public Transform()
        {
            if(Form1.mainScene != null)
                Form1.mainScene.onUpdate += this.Update;
        }

        //Transformation functions
        public void Translate (Vector3 vector)
        {
            position += vector;
        }

        public void Rotate(Vector3 eulerVector)
        {
            rotation += eulerVector;
        }

        public void Scale(Vector3 v)
        {
            scaleFactors = scaleFactors.Scale(v);
        }

        public void Scale(float s)
        {
            scaleFactors = scaleFactors.Scale(s);
        }

        public void Update(int deltaTime)
        {
            OnUpdate?.Invoke(deltaTime, this);
        }

        public static int SortByZ(Transform t1, Transform t2)
        {
            if (t1.SortableZ > t2.SortableZ) return -1;
            else if (t1.SortableZ == t2.SortableZ) return 0;
            else return 1;
        }

        public virtual float SortableZ
        {
            get
            {
                return position.z;
            }
        }

        public abstract Transform ToSpace(Transform space);
        public Transform TransformToOrigin()
        {
            return ToSpace(Origin);
        }
        public abstract Triangle3[] Triangles { get; }
    }

    public abstract class FaceTransform : Transform
    {
        public Vector3[] vertices;
        protected Face[] faces;

        public FaceTransform(Vector3[] vertices, Face[] faces)
        {
            this.vertices = vertices;
            this.faces = faces;
        }

        public override Triangle3[] Triangles
        {
            get
            {
                List<Triangle3> triangles = new List<Triangle3>();
                foreach(Face face in faces)
                {
                    triangles = triangles.Concat(face.GetTriangles(this)).ToList();
                }
                return triangles.ToArray();
            }
        }
    }

    public class SolidTransform : FaceTransform
    {
        public SolidTransform(Vector3 position, Vector3 rotation, Vector3[] vertices, Face[] faces) : base(vertices, faces)
        {
            this.position = position;
            this.rotation = rotation;
        }

        //Creates a new transform that is identical to this, except from the perspective of a given transform.
        public override Transform ToSpace(Transform space)
        {
            Vector3[] newVertices = new Vector3[vertices.Length];
            
            //Iterates through the vertices in the solid
            for(int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];

                //Rotates the vertex
                vertex = vertex.Rotate(this.rotation);

                //Translates the vertex
                vertex = vertex + this.position - space.position;

                //Orbits the vertex
                vertex = vertex.Orbit(space.rotation);

                newVertices[i] = vertex;
            }

            //At the end of the loop, newVertices is populated with transformed vertices.
            //Find the actual transfrom's position and rotation
            Vector3 newPosition = (this.position - space.position).Orbit(space.rotation);
            Vector3 newRotation = this.rotation - space.rotation;

            //Create a new SolidTransform with the new vertices and all the same edges and triangles.
            SolidTransform newTransform = new SolidTransform(newPosition, newRotation, newVertices, faces);
            return newTransform;
        }
    }
    
    public class Face
    {
        private int[] vertices; //The indeces of the vertices of the face in relation to the containing solid's vertices array
        public Color albedo;
        public ITriangleShader shader;

        public Face(int[] vertices, Color albedo, ITriangleShader shader)
        {
            this.vertices = vertices;
            this.albedo = albedo;
            this.shader = shader;
        }

        public Triangle3[] GetTriangles(FaceTransform solid)
        {
            Triangle3[] triangles = new Triangle3[vertices.Length - 2];
            for(int i = 1; i < vertices.Length - 1; i++)
            {
                triangles[i - 1] = new Triangle3(new Vector3[] { solid.vertices[vertices[0]], solid.vertices[vertices[i]], solid.vertices[vertices[i + 1]] }, albedo, shader);
            }
            return triangles;
        }
    }

    public class PointTransform : Transform
    {

        new public static Transform Origin
        {
            get
            {
                return new PointTransform(Vector3.Zero, Vector3.Zero);
            }
        }

        public override Triangle3[] Triangles { get { return new Triangle3[0]; } }

        public PointTransform(Vector3 position, Vector3 rotation)
        {
            this.position = position;
            this.rotation = rotation;
            this.scaleFactors = Vector3.Zero;
        }

        public override Transform ToSpace(Transform space)
        {
            Vector3 newPosition = (this.position - space.position).Orbit(space.rotation);
            Vector3 newRotation = this.rotation - space.rotation;
            return new PointTransform(newPosition, newRotation);
        }
    }

    public class SunTransform : FaceTransform
    {

        //A transform that exists at infinity
        //Useful for suns, stars, moons, or other celestial bodies
        //Possibly eventually something else, too

        public SunTransform(Vector3 rotation, Vector3[] vertices, Face[] faces) : base(vertices, faces)
        {
            this.rotation = rotation;
        }

        public override Transform ToSpace(Transform space)
        {
            Vector3[] newVertices = new Vector3[vertices.Length];
            for(int i = 0; i < vertices.Length; i++)
            {
                newVertices[i] = vertices[i].Rotate(rotation).Orbit(space.rotation);
            }
            return new SunTransform(Vector3.Zero, newVertices, faces);
        }

        public override float SortableZ
        {
            get
            {
                return maxSortableZ;
            }
        }
    }

    public class PlaneTransform : Transform
    {
        //Represents a 2d plane that stretches into infinity
        //Useful for infinite floors or Trumpian walls

        private const float cameraClippingPlane = 0.01f;
        private Vector3 normalOffset = new Vector3(0, 0, cameraClippingPlane);

        public Vector3 normal = Vector3.Zero;

        public Color albedo;
        public FakeNormalShader shader;

        public PlaneTransform(Vector3 normal, float height, Color albedo, FakeNormalShader shader)
        {
            this.normal = normal;
            this.albedo = albedo;
            this.shader = shader;
            this.position = new Vector3(0, height, 0);
        }

        public override Triangle3[] Triangles
        {
            get
            {
                //The first vertex (the one below the camera)
                Vector3 vertex1 = new Vector3(0, position.y, 0);

                Vector3 vertex2 = normal.Rotate(new Vector3((float)Math.PI / 2, 0, 0)) + new Vector3(1, 0, 0);
                Vector3 vertex3 = normal.Rotate(new Vector3((float)Math.PI / 2, 0, 0)) + new Vector3(-1, 0, 0);

                Triangle3 triangle = new Triangle3(new Vector3[] { vertex1, vertex2, vertex3 }, albedo, shader);
                triangle.doubleNormaled = true;

                return new Triangle3[] { triangle };
            }
        }

        public override Transform ToSpace(Transform space)
        {
            shader.normal = normal.Rotate(rotation).Orbit(space.rotation);
            return new PlaneTransform(shader.normal, position.y - space.position.y, albedo, shader);
        }

        public override float SortableZ
        {
            get
            {
                return maxSortableZ - 1;
            }
        }
    }
}
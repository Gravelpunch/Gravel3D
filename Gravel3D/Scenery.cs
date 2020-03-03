namespace Gravel3D.Scenery
{
    using Gravel3D.Transforms;
    using Gravel3D.Vectors;
    using System.Drawing;
    using System;
    using Gravel3D.Renderers;
    using Gravel3D.Lighting;
    using Gravel3D.Components;

    public class CelestialBody : FaceTransform, ILightSource
    {
        public static float sqrt3 = 1.73f;

        public Vector3 lightDirection;
        public Color lightColor;
        public Movement movement;

        private float dayLength;

        public CelestialBody(Vector3[] vertices, Face[] faces, Vector3 rotation, float dayLength, Vector3 lightDirection, Color lightColor) : base(vertices, faces)
        {
            this.rotation = rotation;
            this.lightDirection = lightDirection;
            this.lightColor = lightColor;

            this.dayLength = dayLength;
            this.movement = new Movement(this);
            this.movement.angularVelocity = new Vector3((float)(2 * Math.PI / dayLength), 0, 0);
        }

        public override Transform ToSpace(Transform space)
        {
            Vector3[] newVertices = new Vector3[vertices.Length];
            for(int i = 0; i < vertices.Length; i++)
            {
                newVertices[i] = vertices[i].Rotate(rotation).Orbit(space.rotation);
            }

            return new CelestialBody(newVertices, faces, Vector3.Zero, dayLength, lightDirection.Rotate(rotation).Orbit(space.rotation), lightColor);
        }

        public LightRay GetLightRay(Triangle3 triangle)
        {
            return new LightRay(lightDirection, lightColor);
        }

        public override float SortableZ
        {
            get
            {
                return maxSortableZ;
            }
        }
    }
}
using Gravel3D.Vectors;
using Gravel3D.Renderers;
using Gravel3D.Transforms;
using System.Drawing;
using System;

namespace Gravel3D.Lighting
{

    public interface ILightSource
    {
        LightRay GetLightRay(Triangle3 triangle);
    }

    public struct LightRay
    {
        public Vector3 direction;
        public Color color;

        public LightRay(Vector3 direction, Color color)
        {
            this.direction = direction;
            this.color = color;
        }
    }

}
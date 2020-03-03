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
using Gravel3D.Renderers;

namespace Gravel3D.Solids
{
    //public class Tetrahedron : SolidTransform
    //{
    //    public Tetrahedron(Brush brushFL, Brush brushFR, Brush brushBR, Brush brushBL, Pen penEdge, Vector3 pos, float s) :
    //        base(pos, Vector3.Zero, new Vector3[]
    //        {
    //            new Vector3(s, s, s),
    //            new Vector3(s, -s, -s),
    //            new Vector3(-s, s, -s),
    //            new Vector3(-s, -s, s),
    //        },
    //        new SolidRenderer(new IDraw[] {
    //            new Triangle(new int[] {0, 1, 2}, brushFR, penEdge),
    //            new Triangle(new int[] {1, 3, 2}, brushFL, penEdge),
    //            new Triangle(new int[] {0, 2, 3}, brushBR, penEdge),
    //            new Triangle(new int[] {0, 3, 1}, brushBL, penEdge)
    //        }))
    //    { }
    //}

    public class Cube : SolidTransform
    {
        public Cube(Color front, Color back, Color top, Color bottom, Color left, Color right, ITriangleShader commonShader, Vector3 pos, float s) : base(pos, Vector3.Zero, new Vector3[]
        {
            new Vector3(s, s, s),
            new Vector3(s, s, -s),
            new Vector3(s, -s, s),
            new Vector3(s, -s, -s),
            new Vector3(-s, s, s),
            new Vector3(-s, s, -s),
            new Vector3(-s, -s, s),
            new Vector3(-s, -s, -s),
        }, new Face[] 
        {
            new Face(new int[] { 0, 1, 3, 2 }, left, commonShader),
            new Face(new int[] { 0, 4, 5, 1 }, top, commonShader),
            new Face(new int[] { 1, 5, 7, 3 }, front, commonShader),
            new Face(new int[] { 4, 6, 7, 5 }, right, commonShader),
            new Face(new int[] { 2, 3, 7, 6 }, bottom, commonShader),
            new Face(new int[] { 0, 2, 6, 4 }, back, commonShader)
        })
        {}
    }
}
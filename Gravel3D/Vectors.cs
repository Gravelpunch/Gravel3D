using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gravel3D.Vectors
{
    public class Vector2
    {
        public float x;
        public float y;

        public float Hypotenuse
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y);
            }
        }

        public PointF ToPoint
        {
            get
            {
                return new PointF(x, y);
            }
        }

        //This is used as a vector that represents 'null' or 'no vector.' 
        //When checking for a null vector, DO NOT compare components, directly compare vectors
        //E.I, myVector == Vector2.nullVector
        private static Vector2 _nullVector = new Vector2(0, 0);
        public static Vector2 NullVector
        {
            get
            {
                return Vector2._nullVector;
            }
        }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        //Rotates the vector around the origin by a given number of radians
        public Vector2 Rotate(float theta)
        {
            double sin = Math.Sin(theta);
            double cos = Math.Cos(theta);

            float newX = (float)(x * cos - y * sin);
            float newY = (float)(x * sin + y * cos);

            return new Vector2(newX, newY);
        }

        //Rotates the vector around the given point by a given number of radians
        public Vector2 Rotate(float theta, Vector2 center)
        {
            Vector2 tmp = this - center;
            tmp = tmp.Rotate(theta);
            return tmp + center;

        }

        public Vector2 Scale(Vector2 v)
        {
            return new Vector2(x * v.x, y * v.y);
        }

        public Vector2 Scale(float s)
        {
            return Scale(new Vector2(s, s));
        }

        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2(-v.x, -v.y);
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return v1 + -v2;
        }
    }

    public class Vector3
    {
        public static Vector3 Zero
        {
            get
            {
                return new Vector3(0, 0, 0);
            }
        }

        //This is used as a vector that represents 'null' or 'no vector.' 
        //When checking for a null vector, DO NOT compare components, directly compare vectors
        //E.I, myVector == Vector3.nullVector
        private static Vector3 _nullVector = new Vector3(0, 0, 0);
        public static Vector3 NullVector
        {
            get
            {
                return Vector3._nullVector;
            }
        }

        public float x;
        public float y;
        public float z;

        private float[] components;

        public float Hypotenuse
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            this.components = new float[3] { x, y, z };
        }

        public Vector3 Normalized
        {
            get
            {
                return this.Scale(1 / this.Hypotenuse);
            }
        }

        public Vector3 Scale(Vector3 v)
        {
            return new Vector3(x * v.x, y * v.y, z * v.z);
        }

        public Vector3 Scale(float s)
        {
            return Scale(new Vector3(s, s, s));
        }

        //Rotates the vector by 3 euler angles, given by the input vector.
        //Rotates in the order Z, X, Y.
        //This means that the Z rotation changes no axes,
        //The X rotation changes only the Z axis,
        //And the Y rotation changes all other axes.
        public Vector3 Rotate(Vector3 euler)
        {
            Vector3 tmp = new Vector3(this.x, this.y, this.z);

            //Rotates along Z axis
            Vector2 zRot = new Vector2(tmp.x, tmp.y);
            zRot = zRot.Rotate(euler.z);
            tmp.x = zRot.x;
            tmp.y = zRot.y;

            //Rotates along X axis
            Vector2 xRot = new Vector2(tmp.y, tmp.z);
            xRot = xRot.Rotate(euler.x);
            tmp.y = xRot.x;
            tmp.z = xRot.y;

            //Rotates along Y axis
            Vector2 yRot = new Vector2(tmp.x, tmp.z);
            yRot = yRot.Rotate(euler.y);
            tmp.x = yRot.x;
            tmp.z = yRot.y;

            return tmp;
        }

        //Orbits the vector around the origin.
        //Similar to Rotate, except that it applies the axes in reverse order.
        //This is because it is inteded to be used to help move between spaces.
        public Vector3 Orbit(Vector3 euler)
        {
            Vector3 tmp = new Vector3(this.x, this.y, this.z);

            //Rotates along Y axis
            Vector2 yRot = new Vector2(tmp.x, tmp.z);
            yRot = yRot.Rotate(-euler.y);
            tmp.x = yRot.x;
            tmp.z = yRot.y;

            //Rotates along X axis
            Vector2 xRot = new Vector2(tmp.y, tmp.z);
            xRot = xRot.Rotate(-euler.x);
            tmp.y = xRot.x;
            tmp.z = xRot.y;

            //Rotates along Z axis
            Vector2 zRot = new Vector2(tmp.x, tmp.y);
            zRot = zRot.Rotate(-euler.z);
            tmp.x = zRot.x;
            tmp.y = zRot.y;

            return tmp;
        }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.x, -v.y, -v.z);
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return v1 + -v2;
        }

        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            float x = v1.y * v2.z - v1.z * v2.y;
            float y = v1.z * v2.x - v1.x * v2.z;
            float z = v1.x * v2.y - v1.y * v2.x;

            return new Vector3(x, y, z);
        }

        public static float Dot(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static Vector3 ComponentProduct(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }
    }

    public class BoolVector3
    {
        public bool x;
        public bool y;
        public bool z;

        public BoolVector3(bool x, bool y, bool z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 Tern(Vector3 ifTrue, Vector3 ifFalse)
        {
            float newX = this.x ? ifTrue.x : ifFalse.x;
            float newY = this.y ? ifTrue.y : ifFalse.y;
            float newZ = this.z ? ifTrue.z : ifFalse.z;

            return new Vector3(newX, newY, newZ);
        }

        public Vector3 Tern(Vector3 ifTrue)
        {
            return Tern(ifTrue, Vector3.Zero);
        }
    }
}
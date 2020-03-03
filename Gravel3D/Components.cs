using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Gravel3D.Vectors;
using Gravel3D.Transforms;

namespace Gravel3D.Components
{
    public abstract class Component
    {
        public bool enabled = true;

        public Component() { }

        public Component(Transform parent){
            AttachTo(parent);
        }

        public void AttachTo(Transform parent)
        {
            parent.OnUpdate += updateIfEnabled;
        }

        public void updateIfEnabled(int deltaTime, Transform parent)
        {
            if(enabled) { update(deltaTime, parent); }
        }

        public abstract void update(int deltaTime, Transform parent);

    }

    public class Movement : Component
    {
        public BoolVector3 lockVelToRot = new BoolVector3(false, false, false);    //Whether or not the velocity of this component should 'follow' the transform's
        public Vector3 velocity = new Vector3(0, 0, 0);            //the velocity, in units per second
        public Vector3 angularVelocity = new Vector3(0, 0, 0);     //the rotational velocity, in radians per second

        public Movement(Transform parent) : base(parent)
        {
            this.velocity = Vector3.Zero;
            this.angularVelocity = Vector3.Zero;
        }

        public Movement(Transform parent, Vector3 velocity, BoolVector3 lockVelToRot) : base(parent)
        {
            this.lockVelToRot = lockVelToRot;
            this.velocity = velocity;
            this.angularVelocity = Vector3.Zero;
        }

        public Movement() : base()
        {
            this.velocity = Vector3.Zero;
            this.angularVelocity = Vector3.Zero;
        }

        public Movement(Vector3 velocity, BoolVector3 lockVelToRot) : base()
        {
            this.lockVelToRot = lockVelToRot;
            this.velocity = velocity;
            this.angularVelocity = Vector3.Zero;
        }

        public override void update(int deltaTime, Transform parent)
        {
            parent.position += this.velocity.Rotate(this.lockVelToRot.Tern(parent.rotation, Vector3.Zero)).Scale(deltaTime / 1000f);
            parent.rotation += this.angularVelocity.Scale(deltaTime / 1000f);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using Gravel3D.Vectors;
using Gravel3D.Transforms;
using Gravel3D.Renderers;
using Gravel3D.Solids;
using Gravel3D.Components;
using Gravel3D.Lighting;
using Gravel3D.Scenery;

namespace Gravel3D
{
    public partial class Form1 : Form
    {

        //Config variables
        public static int frameRate = 100;

        public static float focalLength = 1f;

        public static float cameraSpeed = 2f;
        public static float cameraRotSpeed = 1f;

        public static Pen edgePen = new Pen(Color.FromArgb(0, 0, 0), 5f);

        public Dictionary<Keys, Vector3> walkControls = new Dictionary<Keys, Vector3>
        {
            {
                Keys.W,
                new Vector3(0, 0, cameraSpeed)
            },
            {
                Keys.S,
                new Vector3(0, 0, -cameraSpeed)
            },
            {
                Keys.A,
                new Vector3(-cameraSpeed, 0, 0)
            },
            {
                Keys.D,
                new Vector3(cameraSpeed, 0, 0)
            },
            {
                Keys.Space,
                new Vector3(0, cameraSpeed, 0)
            },
            {
                Keys.C,
                new Vector3(0, -cameraSpeed, 0)
            }
        };

        public Dictionary<Keys, Vector3> turnControls = new Dictionary<Keys, Vector3>
        {
            {
                Keys.Up,
                new Vector3(-cameraRotSpeed, 0, 0)
            },
            {
                Keys.Down,
                new Vector3(cameraRotSpeed, 0, 0)
            },
            {
                Keys.Left,
                new Vector3(0, cameraRotSpeed, 0)
            },
            {
                Keys.Right,
                new Vector3(0, -cameraRotSpeed, 0)
            }
        };

        public Dictionary<Keys, bool> controlIsDown = new Dictionary<Keys, bool>(); //Stores which keys are up and down

        public static Scene mainScene;
        public Timer frameRateTimer = new Timer();

        public Movement cameraMovement;

        public Form1()
        {
            InitializeComponent();

            ITriangleShader solidShader = new NormalShader(1);
            FakeNormalShader planeShader = new FakeNormalShader(1, new Vector3(0, 1, 0));

            float sunDepth = 20;
            ITriangleShader sunShader = new NormalShader(0);

            mainScene = new Scene(this, new Transform[] {

                //new Tetrahedron(Brushes.Yellow, Brushes.Green, Brushes.Blue, Brushes.Red, edgePen, new Vector3(-2, 0, 5), 1f),
                
                //new Cube(Color.Red, Color.Orange, Color.White, Color.Yellow, Color.Green, Color.Blue, new Vector3(2, 0, 5), 1f),
                new Cube(Color.Green, Color.Green, Color.Green, Color.Green, Color.Green, Color.Green, solidShader, new Vector3(0, 0, 5), 1f),
                //new SunLightSource(new Vector3(1, 0, 0), new Vector3(0, 0, (float) Math.PI / 2))
                new PlaneTransform(new Vector3(0, 1, 0), -1f, Color.YellowGreen, planeShader),
                new CelestialBody(new Vector3[]
                {
                    new Vector3(0, 1, sunDepth),
                    new Vector3(-(float)Math.Sqrt(3) / 2, -0.5f, sunDepth),
                    new Vector3((float)Math.Sqrt(3) / 2, -0.5f, sunDepth),
                    new Vector3((float)Math.Sqrt(3) / 2, 0.5f, sunDepth),
                    new Vector3(-(float)Math.Sqrt(3) / 2, 0.5f, sunDepth),
                    new Vector3(0, -1, sunDepth)
                }, new Face[] {
                    new Face(new int[] {0, 1, 2 }, Color.Yellow, sunShader),
                    new Face(new int[] {3, 4, 5 }, Color.Yellow, sunShader)
                }, Vector3.Zero, 100f, new Vector3(0, 0, -1), Color.White)

            }, Color.SkyBlue);

            cameraMovement = new Movement(mainScene.mainCamera.transform, Vector3.Zero, new BoolVector3(false, true, false));

            frameRateTimer.Interval = 1000 / frameRate;
            frameRateTimer.Tick += new EventHandler(Timer_Tick);
            frameRateTimer.Start();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            mainScene.update();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            mainScene.update();
            this.Invalidate();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {

            controlIsDown[e.KeyCode] = false; //The key is no longer pressed

            if (walkControls.ContainsKey(e.KeyCode))
            {
                cameraMovement.velocity -= walkControls[e.KeyCode];
            }
            if (turnControls.ContainsKey(e.KeyCode))
            {
                cameraMovement.angularVelocity -= turnControls[e.KeyCode];
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //Don't do anything if the control state exists and it's already down
            if (controlIsDown.ContainsKey(e.KeyCode) && controlIsDown[e.KeyCode]) { return; }

            controlIsDown[e.KeyCode] = true; //The key is now pressed.

            if (walkControls.ContainsKey(e.KeyCode))
            {
                cameraMovement.velocity += walkControls[e.KeyCode];
            }
            if (turnControls.ContainsKey(e.KeyCode))
            {
                cameraMovement.angularVelocity += turnControls[e.KeyCode];
            }
        }

        private void Form1_Paint (object sender, PaintEventArgs e)
        {
            mainScene.mainCamera.Render(mainScene.contents.ToArray(), e.Graphics);

        }

        public static int wrap(int v, int length)
        {
            int m = v % length;
            if(m < 0)
            {
                m += length;
            }
            return m;
        }
    }

    public class Scene
    {
        public Camera mainCamera;

        public List<Transform> contents;
        
        public List<ILightSource> LightSources
        {
            get
            {
                List<ILightSource> lightSources = new List<ILightSource>();
                foreach(Transform transform in contents)
                {
                    if (transform is ILightSource) lightSources.Add(transform as ILightSource);
                }
                return lightSources;
            }
        }

        public Color skyColor;

        public delegate void delegateUpdate(int deltaTime);
        public event delegateUpdate onUpdate;

        public float baseLighting = 0.25f;

        private Stopwatch deltaStopWatch = new Stopwatch();

        private static Scene instance;
        public static Scene Instance
        {
            get
            {
                return instance;
            }
        }

        public Scene(Form screen, Transform[] contents, Color skyColor)
        {
            this.mainCamera = new Camera(screen, Form1.focalLength);
            this.contents = new List<Transform>(contents);
            this.contents.Add(this.mainCamera.transform);
            this.skyColor = skyColor;

            if(instance == null)
            {
                instance = this;
            }

            foreach(Transform transform in this.contents)
            {
                //the initial contents of the scene haven't had a chance to subscribe to onUpdate yet
                //(Because it hasn't existed yet) so do it now
                onUpdate += transform.Update;
            }
        }

        public void update()
        {
            deltaStopWatch.Stop();
            int deltaTime = (int)deltaStopWatch.ElapsedMilliseconds;
            deltaStopWatch.Reset();

            onUpdate?.Invoke(deltaTime);

            deltaStopWatch.Start();
        }
    }
}
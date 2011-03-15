using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PSMoveSharp;

namespace MoveMeFlowers
{
    public partial class MoveMeFlowersGUI : Form
    {

        protected bool glControl_loaded = false;

        public MoveMeFlowersGUI()
        {
            InitializeComponent();
            this.textBoxServerAddress.Text = global::MoveMeFlowers.Properties.Settings.Default.most_recent_server;
        }

        struct PaletteColor
        {
            public float r;
            public float g;
            public float b;
        }

        PaletteColor[] palette;

        private void GLControl_Load(object sender, EventArgs e)
        {
            glControl_loaded = true;

            palette = new PaletteColor[12];
            
            palette[0].r = 1.0f;
            palette[0].g = 0.5f;
            palette[0].b = 0.5f;

            palette[1].r = 1.0f;
            palette[1].g = 0.75f;
            palette[1].b = 0.5f;
            
            palette[2].r = 1.0f;
            palette[2].g = 1.0f;
            palette[2].b = 0.5f;
            
            palette[3].r = 0.75f;
            palette[3].g = 1.0f;
            palette[3].b = 0.5f;

            palette[4].r = 0.5f;
            palette[4].g = 1.0f;
            palette[4].b = 0.5f;

            palette[5].r = 0.5f;
            palette[5].g = 1.0f;
            palette[5].b = 0.75f;

            palette[6].r = 0.5f;
            palette[6].g = 1.0f;
            palette[6].b = 1.0f;

            palette[7].r = 0.5f;
            palette[7].g = 0.75f;
            palette[7].b = 1.0f;

            palette[8].r = 0.5f;
            palette[8].g = 0.5f;
            palette[8].b = 1.0f;

            palette[9].r = 0.75f;
            palette[9].g = 0.5f;
            palette[9].b = 1.0f;

            palette[10].r = 1.0f;
            palette[10].g = 0.5f;
            palette[10].b = 1.0f;

            palette[11].r = 1.0f;
            palette[11].g = 0.5f;
            palette[11].b = 0.75f;

            Application.Idle += new EventHandler(Application_Idle);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            GLControl.Invalidate();
        }

        private void SimpleCamera(OpenTK.Vector3 average_point)
        {
            {
                const float yFov = 1.00899694f; // 75
                const float near = 100;
                const float far = 7000;
                float aspect_ratio = GLControl.Width / GLControl.Height;
                OpenTK.Matrix4 projection;
                projection = OpenTK.Matrix4.CreatePerspectiveFieldOfView(yFov, aspect_ratio, near, far);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.LoadMatrix(ref projection);
            }

            {
                const float eye_height = 5000;
                OpenTK.Matrix4 lookat;
                lookat = OpenTK.Matrix4.LookAt(0.0f, eye_height, 0.0f,
                                               average_point.X, average_point.Y, average_point.Z,
                                               0.0f, 0.0f, 1.0f);

                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.LoadMatrix(ref lookat);
            }
        }

        private void BetterCamera(OpenTK.Vector3 average_point)
        {
            {
                const float yFov = 1.00899694f; // 75
                const float near = 10;
                const float far = 7000;
                float aspect_ratio = GLControl.Width / GLControl.Height;
                OpenTK.Matrix4 projection;
                projection = OpenTK.Matrix4.CreatePerspectiveFieldOfView(yFov, aspect_ratio, near, far);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.LoadMatrix(ref projection);
            }

            OpenTK.Vector3 eye;

            float radius = Program.camera.GetDistance();
            float theta = Program.camera.GetAngleZ();
            float phi = Program.camera.GetAngleX();

            Program.form.labelAngle.Text = "Angle: " + theta + " " + phi;
            eye.X = radius * (float)Math.Cos((double)theta) * (float)Math.Cos((double)phi);
            eye.Y = radius * (float)Math.Sin((double)theta);
            eye.Z = radius * (float)Math.Cos((double)theta) * (float)Math.Sin((double)phi);

            OpenTK.Vector3 eye_direction = eye;
            eye_direction.Normalize();
            OpenTK.Vector3 zero = new OpenTK.Vector3(0.0f, 0.0f, 0.0f);
            OpenTK.Vector3 up = new OpenTK.Vector3(0.0f, 1.0f, 0.0f);
            OpenTK.Vector3 right = OpenTK.Vector3.Cross(eye_direction, up);
            //up = OpenTK.Vector3.Cross(right, eye_direction);
            OpenTK.Matrix4 lookat;
            lookat = OpenTK.Matrix4.LookAt(eye, zero, up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MultMatrix(ref lookat);
            GL.Translate(average_point);
        }

        private void DrawSphere(float Radius, uint Precision)
        {
            if (Radius < 0f)
                Radius = -Radius;
            if (Radius == 0f)
                throw new DivideByZeroException("DrawSphere: Radius cannot be 0f.");
            if (Precision == 0)
                throw new DivideByZeroException("DrawSphere: Precision of 8 or greater is required.");

            const float HalfPI = (float)(Math.PI * 0.5);
            float OneThroughPrecision = 1.0f / Precision;
            float TwoPIThroughPrecision = (float)(Math.PI * 2.0 * OneThroughPrecision);

            float theta1, theta2, theta3;
            OpenTK.Vector3 Normal, Position;

            for (uint j = 0; j < Precision / 2; j++)
            {
                theta1 = (j * TwoPIThroughPrecision) - HalfPI;
                theta2 = ((j + 1) * TwoPIThroughPrecision) - HalfPI;

                GL.Begin(BeginMode.TriangleStrip);
                for (uint i = 0; i <= Precision; i++)
                {
                    theta3 = i * TwoPIThroughPrecision;

                    Normal.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    Normal.Y = (float)Math.Sin(theta2);
                    Normal.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    Position.X = Radius * Normal.X;
                    Position.Y = Radius * Normal.Y;
                    Position.Z = Radius * Normal.Z;

                    GL.Normal3(Normal);
                    GL.Color3(i * OneThroughPrecision, 2.0f * (j + 1) * OneThroughPrecision, 1.0f);
                    GL.Vertex3(Position);

                    Normal.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    Normal.Y = (float)Math.Sin(theta1);
                    Normal.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    Position.X = Radius * Normal.X;
                    Position.Y = Radius * Normal.Y;
                    Position.Z = Radius * Normal.Z;

                    GL.Normal3(Normal);
                    GL.Color3(i * OneThroughPrecision, 2.0f * (j + 1) * OneThroughPrecision, 1.0f);
                    GL.Vertex3(Position);
                }
                GL.End();
            }
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl_loaded) // Play nice
                return;

            if (Program.client_connected == false || Program.flower.Capturing() == false || Program.camera.SphereVisible())
            {
                GL.ClearColor(Color.Black);
            }
            else
            {
                GL.ClearColor(Color.Red);
            }
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            Program.flower.StartRead();


            int current_slot = Program.flower.GetCurrentSlot();
            LinkedList<MoveSample> samples = Program.flower.GetSamples(current_slot);

            // find average point
            OpenTK.Vector3 average_point = new OpenTK.Vector3(0.0f, 0.0f, 0.0f);
            foreach (MoveSample sample in samples) {
                average_point.X += sample.x;
                average_point.Y += sample.y;
                average_point.Z += sample.z;
            }
            average_point /= samples.Count;
            BetterCamera(average_point);
            Program.form.labelCenter.Text = "Position: (" + average_point.X + " , " + average_point.Y + " , " + average_point.Z + ")";
            GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            GL.LineWidth(2.0f);

            //DrawSphere(20.0f, 8);

            for (int i = 0; i < MoveMeFlowerStateConstants.MAX_FLOWERS; i++)
            {
                LinkedList<MoveSample> slot_samples = Program.flower.GetSamples(i);
                GL.Color3(palette[i].r, palette[i].g, palette[i].b);
                GL.Begin(BeginMode.LineStrip);
                foreach (MoveSample sample in slot_samples)
                {
                    GL.Vertex3(-sample.x, sample.y, -sample.z);
                }
                GL.End();
            }

            /*
            int palette_index = 0;
            GL.Begin(BeginMode.LineStrip);
            foreach (MoveSample sample in samples) {
                GL.Color3(palette[palette_index].r, palette[palette_index].g, palette[palette_index].b);
                GL.Vertex3(-sample.x, sample.y, -sample.z);
                palette_index = (palette_index + 1) % palette.Length;
            }
            GL.End();
             */

            Program.flower.StopRead();
            GLControl.SwapBuffers();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (!Program.client_connected)
            {
                try
                {
                    Program.client_connect(textBoxServerAddress.Text, Convert.ToInt32(textBoxServerPort.Text));
                }
                catch
                { }
            }
            else
            {
                Program.client_disconnect();
            }

            if (Program.client_connected)
            {
                buttonConnect.Text = "Disconnect";
            }
            else
            {
                buttonConnect.Text = "Connect";
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (!Program.client_paused)
            {
                Program.moveClient.Pause();
            }
            else
            {
                Program.moveClient.Resume();
            }

            Program.client_paused = !Program.client_paused;
            if (Program.client_paused)
            {
                buttonPause.Text = "Resume";
            }
            else
            {
                buttonPause.Text = "Pause";
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            Program.flower.Reset();
        }
    }
}

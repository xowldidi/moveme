using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PSMoveSharp;

namespace PSMoveSharpAR
{
    public partial class ARGUI : Form
    {
        bool glControl_loaded = false;

        public delegate void ProcessPSMoveStateDelegate();
        public ProcessPSMoveStateDelegate updateGuiDelegate;

        public PSMoveSharpState AR_state;
        public System.Drawing.Image AR_image;

        public ObjModel AR_model = null;
        public Bitmap AR_model_texture;
        public int AR_model_texture_id;

        public ARGUI()
        {
            InitializeComponent();
            this.textBoxServerAddress.Text = global::PSMoveSharpAR.Properties.Settings.Default.most_recent_server;
            updateGuiDelegate = updateState;
            AR_model = new ObjModel();
            AR_model.Load("../../gladius.obj");
            AR_model_texture = new Bitmap("../../gladius.png");
        }

        static UInt32 processed_packet_index = 0;

        private void updateToolbar()
        {
            buttonConnect.Text = Program.client_connected ? "Disconnect" : "Connect";
            textBoxServerAddress.Enabled = !Program.client_connected;
            textBoxServerPort.Enabled = !Program.client_connected;
            buttonPause.Enabled = Program.client_connected;
            buttonPause.Text = Program.client_paused ? "Resume" : "Pause";
            int slices = Program.moveClient.GetLatestState().serverConfig.num_image_slices;
            if (imageSliceBox.Focused == false) {
                // only change it when it is unused
                if (imageSliceBox.SelectedIndex != slices - 1)
                {
                    // initial setting on connection
                    imageSliceBox.SelectedIndex = slices - 1;
                }
            }
            
        }

        private void updateState()
        {
            if (Program.moveClient == null)
            {
                return;
            }

            PSMoveSharpState state = Program.moveClient.GetLatestState();
            PSMoveSharpCameraFrameState camera_frame_state = Program.moveClient.GetLatestCameraFrameState();
            if (processed_packet_index == state.packet_index)
            {
                return;
            }

            updateToolbar();
            updateAR();
        }

        private void updateAR()
        {
            if (Program.image_paused)
            {
                ImagePausedToggleButton.Text = "Camera Start";
            }
            else
            {
                ImagePausedToggleButton.Text = "Camera Pause";
            }

            Bitmap TextureBitmap;

            if (Program.image_paused)
            {
                return;
            }

            AR_image = Program.moveClient.GetLatestCameraFrameState().GetCameraFrameAndState(ref AR_state);
            TextureBitmap = new Bitmap(AR_image);

            System.Drawing.Imaging.BitmapData TextureData = TextureBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, Program.texture_id_);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureBitmap.Width, TextureBitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, TextureData.Scan0);
            ErrorCode e = GL.GetError();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            glControl.Invalidate();
            TextureBitmap.UnlockBits(TextureData);
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            glControl_loaded = true;
            GL.Enable(EnableCap.Texture2D);
            GL.GenTextures(1, out Program.texture_id_);
            GL.BindTexture(TextureTarget.Texture2D, Program.texture_id_);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ClearColor(Color.SkyBlue);

            GL.Enable(EnableCap.Texture2D);
            GL.GenTextures(1, out AR_model_texture_id);
            GL.BindTexture(TextureTarget.Texture2D, AR_model_texture_id);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            System.Drawing.Imaging.BitmapData TextureData = AR_model_texture.LockBits(new System.Drawing.Rectangle(0, 0, AR_model_texture.Width, AR_model_texture.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, AR_model_texture.Width, AR_model_texture.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, TextureData.Scan0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            AR_model_texture.UnlockBits(TextureData);
        }

        private void DrawVideo()
        {
            float pseyeAspectRatio = 640.0f / 480.0f;
            float controlAspectRatio = 640.0f / 480.0f;
            float fullScreenScaleXY = controlAspectRatio / pseyeAspectRatio;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Scale(1.0f / controlAspectRatio, 1.0f, 1.0f);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Color4(1.0, 1.0, 1.0, 1.0);
            GL.Disable(EnableCap.DepthTest);

            GL.BindTexture(TextureTarget.Texture2D, Program.texture_id_);

            GL.Begin(BeginMode.TriangleStrip);
            GL.TexCoord2(0, 1);
            GL.Vertex3(-pseyeAspectRatio, -1.0, 0.0);
            GL.TexCoord2(0, 0);
            GL.Vertex3(-pseyeAspectRatio, 1.0, 0.0);
            GL.TexCoord2(1, 1);
            GL.Vertex3(pseyeAspectRatio, -1.0, 0.0);
            GL.TexCoord2(1, 0);
            GL.Vertex3(pseyeAspectRatio, 1.0, 0.0);
            GL.End();

            GL.Enable(EnableCap.DepthTest);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void SetupMatrices(int gem_num, float aspect_ratio)
        {
            const float yFov = 0.837758041f;                 // 48 degrees vertical field-of-view
            const float near = 10, far = 3000;         // 50cm to 3m tracking limits (but reduce near to avoid object clipping)

            OpenTK.Matrix4 projection;
            projection = OpenTK.Matrix4.CreatePerspectiveFieldOfView(yFov, aspect_ratio, near, far);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref projection);

            float cameraPitchAngle = AR_state.gemStates[gem_num].camera_pitch_angle;

            Console.WriteLine("{0}", cameraPitchAngle);

            OpenTK.Matrix4 rotation, scale, lookat;

            rotation = OpenTK.Matrix4.CreateRotationX(cameraPitchAngle);
            scale = OpenTK.Matrix4.Scale(-1.0f, 1.0f, 1.0f);

            lookat = OpenTK.Matrix4.LookAt(0, 0, 0,
                                           0, 0, 1,
                                           0, 1, 0);
            OpenTK.Matrix4 view = lookat * (scale * rotation);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref view);
            OpenTK.Matrix4 mv_mat;
            GL.GetFloat(GetPName.ModelviewMatrix, out mv_mat);
            //Console.WriteLine("{0}", mv_mat);
            OpenTK.Vector3 gem_position = new OpenTK.Vector3(AR_state.gemStates[gem_num].pos.x, AR_state.gemStates[gem_num].pos.y, AR_state.gemStates[gem_num].pos.z);
            OpenTK.Quaternion gem_rotation = new OpenTK.Quaternion(AR_state.gemStates[gem_num].quat.x, AR_state.gemStates[gem_num].quat.y, AR_state.gemStates[gem_num].quat.z, AR_state.gemStates[gem_num].quat.w);
            gem_rotation.Normalize();
            OpenTK.Matrix4 gem_rotation_matrix = OpenTK.Matrix4.Rotate(gem_rotation);
            OpenTK.Matrix4 gem_translation_matrix = OpenTK.Matrix4.CreateTranslation(gem_position);
            OpenTK.Matrix4 gem_model_matrix = gem_rotation_matrix;
            gem_model_matrix.M41 = gem_position.X;
            gem_model_matrix.M42 = gem_position.Y;
            gem_model_matrix.M43 = gem_position.Z;
            gem_model_matrix.M44 = 1.0f;
            GL.MultMatrix(ref gem_model_matrix);
        }

        public void DrawSphere(float Radius, uint Precision)
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
                    GL.TexCoord2(i * OneThroughPrecision, 2.0f * (j + 1) * OneThroughPrecision);
                    GL.Vertex3(Position);

                    Normal.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    Normal.Y = (float)Math.Sin(theta1);
                    Normal.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    Position.X = Radius * Normal.X;
                    Position.Y = Radius * Normal.Y;
                    Position.Z = Radius * Normal.Z;

                    GL.Normal3(Normal);
                    GL.TexCoord2(i * OneThroughPrecision, 2.0f * j * OneThroughPrecision);
                    GL.Vertex3(Position);
                }
                GL.End();
            }
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl_loaded) // Play nice
                return;


            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (Program.moveClient != null && Program.client_connected) {
                DrawVideo();
                SetupMatrices(0, 4.0f / 3.0f);

                GL.Color4(1.0, 1.0, 1.0, 1.0);
                if (AR_model.IsLoaded())
                {
                    AR_model.Draw(AR_model_texture_id);
                }
                else
                {
                    DrawSphere(100.0f, 16);
                }
                
            }

            glControl.SwapBuffers();
        }

        private void connectButton_Click(object sender, EventArgs e)
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
        }

        private void pauseButton_Click(object sender, EventArgs e)
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
        }

        private void ImagePausedToggleButton_Click(object sender, EventArgs e)
        {
            if (Program.image_paused)
            {
                Program.moveClient.CameraFrameResume();
                Program.image_paused = false;
            }
            else
            {
                Program.moveClient.CameraFramePause();
                Program.image_paused = true;
            }
        }

        private void imageSliceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int selected_index = comboBox.SelectedIndex;
            string selected_name = (string)comboBox.SelectedItem;
            Console.WriteLine("{0} @ {1}", selected_name, selected_index);
            uint slices = (uint)selected_index + 1;
            if (slices < 1)
            {
                slices = 1;
            }
            else if (slices > 8)
            {
                slices = 8;
            }
            if (Program.moveClient != null)
            {
                Program.moveClient.CameraFrameSetNumSlices(slices);
                Program.moveClient.SetNumImageSlices((int)slices);
            }
        }

        private void textBoxServerPort_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxServerAddress_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public class ObjModel
    {
        public class FaceVertexIndex
        {
            public int vertex;
            public int texture;
            public int normal;
        }

        protected List<OpenTK.Vector3> vertices;
        protected List<OpenTK.Vector3> normals;
        protected List<OpenTK.Vector2> texcoords;
        protected List<FaceVertexIndex> indexes;
        protected bool is_loaded;

        public ObjModel()
        {
            is_loaded = false;
            vertices = new List<OpenTK.Vector3>();
            normals = new List<OpenTK.Vector3>();
            texcoords = new List<OpenTK.Vector2>();
            indexes = new List<FaceVertexIndex>();
        }

        ~ObjModel()
        {

        }

        private void ReadFaceVertexIndex(String text, ref FaceVertexIndex v)
        {
            String[] ints = text.Split('/');
            v.vertex = int.Parse(ints[0]);
            v.texture = int.Parse(ints[1]);
            v.normal = int.Parse(ints[2]);
        }

        public bool IsLoaded()
        {
            return is_loaded;
        }

        public bool Load(String filename)
        {
            StreamReader re = null;
            try
            {
                re = File.OpenText(filename);
            }
            catch
            {
                // can't open file
                return false;
            }

            int line_count = 0;

            do
            {
                String line = null;
                try
                {
                    line = re.ReadLine();
                }
                catch
                {
                    // eof
                    break;
                }
                if (line == null)
                {
                    break;
                }

                line_count++;
                String[] words = line.Split(' ');
                if (words[0] == "v")
                {
                    OpenTK.Vector3 v3;
                    v3.X = float.Parse(words[1]);
                    v3.Y = float.Parse(words[2]);
                    v3.Z = float.Parse(words[3]);
                    vertices.Add(v3);
                }
                else if (words[0] == "vt")
                {
                    OpenTK.Vector2 v2;
                    v2.X = float.Parse(words[1]);
                    v2.Y = 1.0f - float.Parse(words[2]);
                    texcoords.Add(v2);
                }
                else if (words[0] == "vn")
                {
                    OpenTK.Vector3 v3;
                    v3.X = float.Parse(words[1]);
                    v3.Y = float.Parse(words[2]);
                    v3.Z = float.Parse(words[3]);
                    normals.Add(v3);
                }
                else if (words[0] == "f")
                {
                    FaceVertexIndex i1 = new FaceVertexIndex();
                    FaceVertexIndex i2 = new FaceVertexIndex();
                    FaceVertexIndex i3 = new FaceVertexIndex();
                    FaceVertexIndex i4 = new FaceVertexIndex();
                    switch (words.Length)
                    {
                        case 4:
                            // three indices
                            ReadFaceVertexIndex(words[1], ref i1);
                            ReadFaceVertexIndex(words[2], ref i2);
                            ReadFaceVertexIndex(words[3], ref i3);
                            indexes.Add(i1);
                            indexes.Add(i2);
                            indexes.Add(i3);
                            break;
                        case 5:
                            // four indices, quad, two triangles
                            ReadFaceVertexIndex(words[1], ref i1);
                            ReadFaceVertexIndex(words[2], ref i2);
                            ReadFaceVertexIndex(words[3], ref i3);
                            ReadFaceVertexIndex(words[4], ref i4);
                            indexes.Add(i1);
                            indexes.Add(i2);
                            indexes.Add(i3);

                            indexes.Add(i1);
                            indexes.Add(i3);
                            indexes.Add(i4);
                            break;
                        default:
                            Console.WriteLine("Unhandled face index size {0}", words.Length);
                            break;
                    }
                }
            } while (true);

            /*
            foreach (FaceVertexIndex fvi in indexes)
            {
                // Obj indices start at 1
                fvi.vertex -= 1;
                fvi.texture -= 1;
                fvi.normal -= 1;
                Console.WriteLine("{0}, {1}, {2}", fvi.vertex, fvi.texture, fvi.normal);
            }
             */

            is_loaded = true;
            return true;
        }

        public void Draw(int texture_id)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture_id);
            GL.Begin(BeginMode.Triangles);
            foreach (FaceVertexIndex fvi in indexes)
            {
                GL.Normal3(normals[fvi.normal-1]);
                GL.TexCoord2(texcoords[fvi.texture-1]);
                GL.Vertex3(vertices[fvi.vertex-1]);
            }
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}

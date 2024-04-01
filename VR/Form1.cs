namespace VR
{
    using SharpGL;
    using SharpGLTF.Geometry.VertexTypes;
    using SharpGLTF.Materials;
    using SharpGLTF.Scenes;
    using SharpGLTF.Schema2;
    using System;
    using System.Drawing;
    using System.Numerics;

    public partial class Form1 : Form
    {
        private Scene _scene;
        OpenGLControl openGLControl;
        public Form1()
        {
            InitializeComponent();
            openGLControl = new OpenGLControl();
            this.Controls.Add(openGLControl);
            // Load a glTF model
            _scene = SharpGLTF.Schema2.ModelRoot.Load("D:\\godot\\project\\game1\\AvatarSample_A.vrm").DefaultScene;
            //this.Controls.Add(_scene);

            // Set up OpenGL control
            openGLControl.OpenGLInitialized += OpenGLControl_OpenGLInitialized;
            openGLControl.Resized += OpenGLControl_Resized;
            openGLControl.OpenGLDraw += OpenGLControl_OpenGLDraw;
        }
        private void OpenGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            OpenGL gl = openGLControl.OpenGL;

            gl.ClearColor(0, 0, 0, 0);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
        }

        private void OpenGLControl_Resized(object sender, EventArgs e)
        {
            // Set up the projection matrix
            OpenGL gl = openGLControl.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60.0, (double)Width / (double)Height, 0.1, 100.0);
            gl.LookAt(3, 3, 3, 0, 0, 0, 0, 1, 0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        private void OpenGLControl_OpenGLDraw(object sender, RenderEventArgs args)
        {
            OpenGL gl = openGLControl.OpenGL;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            // TODO: Render your glTF model here using OpenGL calls
            // You will need to traverse the scene, get the mesh primitives,
            // and then draw them using the vertex data and indices.

            //openGLControl.SwapBuffers();
        }
    }
}

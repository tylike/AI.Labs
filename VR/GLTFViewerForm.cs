namespace VR
{
    using SharpGL;
    using SharpGLTF.Schema2;
    using System;
    using System.Windows.Forms;
    public class GLTFViewerForm : Form
    {
        private OpenGLControl openGLControl;
        private Scene gltfScene;

        public GLTFViewerForm()
        {
            // 初始化OpenGL控件
            InitializeOpenGLControl();

            // 加载GLTF模型
            LoadGLTFModel("D:\\3d\\CesiumMan.gltf");
        }

        private void InitializeOpenGLControl()
        {
            // 创建OpenGL控件实例
            openGLControl = new OpenGLControl
            {
                Dock = DockStyle.Fill
            };

            // 将控件添加到窗体
            Controls.Add(openGLControl);

            // 设置OpenGL初始化事件
            openGLControl.OpenGLInitialized += OpenGLControl_OpenGLInitialized;
            // 设置OpenGL绘制事件
            openGLControl.OpenGLDraw += OpenGLControl_OpenGLDraw;
        }

        private void LoadGLTFModel(string modelPath)
        {
            // 使用SharpGLTF.Toolkit加载GLTF模型
            gltfScene = ModelRoot.Load(modelPath).DefaultScene;
            if (gltfScene == null)
            {
                throw new InvalidOperationException("GLTF模型没有默认场景。");
            }
        }

        private void OpenGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            // 设置OpenGL初始化参数
            var gl = openGLControl.OpenGL;
            gl.ClearColor(0, 0, 0, 0);
        }

        private void OpenGLControl_OpenGLDraw(object sender, RenderEventArgs args)
        {
            // 绘制场景
            var gl = openGLControl.OpenGL;

            // 清除颜色和深度缓冲区
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            // TODO: 添加绘制GLTF模型的逻辑
            // 设置摄像机视角、投影等
            SetCamera(gl);

            // 遍历场景中的所有节点并绘制
            DrawScene(gltfScene, gl);

            // 刷新OpenGL控件
            openGLControl.Invalidate();
        }
    }
}

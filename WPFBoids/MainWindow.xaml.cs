using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using SharpGL;
using SharpGL.WPF;
using SharpGL.SceneGraph;
using SharpGL.Enumerations;

namespace WFPBoids
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenGLControl GLControl;
        public MainWindow()
        {
            InitializeComponent();

            GLControl = new OpenGLControl();
            GLControl.FrameRate = 60;
            GLControl.DrawFPS = true;
            MainGrid.Children.Add(GLControl);

            GLControl.OpenGLDraw += GLControl_OpenGLDraw;
            GLControl.Resized += GLControl_Resized;
            GLControl.OpenGLInitialized += GL_Init;


            for (int i = 0; i < 300; i++)
                Boids.Add(new Boid(0, 0, 0));
        }




        void GL_Init(object sender, OpenGLRoutedEventArgs args)
        {
            OpenGL GL = args.OpenGL;
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.ShadeModel(ShadeModel.Smooth);
            GL.Enable(OpenGL.GL_BLEND);
            GL.BlendFunc(BlendingSourceFactor.SourceAlpha, BlendingDestinationFactor.OneMinusSourceAlpha);
            GL.Enable(OpenGL.GL_POINT_SMOOTH);

            GL.PointSize(15f);

        }

        List<Boid> Boids = new List<Boid>();

        List<Boid> Predators = new List<Boid>();
        const float left_border = -500.0f;
        const float right_border = 500.0f;
        const float top_border = 500.0f;
        const float bottom_border = -500.0f;

        void GLControl_Resized(object sender, OpenGLRoutedEventArgs args)
        {
            OpenGL GL = args.OpenGL;
            GL.Clear(OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_COLOR_BUFFER_BIT);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho2D(
                left: left_border,
                right: right_border,
                bottom: bottom_border,
                top: top_border

                );
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        void GLControl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            Boid.SetOptions(new BoidParameters
            {
                AlignPower = (float)AlignmentSlider.Value,
                CenterPower = (float)CenteringSlider.Value,
                CohesionPower = (float)CohesionSlider.Value,
                DesiredSeparation = (float)SeparationRadiusSlider.Value,
                FriendRadius = (float)SearchRadiusSlider.Value,
                SeparationPower = (float)SeparationSlider.Value,
                SpeedLimit = (float)SpeedLimitSlider.Value,
                AvoidPower = (float)AvoidingSlider.Value
            });

            OpenGL GL = GLControl.OpenGL;
            GL.Clear(OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_COLOR_BUFFER_BIT);

            Parallel.ForEach(Boids, x =>
            {
                x.Run(Boids);
                x.Avoid(Predators, 80);
                x.Border(
                    left: left_border,
                    right: right_border,
                    bottom: bottom_border,
                    top: top_border
                    );
            });

            foreach (Boid B in Boids)
            {

                GL.Color(B.UnitColor);
                GL.PushMatrix();
                GL.Translate(B.Position.X, B.Position.Y, B.Position.Z);

                Double d = Math.Atan2(B.Velocity.Y, B.Velocity.X) + Math.PI / 2;
                d = 180 * d / Math.PI;
                GL.Rotate(0f, 0f, (float)d);

                GL.Begin(BeginMode.Polygon);
                GL.Vertex(0, -6, 0);
                GL.Vertex(-4, 6, 0);
                GL.Vertex(4, 6, 0);
                GL.End();
                GL.PopMatrix();
            }

            Parallel.ForEach(Predators, x =>
            {
                x.Run(Boids);
                x.Border(
                    left: left_border,
                    right: right_border,
                    bottom: bottom_border,
                    top: top_border
                    );
            });

            foreach (Boid P in Predators)
            {
                GL.Color(P.UnitColor);

                GL.Begin(BeginMode.Points);
                GL.Vertex(P.Position);
                GL.End();

            }
        }

        private void RepeatButton_AddBoid(object sender, RoutedEventArgs e)
        {
            Boids.Add(new Boid(0, 0, 0));
        }

        private void RepeatButton_AddPredator(object sender, RoutedEventArgs e)
        {
            Predators.Add(new Predator(0, 0, 0));
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Predators.Clear();
            Boids.Clear();
        }
    }
}

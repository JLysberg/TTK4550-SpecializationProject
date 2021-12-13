using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using EyeTrackingAPIWrapper;

namespace DynamicEnvironment
{
    public class GraphicsWindow : GameWindow
    {
        // Constant setting parameters
        private const string VERSION = "VT";
        private const double MINMOVETIME = 2.0d; // 2
        private const double MAXMOVETIME = 5.0d; // 5
        private const float MINMOVEDIST = 0.5f;
        private const float MAXMOVEDIST = 1.0f; // 1.0f
        private const float TESTRATE = 0.0f;
        private const double WARPRATE = 0.5f;       // 0.5 => Warp every ~4s (avg)
        private const double MOVERATE = 0.2f;       // 0.2 => Move every ~10s (avg)

        private List<Dot.Displacement.DpType> dpTypes = new List<Dot.Displacement.DpType> { };

        // Output directory/file parameters
        private string timeSeriesOutputDirectory = Path.Combine(
            Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).
            Parent.Parent.Parent.FullName, "Outputs\\ETTimeSeries\\Dynamic" + VERSION + "\\");
        private string userID;

        // Time
        private int frameCount = 0;
        private int warpFrameCount = 0;
        private double t;

        // Random number generator
        public Random rand = new Random();

        // OpenTK drawing
        Dot gazeDot;
        private Shader shader;

        // Test state parameters
        int testStateCounter = 0;
        bool testStateInit = true;

        private class Dot
        {
            private const int RES = 20;
            private const float RADIUS = 0.005f;
            public float[] vertices = new float[3 * RES];

            public Displacement Dp = new Displacement();

            public int VertexBuffer;
            public int VertexArray;

            private void generateCircle(float aspectX, float aspectY)
            {
                for (int i = 0; i < 3; i++)
                {
                    vertices[i] = 0.0f;
                }
                for (int i = 3; i < vertices.Length; i += 3)
                {
                    double rad = (double)((i - 3) / (double)(vertices.Length - 6) * 2 * Math.PI);

                    vertices[i] = RADIUS * (float)Math.Cos(rad) * aspectY / aspectX;
                    vertices[i + 1] = RADIUS * (float)Math.Sin(rad);
                }
            }

            public Dot(float aspectX, float aspectY)
            {
                generateCircle(aspectX, aspectY);
            }

            public class Displacement
            {
                public Func<double, Point> Position;
                public bool Stationary;

                public Displacement(DpType mt, Point prevPnt,
                    double t0, double dt, float minD, float maxD)
                {
                    Point[] pnts;
                    Point nextPnt = new Point(minD, maxD, prevPnt);
                    Stationary = false;
                    switch (mt)
                    {
                        case DpType.LINEAR:
                            pnts = new Point[] 
                            { 
                                prevPnt, 
                                nextPnt 
                            };
                            break;
                        case DpType.QUADRATIC:
                            pnts = new Point[] 
                            {
                                prevPnt,
                                new Point(minD, prevPnt, nextPnt), 
                                nextPnt 
                            };
                            break;
                        case DpType.CUBIC:
                            Point tmpPnt = new Point(minD, prevPnt, nextPnt);
                            pnts = new Point[]
                            {
                                prevPnt,
                                tmpPnt,
                                new Point(minD, maxD, tmpPnt),
                                nextPnt
                            };
                            break;
                        default:
                            pnts = new Point[] { };
                            break;
                    }
                    Position = (t) =>
                    {
                        if (t < t0 + dt) return getCasteljauPoint(
                            pnts, pnts.Length - 1, 0, (float)((t - t0) / dt));
                        else 
                        {
                            Stationary = true;
                            return nextPnt;
                        }
                    };
                }

                public Displacement(Point prevPnt, Point nextPnt, double t0, double dt)
                {
                    Point[] pnts;
                    Stationary = false;
                    pnts = new Point[]
                    {
                        prevPnt,
                        nextPnt
                    };
                    Position = (t) =>
                    {
                        if (t < t0 + dt) return getCasteljauPoint(
                            pnts, pnts.Length - 1, 0, (float)((t - t0) / dt));
                        else
                        {
                            Stationary = true;
                            return nextPnt;
                        }
                    };
                }

                public Displacement(Point prevPnt, float minD, float maxD)
                {
                    Stationary = true;
                    Point pnt = new Point(minD, maxD, prevPnt);
                    Position = (t) =>
                    {
                        return pnt;
                    };
                }

                public Displacement()
                {
                    Stationary = true;
                    Position = (t) =>
                    {
                        return new Point(0.0f, 0.0f);
                    };
                }

                private Point getCasteljauPoint(Point[] pts, int r, int i, double t)
                {
                    if (r == 0) return pts[i];

                    Point p1 = getCasteljauPoint(pts, r - 1, i, t);
                    Point p2 = getCasteljauPoint(pts, r - 1, i + 1, t);

                    return new Point(
                        (float)((1 - t) * p1.X + t * p2.X), 
                        (float)((1 - t) * p1.Y + t * p2.Y));
                }

                public enum DpType
                {
                    LINEAR,
                    QUADRATIC,
                    CUBIC
                }
            }
        }

        private class Point
        {
            private Random rand = new Random();
            public float X;
            public float Y;
            public Point(float x, float y) { X = x; Y = y; }
            public Point(float minDistance, float maxDistance, Point fromPnt)
            {
                Point pnt;
                float distance;

                do
                {
                    pnt = new Point();
                    distance = euclidean(fromPnt, pnt);
                } while (distance < minDistance || distance > maxDistance);

                X = pnt.X;
                Y = pnt.Y;
            }

            public Point(float minDistance, Point fromPnt, Point toPnt)
            {
                Point pnt = new Point(minDistance, euclidean(fromPnt, toPnt), fromPnt);
                X = pnt.X;
                Y = pnt.Y;
            }

            public Point()
            {
                X = (float)rand.NextDouble() * 1.8f - 0.9f;
                Y = (float)rand.NextDouble() * 1.8f - 0.9f;
            }

            private float euclidean(Point fromPnt, Point toPnt)
            {
                return (float)Math.Sqrt(
                    Math.Pow(fromPnt.X - toPnt.X, 2) + 
                    Math.Pow(fromPnt.Y - toPnt.Y, 2));
            }
        }

        private enum State
        {
            FIXED,
            WARP,
            WARPING,
            MOVE,
            MOVING,
            TEST,
        }

        private State state;

        public GraphicsWindow(int winAspectX, int winAspectY, string userID, 
            bool enLinMove, bool enQuadMove, bool enCubicMove) : 
            base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.userID = userID;
            string labellingID = VERSION
                + "-WARP"
                + (enLinMove ? ",LIN" : "")
                + (enQuadMove ? ",QUAD" : "")
                + (enCubicMove ? ",CUBIC" : "");

            timeSeriesOutputDirectory = Path.Combine(
                Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).
                Parent.Parent.Parent.FullName, "Outputs\\ETTimeSeries\\Dynamic" + labellingID + "\\");
            Directory.CreateDirectory(timeSeriesOutputDirectory);

            base.Title = "Dynamic labelling environment " + labellingID;
            base.RenderFrequency = 60.0f;
            base.Size = new Vector2i(winAspectX, winAspectY);
            base.WindowState = WindowState.Fullscreen;
            base.WindowBorder = WindowBorder.Hidden;

            if (enLinMove) dpTypes.Add(Dot.Displacement.DpType.LINEAR);
            if (enQuadMove) dpTypes.Add(Dot.Displacement.DpType.QUADRATIC);
            if (enCubicMove) dpTypes.Add(Dot.Displacement.DpType.CUBIC);

            gazeDot = new Dot(winAspectX, winAspectY);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //if (KeyboardState.IsKeyDown(Keys.M))
            //{
                
            //}

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // Clear window
            GL.Clear(ClearBufferMask.ColorBufferBit);

            t = (float) (frameCount++ / RenderFrequency);
            if (t == 0)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
            if (t == 3)
            {
                // Set window clear color to black
                GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

                ETDataStream.Open();
            }

            switch (state)
            {
                case State.FIXED:
                    ETDataStream.SetLabel(GazeData.LabelType.FIXATION);

                    //state = stateNext();
                    // Only change from FIXED state in 2s intervals
                    if (frameCount % RenderFrequency*2 == 0) {
                        state = stateNext();
                    }
                    break;
                case State.WARP:
                    ETDataStream.SetLabel(GazeData.LabelType.SACCADE);

                    warpGazeDot();
                    warpFrameCount = 0;

                    state = State.WARPING;
                    break;
                case State.WARPING:
                    if (warpFrameCount++ > 3) state = State.FIXED;
                    break;
                case State.MOVE:
                    ETDataStream.SetLabel(GazeData.LabelType.SMOOTH);
                    
                    int randDpIndex = rand.Next(dpTypes.Count);
                    double randDt = rand.NextDouble() * (MAXMOVETIME - MINMOVETIME) + MINMOVETIME;
                    moveGazeDot(dpTypes[randDpIndex], randDt);

                    state = State.MOVING;
                    break;
                case State.MOVING:
                    if (gazeDot.Dp.Stationary) state = State.FIXED;
                    break;
                case State.TEST:
                    if (testStateInit)
                    {
                        moveGazeDotTo(new Point(.0f, .0f), 3.0d);
                        testStateInit = false;
                        state = State.MOVING;
                        break;
                    }
                    state = State.FIXED;
                    break;
                    switch (testStateCounter++ % 9)
                    {
                        case 0:
                            //moveGazeDotTo(new Point(1.0f, 1.0f), 3.0d * 16 / 10);
                            moveGazeDotTo(new Point(1.0f, 1.0f), 0.0d);
                            break;
                        case 1:
                            //moveGazeDotTo(new Point(1.0f, -1.0f), 3.0d);
                            moveGazeDotTo(new Point(1.0f, -1.0f), 0.0d);
                            break;
                        case 2:
                            //moveGazeDotTo(new Point(-1.0f, -1.0f), 3.0d * 16 / 10);
                            moveGazeDotTo(new Point(-1.0f, -1.0f), 0.0d);
                            break;
                        case 3:
                            //moveGazeDotTo(new Point(-1.0f, 1.0f), 3.0d);
                            moveGazeDotTo(new Point(-1.0f, 1.0f), 0.0d);
                            break;
                        case 4:
                            moveGazeDotTo(new Point(0.0f, 0.0f), 0.0d);
                            break;
                        case 5:
                            //moveGazeDotTo(new Point(1.0f, -1.0f), 3.0d * 16 / 10);
                            moveGazeDotTo(new Point(1.0f, -1.0f), 0.0d);
                            break;
                        case 6:
                            //moveGazeDotTo(new Point(1.0f, 1.0f), 3.0d);
                            moveGazeDotTo(new Point(1.0f, 1.0f), 0.0d);
                            break;
                        case 7:
                            //moveGazeDotTo(new Point(-1.0f, -1.0f), 3.0d * 16 / 10);
                            moveGazeDotTo(new Point(-1.0f, -1.0f), 0.0d);
                            break;
                        case 8:
                            //moveGazeDotTo(new Point(-1.0f, 1.0f), 3.0d);
                            moveGazeDotTo(new Point(-1.0f, 1.0f), 0.0d);
                            break;
                    }
                    state = State.MOVING;
                    break;
            }

            drawGazeDot();

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnLoad()
        {
            // Set window clear color
            GL.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);

            // Generate and bind vertex array
            gazeDot.VertexArray = GL.GenVertexArray();
            GL.BindVertexArray(gazeDot.VertexArray);

            // Generate and bind vertex buffer
            gazeDot.VertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, gazeDot.VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, gazeDot.vertices.Length * sizeof(float),
                gazeDot.vertices, BufferUsageHint.StaticDraw);

            // Define shader object
            shader = new Shader();

            // Define how OpenGL should interpret vertex data
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            base.OnLoad();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            OnUnload();

            base.OnClosing(e);
        }

        protected override void OnUnload()
        {
            // Unbind and delete buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(gazeDot.VertexBuffer);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(gazeDot.VertexArray);

            // Dispose of shader
            shader.Dispose();

            // Close ET stream and dump data to generated path
            string timeseriesPath = timeSeriesOutputDirectory + 
                "ts_" + userID + "_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".csv";
            ETDataStream.Close(true, timeseriesPath);

            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);

            base.OnResize(e);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            switch(e.Key)
            {
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Q:
                    warpGazeDot();
                    break;
                case Keys.W:
                    moveGazeDot(Dot.Displacement.DpType.LINEAR, 3);
                    break;
                case Keys.E:
                    moveGazeDot(Dot.Displacement.DpType.QUADRATIC, 3);
                    break;
                case Keys.R:
                    moveGazeDot(Dot.Displacement.DpType.CUBIC, 3);
                    break;
                case Keys.Space:
                    ETDataStream.SetLabel(GazeData.LabelType.UNDEFINED);
                    break;
            }

            base.OnKeyDown(e);
        }

        private void drawGazeDot()
        {
            // Apply shader
            shader.Use();

            // Create and apply translation
            Matrix4 trans = Matrix4.CreateTranslation(
                gazeDot.Dp.Position(t).X, gazeDot.Dp.Position(t).Y, 0.0f);
            int transLocation = GL.GetUniformLocation(shader.Handle, "translation");
            GL.UniformMatrix4(transLocation, true, ref trans);

            // Bind to Circle vertex array and buffer
            GL.BindVertexArray(gazeDot.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, gazeDot.VertexBuffer);

            // Copy circle vertex data to GPU memory
            GL.BufferData(BufferTarget.ArrayBuffer, gazeDot.vertices.Length * sizeof(float),
                gazeDot.vertices, BufferUsageHint.StaticDraw);

            // Draw
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, gazeDot.vertices.Length);
        }

        private State stateNext()
        {
            double randState = rand.NextDouble();
            if (TESTRATE > 0) return State.TEST;
            else if (randState < WARPRATE) return State.WARP;
            else if (randState < WARPRATE + MOVERATE && dpTypes.Count != 0) return State.MOVE;
            else return State.FIXED;
        }

        private void warpGazeDot()
        {
            gazeDot.Dp = new Dot.Displacement(gazeDot.Dp.Position(t), MINMOVEDIST, MAXMOVEDIST);
        }

        private void moveGazeDot(Dot.Displacement.DpType dpType, double dt)
        {
            gazeDot.Dp = new Dot.Displacement(dpType, gazeDot.Dp.Position(t),
                t, dt, MINMOVEDIST, MAXMOVEDIST);
        }

        private void moveGazeDotTo(Point nextPnt, double dt)
        {
            gazeDot.Dp = new Dot.Displacement(gazeDot.Dp.Position(t), nextPnt, t, dt);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

using EyeTrackingAPIWrapper;


namespace Osiris.Fortnite.MatchAnalytics.Heatmap
{
    public class HeatMapDataPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public double Weight { get; set; }
    }


    public class HeatMapImage
    {
        /// <summary>
        /// width of img
        /// </summary>
        private int w;

        /// <summary>
        /// height of img
        /// </summary>
        private int h;

        /// <summary>
        /// gaussian kernel size
        /// </summary>
        private int gSize;

        /// <summary>
        /// gaussian kernel sigma
        /// </summary>
        private double gSigma;

        /// <summary>
        /// radius
        /// </summary>
        private int r;

        /// <summary>
        /// Two dimensional matrix corresponding to data list
        /// </summary>
        private double[,] heatVals;

        /// <summary>
        /// Color map matrix
        /// </summary>
        private byte[] ColorArgbValues;

        /// <summary>
        /// gaussian kernel
        /// </summary>
        private double[,] kernel;

        /// <summary>
        /// color numbers
        /// </summary>
        private const int NUMCOLORS = 1000;

        /// <summary>
        /// width of img
        /// </summary>
        public int W { get => w; set => w = value; }

        /// <summary>
        /// height of img
        /// </summary>
        public int H { get => h; set => h = value; }

        /// <summary>
        /// gaussian kernel
        /// </summary>
        public double[,] Kernel { get => kernel; }

        /// <summary>
        /// Two dimensional matrix corresponding to data list
        /// </summary>
        public double[,] HeatVals { get => heatVals; }

        /// <summary>
        /// construction
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="gSize">gaussian kernel size</param>
        /// <param name="gSigma">gaussian kernel sigma</param>
        public HeatMapImage(int width, int height, int gSize, double gSigma)
        {
            this.w = width;
            this.h = height;
            CreateColorMap();

            //Judge the Gaussian kernel size
            if (gSize < 3 || gSize > 1001)
            {
                throw new Exception("Kernel size is invalid");
            }
            this.gSize = gSize % 2 == 0 ? gSize + 1 : gSize;
            //Gaussian sigma value, calculate the radius r
            this.r = this.gSize / 2;
            this.gSigma = gSigma;
            //Calculate the Gaussian kernel
            kernel = new double[this.gSize, this.gSize];
            this.gaussiankernel();
            //Initialize Gaussian cumulative graph
            heatVals = new double[h, w];
        }

        /// <summary>
        /// Create a ColorMap to query colors based on values
        /// </summary>
        private void CreateColorMap()
        {
            ColorBlend colorBlend = new ColorBlend(6);
            colorBlend.Colors = new Color[6]
            {
                // Colors that make up heat mat, in ascending order of density
                Color.FromArgb(255, 255, 255, 255),
                Color.FromArgb(255, 73, 240, 254),
                Color.FromArgb(255, 74, 253, 79),
                Color.FromArgb(255, 255, 255, 0),
                Color.FromArgb(255, 254, 157, 50),
                Color.FromArgb(255, 255, 0, 0)
            };

            // Sets thresholds for different colors
            colorBlend.Positions = new float[6] { 0.0f, 0.10f, 0.20f, 0.35f, 0.50f, 1.0f };
            Color startColor = colorBlend.Colors[0];
            Color endColor = colorBlend.Colors[colorBlend.Colors.Length - 1];

            using Bitmap colorMapBitmap = new Bitmap(NUMCOLORS, 1, PixelFormat.Format32bppArgb);
            Rectangle colorRect = new Rectangle(0, 0, colorMapBitmap.Width, colorMapBitmap.Height);
            using (Graphics bitmapGraphics = Graphics.FromImage(colorMapBitmap))
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(colorRect, startColor, endColor, LinearGradientMode.Horizontal))
                {
                    brush.InterpolationColors = colorBlend;
                    bitmapGraphics.FillRectangle(brush, colorRect);
                }
            }
            BitmapData colorMapData = colorMapBitmap.LockBits(colorRect, ImageLockMode.ReadOnly, colorMapBitmap.PixelFormat);
            IntPtr colorPtr = colorMapData.Scan0;
            int colorBytes = Math.Abs(colorMapData.Stride) * colorMapBitmap.Height;
            ColorArgbValues = new byte[colorBytes];
            System.Runtime.InteropServices.Marshal.Copy(colorPtr, ColorArgbValues, 0, colorBytes);
            colorMapBitmap.UnlockBits(colorMapData);
        }

        /// <summary>
        /// Gaussian kernel
        /// </summary>
        private void gaussiankernel()
        {
            for (int y = -r, i = 0; i < gSize; y++, i++)
            {
                for (int x = -r, j = 0; j < gSize; x++, j++)
                {
                    kernel[i, j] = Math.Exp(((x * x) + (y * y)) / (-2 * gSigma * gSigma)) / (2 * Math.PI * gSigma * gSigma);
                }
            }
        }

        /// <summary>
        /// Gaussian kernel multiplied by weight
        /// </summary>
        /// <param name="weight"></param>
        /// <returns></returns>
        private double[,] MultiplyKernel(double weight)
        {
            double[,] wKernel = (double[,])kernel.Clone();
            for (int i = 0; i < gSize; i++)
            {
                for (int j = 0; j < gSize; j++)
                {
                    wKernel[i, j] *= weight;
                }
            }
            return wKernel;
        }

        /// <summary>
        /// All weights are normalized and compressed to a value within the number of colors
        /// </summary>
        private void RescaleArray()
        {
            float max = 0;
            foreach (float value in heatVals)
            {
                if (value > max)
                {
                    max = value;
                }
            }

            for (int i = 0; i < heatVals.GetLength(0); i++)
            {
                for (int j = 0; j < heatVals.GetLength(1); j++)
                {
                    heatVals[i, j] *= (NUMCOLORS - 1) / max;
                    if (heatVals[i, j] > NUMCOLORS - 1)
                    {
                        heatVals[i, j] = NUMCOLORS - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Set a group of data at once
        /// </summary>
        /// <param name="datas"></param>
        public void SetDatas(List<HeatMapDataPoint> dataPoints)
        {
            foreach (HeatMapDataPoint point in dataPoints)
            {
                int i, j, tx, ty, ir, jr;
                int radius = gSize >> 1;

                int x = point.X;
                int y = point.Y;
                double[,] kernelMultiplied = MultiplyKernel(point.Weight);

                for (i = 0; i < gSize; i++)
                {
                    ir = i - radius;
                    ty = y + ir;

                    if (ty < 0)
                    {
                        continue;
                    }

                    if (ty >= h)
                    {
                        break;
                    }

                    for (j = 0; j < gSize; j++)
                    {
                        jr = j - radius;
                        tx = x + jr;

                        // skip column
                        if (tx < 0)
                        {
                            continue;
                        }

                        if (tx < w)
                        {
                            heatVals[ty, tx] += kernelMultiplied[i, j];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save data one by one
        /// </summary>
        /// <param name="data"></param>
        //public void SetAData(HeatMapDataPoint point)
        //{
        //    int i, j, tx, ty, ir, jr;
        //    int radius = gSize >> 1;

        //    int x = point.X;
        //    int y = point.Y;
        //    double[,] kernelMultiplied = MultiplyKernel(point.Weight);

        //    for (i = 0; i < gSize; i++)
        //    {
        //        ir = i - radius;
        //        ty = y + ir;

        //        if (ty < 0)
        //        {
        //            continue;
        //        }

        //        if (ty >= h)
        //        {
        //            break;
        //        }

        //        for (j = 0; j < gSize; j++)
        //        {
        //            jr = j - radius;
        //            tx = x + jr;

        //            if (tx < 0)
        //            {
        //                continue;
        //            }

        //            if (tx < w)
        //            {
        //                heatVals[ty, tx] += kernelMultiplied[i, j];
        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// Calculate heatMap based on stored data
        /// </summary>
        /// <returns></returns>
        public Bitmap GetHeatMap()
        {
            RescaleArray();
            Bitmap heatMap = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, heatMap.Width, heatMap.Height);

            BitmapData heatMapData = heatMap.LockBits(rect, ImageLockMode.WriteOnly, heatMap.PixelFormat);
            IntPtr ptrw = heatMapData.Scan0;
            int wbytes = Math.Abs(heatMapData.Stride) * heatMap.Height;
            byte[] argbValuesW = new byte[wbytes];
            System.Runtime.InteropServices.Marshal.Copy(ptrw, argbValuesW, 0, wbytes);


            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    int colorIndex = double.IsNaN(heatVals[i, j]) ? 0 : (int)heatVals[i, j];
                    int index = (i * heatMap.Width + j) * 4;
                    argbValuesW[index] = ColorArgbValues[4 * colorIndex];
                    argbValuesW[index + 1] = ColorArgbValues[4 * colorIndex + 1];
                    argbValuesW[index + 2] = ColorArgbValues[4 * colorIndex + 2];
                    argbValuesW[index + 3] = ColorArgbValues[4 * colorIndex + 3];
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(argbValuesW, 0, ptrw, wbytes);
            heatMap.UnlockBits(heatMapData);
            return heatMap;
        }

        /// <summary>
        /// Output to check the color map.
        /// </summary>
        /// <returns></returns>
        //public Bitmap CheckColorMap()
        //{
        //    Bitmap checkColor = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        //    int step = (NUMCOLORS - w) / h + 1;
        //    for (int i = 0; i < h; i++)
        //    {
        //        for (int j = 0; j < w; j++)
        //        {
        //            if ((i * step) + j >= NUMCOLORS)
        //            {
        //                return checkColor;
        //            }
        //            Color color = Color.FromArgb(ColorArgbValues[i * step * 4],
        //                                            ColorArgbValues[i * step * 4 + 1],
        //                                            ColorArgbValues[i * step * 4 + 2],
        //                                            ColorArgbValues[i * step * 4 + 3]);
        //            checkColor.SetPixel(j, i, color);
        //        }
        //    }
        //    return checkColor;
        //}

        //public void SaveHeatMap(List<ReplayLogEntry> MatchLogs, int height, int width, String path, String name)
        //{
        //    GazePointInteractor datasGen = new GazePointInteractor();

        //    //List<GazePoint> gazePoints = datasGen.CSVToGazePoints(@"C:\Users\Demo\source\repos\HeatMap\HeatMap\FullGame.csv");
        //    List<GazePoint> gazePoints = datasGen.MatchLogHandler(MatchLogs);

        //    List<HeatMapDataPoint> dataPoints = datasGen.ChangeToHeatMapDataPoints(gazePoints, width, height);


        //    // Set datas
        //    HeatMapImage heatMapImage = new HeatMapImage(width, height, 100, 15);
        //    heatMapImage.SetDatas(dataPoints);

        //    // Make map
        //    Bitmap img = heatMapImage.GetHeatMap();

        //    img.Save(name);
        //}
    }

    public class GazePoint
    {
        public float GazePointOnDisplayNormalizedX { get; set; }
        public float GazePointOnDisplayNormalizedY { get; set; }

    }

    public class GazePointInteractor
    {
        public static List<HeatMapDataPoint> ChangeToHeatMapDataPoints(List<GazeData> GazeData, int width, int height)
        {
            List<HeatMapDataPoint> dataPoints = new List<HeatMapDataPoint>();
            foreach (GazeData Data in GazeData)
            {
                int xPosition = (int)(Data.Left.GazePointOnDisplayNormalized_X * width);
                int yPosition = (int)(Data.Left.GazePointOnDisplayNormalized_Y * height);


                dataPoints.Add(new HeatMapDataPoint() { X = xPosition, Y = yPosition, Weight = 1 });
            }
            return dataPoints;
        }

        //public List<GazePoint> CSVToGazePoints(string path)
        //{
        //    using (var reader = new StreamReader(path))
        //    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        //    {
        //        List<GazePoint> records = csv.GetRecords<GazePoint>().ToList();
        //        return records;
        //    }


        //}

        //// Function below must be adjusted to master list
        //public List<GazePoint> MatchLogHandler(List<ReplayLogEntry> MatchLogs)
        //{
        //    List<GazePoint> gazePoints = new List<GazePoint>();
        //    foreach (ReplayLogEntry entry in MatchLogs)
        //    {
        //        float xPosition = 0; // entry.GazePointOnDisplayNormalized_X;
        //        float yPosition = 0; // entry.GazePointOnDisplayNormalized_X;

        //        GazePoint point = new GazePoint();
        //        point.GazePointOnDisplayNormalizedX = xPosition;
        //        point.GazePointOnDisplayNormalizedY = yPosition;

        //        gazePoints.Add(point);
        //    }
        //    return gazePoints;
        //}
    }


    /*
    class Program
    {
        const int WIDTH = 1600;
        const int HEIGHT = 1200;
        static void Main(string[] args)
        {
            //MockDatasGen datasGen;
            //Console.WriteLine("Create some mock datas");
            //datasGen = new MockDatasGen(WIDTH, HEIGHT);
            //List<DataType> datas = datasGen.CreateMockDatas(1);

                CsvHandler datasGen = new CsvHandler();
                List<GazePoint> records = datasGen.CSVToGazePoints(@"C:\Users\Demo\source\repos\HeatMap\HeatMap\FullGame.csv");
                List<HeatMapDataPoint> datas = datasGen.ChangeToHeatMapDataPoints(records, WIDTH, HEIGHT);

            Console.WriteLine("Set datas");
            HeatMapImage heatMapImage = new HeatMapImage(WIDTH, HEIGHT, 100, 15);
            heatMapImage.SetDatas(datas);
            Console.WriteLine("Calculate and generate heatmap");
            Bitmap img = heatMapImage.GetHeatMap();
            img.Save("heat_map.png");
        }
    }
    */
}
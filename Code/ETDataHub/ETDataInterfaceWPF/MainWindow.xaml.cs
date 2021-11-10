using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using EyeTrackingAPIWrapper;
using Osiris.Fortnite.MatchAnalytics.Heatmap;

namespace ETDataHub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer ETStartupTimer = new Timer();
        string TimeSeriesOutputDirectory = Path.Combine(
            Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).
            Parent.Parent.Parent.FullName, "Outputs\\ETTimeSeries\\Testing\\");
        string VisualsOutputDirectory = Path.Combine(
            Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).
            Parent.Parent.Parent.FullName, "Outputs\\ETVisuals\\");
        string ResourcesDirectory = Path.Combine(
            Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).
            Parent.Parent.Parent.FullName, "Resources\\");
        string UserID = "JLysberg";
        int currentImageIndex;

        public MainWindow()
        {
            InitializeComponent();

            ETStartupTimer.Interval = 3000;
            ETStartupTimer.Elapsed += ETStartupTimer_Elapsed;

            LoadRandomImage();

            Directory.CreateDirectory(VisualsOutputDirectory);
            Directory.CreateDirectory(TimeSeriesOutputDirectory);
        }

        private void LoadRandomImage()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(ResourcesDirectory);
            FileInfo[] imageList = dirInfo.GetFiles();

            int randomIndex;
            do
            {
                randomIndex = new Random().Next(0, imageList.Length);
            } while (randomIndex == currentImageIndex);
            currentImageIndex = randomIndex;

            RunInUIThread(new Action(() =>
            {

                string imageSource = imageList[randomIndex].FullName;
                Uri fileUri = new Uri(imageSource);
                imgStaticEnv.Source = new BitmapImage(fileUri);
            }));
        }

        private void ETStartupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ETStartupTimer.Enabled = false;

            RunInUIThread(new Action(() =>
            {
                tbETStatus.Text = "Tracking gaze";
                tbETStatus.Background = new SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(0xff, 0x00, 0xff, 0x00));
            }));

            ETDataStream.Open();
        }

        private void btnToggleET_Checked(object sender, RoutedEventArgs e)
        {
            ETStartupTimer.Enabled = true;
        }

        private void btnToggleET_Unchecked(object sender, RoutedEventArgs e)
        {
            RunInUIThread(new Action(() =>
            {
                tbETStatus.Text = "Not tracking";
                tbETStatus.Background = new SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(0xff, 0xff, 0x00, 0x00));
            }));

            string timeseriesPath = TimeSeriesOutputDirectory + "ts_" + UserID + "_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".csv";

            ETDataStream.Close(true, timeseriesPath);

            List<GazeData> sessionData = ETDataStream.GetSessionData();
            DrawHeatMap(sessionData);
        }

        private void btnLabelGaze_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ETDataStream.SetLabel(GazeData.LabelType.FIXATION);
        }

        private void btnLabelGaze_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ETDataStream.SetLabel(GazeData.LabelType.SACCADE);
        }

        private void DrawHeatMap(List<GazeData> sessionData)
        {
            string heatmapPath = VisualsOutputDirectory + "hm_" + UserID + "_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png";

            const int WIDTH = 1920;
            const int HEIGHT = 1200;

            List<HeatMapDataPoint> datas = GazePointInteractor.ChangeToHeatMapDataPoints(sessionData, WIDTH, HEIGHT);

            HeatMapImage heatMapImage = new HeatMapImage(WIDTH, HEIGHT, 100, 15);

            heatMapImage.SetDatas(datas);

            Bitmap img = heatMapImage.GetHeatMap();

            img.Save(heatmapPath);
        }

        private void RunInUIThread(Action a)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                a.Invoke();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() => {
                      a.Invoke();
                  }));
            }
        }

        private void btnLabelGaze_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ETDataStream.SetLabel(GazeData.LabelType.BLINK);
        }

        private void btnLabelGaze_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ETDataStream.SetLabel(GazeData.LabelType.FIXATION);
        }
    }
}

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
using OpenCvSharp;
using Basler.Pylon;
using System.Threading;
using System.Drawing;
using System.IO;

namespace WpfopencvTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private bool IsLoop = false;
        private Camera camera = null;
        private bool grabbing = false;
        private PixelDataConverter converter = new PixelDataConverter();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //IsLoop = await StartRollingCamera(0);
            //ImageData.Source = null;
            conShot();
        }

        private void btnFinifh_Click(object sender, RoutedEventArgs e)
        {
            grabbing = false;
            //IsLoop = false;
        }

        /*private async Task<bool> StartRollingCamera(int index)
        {
            //カメラ画像取得用のVideoCaptureのインスタンス生成
            var capture = new VideoCapture(index);

            //カメラの接続確認
            if (!capture.IsOpened())
            {
                MessageBox.Show("Can't use camera.");
                return false;
            }

            //カメラの映像取得
            IsLoop = true;
            using (capture)
            using (Mat img = new Mat())
            {
                await Task.Run(() =>
                {
                    while (IsLoop)
                    {
                        capture.Read(img);

                        if (img.Empty()) break;
                        Dispatcher.Invoke(() => ImageData.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(img));
                    }
                });
            }
            return false;
        }*/
        private void CamInit()
        {
            var baslerSerial = BaslerDeviceEnumerator.DeviceConnectList()[0].Serial;

            // Create a new camera object.
            camera = new Camera(baslerSerial);

            grabbing = false;
        }
        private void conShot()
        {
            if (!grabbing)
            {
                grabbing = true;

                try
                {
                    Thread thread = new Thread(() => th_grab(0));
                    thread.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }
        private void th_grab(int snap_wait = 500)
        {
            try
            {
                // Set the acquisition mode to free running continuous acquisition when the camera is opened.
                camera.CameraOpened += Configuration.AcquireContinuous;

                // Open the connection to the camera device.
                camera.Open();

                camera.StreamGrabber.Start();

                while (grabbing)
                {
                    IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);

                    using (grabResult)
                    {
                        if (grabResult.GrabSucceeded)
                        {
                            Mat img = convertIImage2Mat(grabResult);

                            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(img);
                            /*Dispatcher.BeginInvoke((Action)(() =>
                            {
                                using(MemoryStream memory = new MemoryStream())
                                {
                                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                                    memory.Position = 0;
                                    BitmapImage bitmapimage = new BitmapImage();
                                    bitmapimage.BeginInit();
                                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmapimage.StreamSource = memory;
                                    bitmapimage.EndInit();

                                    ImageData.Source = bitmapimage;
                                }
                            }));*/

                            Dispatcher.Invoke(() => ImageData.Source = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(img));
                        }
                    }

                    Thread.Sleep(snap_wait);
                }

                camera.StreamGrabber.Stop();
                camera.Close();
            }
            catch(Exception exception)
            {
                if (camera.IsOpen)
                    camera.Close();

                MessageBox.Show("Exception: {0}" + exception.Message);
            }
        }
        private Mat convertIImage2Mat(IGrabResult grabResult)
        {
            converter.OutputPixelFormat = PixelType.BGR8packed;
            byte[] buffer = grabResult.PixelData as byte[];
            return new Mat(grabResult.Height, grabResult.Width, MatType.CV_8U, buffer);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            CamInit();
        }
    }
}

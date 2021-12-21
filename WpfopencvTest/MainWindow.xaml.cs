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

namespace WpfopencvTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private bool IsLoop = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            IsLoop = await StartRollingCamera(0);
            ImageData.Source = null;
        }

        private void btnFinifh_Click(object sender, RoutedEventArgs e)
        {
            IsLoop = false;
        }

        private async Task<bool> StartRollingCamera(int index)
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
        }
    }
}

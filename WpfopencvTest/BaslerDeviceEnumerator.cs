using System.Collections.Generic;
using System.Linq;
using Basler.Pylon;

namespace WpfopencvTest
{
    public class BaslerDevice
    {
        public string Name { get; set; }
        public string Serial { get; set; }
    }
    public static class BaslerDeviceEnumerator
    {
        public static List<BaslerDevice> DeviceConnectList()
        {
            List<ICameraInfo> allCameras = CameraFinder.Enumerate();
            var listCam = new List<BaslerDevice>();

            foreach (ICameraInfo cameraInfo in allCameras)
            {
                bool newitem = true;
                foreach (var list in listCam)
                {
                    ICameraInfo tag = list as ICameraInfo;

                    // Is the camera found already in the list of cameras?
                    if (tag[CameraInfoKey.FullName] == cameraInfo[CameraInfoKey.FullName])
                    {
                        tag = cameraInfo;
                        newitem = false;
                        break;
                    }

                }
                if (newitem)
                {
                    listCam.Add(new BaslerDevice() { Name = cameraInfo[CameraInfoKey.FriendlyName], Serial = cameraInfo[CameraInfoKey.SerialNumber] });

                }
            }

            return listCam;
        }
    }
}

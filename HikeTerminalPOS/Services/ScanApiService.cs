using System;
using CommunityToolkit.Mvvm.Messaging;
using SocketMobile.Capture;
using HikePOS.Services;

namespace HikePOS.Services
{
    //Start #84747 SOCKET SCANNER S720 Create Comman
    public class ScanApiService : IScanApiService
    {   
        public CaptureHelper Capture = null;
        public bool IsSocketCamSwitchEnable;
        public bool IsSocketCamEnable;
        public void CloseService()
        {
            close();
        }

        public void StartService()
        {
            start();
        }

        void start()
        {
              if (!MainThread.IsMainThread)
            {
                _ = MainThread.InvokeOnMainThreadAsync(() =>
                {
                    StartCaptureClient();
                });
            }
            else
            {
                StartCaptureClient();
            }
        }

        void close()
        {
           if (!MainThread.IsMainThread)
            {
                _ = MainThread.InvokeOnMainThreadAsync(() =>
               {
                   StopCaptureClient();
               });
            }
            else
            {
                StopCaptureClient();
            }
        }

        public void OpenCapture()
        {
            try
            {
                string appid = "ios:com.hikeup";
                string devid = "182eab6c-6826-e711-8103-e0071b715b91";
                string appkey = "MCwCFDr+C8eXhYUvhf7o+4/MZTj1plR3AhQv03W1uY/grhx04ToAaOj3xrkAeA==";
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    appid = "android:com.hikeup";
                    devid = "182eab6c-6826-e711-8103-e0071b715b91";
                    appkey = "MCwCFGGuljyh8QZw0CdpGK0whOikDsTwAhRtxVIUsCZzYyfIwE7k+z32Ey1yDw==";
                }
                Capture.OpenAsync(appid, devid, appkey)
                .ContinueWith(result =>
                {
                    System.Diagnostics.Debug.Print("Open Capture returns {0}", result.Result);
                    if (SktErrors.SKTSUCCESS(result.Result))
                    {
                        Capture.DeviceArrival += OnDeviceArrival;
                        Capture.DeviceRemoval += OnDeviceRemoval;
                        Capture.DecodedData += OnDecodedData;
                    }
                });
            }
            catch(Exception ex)
            {
                ex.Track();
            }
            //return result;
        }

        private async void getSocketCamStatusInit()
        {
            try
            {
                var getStatus = await Capture.GetSocketCamStatusAsync();
                if (getStatus.Status != CaptureHelper.SocketCamStatus.NotSupported)
                {
                    IsSocketCamSwitchEnable = true;
                }

                switch (getStatus.Status)
                {
                    case CaptureHelper.SocketCamStatus.Enable:
                        IsSocketCamEnable = true;
                        break;

                    case CaptureHelper.SocketCamStatus.Disable:
                        IsSocketCamEnable = false;
                        break;

                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }

        private void StartCaptureClient()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    Capture = new CaptureHelper();
                    OpenCapture();
                }
                else
                {
                    if (Capture == null)
                    {
                        Capture = new CaptureHelper();
                        OpenCapture();
                    }
                }
                
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        
        private void StopCaptureClient()
        {
            if (Capture == null)
                return;
            try
            {
                Capture.CloseAsync().ContinueWith(result =>
                {
                    try
                    {
                        if (SktErrors.SKTSUCCESS(result.Result))
                        {
                            Capture.DeviceArrival -= OnDeviceArrival;
                            Capture.DeviceRemoval -= OnDeviceRemoval;
                            Capture.DecodedData -= OnDecodedData;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                });
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        public void OnDeviceArrival(object sender, CaptureHelper.DeviceArgs arrivedDevice)
        {
            try
            {
                if(arrivedDevice?.CaptureDevice == null)
                {
                    return;
                }

                var deviceInfo = arrivedDevice.CaptureDevice.GetDeviceInfo();
                if(deviceInfo != null && deviceInfo.Type != 5)
                {
                    string FriendlyName = deviceInfo.Name;

                    App.Instance.IsBarcodeScannerConnected = true;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("Barcode"));
                    });
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }

        public void OnDeviceRemoval(object sender, CaptureHelper.DeviceArgs removedDevice)
        {
            App.Instance.IsBarcodeScannerConnected = false;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("Barcode"));
            });

        }

        public void OnDecodedData(object sender, CaptureHelper.DecodedDataArgs decodedData)
        {
            if(!string.IsNullOrEmpty(decodedData?.DecodedData?.DataToUTF8String))
            {
                string value = decodedData.DecodedData.DataToUTF8String;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    WeakReferenceMessenger.Default.Send(new Messenger.BarcodeMessenger(value?.Trim()));
                });
            }

        }

    }
}


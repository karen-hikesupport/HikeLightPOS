using System.Diagnostics;
using HikePOS;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using BarcodeScanning;

namespace HikePOS;

public delegate void ScanResultDelegate(string barcode);
public partial class CameraScanPage : BaseContentPage<BaseViewModel>
{
    private bool isLight = false;
    public event EventHandler<List<string>> CameraBarcodeReader;

    public CameraView _CameraBarcodeReaderView
    {
        get
        {
            return CameraBarcodeReaderView;
        }
    }

    CameraView CameraBarcodeReaderView = new CameraView() 
    { 
        AimMode = true ,  
        CaptureQuality = CaptureQuality.High,
        ForceInverted= true,
        TapToFocusEnabled= true,
        BarcodeSymbologies = BarcodeFormats.All,
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Fill         
    } ; 



    public CameraScanPage()
    {
		InitializeComponent();

        if(mainStackView.Children == null || (mainStackView.Children!= null &&  mainStackView.Children.Count <= 0))
        {
            this.CameraBarcodeReaderView.OnDetectionFinished += CameraBarcodeReaderView_BarcodesDetected;
            mainStackView.Children.Add(this.CameraBarcodeReaderView);
        }
        DefineGestures();
    }

    private void DefineGestures()
    {
        var tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += (s, e) =>
        {
            if (!isLight)
            {
                imgLight.Source = "turn_off.png";
                this.CameraBarcodeReaderView.TorchOn = true;
            }
            else
            {
                imgLight.Source = "turn_on.png";
                this.CameraBarcodeReaderView.TorchOn = false;
            }
            isLight = !isLight;
        };
        imgLight.GestureRecognizers.Add(tapGestureRecognizer);
    }


    public async Task SetCamera()
    {
        try
        {
            var permmition = await CheckAndRequestCameraPermission();
            if (permmition != PermissionStatus.Denied)
            {
                stkTouch.IsVisible = true;
                stkPermissions.IsVisible = false;
                this.CameraBarcodeReaderView.CameraEnabled = true;
                this.CameraBarcodeReaderView.PauseScanning = false;
                this.CameraBarcodeReaderView.VibrationOnDetected = true;
            }
            else
            {
                stkPermissions.IsVisible = true;
                stkTouch.IsVisible = false;
            }
        }
        catch(Exception ex)
        {
            ex.Track();
        }
        
    }

    protected async override void OnAppearing()
    {
        await SetCamera();
        base.OnAppearing();
    }
    private void CameraBarcodeReaderView_BarcodesDetected(object sender, OnDetectionFinishedEventArg e)
    {
        if (e.BarcodeResults == null)
        {
            return;
        }
        var data = e.BarcodeResults.FirstOrDefault();
        string result = data?.DisplayValue;
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        result = result.Trim();
        string result2 = string.Empty;
        if((data.BarcodeFormat == BarcodeScanning.BarcodeFormats.Ean13 || data.BarcodeFormat == BarcodeScanning.BarcodeFormats.Upca) && DeviceInfo.Platform == DevicePlatform.iOS && result.Length == 13 && result.StartsWith("0"))
        {
            result2 = result.Remove(0,1);
        }
        //Start ticket:#102270 Couldnt scan barcodes on iPad. By PR
        else if((data.BarcodeFormat == BarcodeScanning.BarcodeFormats.Ean13 || data.BarcodeFormat == BarcodeScanning.BarcodeFormats.Upca) && result.Length == 12 && result.All(char.IsDigit))
        {
            result2 = result;
            result = "0" + result;
        }
        //End ticket:#102270 By PR

        try
        {
            this.CameraBarcodeReaderView.PauseScanning = true;
            DependencyService.Get<Interfaces.IPlayAndVibrate>().PlayBeepAndVibrate();
            List<string> _strarray = new List<string>();
            if(string.IsNullOrEmpty(result2))
            {
                _strarray.Add(result);
            }
            else
            {
                _strarray  = new List<string> () { result , result2};
            }
            CameraBarcodeReader?.Invoke(this, _strarray);
        }
        catch (Exception ex)
        {
            this.CameraBarcodeReaderView.PauseScanning = false;
            ex.Track();
        }

    }

    public static async Task<PermissionStatus> CheckAndRequestCameraPermission()
    {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Camera>();
          
            if (status != PermissionStatus.Granted)
                status =  await Permissions.RequestAsync<Permissions.Camera>();

        return status;
    }

    void Back_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        this.Navigation.PopModalAsync();

    }

    void OpenSettings(System.Object sender, System.EventArgs e)
    {
        AppInfo.Current.ShowSettingsUI();
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.CameraBarcodeReaderView.CameraEnabled = false;
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models.Payment;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
	public partial class CastlesConfigurationViewModel : BaseViewModel
    {
        public event EventHandler<CastlesConfigurationDto> ConfigurationSuccessed;
        public CastlesConfigurationDto ConfigurationModel { get; set; }
        public string _iPAddress { get; set; }
        public string IPAddress { get { return _iPAddress; } set { _iPAddress = value; SetPropertyChanged(nameof(IPAddress)); } }

        public string _port { get; set; }
        public string Port { get { return _port; } set { _port = value; SetPropertyChanged(nameof(Port)); } }

        bool _isStatusDisplay { get; set; }
        public bool IsStatusDisplay { get { return _isStatusDisplay; } set { _isStatusDisplay = value; SetPropertyChanged(nameof(IsStatusDisplay)); } }

        string _status = "Please wait connecting...";
        public string Status
        { get { return _status; }
          set
            {
                _status = value;
                SetPropertyChanged(nameof(Status));
            }
        }

        string _pairUnpairText = "Pair";
        public string PairUnpairText { get { return _pairUnpairText; } set { _pairUnpairText = value; SetPropertyChanged(nameof(PairUnpairText)); } }

        public ICommand PairCommand { get; set; }

        public ICommand ClearConfigurationCommand { get; set; }
        public CastlesConfigurationViewModel()
        {
            PairCommand = new Command(Pair);
            ClearConfigurationCommand = new Command(HandleTapped);
        }
        public override void OnAppearing()
        {
            base.OnAppearing();
            IsStatusDisplay = false;
            if (string.IsNullOrEmpty(ConfigurationModel?.AuthToken))
            {
                PairUnpairText = "Pair";
                Status = "";
            }
            else
            {
                PairUnpairText = "UnPair";
                Status = "Connected";

            }
            IPAddress = ConfigurationModel?.IPAddress;
            Port = ConfigurationModel?.Port;

        }
        #region Methods
        void Pair()
        {
            if (PairUnpairText.Contains("Cancel"))
            {
                PairUnpairText = "Pair";
                IsStatusDisplay = false;
            }
            else
            {
                CastlesConfiguration();
            }
        }
        async void CastlesConfiguration()
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigurationModel?.AuthToken))
                {
                    //using (new Busy(this, true))
                    {
                        if (string.IsNullOrEmpty(IPAddress) || string.IsNullOrWhiteSpace(IPAddress))
                        {
                            App.Instance.Hud.DisplayToast("Please enter IPAddress");
                            return;
                        }
                        if (!IsValidIPv4Regex(IPAddress))
                        {
                            App.Instance.Hud.DisplayToast("Please enter a valid IP address");
                            return;
                        }
                        if (string.IsNullOrEmpty(Port) || string.IsNullOrWhiteSpace(Port))
                        {
                            App.Instance.Hud.DisplayToast("Please enter Port");
                            return;
                        }
                        int port = 0;
                        if (!Int32.TryParse(Port, out port))
                        {
                            App.Instance.Hud.DisplayToast("Please enter valid port");
                            return;
                        }

                        IsStatusDisplay = true;
                        StatusChange("Please wait connecting...");
                        PairUnpairText = "Cancel";
                        await Task.Delay(1);
                        if (await TryConnectAsync(IPAddress,port))
                        {                            
                            OnDeviceReady();
                        }
                        else
                        {
                            StatusChange("There was an error with your configuration.");
                        }

                    }
                }
                else
                {
                    ConfigurationModel = null;
                    Settings.Castlessettings = null;
                    ConfigurationSuccessed?.Invoke(this, null);
                    IsStatusDisplay = false;
                    PairUnpairText = "Pair";
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void StatusChange(string strstatus)
        {
            if(MainThread.IsMainThread)
            {
                 Status = strstatus;
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                   Status = strstatus;
                });
            }
        }

        bool IsValidIPv4Regex(string input)
        {
            var pattern = @"^((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\.){3}" +
                        @"(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)$";
            return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
        }

        async Task<bool> TryConnectAsync(string ip, int port, int timeoutMs = 5000)
        {
            var serverAddress =  System.Net.IPAddress.Parse(ip);
            var endPoint = new IPEndPoint(serverAddress, port);

            using var clientSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            clientSocket.ReceiveTimeout = timeoutMs;
            clientSocket.SendTimeout = timeoutMs;

            var connectTask = clientSocket.ConnectAsync(endPoint);

            if (await Task.WhenAny(connectTask, Task.Delay(timeoutMs)) != connectTask)
            {
                StatusChange("There was an error with your configuration.");
                clientSocket?.Shutdown(SocketShutdown.Both);
                clientSocket?.Close();
                return false;
            }

            try
            {
                await connectTask;
                var result = clientSocket.Connected;
                clientSocket?.Shutdown(SocketShutdown.Both);
                clientSocket?.Close();
                return result;
            }
            catch (SocketException ex)
            {
                Status = $"Socket error: {ex.SocketErrorCode}";
                Console.WriteLine(Status);
                clientSocket?.Shutdown(SocketShutdown.Both);
                clientSocket?.Close();
                return false;
            }
            catch (Exception ex)
            {
                Status = $"Connect failed: {ex.Message}";
                Console.WriteLine(Status);
                  clientSocket?.Shutdown(SocketShutdown.Both);
                clientSocket?.Close();
                return false;
            }
        }


        void HandleTapped()
        {
           IPAddress = string.Empty;
           Port = string.Empty;
        }

        public void OnDeviceReady()
        {
            if(ConfigurationModel == null)
                ConfigurationModel = new CastlesConfigurationDto();

            ConfigurationModel.IPAddress =  IPAddress;
            ConfigurationModel.Port = Port;
            ConfigurationModel.AuthToken = IPAddress + ":" + Port;
            StatusChange("Connected");
            PairUnpairText = "UnPair";
            Settings.Castlessettings = ConfigurationModel;
            ConfigurationSuccessed?.Invoke(this, ConfigurationModel);
        }

        #endregion
    }
}


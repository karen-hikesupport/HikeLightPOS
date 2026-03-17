using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models.Payment;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
	public partial class HikePayConfigurationViewModel : BaseViewModel
    {
        public event EventHandler<NadaPayConfigurationDto> ConfigurationSuccessed;
        public NadaPayConfigurationDto ConfigurationModel { get; set; }
        public string _iPAddress { get; set; }
        public string IPAddress { get { return _iPAddress; } set { _iPAddress = value; SetPropertyChanged(nameof(IPAddress)); } }

        bool _isStatusDisplay { get; set; }
        public bool IsStatusDisplay { get { return _isStatusDisplay; } set { _isStatusDisplay = value; SetPropertyChanged(nameof(IsStatusDisplay)); } }

        string _status = "Please wait ...";
        public string Status
        { get { return _status; }
          set
            {
                _status = value;
                SetPropertyChanged(nameof(Status));
            }
        }

        string _pairUnpairText = "Save";
        public string PairUnpairText { get { return _pairUnpairText; } set { _pairUnpairText = value; SetPropertyChanged(nameof(PairUnpairText)); } }

        public ICommand PairCommand { get; set; }

        public ICommand ClearConfigurationCommand { get; set; }
        public HikePayConfigurationViewModel()
        {
            PairCommand = new Command(Pair);
            ClearConfigurationCommand = new Command(HandleTapped);
        }
        public override void OnAppearing()
        {
            base.OnAppearing();
            IsStatusDisplay = false;
            PairUnpairText = "Save";
            IPAddress = ConfigurationModel?.IpAddress;

        }
        #region Methods
        void Pair()
        {
          CastlesConfiguration();
        }
        void CastlesConfiguration()
        {
            try
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

                IsStatusDisplay = true;
                OnDeviceReady();
                
            }
            catch (Exception ex)
            {
                ex.Track();
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
          // Port = string.Empty;
        }

        public void OnDeviceReady()
        {
            if (ConfigurationModel == null)
                ConfigurationModel = new NadaPayConfigurationDto();

            ConfigurationModel.IpAddress = IPAddress;
           // ConfigurationModel.IsLocalConfigured = true;
            ConfigurationSuccessed?.Invoke(this, ConfigurationModel);
            _= Close();
        }

        #endregion
    }
}


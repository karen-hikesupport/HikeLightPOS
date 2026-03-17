using System.Net;
using System.Net.Sockets;
using System.Text;
using HikePOS.Droid.P2P;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using HikePOS.Models;
using HikePOS.Services;
using Newtonsoft.Json;

[assembly: Dependency(typeof(PeerComunicator))]
namespace HikePOS.Droid.P2P
{
    public class PeerComunicator : IPeerComunicator
    {


        TcpListener server;
        TcpClient client;
        NetworkStream stream;
        const int UdpPort = 5005;
        const int TcpPort = 6000;

        UdpClient udpServer;

        CancellationTokenSource receiveTokenSource;
        public event Action<string> MessageReceived;


        public PeerComunicator()
        {
        }

        #region TCP Communication

        private void StartTcpServer()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    server = new TcpListener(IPAddress.Any, TcpPort);
                    server.Start();
                    System.Diagnostics.Debug.WriteLine("TCP server started, waiting for client...");

                    client = await server.AcceptTcpClientAsync();
                    System.Diagnostics.Debug.WriteLine("Client connected to server");
                    stream = client.GetStream();

                    StartReceiving();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Server error: {ex.Message}");
                }
            });

        }

        private void ConnectToTcpServer(string host)
        {
            _ = Task.Run(async () =>
            {

                try
                {
                    client = new TcpClient();
                    await client.ConnectAsync(host, TcpPort);
                    System.Diagnostics.Debug.WriteLine("Connected to server");
                    stream = client.GetStream();

                    StartReceiving();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Client connection error: {ex.Message}");
                }
            });
        }

        void StartReceiving()
        {
            receiveTokenSource = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                //  var buffer = new byte[1024];
                while (!receiveTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Read 4 bytes for the length
                        byte[] lengthBytes = new byte[4];
                        stream.ReadExactly(lengthBytes, 0, 4); // custom method or loop to read exactly 4 bytes
                        int length = BitConverter.ToInt32(lengthBytes, 0);

                        // Read the full message
                        byte[] buffer = new byte[length];
                        stream.ReadExactly(buffer, 0, length); // custom method again

                        // int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        // if (bytesRead > 0)
                        // {
                        // string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        string message = Encoding.UTF8.GetString(buffer);
                        System.Diagnostics.Debug.WriteLine($"Received: {message}");
                        MessageReceived?.Invoke(message);
                        //}
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Receive error: {ex.Message}");
                        break;
                    }
                }
            }, receiveTokenSource.Token);
        }

        public void SendMessage(string message)
        {
            if (stream == null || !stream.CanWrite) return;

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            stream.Write(lengthBytes, 0, lengthBytes.Length);
            stream.Write(messageBytes, 0, messageBytes.Length);

            // byte[] data = Encoding.UTF8.GetBytes(message);
            // _ = stream.WriteAsync(data, 0, data.Length);
            System.Diagnostics.Debug.WriteLine($"Sent: {message}");
        }
        public void ReadExactly(byte[] buffer, int offset, int count)
        {
            int bytesRead;
            while (count > 0 && (bytesRead = stream.Read(buffer, offset, count)) > 0)
            {
                offset += bytesRead;
                count -= bytesRead;
            }

            if (count > 0)
                throw new EndOfStreamException("Unexpected end of stream");
        }

        public void StartPeerConnection(string storeID, string userName, string storeName)
        {
            StartUdpListener();
            StartTcpServer();
            Settings.UniqueDeviceID = Android.Provider.Settings.Secure.GetString(Platform.CurrentActivity.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
            string zoneInfo = string.Empty;
            if (Settings.StoreZoneAndFormatDetail != null)
                zoneInfo = Newtonsoft.Json.JsonConvert.SerializeObject(Settings.StoreZoneAndFormatDetail);

            var data = new Dictionary<string, string>
                {
                    { "tenantId", storeID },
                    { "userName", userName },
                    { "storeName", storeName },
                    { "ZoneAndFormatDetailDto", zoneInfo },
                    { "ActiveDisplayApp", "true" },
                    { "storeLogo", Settings.StoreShopDto.LogoImagePath ?? "" },

                };
            string json = JsonConvert.SerializeObject(data);
            SendMessage(json);
        }

        public void ReceivePeerData(Action<PeerResponse> peerActionResponse)
        {

            MessageReceived += msg =>
            {
                var peerResponse = new PeerResponse();
                peerResponse.IsMessage = true;
                if (msg != null)
                {
                    //MessagingCenter.Send<MessageCenterSubscribeClass>(new MessageCenterSubscribeClass(), data.ToString());
                    peerResponse.Data = msg;



                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DATA IS NULL");
                    peerResponse.Data = ("DATA IS NULL");
                }
                peerActionResponse.Invoke(peerResponse);
            };

        }

        public void SendInvoiceMessage(string invoice)
        {
            SendMessage(invoice);
        }

        #endregion
        #region TDP Communication
        private void StartUdpListener()
        {
            _ = Task.Run(async () =>
            {
                udpServer = new UdpClient(UdpPort);
                System.Diagnostics.Debug.WriteLine("UDP listener started.");

                while (true)
                {
                    var result = await udpServer.ReceiveAsync();
                    string msg = Encoding.UTF8.GetString(result.Buffer);
                    System.Diagnostics.Debug.WriteLine($"UDP received: {msg} from {result.RemoteEndPoint}");

                    if (msg == "DISCOVER_SERVER")
                    {
                        var response = Encoding.UTF8.GetBytes("SERVER_RESPONSE");
                        await udpServer.SendAsync(response, response.Length, result.RemoteEndPoint);
                        System.Diagnostics.Debug.WriteLine("UDP discovery response sent.");
                    }
                }
            });
        }
        void OnUdpDiscover()
        {
            _ = Task.Run(async () =>
            {
                using var udp = new UdpClient();
                udp.EnableBroadcast = true;

                byte[] msg = Encoding.UTF8.GetBytes("DISCOVER_SERVER");
                var broadcastEP = new IPEndPoint(IPAddress.Broadcast, UdpPort);
                await udp.SendAsync(msg, msg.Length, broadcastEP);
                System.Diagnostics.Debug.WriteLine("UDP discovery sent.");

                var task = udp.ReceiveAsync();
                if (await Task.WhenAny(task, Task.Delay(3000)) == task)
                {
                    var response = task.Result;
                    string responseMsg = Encoding.UTF8.GetString(response.Buffer);
                    if (responseMsg == "SERVER_RESPONSE")
                    {
                        var discoveredIp = response.RemoteEndPoint.Address.ToString();
                        System.Diagnostics.Debug.WriteLine($"Discovered peer IP: {discoveredIp}");
                        ConnectToTcpServer(discoveredIp);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("UDP discovery timeout.");
                }
            });
        }

        #endregion

    }
}
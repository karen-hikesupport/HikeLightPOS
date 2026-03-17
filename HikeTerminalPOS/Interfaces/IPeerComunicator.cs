using System;
using System.Collections.Generic;

namespace HikePOS.Interfaces
{
    public interface IPeerComunicator
    {
        void StartPeerConnection(string tenantId, string userName, string storeName);

        void ReceivePeerData(Action<PeerResponse> peerActionResponse);
        void SendInvoiceMessage(string invoice);

    

    }


    public class PeerResponse
    {
        public bool IsMessage { get; set; }
        public string Data { get; set; }
        public Dictionary<string, string> SenderInfo { get; set; }
    }
}

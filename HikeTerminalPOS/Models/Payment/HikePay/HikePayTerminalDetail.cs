using System;
using Newtonsoft.Json;
using System.Web;
using HikePOS.Models.Payment;
using System.Globalization;
namespace HikePOS.Models.Payment
{
    public class Assignment
    {
        public int status { get; set; }
        public string companyId { get; set; }
        public string merchantId { get; set; }
        public ReassignmentTarget reassignmentTarget { get; set; }
        public string storeId { get; set; }
    }

    public class Bluetooth
    {
        public string ipAddress { get; set; }
        public string macAddress { get; set; }
    }

    public class Cellular
    {
        public int status { get; set; }
        public string iccid { get; set; }
        public string iccid2 { get; set; }
    }

    public class TerminalConnectivity
    {
        public Bluetooth bluetooth { get; set; }
        public Cellular cellular { get; set; }
        public Ethernet ethernet { get; set; }
        public Wifi wifi { get; set; }
    }

    public class Ethernet
    {
        public string ipAddress { get; set; }
        public string linkNegotiation { get; set; }
        public string macAddress { get; set; }
    }

    public class ReassignmentTarget
    {
        public string companyId { get; set; }
        public bool inventory { get; set; }
        public string merchantId { get; set; }
        public string storeId { get; set; }
    }

    public class HikePayTerminalDetail
    {
        public Assignment assignment { get; set; }
        public TerminalConnectivity connectivity { get; set; }
        public string firmwareVersion { get; set; }
        public string id { get; set; }
        public DateTime lastActivityAt { get; set; }
        public DateTime lastTransactionAt { get; set; }
        public string model { get; set; }
        public string restartLocalTime { get; set; }
        public string serialNumber { get; set; }
    }
    public class HikePayTerminalDetailData
    {
        public List<HikePayTerminalDetail> Data { get; set; }
        public int ItemsTotal { get; set; }
        public int PagesTotal { get; set; }
    }
    public class Wifi
    {
        public string ipAddress { get; set; }
        public string macAddress { get; set; }
        public string ssid { get; set; }
    }
    public class HikePayTerminalDetailRequest
    {
        public string serialNumber { get; set; }
        public string merchantIds { get; set; }
        public string storeIds { get; set; }
        public string balanceAccountId { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
    }

}

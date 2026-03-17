using System;
namespace HikePOS.Models
{
    public class CustomerDisplayCodeRequestModel
    {
        public int id { get; set; }
        public string deviceId { get; set; }
        public string devicePairPin { get; set; }
    }



    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CustomerDisplayCodeResponseModel : FullAuditedPassiveEntityDto
    {
        public int id { get; set; }
        public string deviceId { get; set; }
        public string devicePairPin { get; set; }
    }

    //public class CustomerDisplayCodeResponseModel
    //{
    //    public Result result { get; set; }
    //    public object targetUrl { get; set; }
    //    public bool success { get; set; }
    //    public object error { get; set; }
    //    public bool unAuthorizedRequest { get; set; }
    //    public bool __abp { get; set; }
    //}
}

using HikePOS.Services;

namespace HikePOS.Models
{
	public class LoginModel
	{
        public LoginModel() {
			DeviceDetails = new DeviceDetail() {
				Id = GetDeviceInfo.GetDeviceID(),
				Model = DeviceInfo.Current.Model,
				Version = DeviceInfo.Current.Version.ToString(),
				Platform = DeviceInfo.Current.Platform.ToString()
			};
		}

		public string TenancyName { get; set; }
		public string UsernameOrEmailAddress { get; set; }
		public string Password { get; set; }
		//public string SignalRConnectionId { get; set; }
		public DeviceDetail DeviceDetails { get; set; }
		public string NotificationToken { get; set; }
	}

	public class DeviceDetail { 
		public string Id { get; set; }
		public string Model { get; set; }
		public string Version { get; set;}
		public string Platform { get; set;}
	}

	public class CheckTenantInputModel
	{
		public string Tenant { get; set; }
	}
    public class LoginInformation
    {
        public User user { get; set; }
        public Tenant tenant { get; set; }
        public List<string> roles { get; set; }
        public List<int> outlets { get; set; }
        public bool enableZohoSaleIQ { get; set; }
    }
    public class Tenant
    {
        public string tenancyName { get; set; }
        public string name { get; set; }
        public string editionDisplayName { get; set; }
        public object logoId { get; set; }
        public object logoFileType { get; set; }
        public object customCssId { get; set; }
        public object resellerInfo { get; set; }
        public int hikeSubscriptionType { get; set; }
        public int databaseType { get; set; }
        public string databaseName { get; set; }
        public string connectionToken { get; set; }
        public int id { get; set; }
    }

    public class User
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string userName { get; set; }
        public string displayName { get; set; }
        public string emailAddress { get; set; }
        public object profilePictureId { get; set; }
        public bool isOwner { get; set; }
        public string userPin { get; set; }
        public bool isSalesUser { get; set; }
        public double? maximumDiscount { get; set; }
        public int id { get; set; }
    }

}

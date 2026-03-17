namespace HikePOS.Models
{
	public class AccessTockenDto
	{
		public string access_token { get; set; }
		public string token_type { get; set; }
		public string refresh_token { get; set; }
		public string error { get; set; }
        private object expires_in { get; set; }
    }
}

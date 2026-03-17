using System;
namespace HikePOS.Models
{
	public class UpdateProfilePictureModel
	{
		public int x { get; set; }
		public int y { get; set; }
		public int width { get; set; }
		public int height { get; set; }
		public string uploadtype { get; set; }
		public string imagesrc { get; set; }
	}
}

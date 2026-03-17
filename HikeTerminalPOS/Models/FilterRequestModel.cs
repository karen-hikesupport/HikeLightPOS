using System;
namespace HikePOS.Models
{
	public class PagedInputDto
	{
		public int maxResultCount { get; set; }
		public int skipCount { get; set; }
	}

	public class PagedAndSortedInputDto : PagedInputDto
	{
		public string sorting { get; set; }
	}

	public class PagedSortedAndFilteredInputDto : PagedAndSortedInputDto
	{
		public string filter { get; set; }
	}
}

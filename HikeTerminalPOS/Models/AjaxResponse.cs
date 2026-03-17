using System;
using System.Collections.Generic;

namespace HikePOS.Models
{
	public class AjaxResponse
	{
		public AjaxResponse() {
			error = new Error();
		}
		public string targetUrl { get; set; }
		public bool success { get; set; }
		public Error error { get; set; }
		public bool unAuthorizedRequest { get; set; }
		public bool __abp { get; set; }
	}

	public class ValidationError
	{
		public ValidationError() {
			members = new List<string>();
		}	
		public string message { get; set; }
		public List<string> members { get; set; }
	}

	public class Error
	{
		public Error() {
			validationErrors = new List<ValidationError>();
		}
		public int code { get; set; }
		public string message { get; set; }
		public string details { get; set; }
		public List<ValidationError> validationErrors { get; set; }
	}



}

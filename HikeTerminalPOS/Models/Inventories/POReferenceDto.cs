using System;
using HikePOS.Enums;
using System.Collections.ObjectModel;
using System.Linq;
using Realms;
namespace HikePOS.Models
{
	public class POReferenceDto : FullAuditedPassiveEntityDto
	{
        public int sequenceNumber { get; set; }
        public string prefix { get; set; }
        public object suffix { get; set; }
        public int? poReceiptTemplateId { get; set; }

    }
}
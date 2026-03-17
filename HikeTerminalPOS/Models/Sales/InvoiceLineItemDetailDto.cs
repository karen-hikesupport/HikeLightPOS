using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
    public class InvoiceLineItemDetailDto : FullAuditedPassiveEntityDto
    {

        public int InvoiceId { get; set; }

        public int InvoiceLineItemId { get; set; }

        public InvoiceItemAgeVerification Key { get; set; }

        public string Value { get; set; }

        public DateTime? CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }

        public int? LastModifierUserId { get; set; }
        public int? CreatorUserId { get; set; }


        public InvoiceLineItemDetailDB ToModel()
        {
            InvoiceLineItemDetailDB invoiceLineItemDetailDB = new InvoiceLineItemDetailDB
            {
                Id = Id,
                IsActive = IsActive,
                InvoiceId = InvoiceId,
                InvoiceLineItemId = InvoiceLineItemId,
                Key = (int)Key,
                Value = Value,
                LastModificationTime = LastModificationTime,
                CreationTime = CreationTime,
                LastModifierUserId = LastModifierUserId,
                CreatorUserId = CreatorUserId

            };
            return invoiceLineItemDetailDB;
        }
        public static InvoiceLineItemDetailDto FromModel(InvoiceLineItemDetailDB invoiceLineItemDetailDB)
        {
            if (invoiceLineItemDetailDB == null)
                return null;
            InvoiceLineItemDetailDto InvoiceLineItemDetailDto = new InvoiceLineItemDetailDto
            {
                Id = invoiceLineItemDetailDB.Id,
                IsActive = invoiceLineItemDetailDB.IsActive,
                InvoiceId = invoiceLineItemDetailDB.InvoiceId,
                InvoiceLineItemId = invoiceLineItemDetailDB.InvoiceLineItemId,
                Key = (InvoiceItemAgeVerification)invoiceLineItemDetailDB.Key,
                Value = invoiceLineItemDetailDB.Value,
                LastModificationTime = invoiceLineItemDetailDB.LastModificationTime?.UtcDateTime,
                CreationTime = invoiceLineItemDetailDB.CreationTime?.UtcDateTime,
                LastModifierUserId = invoiceLineItemDetailDB.LastModifierUserId,
                CreatorUserId = invoiceLineItemDetailDB.CreatorUserId
            };
            return InvoiceLineItemDetailDto;

        }

    }

    public partial class InvoiceLineItemDetailDB : IRealmObject
    {

        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int InvoiceId { get; set; }

        public int InvoiceLineItemId { get; set; }

        public int Key { get; set; }

        public string Value { get; set; }

        public DateTimeOffset? CreationTime { get; set; }
        public DateTimeOffset? LastModificationTime { get; set; }

        public int? LastModifierUserId { get; set; }
        public int? CreatorUserId { get; set; }

    }
}

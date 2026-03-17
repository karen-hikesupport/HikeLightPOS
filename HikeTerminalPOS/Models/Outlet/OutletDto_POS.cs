using System;
using System.Collections.ObjectModel;
using MongoDB.Bson;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
    public class OutletDto_POS : FullAuditedPassiveEntityDto
    {

        public OutletDto_POS()
        {
            OutletRegisters = new ObservableCollection<RegisterDto>();
        }

        public string Title { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public AddressDto Address { get; set; }
        public int? TaxId { get; set; }
        public string TaxName { get; set; }
        public int? ReceiptTemplateId { get; set; }
        public bool IsDefault { get; set; }
        public string ReceiptTemplateName { get; set; }
        public ObservableCollection<RegisterDto> OutletRegisters { get; set; }

        [JsonIgnore]
        bool _IsSelected { get; set; } = false;
        public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; SetPropertyChanged(nameof(IsSelected)); } }


        public OutletDB_POS ToModel()
        {
            OutletDB_POS model = new OutletDB_POS
            {
                Id = Id,
                IsActive = IsActive,
                Title = Title,
                Email = Email,
                Phone = Phone,
                Address = Address?.ToModel(),
                TaxId = TaxId,
                TaxName = TaxName,
                ReceiptTemplateId = ReceiptTemplateId,
                ReceiptTemplateName = ReceiptTemplateName,
                IsSelected = IsSelected
            };

            OutletRegisters?.ForEach(a => model.OutletRegisters.Add(a.ToModel()));
            return model;
        }

        public static OutletDto_POS FromModel(OutletDB_POS dbModel)
        {
            if (dbModel == null)
                return null;

            OutletDto_POS model = new OutletDto_POS
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                Title = dbModel.Title,
                Email = dbModel.Email,
                Phone = dbModel.Phone,
                Address = AddressDto.FromModel(dbModel.Address),
                TaxId = dbModel.TaxId,
                TaxName = dbModel.TaxName,
                ReceiptTemplateId = dbModel.ReceiptTemplateId,
                ReceiptTemplateName = dbModel.ReceiptTemplateName,
                IsSelected = dbModel.IsSelected
            };

            model.OutletRegisters =new ObservableCollection<RegisterDto>(dbModel.OutletRegisters.Select(a => RegisterDto.FromModel(a)));
            return model;
        }

    }

    public partial class OutletDB_POS : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public AddressDB Address { get; set; }
        public int? TaxId { get; set; }
        public string TaxName { get; set; }
        public int? ReceiptTemplateId { get; set; }
        public string ReceiptTemplateName { get; set; }
        public IList<RegisterDB> OutletRegisters { get; }
        public bool IsSelected { get; set; }

    }
}

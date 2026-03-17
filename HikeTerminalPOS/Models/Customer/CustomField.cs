using System;
using HikePOS.Models.Enum;
using Realms;

namespace HikePOS.Models.Customer
{
    public class CustomField : FullAuditedPassiveEntityDto
    {
        public int CustomFieldType { get; set; }
        public string CustomFieldTypeText { get; set; }
        string _FieldName { get; set; }
        public string FieldName { get { return _FieldName; } set { _FieldName = value; SetPropertyChanged(nameof(FieldName)); } }

        int _Index { get; set; }
        public int Index { get { return _Index; } set { _Index = value; SetPropertyChanged(nameof(Index)); } }
        public bool IsDeleted { get; set; }

        public CustomFieldDB ToModel()
        {
            CustomFieldDB customerAddressDB = new CustomFieldDB
            {
                Id = Id,
                IsActive = IsActive,
                CustomFieldTypeText = CustomFieldTypeText,
                CustomFieldType = CustomFieldType,
                FieldName = FieldName,
                Index = Index,
                IsDeleted = IsDeleted
            };
            return customerAddressDB;
        }

        public static CustomField FromModel(CustomFieldDB dbModel)
        {
            if (dbModel == null)
                return null;

            CustomField customerAddressDto = new CustomField
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                CustomFieldTypeText = dbModel.CustomFieldTypeText,
                CustomFieldType = dbModel.CustomFieldType,
                FieldName = dbModel.FieldName,
                Index = dbModel.Index,
                IsDeleted = dbModel.IsDeleted

            };
            return customerAddressDto;

        }
    }

    public partial class CustomFieldDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int CustomFieldType { get; set; }
        public string CustomFieldTypeText { get; set; }
        public string FieldName { get; set; }
        public int Index { get; set; }
        public bool IsDeleted { get; set; }
    }
}

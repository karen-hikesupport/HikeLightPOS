using System;
using System.Text.Json.Serialization;
using Realms;

namespace HikePOS.Models
{
    public class DenominationDto : FullAuditedPassiveEntityDto
    {
        //public decimal value { get; set; }

        decimal _value { get; set; }
        public decimal Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                SetPropertyChanged(nameof(Value));
            }
        }

        bool _IsEditable { get; set; }
        public bool IsEditable 
        { 
            get
            {
                return _IsEditable;   
            }
            set
            {
                _IsEditable = value;
                SetPropertyChanged(nameof(IsEditable));
            }
        }

        [JsonIgnore]
         public bool IsRemove => Id > 0 ? true : false;

        public DenominationDB ToModel()
        {
            DenominationDB denomination = new DenominationDB
            {
                Id = Id,
                IsActive = IsActive,
                Value = Value,
                IsEditable = IsEditable
            };

            return denomination;
        }

        public static DenominationDto FromModel(DenominationDB denominationDB)
        {
            DenominationDto denomination = new DenominationDto
            {
                Id = denominationDB.Id,
                IsActive = denominationDB.IsActive,
                Value = denominationDB.Value,
                IsEditable = denominationDB.IsEditable
            };

            return denomination;
        }
    }

    public partial class DenominationDB : IRealmObject
    {
        public decimal Value { get; set; }
        public bool IsEditable { get; set; }
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}

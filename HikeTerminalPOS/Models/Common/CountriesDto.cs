using System;
using Realms;

namespace HikePOS.Models
{
	public class CountriesDto : FullAuditedPassiveEntityDto
	{
		public string value { get; set; }
		public string displayText { get; set; }
		public bool isSelected { get; set; }

        public CountriesDB ToModel()
        {
            CountriesDB model = new CountriesDB
            {
                Id = Id,
                IsActive = IsActive,
                value = value,
                displayText = displayText,
                isSelected = isSelected
            };

            return model;
        }

        public static CountriesDto FromModel(CountriesDB dbModel)
        {
            if (dbModel == null)
                return null;

            CountriesDto model = new CountriesDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                value = dbModel.value,
                displayText = dbModel.displayText,
                isSelected = dbModel.isSelected
            };

            return model;
        }

    }

    public partial class CountriesDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string value { get; set; }
        public string displayText { get; set; }
        public bool isSelected { get; set; }

    }
}

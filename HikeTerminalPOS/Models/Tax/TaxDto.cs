using System.Collections.Generic;
using Realms;
namespace HikePOS.Models
{
	public class TaxDto : FullAuditedPassiveEntityDto
	{
		public TaxDto() {
			SubTaxes = new List<SubTaxDto>();
		}

		public string Name { get; set; }
		public decimal Rate { get; set; }
		public string Description { get; set; }
		public bool isDefault { get; set; }
		public bool IsGroup { get; set; }
		public List<SubTaxDto> SubTaxes { get; set; }
        public TaxDB ToModel()
        {
            TaxDB taxDB = new TaxDB
            {
                Id = Id,
                IsActive = IsActive,
                Name = Name,
                Rate = Rate,
                Description = Description,
                isDefault = isDefault,
                IsGroup = IsGroup,

            };
            SubTaxes?.ForEach(i => taxDB.SubTaxes.Add(i));
            return taxDB;
        }
        public static TaxDto FromModel(TaxDB taxDB)
        {
            if (taxDB == null)
                return null;
            TaxDto taxDto = new TaxDto
            {
                Id = taxDB.Id,
                IsActive = taxDB.IsActive,
                Name = taxDB.Name,
                Rate = taxDB.Rate,
                Description = taxDB.Description,
                isDefault = taxDB.isDefault,
                IsGroup = taxDB.IsGroup,
            };
            if (taxDB.SubTaxes != null)
                taxDto.SubTaxes = new List<SubTaxDto>(taxDB.SubTaxes);

            return taxDto;

        }

    }

    public partial class SubTaxDto : IRealmObject
	{
		public int GroupTaxId { get; set; }
		public int TaxId { get; set; }
		public string Name { get; set; }
		public decimal Rate { get; set; }
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }

    }

    public partial class TaxDB : IRealmObject
    {
        public string Name { get; set; }
        public decimal Rate { get; set; }
        public string Description { get; set; }
        public bool isDefault { get; set; }
        public bool IsGroup { get; set; }
        public IList<SubTaxDto> SubTaxes { get; }
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }

    }

    //public class ProductTaxDto : FullAuditedPassiveEntityDto
    //{

    //	public ProductTaxDto()
    //	{
    //		SubTaxes = new List<ProductSubTaxDto>();
    //	}

    //	public string Name { get; set; }
    //	public decimal Rate { get; set; }
    //	public string Description { get; set; }
    //	public bool isDefault { get; set; }
    //	public bool IsGroup { get; set; }
    //	public List<ProductSubTaxDto> SubTaxes { get; set; }
    //}

    //public class ProductSubTaxDto : FullAuditedPassiveEntityDto
    //{
    //	public int GroupTaxId { get; set; }
    //	public int TaxId { get; set; }
    //	public string Name { get; set; }
    //	public decimal Rate { get; set; }
    //}

}

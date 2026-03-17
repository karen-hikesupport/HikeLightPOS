using System;
using HikePOS.Enums;
using Realms;
namespace HikePOS.Models
{
	public class ShopDto : FullAuditedPassiveEntityDto
	{
		public int TenantId { get; set; }
		public string TradingName { get; set; }
		public string LegalEntityName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string ContactEmail { get; set; }
		public string ContactPhone { get; set; }
		public string ContactMobile { get; set; }
		public string Website { get; set; }
		public string Fax { get; set; }
		public IndustryType IndustryType { get; set; }
		public SellerBy SellerBy { get; set; }
		Guid? _logoImage { get; set; }
        public Guid? LogoImage { get { return _logoImage; } set { _logoImage = value; SetPropertyChanged(nameof(LogoImage)); } }

		//Ticket #11252 Start : Product images are not showing on POS. By Nikhil 
		public string AwsS3BucketUrl { get; set; }
		//Ticket #11252 End. By Nikhil
		public string _logoImagePath { get; set; }
		public string LogoImagePath
		{
			get
			{
				if (LogoImage == null)
					return null;
				return _logoImagePath;
			}
			set
			{
				_logoImagePath = value;
			}

		}
        public string NadaPayAccountId { get; set; }

        public bool IsHikePayActivated { get; set; }


        public ShopDB ToModel()
        {
            ShopDB shopDB = new ShopDB
            {
                TenantId = TenantId,
                TradingName = TradingName,
                LegalEntityName = LegalEntityName,
                FirstName = FirstName,
                LastName = LastName,
                ContactEmail = ContactEmail,
                ContactPhone = ContactPhone,
                ContactMobile = ContactMobile,
                Website = Website,
                Fax = Fax,
                IndustryType = (int)IndustryType,
                SellerBy = (int)SellerBy,
                LogoImage = LogoImage,
                AwsS3BucketUrl = AwsS3BucketUrl,
                LogoImagePath = LogoImagePath,
                NadaPayAccountId = NadaPayAccountId,
                IsHikePayActivated = IsHikePayActivated,

            };
            return shopDB;
        }
        public static ShopDto FromModel(ShopDB shopDB)
        {
            if (shopDB == null)
                return null;
            ShopDto shopDto = new ShopDto
            {
                TenantId = shopDB.TenantId,
                TradingName = shopDB.TradingName,
                LegalEntityName = shopDB.LegalEntityName,
                FirstName = shopDB.FirstName,
                LastName = shopDB.LastName,
                ContactEmail = shopDB.ContactEmail,
                ContactPhone = shopDB.ContactPhone,
                ContactMobile = shopDB.ContactMobile,
                Website = shopDB.Website,
                Fax = shopDB.Fax,
                IndustryType = (IndustryType)shopDB.IndustryType,
                SellerBy = (SellerBy)shopDB.SellerBy,
                LogoImage = shopDB.LogoImage,
                AwsS3BucketUrl = shopDB.AwsS3BucketUrl,
                LogoImagePath = shopDB.LogoImagePath,
                NadaPayAccountId = shopDB.NadaPayAccountId,
               IsHikePayActivated = shopDB.IsHikePayActivated,
            };
            return shopDto;

        }


    }
    public partial class ShopDB : IRealmObject
    {
        public int TenantId { get; set; }
        public string TradingName { get; set; }
        public string LegalEntityName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactMobile { get; set; }
        public string Website { get; set; }
        public string Fax { get; set; }
        public int IndustryType { get; set; }
        public int SellerBy { get; set; }
        public Guid? LogoImage { get; set; }
        public string AwsS3BucketUrl { get; set; }
        public string LogoImagePath { get; set; }
        public string NadaPayAccountId { get; set; }
         public bool IsHikePayActivated { get; set; }
    }

}

using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public partial class AddressDto
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }

        [JsonIgnore]
        public string FullAddress
        {

            get
            {
                string CompletAdd = "";

                if (!string.IsNullOrEmpty(City))
                    CompletAdd += City + ",";
                if (!string.IsNullOrEmpty(State))
                    CompletAdd += " " + State;
                if (!string.IsNullOrEmpty(PostCode))
                    CompletAdd += " " + PostCode;

                if (string.IsNullOrEmpty(CompletAdd))
                    return "";

                return CompletAdd.TrimEnd(',');

            }
        }


        public AddressDB ToModel()
        {
            AddressDB customerAddressDB = new AddressDB
            {
                Address1 = Address1,
                Address2 = Address2,
                City = City,
                State = State,
                Country = Country,
                PostCode = PostCode
            };
            return customerAddressDB;
        }
        public static AddressDto FromModel(AddressDB customerAddressDB)
        {
            if (customerAddressDB == null)
                return null;
            AddressDto customerAddressDto = new AddressDto
            {
                Address1 = customerAddressDB.Address1,
                Address2 = customerAddressDB.Address2,
                City = customerAddressDB.City,
                State = customerAddressDB.State,
                Country = customerAddressDB.Country,
                PostCode = customerAddressDB.PostCode
            };
            return customerAddressDto;
        }
    }

    public partial class AddressDB : IRealmObject
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
    }


    //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
    public class CustomerAddressDto : FullAuditedPassiveEntityDto
	{
        public string KeyId { get; set; }
        public bool IsSync { get; set; } = true;

		[JsonProperty("SyncReference")]
		public string TempId { get; set; }

		public int CustomerId { get; set; }
		public int AddressId { get; set; }
		//  public AddressTypes AddressType { get; set; }
		public string AddressTypeName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Country { get; set; }
		public string CountryName { get; set; }
		public string PostCode { get; set; }
		public bool IsSelected { get; set; }
		public string ReceiverName { get; set; }
		public string ReceiverCompanyName { get; set; }
		public string ReceiverPhone { get; set; }
		[JsonIgnore]
		public string FullAddress
		{

			get
			{
				string CompletAdd = "";
				if (!string.IsNullOrEmpty(ReceiverName))
					CompletAdd += ReceiverName + "\n";
				if (!string.IsNullOrEmpty(ReceiverCompanyName))
					CompletAdd += ReceiverCompanyName + "\n";
				if (!string.IsNullOrEmpty(Address1))
					CompletAdd +=  Address1 + "\n";
				if (!string.IsNullOrEmpty(City))
					CompletAdd += City + ",";
				if (!string.IsNullOrEmpty(State))
					CompletAdd +=  State + ",";
				if (!string.IsNullOrEmpty(PostCode))
					CompletAdd +=  PostCode + "," + "\n";
				if (!string.IsNullOrEmpty(CountryName))
					CompletAdd += CountryName;
				if (!string.IsNullOrEmpty(ReceiverPhone))
					CompletAdd += "\nPhone:" + ReceiverPhone;

				return CompletAdd.TrimEnd(',');

			}
		}
        public CustomerAddressDB ToModel()
        {
            CustomerAddressDB customerAddressDB = new CustomerAddressDB
            {
                KeyId = Id > 0 ? Id.ToString() : (KeyId ?? Guid.NewGuid().ToString()),
                Id = Id,
                IsActive = IsActive,
                IsSync = IsSync,
                TempId = TempId,
                CustomerId = CustomerId,
                AddressId = AddressId,
                AddressTypeName = AddressTypeName,
                FirstName = FirstName,
                LastName = LastName,
                Address1 = Address1,
                Address2 = Address2,
                City = City,
                State = State,
                Country = Country,
                CountryName = CountryName,
                PostCode = PostCode,
                IsSelected = IsSelected,
                ReceiverName = ReceiverName,
                ReceiverCompanyName = ReceiverCompanyName,
                ReceiverPhone = ReceiverPhone

            };
            return customerAddressDB;
        }
        public static CustomerAddressDto FromModel(CustomerAddressDB customerAddressDB)
        {
            CustomerAddressDto customerAddressDto = new CustomerAddressDto
            {
                KeyId = customerAddressDB.KeyId,
                Id = customerAddressDB.Id,
                IsActive = customerAddressDB.IsActive,
                IsSync = customerAddressDB.IsSync,
                TempId = customerAddressDB.TempId,
                CustomerId = customerAddressDB.CustomerId,
                AddressId = customerAddressDB.AddressId,
                AddressTypeName = customerAddressDB.AddressTypeName,
                FirstName = customerAddressDB.FirstName,
                LastName = customerAddressDB.LastName,
                Address1 = customerAddressDB.Address1,
                Address2 = customerAddressDB.Address2,
                City = customerAddressDB.City,
                State = customerAddressDB.State,
                Country = customerAddressDB.Country,
                CountryName = customerAddressDB.CountryName,
                PostCode = customerAddressDB.PostCode,
                IsSelected = customerAddressDB.IsSelected,
                ReceiverName = customerAddressDB.ReceiverName,
                ReceiverCompanyName = customerAddressDB.ReceiverCompanyName,
                ReceiverPhone = customerAddressDB.ReceiverPhone

            };
            return customerAddressDto;

        }

    }
    //Ticket end:#26664 .by rupesh
    public partial class CustomerAddressDB : IRealmObject
    {
        [PrimaryKey]
        public string KeyId { get; set; }
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsSync { get; set; } = true;
        public string TempId { get; set; }
        public int CustomerId { get; set; }
        public int AddressId { get; set; }
        public string AddressTypeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string CountryName { get; set; }
        public string PostCode { get; set; }
        public bool IsSelected { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverCompanyName { get; set; }
        public string ReceiverPhone { get; set; }

    }

}

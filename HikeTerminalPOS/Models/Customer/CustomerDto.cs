using System;
using HikePOS.Enums;
using Newtonsoft.Json;

namespace HikePOS.Models
{
	public class CustomerDto : FullAuditedPassiveEntityDto
	{
		public CustomerDto(){
			Address = new AddressDto();
			CustomerGroup = new CustomerGroupDto();
		}

		public bool IsSync { get; set; } = true;

        [JsonProperty("SyncReference")]
		public string TempId { get; set; }

        [JsonIgnore]
        string _FirstName { get; set; }
        public string FirstName { get { return _FirstName; } set { _FirstName = value; 
                SetPropertyChanged(nameof(FirstName));
                SetPropertyChanged(nameof(FullName));
            } }

        [JsonIgnore]
	    string _LastName { get; set; }

        public string LastName
		{
			get { return _LastName; }
			set
			{
				_LastName = value;
				SetPropertyChanged(nameof(LastName));
				SetPropertyChanged(nameof(LastName));
			}
		}

        [JsonIgnore]
        string _CompanyName { get; set; }

        public string CompanyName { get { return _CompanyName; } set { _CompanyName = value;  SetPropertyChanged(nameof(CompanyName));} }

        [JsonIgnore]
		public string _phone { get; set; }
		public string Phone { get { return _phone; } set { _phone = value; SetPropertyChanged(nameof(Phone)); } }

		public string CustomerCode { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public int? PrimaryAddressId { get; set; }
		public int? BillingAddressId { get; set; }
		public int? CustomerGroupId { get; set; }
		public string Note { get; set; }
		public bool _allowLoyalty { get; set; }
		public bool AllowLoyalty { 

			get {
				return _allowLoyalty;
			} set
			{
				_allowLoyalty = value;
				SetPropertyChanged(nameof(AllowLoyalty));
			}
		}

        [JsonIgnore]
		public string FullName { 
			get {
                var str = "";
                if (!string.IsNullOrEmpty(FirstName))
                {
                    str = FirstName.ToUppercaseFirstCharacter();
                }
                if(!string.IsNullOrEmpty(LastName))
                {
                    if(!string.IsNullOrEmpty(str))
                    {
                        str = str + " ";
                    }
                    str = str + LastName.ToUppercaseFirstCharacter();
                }
                return str;
			}
		}

        [JsonIgnore]
		public string ContactDetail
		{
			get
			{
				if (!string.IsNullOrEmpty(Phone) && !string.IsNullOrEmpty(Email))
					return Phone + " / " + Email;
				else if (!string.IsNullOrEmpty(Phone))
					return Phone;
				else if (!string.IsNullOrEmpty(Email))
					return Email;
				else
					return "";
			}
		}

		DateTime? _birthDate { get; set; }
		public DateTime? BirthDate 
		{ 
			get {
				//if (_birthDate == null)
				//	return DateTime.Now.Date;
				//else
					return _birthDate;
			} 
			set
			{
				_birthDate = value;
				PickerDate = value;
				SetPropertyChanged(nameof(BirthDate));
			}
		}


		DateTime? _pickerDate { get; set; }
		public DateTime? PickerDate
		{
			get
			{
				if (_pickerDate == null)
					return DateTime.Now.Date;
				else
					return _pickerDate;
			}
			set
			{
				_pickerDate = value;
				SetPropertyChanged(nameof(PickerDate));
			}
		}


		public int _gender { get; set; }
		public int Gender
		{
			get
			{
				return _gender;
			}
			set
			{
				_gender = value;
				SetPropertyChanged(nameof(Gender));
			}
		}



		public bool IsLoyaltyWelcomeEmailSent { get; set; }
		public decimal CurrentLoyaltyBalance { get; set; }
		public decimal TotalLoyaltySpent { get; set; }

		public decimal? CreditLimit{get; set;} 

        [JsonIgnore]
		public string _strCreditLimit { get; set; } 
		public string StrCreditLimit { 
			get{
				return _strCreditLimit;
			} 
			set {
				_strCreditLimit = value;
				SetPropertyChanged(nameof(StrCreditLimit));
			}		
		}


		public decimal? CreditBalance { get; set; }
		public decimal? OutStandingBalance { get; set; }
		public Guid? ProfileImage { get; set; }
		public int? HowDidYouHearAboutus { get; set; }

        [JsonIgnore]
		AddressDto _address { get; set; }
		public AddressDto Address {get { return _address; } set { _address = value; SetPropertyChanged(nameof(Address)); } }
		
        public CustomerGroupDto CustomerGroup { get; set; }

        [JsonIgnore]
		bool _isAbletoEdit { get; set; }
		public bool IsAbletoEdit { get { return _isAbletoEdit; } set { _isAbletoEdit = value; SetPropertyChanged(nameof(IsAbletoEdit)); } }

		public DateTime? CreationTime { get; set;}

		public decimal? VisitCurrentMonth { get; set; }
		public decimal? AvgMonthlyVisit { get; set; }

		public decimal? TotalPurchase { get; set; }
		public decimal? AveragePurchase { get; set; }

        public string CustomeField1 { get; set; }
        public string CustomeField2 { get; set; }
        public string CustomeField3 { get; set; }
        public string CustomeField4 { get; set; }


        public bool _toAllowForMarketing { get; set; }
        public bool ToAllowForMarketing
        {

            get
            {
                return _toAllowForMarketing;
            }
            set
            {
                _toAllowForMarketing = value;
                SetPropertyChanged(nameof(ToAllowForMarketing));
            }
        }
        public InvoiceFrom CustomerFrom { get; set; } =  InvoiceFrom.iPad;
    }
	//Ticket start:#32389 iPad :: Feature request :: Allow to add multiple email address.by rupesh
	public class SelectEmail  : FullAuditedPassiveEntityDto
    {
		public string Email { get; set; }
		bool _IsSelected { get; set; }
		public bool IsSelected
		{
			get
			{
				return _IsSelected;
			}
			set
			{
				_IsSelected = value;
				SetPropertyChanged(nameof(IsSelected));
			}
		}

	}
	//Ticket end:#32389 .by rupesh
}

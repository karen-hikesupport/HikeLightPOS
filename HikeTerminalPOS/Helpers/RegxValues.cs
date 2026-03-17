using System;
namespace HikePOS.Helpers
{
	public static class RegxValues
	{
		public const string DefaultRegX = @"$";
		public const string EmailRegx = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
			@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
		public const string StoreNameRegx = @"^[a-zA-Z][a-zA-Z0-9]*[a-zA-Z]*$";
		//public const string PasswordRegx = @"^([1-zA-Z0-1@.\s]{1,255})$";
		public const string PasswordRegx = @"$";


		//at least 6 characters, 1 numeric, 1 lowercase, 1 uppercase, 1 special character
		//public const string UserPasswordRegx = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[$@$!%*#?&])[A-Za-z\d$@$!%*#?&]{6,}$";

		//at least 6 characters, 1 numeric
		public const string UserPasswordRegx = @"^(?=.*\d)[A-Za-z\d$@$!%*#?&]{6,}$";

		//public const string UserPasswordAtleastOneDigitRegx = @"^(?=.*\d).+$";
		//public const string UserPasswordMinimum6CharactersRegx = @"^(?=.*\d).+$";

	}
}

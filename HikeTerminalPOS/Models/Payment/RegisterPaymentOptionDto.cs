using Realms;

namespace HikePOS.Models
{
	public class RegisterPaymentOptionDto
	{
		public string RegisterName { get; set; }
		public int RegisterId { get; set; }
		public string PaymentOptionName { get; set; }
		public int PaymentOptionId { get; set; }

        public RegisterPaymentOptionDB ToModel()
        {
            RegisterPaymentOptionDB model = new RegisterPaymentOptionDB
            {
                PaymentOptionId = PaymentOptionId,
                PaymentOptionName = PaymentOptionName,
                RegisterId = RegisterId,
                RegisterName = RegisterName
            };

            return model;
        }

        public static RegisterPaymentOptionDto FromModel(RegisterPaymentOptionDB dbModel)
        {
            if (dbModel == null)
                return null;

            RegisterPaymentOptionDto model = new RegisterPaymentOptionDto
            {
                PaymentOptionId = dbModel.PaymentOptionId,
                PaymentOptionName = dbModel.PaymentOptionName,
                RegisterId = dbModel.RegisterId,
                RegisterName = dbModel.RegisterName
            };

            return model;
        }
    }

    public partial class RegisterPaymentOptionDB : IRealmObject
    {
        public string RegisterName { get; set; }
        public int RegisterId { get; set; }
        public string PaymentOptionName { get; set; }
        public int PaymentOptionId { get; set; }
    }
}

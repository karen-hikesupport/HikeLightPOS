
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models.Payment
{
    public class Commission
    {
        public decimal FixedAmount { get; set; }
        public decimal VariablePercentage { get; set; }
    }

    public class HikePaySplitProfile
    {
        public string Description { get; set; }
        public List<Rule> Rules { get; set; }
        public string SplitConfigurationId { get; set; }

        public HikePaySplitProfileDB ToModel()
        {
            HikePaySplitProfileDB model = new HikePaySplitProfileDB
            {
                Description = Description,
                Rules = JsonConvert.SerializeObject(Rules),
                SplitConfigurationId = SplitConfigurationId,
            };

            return model;
        }

        public static HikePaySplitProfile FromModel(HikePaySplitProfileDB dbModel)
        {
            if (dbModel == null)
                return null;

            HikePaySplitProfile model = new HikePaySplitProfile
            {
                Description = dbModel.Description,
                Rules = JsonConvert.DeserializeObject<List<Rule>>(dbModel.Rules),
                SplitConfigurationId = dbModel.SplitConfigurationId,
            };

            return model;
        }
    }

    public class Rule
    {
        public decimal FundingSource { get; set; }
        public decimal ShopperInteraction { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string RuleId { get; set; }
        public SplitLogic SplitLogic { get; set; }
    }

    public class SplitLogic
    {
        public object AcquiringFees { get; set; }
        public object AdyenCommission { get; set; }
        public object AdyenFees { get; set; }
        public object AdyenMarkup { get; set; }
        public decimal Chargeback { get; set; }
        public object ChargebackCostAllocation { get; set; }
        public object Interchange { get; set; }
        public decimal PaymentFee { get; set; }
        public object Refund { get; set; }
        public object RefundCostAllocation { get; set; }
        public object Remainder { get; set; }
        public object SchemeFee { get; set; }
        public decimal Surcharge { get; set; }
        public decimal Tip { get; set; }
        public object AdditionalCommission { get; set; }
        public Commission Commission { get; set; }
        public string SplitLogicId { get; set; }
    }
    public partial class HikePaySplitProfileDB : IRealmObject
    {
        public string Description { get; set; }
        public string Rules { get; set; }
        public string SplitConfigurationId { get; set; }
    }
    }
    

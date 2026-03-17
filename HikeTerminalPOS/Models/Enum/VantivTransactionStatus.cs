using System;
namespace HikePOS.Enums
{
	public enum VantivTransactionStatus
	{
		Unknown,
		Approved,
		PartiallyApproved,
		ApprovedExceptCashback,
		ApprovedByMerchant,
		CallIssuer,
		Declined,
		NeedsToBeReversed
	}
}

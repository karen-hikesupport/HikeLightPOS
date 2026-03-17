using System;
using System.Collections.Generic;
using Realms;
using Newtonsoft.Json;

namespace HikePOS
{
	public partial class SubscriptionDB : IRealmObject
	{
		public int EditionId { get; set; }

		public int TenantId { get; set; }

		public string Description { get; set; }

		public DateTimeOffset StartDate { get; set; }

		public DateTimeOffset EndDate { get; set; }

		public string DiscountCode { get; set; }

		public decimal DiscountAmount { get; set; }

		public decimal? AddtionalCharge { get; set; }

		public decimal? TotalAmount { get; set; }

		public bool? IsCancelled { get; set; }

		public string CancelReason { get; set; }

		public string StripeCustomerId { get; set; }

		public string StripeSubscriptionId { get; set; }

		public string StripeCCDetailId { get; set; }

		public bool IsActive { get; set; }

		public string Name { get; set; }

		public string DisplayName { get; set; }

		public string StripePlanId { get; set; }

		public MyEditionDB Edition { get; set; }

		public string UserEmail { get; set; }

		public DateTimeOffset CreationTime { get; set; }

        public double ExtendedDays { get; set; }

		public string ExtendedStripePlanId { get; set; }

		public string ExtendedSubscriptionId { get; set; }

		public string ExtendedSubscriptionReason { get; set; }

        public string Culture { get; set; } //= "ar-SY";

        public string Language { get; set; }

        public bool IsOnHold { get; set; }
	}

    public class SubscriptionDto
    {
        public int EditionId { get; set; }

        public int TenantId { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string DiscountCode { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal? AddtionalCharge { get; set; }

        public decimal? TotalAmount { get; set; }

        public bool? IsCancelled { get; set; }

        public string CancelReason { get; set; }

        public string StripeCustomerId { get; set; }

        public string StripeSubscriptionId { get; set; }

        public string StripeCCDetailId { get; set; }

        public bool IsActive { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string StripePlanId { get; set; }

        public MyEdition Edition { get; set; }

        public string UserEmail { get; set; }

        public DateTime CreationTime { get; set; }

        public double ExtendedDays { get; set; }

        public string ExtendedStripePlanId { get; set; }

        public string ExtendedSubscriptionId { get; set; }

        public string ExtendedSubscriptionReason { get; set; }

        public string Culture { get; set; } //= "ar-SY";

        public string Language { get; set; }

        public bool IsOnHold { get; set; }

		public SubscriptionDB ToModel()
		{
			SubscriptionDB subscription = new SubscriptionDB
			{
                EditionId = EditionId,
                TenantId = TenantId,
                Description = Description,
                StartDate = StartDate,
                EndDate = EndDate,
                DiscountCode = DiscountCode,
                DiscountAmount = DiscountAmount,
                AddtionalCharge = AddtionalCharge,
                TotalAmount = TotalAmount,
                IsCancelled = IsCancelled,
                CancelReason = CancelReason,
                StripeCustomerId = StripeCustomerId,
                StripeSubscriptionId = StripeSubscriptionId,
                StripeCCDetailId = StripeCCDetailId,
                IsActive = IsActive,
                Name = Name,
                DisplayName = DisplayName,
                StripePlanId = StripePlanId,
                Edition = Edition.ToModel(),
                UserEmail = UserEmail,
                CreationTime = CreationTime,
                ExtendedDays = ExtendedDays,
                ExtendedStripePlanId = ExtendedStripePlanId,
                ExtendedSubscriptionId = ExtendedSubscriptionId,
                ExtendedSubscriptionReason = ExtendedSubscriptionReason,
                Culture = Culture,
                Language = Language,
                IsOnHold = IsOnHold
            };
			return subscription;
        }

        public static SubscriptionDto FromModel(SubscriptionDB subscriptionDB)
        {
            if (subscriptionDB == null)
                return null;

            SubscriptionDto subscription = new SubscriptionDto
            {
                EditionId = subscriptionDB.EditionId,
                TenantId = subscriptionDB.TenantId,
                Description = subscriptionDB.Description,
                StartDate = subscriptionDB.StartDate.DateTime,
                EndDate = subscriptionDB.EndDate.DateTime,
                DiscountCode = subscriptionDB.DiscountCode,
                DiscountAmount = subscriptionDB.DiscountAmount,
                AddtionalCharge = subscriptionDB.AddtionalCharge,
                TotalAmount = subscriptionDB.TotalAmount,
                IsCancelled = subscriptionDB.IsCancelled,
                CancelReason = subscriptionDB.CancelReason,
                StripeCustomerId = subscriptionDB.StripeCustomerId,
                StripeSubscriptionId = subscriptionDB.StripeSubscriptionId,
                StripeCCDetailId = subscriptionDB.StripeCCDetailId,
                IsActive = subscriptionDB.IsActive,
                Name = subscriptionDB.Name,
                DisplayName = subscriptionDB.DisplayName,
                StripePlanId = subscriptionDB.StripePlanId,
                Edition = MyEdition.FromModel(subscriptionDB.Edition),
                UserEmail = subscriptionDB.UserEmail,
                CreationTime = subscriptionDB.CreationTime.DateTime,
                ExtendedDays = subscriptionDB.ExtendedDays,
                ExtendedStripePlanId = subscriptionDB.ExtendedStripePlanId,
                ExtendedSubscriptionId = subscriptionDB.ExtendedSubscriptionId,
                ExtendedSubscriptionReason = subscriptionDB.ExtendedSubscriptionReason,
                Culture = subscriptionDB.Culture,
                Language = subscriptionDB.Language,
                IsOnHold = subscriptionDB.IsOnHold
            };
            return subscription;
        }
    }

    public class MyEdition
	{
		public EditionType? EditionType { get; set; }

		public decimal? Amount { get; set; }

		public string Currency { get; set; }

		public string CountryCode { get; set; }

		public string StripePlanId { get; set; }

		public bool HasMultipleOutlet { get; set; }

		public int MaxUserCount { get; set; }

		public int MaxProductCount { get; set; }

        public PlanType? PlanType { get; set; }



        public MyEditionDB ToModel()
        {
            MyEditionDB subscription = new MyEditionDB
            {
                Amount = Amount,
                Currency = Currency,
                CountryCode = CountryCode,
                StripePlanId = StripePlanId,
                HasMultipleOutlet = HasMultipleOutlet,
                MaxUserCount = MaxUserCount,
                MaxProductCount = MaxProductCount,
                PlanType = PlanType.HasValue ? (int)PlanType.Value : null,
                EditionType =EditionType.HasValue ? (int)EditionType.Value : null,
            };
            return subscription;
        }

        public static MyEdition FromModel(MyEditionDB subscriptionDB)
        {
            if (subscriptionDB == null)
                return null;

            MyEdition subscription = new MyEdition
            {
                Amount = subscriptionDB.Amount,
                Currency = subscriptionDB.Currency,
                CountryCode = subscriptionDB.CountryCode,
                StripePlanId = subscriptionDB.StripePlanId,
                HasMultipleOutlet = subscriptionDB.HasMultipleOutlet,
                MaxUserCount = subscriptionDB.MaxUserCount,
                MaxProductCount = subscriptionDB.MaxProductCount,
                PlanType = subscriptionDB.PlanType.HasValue ? (PlanType)subscriptionDB.PlanType.Value : null,
                EditionType = subscriptionDB.EditionType.HasValue ? (EditionType)subscriptionDB.EditionType.Value : null,
            };
            return subscription;
        }

    }

    public partial class MyEditionDB : IRealmObject
    {

        public decimal? Amount { get; set; }

        public string Currency { get; set; }

        public string CountryCode { get; set; }

        public string StripePlanId { get; set; }

        public bool HasMultipleOutlet { get; set; }

        public int MaxUserCount { get; set; }

        public int MaxProductCount { get; set; }

        public int? PlanType { get; set; }

        public int? EditionType { get; set; }
    }

    public enum EditionType : int
	{
		Monthly = 0,
		Yearly = 1
	}

	public enum PlanType : int
	{
		Trial = 1,
		StartUp = 2,
		OneStore = 3,
		MultiStore = 4,
		Register = 5,
		OnHold = 6,
		Reseller = 7,
		Basic = 8,
		Advance = 9,
		Essential = 10,
		Plus = 11,
		Outlet = 12,
	}


}

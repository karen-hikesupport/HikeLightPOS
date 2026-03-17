using System;
namespace HikePOS.Enums
{
	public enum Permissions
	{
		//Pages,
		Pages_Administration,
		//Pages_Administration_Roles,
		//Pages_Administration_Roles_Create,
		//Pages_Administration_Roles_Edit,
		//Pages_Administration_Roles_Delete,
		//Pages_Administration_Users,
		//Pages_Administration_Users_Create,
	//	Pages_Administration_Users_Edit,
	//	Pages_Administration_Users_Delete,
	//	Pages_Administration_Users_ChangePermissions,
	//	Pages_Administration_Users_Impersonation,
	//	Pages_Administration_Languages,
	//	Pages_Administration_Languages_Create,
	//	Pages_Administration_Languages_Edit,
	//	Pages_Administration_Languages_Delete,
	//	Pages_Administration_Languages_ChangeTexts,
	//	Pages_Administration_AuditLogs,
	//	Pages_Administration_OrganizationUnits,
	//	Pages_Administration_OrganizationUnits_ManageOrganizationTree,
	//	Pages_Administration_OrganizationUnits_ManageMembers,
	//	Pages_Administration_Tenant_Settings,
	//	Pages_Tenant_Dashboard,
	
		//Pages_Tenant_POS,
		//Pages_Tenant_POS_EnterSale,
		///Pages_Tenant_POS_EnterSale_TogiveDiscount, 	-Done
		//Pages_Tenant_POS_EnterSale_TogiveTips,		-Done
		Pages_Tenant_POS_EnterSale_ToremoveTax,
		Pages_Tenant_POS_EnterSale_ToRemovePayment,
		///Pages_Tenant_POS_EnterSale_CustomSale,		-Done
		///Pages_Tenant_POS_EnterSale_GiftCard,			-Done
		Pages_Tenant_POS_EnterSale_BackOrder,
		///Pages_Tenant_POS_EnterSale_OnAccount,		-Done
		///Pages_Tenant_POS_EnterSale_LayBy,			-Done
		///Pages_Tenant_POS_EnterSale_Credit,			-Done
		///Pages_Tenant_POS_EnterSale_Loyalty,			-Done
		///Pages_Tenant_POS_EnterSale_RefundSale,		-Done
		///Pages_Tenant_POS_SalesHistory,				-Done
		///Pages_Tenant_POS_CloseRegister,				-Done
		///Pages_Tenant_POS_CloseRegister_CashInCashOut,-Done



		//Pages_Tenant_Customers,
		//Pages_Tenant_Customers_Customer,
		///Pages_Tenant_Customers_Customer_Create, - DONE
		//Pages_Tenant_Customers_Customer_Edit,
		//Pages_Tenant_Customers_Customer_Delete,
		//Pages_Tenant_Customers_CustomerImportExport,
		Pages_Tenant_Customers_GroupPriceList, // - Where it is used?
		Pages_Tenant_Customers_Groups, //- select group in add customer
		//Pages_Tenant_Customers_Groups_Create,
		//Pages_Tenant_Customers_Groups_Edit,
		//Pages_Tenant_Customers_Groups_Delete,





		//Pages_Tenant_Inventories,
		//Pages_Tenant_Inventories_Inventory,
		//Pages_Tenant_Inventories_MyInventory,
		//Pages_Tenant_Inventories_InventoryHistory,
		//Pages_Tenant_Inventories_Pipeline,
		//Pages_Tenant_Inventories_Purchase,
		//Pages_Tenant_Inventories_Purchase_InventoryPurchase,
		//Pages_Tenant_Inventories_Stocktake,
		//Pages_Tenant_Inventories_Stocktake_AddInventoryCount,
		//Pages_Tenant_Inventories_Stocktake_NewInventoryCount,
		//Pages_Tenant_Inventories_Suppliers,
		//Pages_Tenant_Inventories_Suppliers_Create,
		//Pages_Tenant_Inventories_Suppliers_Edit,
		//Pages_Tenant_Inventories_Suppliers_Delete,
		//Pages_Tenant_Inventories_Transfers,
		//Pages_Tenant_Inventories_Transfers_TransfersView,


		Pages_Tenant_Products, //-- Entersale listing product as per Permissions 
		Pages_Tenant_Products_Product, // - same above
		//Pages_Tenant_Products_Product_Create,
		//Pages_Tenant_Products_Product_Edit,
		//Pages_Tenant_Products_Product_Delete,
		//Pages_Tenant_Products_Product_UpdateStock,
		//Pages_Tenant_Products_Product_UpdatePrice,
		//Pages_Tenant_Products_Brands,
		//Pages_Tenant_Products_Brands_Create,
		//Pages_Tenant_Products_Brands_Edit,
		//Pages_Tenant_Products_Brands_Delete,
		//Pages_Tenant_Products_Categories,
		//Pages_Tenant_Products_Categories_Create,
		//Pages_Tenant_Products_Categories_Edit,
		//Pages_Tenant_Products_Categories_Delete,
		//Pages_Tenant_Products_DiscountOffers,
		//Pages_Tenant_Products_DiscountOffers_Create,
		//Pages_Tenant_Products_DiscountOffers_Edit,
		//Pages_Tenant_Products_DiscountOffers_Delete,
		Pages_Tenant_Products_GiftCards, // Use?
		//Pages_Tenant_Products_ImportExport,
		//Pages_Tenant_Products_PrintLabelBarcode,

		//Pages_Tenant_Products_Tags,
		//Pages_Tenant_Products_Tags_Create,
		//Pages_Tenant_Products_Tags_Edit,
		//Pages_Tenant_Products_Tags_Delete,

		//Pages_Tenant_Reporting,
		//Pages_Tenant_Reporting_Analysis,
		//Pages_Tenant_Reporting_GiftCardDiscounts,
		//Pages_Tenant_Reporting_InventoryReport,
		//Pages_Tenant_Reporting_RegisterReport,
		//Pages_Tenant_Reporting_RegisterReport_RegisterClosureSummary,
		//Pages_Tenant_Reporting_RegisterReport_RegisterClosureTransactions,
		//Pages_Tenant_Reporting_SalesReport,
		//Pages_Tenant_Reporting_TeamPerformanceReport,
		//Pages_Tenant_Reporting_NewReport,

		///Pages_Tenant_Settings, - Done
		//Pages_Tenant_Settings_AddonsIntegration,
		///Pages_Tenant_Settings_General, - Done
		//Pages_Tenant_Settings_LoyaltyPoints,
		//Pages_Tenant_Settings_MyHikeAccounts,
		//Pages_Tenant_Settings_cleardata,
		Pages_Tenant_Settings_OutletRegisters,
		//Pages_Tenant_Settings_Outlet_Create,
		//Pages_Tenant_Settings_Outlet_Edit,
		//Pages_Tenant_Settings_Registers_Create,
		//Pages_Tenant_Settings_Registers_Edit,
		//Pages_Tenant_Settings_Registers_Delete,
		///Pages_Tenant_Settings_PaymentTypes,
		Pages_Tenant_Settings_PaymentTypes_Create,
		Pages_Tenant_Settings_PaymentTypes_Edit,
		//Pages_Tenant_Settings_PaymentTypes_Delete,
		//Pages_Tenant_Settings_ReceiptTemplates,
		//Pages_Tenant_Settings_ReceiptTemplates_Create,
		//Pages_Tenant_Settings_ReceiptTemplates_Edit,
		//Pages_Tenant_Settings_ReceiptTemplates_Delete,
		//Pages_Tenant_Settings_TaxRules,
		//Pages_Tenant_Settings_TaxRules_Create,
		//Pages_Tenant_Settings_TaxRules_Edit,
		//Pages_Tenant_Settings_TaxRules_Delete,
		Pages_Tenant_UsersYourCrew,
		//Pages_Tenant_UsersYourCrew_ManageUserRolesAccess,
		//Pages_Tenant_UsersYourCrew_Roster,
		//Pages_Tenant_UsersYourCrew_Users
	}
}

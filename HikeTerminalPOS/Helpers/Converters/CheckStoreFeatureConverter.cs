using System;
using System.Globalization;
using HikePOS.Resources;
using System.Linq;
using HikePOS.Models;
using System.Reflection;
using System.Diagnostics;

namespace HikePOS.Helpers
{

    [AcceptEmptyServiceProvider]  
    //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
    public class CheckStoreFeatureConverter : IValueConverter, IMarkupExtension
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value != null && value is string)
            {
                if (string.IsNullOrEmpty(value as string))
                {
                    return false;
                }
            }

            if (Settings.ShopFeatures != null)
            {
                if (value != null && !string.IsNullOrEmpty(parameter?.ToString()))
                {
                    try
                    {

                        bool result = false;

                        PropertyInfo myPropInfo = Settings.ShopFeatures.GetType().GetProperty(parameter.ToString());
                        if (myPropInfo == null)
                            return value;

                        bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out result);


                        var valueType = value.GetType();


                        if (valueType == typeof(bool))
                        {
                          
                            if (result)
                                return value;
                            else
                                return false;

                            
                        }
                        else
                        {
                            if (result)
                                return value;
                            else
                                return Color.FromRgb(68, 80, 91);

                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        return value;
                    }



                }
            }
            return value;

        }
           



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }


    //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.



    /*
    Feature needs to be set in iPad
    -------------------------------
        HikeGiftCardFeature //
        HikeLoyaltyFeature //
        HikeUserPermissionFeature
        HikeCustomFieldsFeature //
        HikeClockInClockOutFeature  //
        HikeInvoiceExchangeFeature  //
        HikeOnAccountFeature    //
        HikeCreditNotFeature    //Need to change in code behiend payment page.cs
        HikeShowTotalNumberOfItemsOnReceiptFeature//
        HikeShowSKUFeature
        HikeCustomerDeliverAddressFeature // Need to change in code behiend
        HikeQuoteSaleFeature //
        HikeCustomerMultiOutletFeature // Not for mobile
        HikeCustomerSecondaryEmailFeature//
        HikeShowSKUFeature//
        HikePOSCustomerMultiOutletFeature//


    */
}

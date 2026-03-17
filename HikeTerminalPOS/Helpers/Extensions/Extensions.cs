using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using HikePOS.Models;
using HikePOS.Helpers;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Threading;
using HikePOS.Services;
using HikePOS.Interfaces;
using HikePOS.Models.Common;
using System.Globalization;
using HikePOS.Enums;
using Fusillade;
using System.Collections;
using Microsoft.AppCenter.Crashes;

namespace HikePOS
{
    public static partial class Extensions
    {

        public static TimeZoneInfo storeTimeZoneInfo = DependencyService.Get<HikePOS.Services.IGetTimeZoneService>().getTimeZoneInfo(Settings.StoreTimeZoneInfoId);

        //Ticket #11319 Start : Send logs to server. By Nikhil
        public static async void SendLogsToServer(SubmitLogServices logService, object model, Dictionary<string, string> requestMap, string exception = "", object paymentResponse = null)
        {
            try
            {

                var title = Settings.TenantName;
                var message = string.Empty;
                if (model is PaymentOptionDto)
                {
                    title += "_" + ((PaymentOptionDto)model).PaymentOptionName;
                    message = JsonConvert.SerializeObject(requestMap);
                }
                else
                {
                    message = JsonConvert.SerializeObject(model);
                }

                if (requestMap == null || requestMap.Count == 0)
                {
                    if (paymentResponse != null)
                        message = JsonConvert.SerializeObject(paymentResponse);
                    else
                        message = "Integrated payment object is null";
                }

                var submitLogDto = new SubmitLogDto()
                {
                    Title = title,
                    Message = message,
                    Exception = exception
                };


                await logService.SubmitLogs(Priority.Background, submitLogDto);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in SendLogsToServer : " + ex.Message + " : " + ex.StackTrace);
            }
        }
        //Ticket #11319 End. By Nikhil

        public static string MessageWithInfo(this string message, string extraInfo)
        {
#if DEBUG

            return message + " in " + extraInfo;
#else
                             return message;
#endif   
        }

        public static void SomethingWentWrong(string extraInfo, Exception ex = null)
        {
            try
            {
                if (ex != null)
                {
                    Logger.SyncLogger("\n ===" + extraInfo + "===1");
                    Logger.SyncLogger(ex.Message + "--\n--" + ex.StackTrace);
                }

                var msg = LanguageExtension.Localize("SomethingWrong");
                if (!string.IsNullOrEmpty(extraInfo))
                    msg += " while " + extraInfo;
                App.Instance.Hud.DisplayToast(msg, Colors.Red, Colors.White);

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in SomethingWentWrong of Extensions : " + e.Message);
            }
        }

        public static void ServerMessage(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                    App.Instance.Hud.DisplayToast(message, Colors.Red, Colors.White);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in ServerMessage of Extensions : " + e.Message);
            }
        }

        //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil	
        static TaxDto noTax;
        public static TaxDto GetNoTaxRecord(TaxServices taxServices)
        {
            if (noTax == null)
                noTax = taxServices.GetLocalTaxById(1);//1 is fixed id for NoTax
            return noTax;
        }

        public static TaxDto defaultTax;
        public static TaxDto GetDefaultTaxRecord(TaxServices taxServices)
        {
            if (defaultTax == null)
            {
                var Register = Settings.CurrentRegister;
                var localTaxes = taxServices.GetLocalTaxes();
                if (Register != null)
                {
                    defaultTax = localTaxes.FirstOrDefault(x => x.Id == Settings.CurrentRegister.DefaultTax);
                }
            }
            if (defaultTax == null)
            {
                var localTaxes = taxServices.GetLocalTaxes();
                defaultTax = localTaxes.FirstOrDefault();
            }
            return defaultTax;
        }

        public static bool IseLnklyPayment(this PaymentOptionType PaymentOptionType)
        {

            return PaymentOptionType == Enums.PaymentOptionType.Linkly
                        || PaymentOptionType == Enums.PaymentOptionType.ANZ
                        || PaymentOptionType == Enums.PaymentOptionType.Bendigo
                        || PaymentOptionType == Enums.PaymentOptionType.Fiserv
                        || PaymentOptionType == Enums.PaymentOptionType.NAB;
        }

        //Ticket #10921 End. By Nikhil

        public static bool IseConduitPayment(this PaymentOptionType PaymentOptionType)
        {

            return PaymentOptionType == Enums.PaymentOptionType.PayJunction
                        || PaymentOptionType == Enums.PaymentOptionType.EVOPayment
                        || PaymentOptionType == Enums.PaymentOptionType.VerifonePaymark
                        || PaymentOptionType == Enums.PaymentOptionType.eConduit
                        || PaymentOptionType == Enums.PaymentOptionType.NorthAmericanBankcard;
        }

        //Ticket #13395  Wrong Refund Total of Partial Refund via NAB. By Nikhil
        public static bool IsIntegratedPayment(this PaymentOptionType paymentOptionType)
        {
            return paymentOptionType == PaymentOptionType.iZettle
                                         || paymentOptionType == PaymentOptionType.Tyro
                                         || paymentOptionType == PaymentOptionType.PayPal
                                         || paymentOptionType == PaymentOptionType.PayPalhere
                                         || paymentOptionType == PaymentOptionType.Mint
                                         || paymentOptionType == PaymentOptionType.VantivIpad
                                         || paymentOptionType == PaymentOptionType.VantivCloud
                                         || paymentOptionType == PaymentOptionType.AssemblyPayment
                                         || paymentOptionType == PaymentOptionType.PayJunction
                                         || paymentOptionType == PaymentOptionType.EVOPayment
                                         || paymentOptionType == PaymentOptionType.VerifonePaymark
                                         || paymentOptionType == PaymentOptionType.NorthAmericanBankcard
                                         || paymentOptionType == PaymentOptionType.eConduit
                                         || paymentOptionType == PaymentOptionType.Moneris
                                         || paymentOptionType == PaymentOptionType.Afterpay
                                         || paymentOptionType == PaymentOptionType.Zip
                                         || paymentOptionType == PaymentOptionType.Square
                                         || paymentOptionType == PaymentOptionType.TD
                                         || paymentOptionType == PaymentOptionType.Elavon
                                         || paymentOptionType == PaymentOptionType.Linkly
                                         || paymentOptionType == PaymentOptionType.NAB
                                         || paymentOptionType == PaymentOptionType.Fiserv
                                         || paymentOptionType == PaymentOptionType.Bendigo
                                         || paymentOptionType == PaymentOptionType.ANZ
                                         || paymentOptionType == PaymentOptionType.Clearent;

        }
        //Ticket #13395 End.By Nikhil

        public static void Track(this Exception ex)
        {
            //SentrySdk.CaptureException(ex);
            try
            {
                var properties = new Dictionary<string, string> {
                    { "User", Settings.CurrentUserEmail },
                    { "Tenant", Settings.TenantName }
                  };

                Crashes.TrackError(ex, properties);

            }
            catch (Exception exx)
            {
                Debug.WriteLine(exx.Message);
            }
        }

        public static string StringToAvtarName(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                var firstChars = str.Split(' ').Where(s => !string.IsNullOrEmpty(s)).Select(s => s.ToCharArray().First()).ToList();

                List<string> CharcterList = firstChars.Select(c => c.ToString()).ToList();
                if (CharcterList.Count > 2)
                {
                    CharcterList = CharcterList.Take(2).ToList();
                }

                return string.Join("", CharcterList).ToUpper();
            }
            else
            {
                return string.Empty;
            }
        }


        public static string ToUppercaseFirstCharacter(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
                action(item);
        }

        public static void Sort<T>(this ObservableCollection<T> collection, IComparer<T> comparer)
        {
            List<T> sorted = collection.ToList();
            sorted.Sort(comparer);

            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }

        //public static void RemoveModel<T>(this ObservableCollection<T> items, int itemId) where T : BaseModel
        //{
        //	var list = items.Where(m => m.Id == itemId).ToList();
        //	foreach (var item in list)
        //		list.Remove(item);
        //}

        //public static void RemoveModel<T>(this ObservableCollection<T> items, T item) where T : BaseModel
        //{
        //	items.RemoveModel(item.Id);
        //}

        public static T Get<T>(this Dictionary<string, T> dict, string id) where T : BaseModel
        {
            if (id == null)
                return null;

            T v = null;
            dict.TryGetValue(id, out v);
            return v;
        }

        //public static void ToToast(this string message, ToastNotificationType type = ToastNotificationType.Info, string title = null)
        //{
        //	MainThread.BeginInvokeOnMainThread(() =>
        //	{
        //		var toaster = DependencyService.Get<IToastNotifier>();
        //		toaster?.Notify(type, title ?? type.ToString().ToUpper(), message, TimeSpan.FromSeconds(2.5f));
        //	});
        //}


        public static DateTime moment(DateTime? date = null)
        {
            if (date == null)
            {
                return DateTime.UtcNow;
            }
            else
            {
                return TimeZoneInfo.ConvertTime(date.Value, TimeZoneInfo.Utc);
            }
        }

        public static DateTime ToStoreTime(this DateTime date)
        {
            try
            {
                // Ticket #15253 iPad slow performance on 'Sales History' tab when loading thousands of Sales orders
                if (storeTimeZoneInfo != null)
                {
                    return TimeZoneInfo.ConvertTime(date, storeTimeZoneInfo);
                }
                else
                {
                    return date;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                return TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local);
            }
        }
        //Ticket start:#14465 Date mismatch issue on receipt. by rupesh
        public static DateTime ToStoreUTCTime(this DateTime date)
        {
            try
            {
                // Ticket #15253 iPad slow performance on 'Sales History' tab when loading thousands of Sales orders
                if (storeTimeZoneInfo != null)
                {
                    date = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
                    return TimeZoneInfo.ConvertTimeToUtc(date, storeTimeZoneInfo);
                }
                else
                {
                    return date;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                return TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
            }
        }
        //Ticket end:#14465 . by rupesh

        public static bool IsValidEmail(this string strIn)
        {
            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        public static decimal ToPositive(this decimal val)
        {
            return Math.Abs(val);
        }

        public static decimal ToNegative(this decimal val)
        {
            return val * -1;
        }

        public static decimal RoundUptoFivecentOnRefund(this decimal val)
        {
            return (Math.Round(val * -20) / -20);
        }

        public static decimal RoundUptoFivecent(this decimal val)
        {
            return (Math.Round(val * 20) / 20);
        }
        //Ticket start:#73187 iPad: Round off totals.by Rupesh
        public static decimal RoundUptoFiftycent(this decimal val)
        {
            return (Math.Round(val * 2) / 2);
        }
        //Ticket end:#73187 .by Rupesh
        public static decimal RoundUptoTencentOnRefund(this decimal val)
        {
            return (Math.Round(val * -10) / -10);


        }


        public static decimal RoundingUptoTwoDecimal(this decimal val)
        {
            return Math.Round(val, 2);
        }


        public static decimal RoundUptoTencent(this decimal val)
        {
            return (Math.Round(val, 1, MidpointRounding.AwayFromZero));
        }

        //Ticket  start :#33589 iPad :: Feature Request :: Rounding off rupees currency.by rupesh
        public static decimal RoundUptoZerocent(this decimal val)
        {
            return (Math.Round(val, 0, MidpointRounding.AwayFromZero));
        }
        //Ticket  end :#33589 .by rupesh

        //Ticket start:#73187 iPad: Round off totals.by Rupesh
        public static decimal RoundUpto10cent(this decimal val)
        {
            return Math.Round(val / 10, 0, MidpointRounding.AwayFromZero) * 10;
        }
        public static decimal RoundUpto100cent(this decimal val)
        {
            return Math.Round(val / 100, 0, MidpointRounding.AwayFromZero) * 100;
        }
        //Ticket end:#73187 iPad: Round off totals.by Rupesh

        public static DateTime ConvertUTCBasedOnCuture(this DateTime date, string cultureName)
        {
            if (cultureName == "ar-sy")
            {
                cultureName = "ar-SY";
                date = DependencyService.Get<IMultilingual>().ConvertIntoCultureDate(date, cultureName);
            }
            return date;
        }

        public static async Task<bool> IsExpired(this SubscriptionDto subscription, bool DisplayMessage = true)
        {
            if (subscription == null)
            {
                if (DisplayMessage)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ExpireMessage"), Colors.Red, Colors.White);
                    await Task.Delay(3000);
                }
                return true;
            }

            if (subscription.IsOnHold)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SubscriptionIsOnHoldMessage"), Colors.Red, Colors.White);
                await Task.Delay(3000);
                return true;
            }
            else
            {
                var enddate = subscription.EndDate.ConvertUTCBasedOnCuture(subscription.Language);
                if (enddate < DateTime.UtcNow)
                {

                    if (enddate.AddDays(subscription.ExtendedDays) > DateTime.UtcNow)
                    {
                        if (DisplayMessage)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SubscriptionExntendedMessage"), Colors.Red, Colors.White);
                            await Task.Delay(3000);
                        }
                        return false;
                    }
                    else
                    {
                        if (DisplayMessage)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ExpireMessage"), Colors.Red, Colors.White);
                            await Task.Delay(3000);
                        }
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }


        public static T Copy<T>(this T a)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.ContractResolver = new IgnoreJsonAttributesResolver();
            settings.Formatting = Formatting.Indented;
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(a, settings));
        }

        public static string ToJson(this object a)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.ContractResolver = new IgnoreJsonAttributesResolver();
            settings.Formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(a, settings);
        }

        public static List<int> GetChunks(this int total)
        {
            var chunks = new List<int>();

            if (total < 1000)
            {
                var isIterate = true;
                while (isIterate)
                {

                    var iterate = new List<int>() { 10, 100, 200, 200 };
                    iterate.ForEach((int value) =>
                    {
                        if (total > 0)
                            chunks.Add(value);

                        var compareTotal = total;
                        compareTotal -= value;
                        if (compareTotal <= 0)
                        {
                            total = compareTotal;
                            isIterate = false;
                            return;
                        }
                        total = compareTotal;
                    });
                }

            }
            else if (total >= 1000 && total < 5000)
            {
                var isIterate = true;
                while (isIterate)
                {

                    var iterate = new List<int>() { 10, 100, 200, 200 };
                    iterate.ForEach((int value) =>
                    {
                        if (total > 0)
                            chunks.Add(value);

                        var compareTotal = total;
                        compareTotal -= value;
                        if (compareTotal <= 0)
                        {
                            total = compareTotal;
                            isIterate = false;
                            return;
                        }
                        total = compareTotal;
                    });
                }

            }
            else if (total >= 5000 && total < 10000)
            {
                var isIterate = true;
                while (isIterate)
                {

                    var iterate = new List<int>() { 10, 20, 50, 100, 100, 200, 200, 500, 500 };
                    iterate.ForEach((int value) =>
                    {
                        if (total > 0)
                            chunks.Add(value);

                        var compareTotal = total;
                        compareTotal -= value;
                        if (compareTotal <= 0)
                        {
                            total = compareTotal;
                            isIterate = false;
                            return;
                        }
                        total = compareTotal;
                    });
                }

            }
            else if (total >= 10000)
            {

                var isIterate = true;
                while (isIterate)
                {

                    var iterate = new List<int>() { 10, 20, 50, 100, 100, 200, 200, 500, 500, 1000, 1000 };
                    iterate.ForEach((int value) =>
                    {
                        if (total > 0)
                            chunks.Add(value);

                        var compareTotal = total;
                        compareTotal -= value;
                        if (compareTotal <= 0)
                        {
                            total = compareTotal;
                            isIterate = false;
                            return;
                        }
                        total = compareTotal;
                    });
                }
            }
            return chunks;
        }


        public static List<List<T>> SplitList<T>(this List<T> me, int size = 50)
        {
            var list = new List<List<T>>();
            for (int i = 0; i < me.Count; i += size)
                list.Add(me.GetRange(i, Math.Min(size, me.Count - i)));
            return list;
        }

        public static List<decimal> getQuickCashOptions(this decimal amount)
        {
            if (amount > 150)
            {
                amount = 150;
            }

            List<decimal> a = new List<decimal>() { 0, 1, 2, 5, 10, 20, 30, 50, 100, 150, 200, 300, 400, 500 };
            List<decimal> quickCashs = new List<decimal>();
            int index = 0;
            a.ForEach((element) =>
            {
                if ((amount > element && amount < a[index + 1]) || amount < element)
                    quickCashs.Add(element);
                index++;
            });

            quickCashs = quickCashs.Take(4).ToList();
            return quickCashs;

        }

        //Ticket #11252 Start : Product images are not showing on POS. By Nikhil 
        public static Uri GetImageUrl(this string imagePath, string type)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {

                string imageurl = "/";

                string prefix = (type == "User_Medium" || type == "User_Thumbnail" || type == "Category_Medium") ? GetStoreBaseUrl() : GetBaseUrl();
                string awsPrefix = string.Empty;

                //Ticket start:#23346 Product Images not loading on POS screen for iPad.by rupesh
                // string awasPrefix = string.Empty;
                if (Settings.StoreGeneralRule.IsEnableS3ForStorage && !string.IsNullOrEmpty(Settings.StoreShopDto.AwsS3BucketUrl))
                {
                    awsPrefix = Settings.StoreShopDto.AwsS3BucketUrl;
                }
                else
                {
                    awsPrefix = prefix;
                }


                switch (type)
                {
                    case "Product_Medium_Entersale":
                        if (imagePath.Contains("/"))
                        {
                            imageurl = awsPrefix + string.Format(imagePath, "120X120");
                        }
                        else
                        {
                            imageurl = prefix + "/Product/GetProductPictureById?size=120&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        }
                        break;
                    case "Product_Medium":
                        if (imagePath.Contains("/"))
                        {
                            imageurl = awsPrefix + string.Format(imagePath, "120X120");
                        }
                        else
                            imageurl = prefix + "/Product/GetProductPictureById?size=120&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        //imageurl = prefix + string.Format(imagePath, "120X120");
                        break;
                    //Ticket #13151 Product images don't load in details screen in iPad.By Nikhil
                    case "Product_Details":
                        if (imagePath.Contains("/"))
                        {
                            imageurl = awsPrefix + string.Format(imagePath, "240X240");
                        }
                        else
                            imageurl = prefix + "/Product/GetProductPictureById?size=240&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        //imageurl = prefix + string.Format(imagePath, "240X240");
                        break;
                    //Ticket #13151 End.By Nikhil
                    case "Product_Thumbnail":
                        //imageurl = prefix + "/Product/GetProductPictureById?size=50&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        imageurl = awsPrefix + string.Format(imagePath, "50X50");
                        break;
                    case "Product_Thumbnail_Entersale":
                        imageurl = prefix + "/Product/GetProductPictureById?size=50&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        imageurl = awsPrefix + string.Format(imagePath, "50X50");
                        break;

                    case "Category_Medium":
                        imageurl = prefix + "/Category/GetCategoryPictureById?size=120&tenantId=" + Settings.TenantId + "&id=" + imagePath;

                        break;
                    case "Category_Thumbnail":
                        imageurl = prefix + "/Category/GetCategoryPictureById?size=50&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        break;

                    case "Offer_Medium":
                        imageurl = prefix + "/Offer/GetOfferPictureById?size=120&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        break;
                    case "Offer_Thumbnail":
                        imageurl = prefix + "/Offer/GetOfferPictureById?size=50&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        break;

                    case "User_Medium":
                        imageurl = prefix + "/Profile/GetUserProfilePicture?size=120&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        break;
                    case "User_Thumbnail":
                        imageurl = prefix + "/Profile/GetUserProfilePicture?size=50&tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        break;

                    case "Store":
                        if (imagePath.Contains("/"))
                        {
                            imageurl = string.Format(imagePath, "240X240");
                        }
                        else
                            imageurl = prefix + "/shop/GetShopPictureById?tenantId=" + Settings.TenantId + "&id=" + imagePath;
                        break;
                    case "Store_Thumbnail":
                        imageurl = "/";
                        break;
                    // Ticket:start:#90938 IOS:FR Age varification.by rupesh
                    case "AgeVerification":
                        imageurl = awsPrefix + imagePath;
                        break;
                    // Ticket:end:#90938 .by rupesh
                    default:
                        imageurl = "/";
                        break;
                }

                //Ticket end:#23346 .by rupesh


                Debug.WriteLine("Product_Thumbnail_Entersale" + imageurl.ToString());
                return new Uri(imageurl);
            }
            else
            {
                return null;
            }
        }

        static string GetBaseUrl()
        {

            if (!string.IsNullOrEmpty(Settings.TenantName))
            {
                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.DesignerTest)
                {
                    return ServiceConfiguration.DesignerProtocol + Settings.TenantName + "." + ServiceConfiguration.DesignerBaseUrl;
                }
                else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.ASYTest)
                {
                    return ServiceConfiguration.AsyProtocol + Settings.TenantName + "." + ServiceConfiguration.AsyBaseUrl;
                }
                else
                {
                    return "https://hikeimages.hikeup.com";
                }
            }
            else
            {
                return string.Empty;
            }
        }
        static string GetStoreBaseUrl()
        {
            if (!string.IsNullOrEmpty(Settings.TenantName))
            {
                if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live || Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Test)
                {
                    return ServiceConfiguration.LiveProtocol + Settings.TenantName + "." + ServiceConfiguration.LiveBaseUrl;
                }
                else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.DesignerTest)
                {
                    return ServiceConfiguration.DesignerProtocol + Settings.TenantName + "." + ServiceConfiguration.DesignerBaseUrl;
                }
                else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.HConnectTest)
                {
                    return ServiceConfiguration.HConnectProtocol + ServiceConfiguration.HConnectBaseUrl;
                }
                else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.StagingTest)
                {
                    return ServiceConfiguration.StagingProtocol + ServiceConfiguration.StagingBaseUrl;
                }
                else
                {
                    return ServiceConfiguration.AsyProtocol + Settings.TenantName + "." + ServiceConfiguration.AsyBaseUrl;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static Uri GetImageUrl1(this string id, string type)
        {
            if (!string.IsNullOrEmpty(id))
            {

                string imageurl = "/";
                switch (type)
                {
                    case "Product_Medium":
                        imageurl = "/Product/GetProductPicture?size=120&tenantId=" + Settings.TenantId + "&productId=";
                        break;
                    case "Product_Medium_Entersale":
                        imageurl = "/Product/GetProductPictureById?size=120&tenantId=" + Settings.TenantId + "&id=";
                        break;
                    case "Product_Thumbnail":
                        imageurl = "/Product/GetProductPicture?size=50&tenantId=" + Settings.TenantId + "&productId=";
                        break;
                    case "Product_Thumbnail_Entersale":
                        imageurl = "/Product/GetProductPictureById?size=50&tenantId=" + Settings.TenantId + "&id=";
                        break;

                    case "Category_Medium":
                        imageurl = "/Category/GetCategoryPictureById?size=120&tenantId=" + Settings.TenantId + "&id=";
                        break;
                    case "Category_Thumbnail":
                        imageurl = "/Category/GetCategoryPictureById?size=50&tenantId=" + Settings.TenantId + "&id=";
                        break;
                    case "Offer_Medium":
                        imageurl = "/Offer/GetOfferPictureById?size=120&tenantId=" + Settings.TenantId + "&id=";
                        break;
                    case "Offer_Thumbnail":
                        imageurl = "/Offer/GetOfferPictureById?size=50&tenantId=" + Settings.TenantId + "&id=";
                        break;
                    case "User_Medium":
                        imageurl = "/Profile/GetUserProfilePicture?size=120&tenantId=" + Settings.TenantId + "&id=";
                        break;
                    case "User_Thumbnail":
                        imageurl = "/Profile/GetUserProfilePicture?size=50&tenantId=" + Settings.TenantId + "&id=";
                        break;
                    case "Store":
                        imageurl = "/shop/GetShopPictureById?tenantId=" + Settings.TenantId + "&id=";
                        break;
                    case "Store_Thumbnail":
                        imageurl = "/";
                        break;
                    default:
                        imageurl = "/";
                        break;
                }
                if (string.IsNullOrEmpty(Settings.TenantName))
                {
                    if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live || Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Test)
                    {
                        return new Uri(ServiceConfiguration.LiveProtocol + imageurl + id);
                    }
                    else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.DesignerTest)
                    {
                        return new Uri(ServiceConfiguration.DesignerProtocol + imageurl + id);
                    }
                    else
                    {
                        return new Uri(ServiceConfiguration.AsyProtocol + imageurl + id);
                    }
                }
                else
                {
                    if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live || Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Test)
                    {
                        return new Uri(ServiceConfiguration.LiveProtocol + Settings.TenantName + "." + ServiceConfiguration.LiveBaseUrl + imageurl + id);
                    }
                    else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.DesignerTest)
                    {
                        return new Uri(ServiceConfiguration.DesignerProtocol + Settings.TenantName + "." + ServiceConfiguration.DesignerBaseUrl + imageurl + id);
                    }
                    else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.HConnectTest)
                    {
                        return new Uri(ServiceConfiguration.HConnectProtocol + ServiceConfiguration.HConnectBaseUrl);
                    }
                    else if (Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.StagingTest)
                    {
                        return new Uri(ServiceConfiguration.StagingProtocol + ServiceConfiguration.StagingBaseUrl);
                    }
                    else
                    {
                        return new Uri(ServiceConfiguration.AsyProtocol + Settings.TenantName + "." + ServiceConfiguration.AsyBaseUrl + imageurl + id);
                    }
                }
            }
            else
            {
                return null;
            }
        }
        //Ticket #11252 End. By Nikhil

        public static Dictionary<string, string> ParseQueryString(this Uri uri)
        {
            var query = uri.Query.Substring(uri.Query.IndexOf('?') + 1); // +1 for skipping '?'
            var pairs = query.Split('&');
            return pairs
                .Select(o => o.Split('='))
                .Where(items => items.Count() == 2)
                .ToDictionary(pair => Uri.UnescapeDataString(pair[0]),
                    pair => Uri.UnescapeDataString(pair[1]));
        }

        //Ticket start:#34015 iPad: hijari calendar is not working on iPad.by rupesh
        public static Uri AddQuery(this Uri uri, string name, string value)
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            query.Add(name, value);

            var uriBuilder = new UriBuilder(uri)
            {
                Query = query.ToString()
            };

            return uriBuilder.Uri;
        }
        //Ticket end:#34015 .by rupesh

        public static string ToKMFormat(this decimal amount)
        {
            if (amount == 0)
            {
                return "0";
            }
            else if (amount >= 1000000)
                return (amount / 1000000).ToString("0.##") + "M";
            else if (amount >= 1000)
                return (amount / 1000).ToString("0.##") + "K";
            else if (amount <= -1000000)
                return (amount / 1000000).ToString("0.##") + "M";
            else if (amount <= -1000)
                return (amount / 1000).ToString("0.##") + "K";
            else
                return amount.ToString("G29");
        }


        public static void SetCulture(string Language)
        {
            try
            {
                string CurrentCulture = "";
                if (!string.IsNullOrEmpty(Language) && Language.Contains("3d"))
                {
                    Settings.StoreCulture = Language;
                    CurrentCulture = Language.Replace("3d", "");
                }
                else if (!string.IsNullOrEmpty(Language) && Language == "en-kw")
                {
                    Settings.StoreCulture = Language;
                    CurrentCulture = "en";
                    //Ticket start:#34447,#94425.by rupesh
                    //Settings.SymbolForDecimalSeperatorForNonDot = ",";
                    //Ticket end:#34447,#94425.by rupesh
                }
                else if (string.IsNullOrEmpty(Language))
                {
                    Settings.StoreCulture = "en";
                    CurrentCulture = "en";
                }
                else
                {
                    CurrentCulture = Language;
                    Settings.StoreCulture = Language;
                }

                var CrossMultilingual = DependencyService.Get<IMultilingual>();
                // var culture = CrossMultilingual.NeutralCultureInfoList.ToList().FirstOrDefault(element => element.Name.ToLower().Contains(CurrentCulture.ToLower()));
                // CultureInfo myCIclone = null;
                // if(culture != null)
                // {
                //       myCIclone = (CultureInfo)culture.Clone();
                // }
                // else
                // {
                //       myCIclone = new CultureInfo(Language);

                // }
                // if (myCIclone == null)
                // {
                //     myCIclone = new CultureInfo("en");
                // }
                // else
                // {
                CultureInfo culture = new CultureInfo(CurrentCulture);
                if (culture == null)
                {
                    culture = new CultureInfo("en");
                }


                //Ticket start:#34642 iPad: should be getting 3 decimal in both Arabic(Kuwait) and English(Kuwait).by rupesh
                if (Language.Contains("3d") || Language == "en-kw" || Language == "ar-kw")
                {
                    culture.NumberFormat.CurrencyDecimalDigits = 3;
                    Settings.StoreDecimalDigit = 3;
                }
                //Ticket end:#34642 .by rupesh
                //Ticket start:#28128 iPad: rounding issue in Arabic and Arabic (Kuwait). by rupesh
                else if (Language == "ar")
                {
                    culture.NumberFormat.CurrencyDecimalDigits = 2;
                    Settings.StoreDecimalDigit = 2;
                }
                //Ticket end:#28128. by rupesh
                else
                {
                    Settings.StoreDecimalDigit = culture.NumberFormat.CurrencyDecimalDigits;
                }
                //  }


                culture.NumberFormat.CurrencySymbol = Settings.StoreCurrencySymbol;
                culture.NumberFormat.CurrencyNegativePattern = 1;
                //Ticket start:#26913 iOS - Separator (comma) Not Applied.by rupesh
                //Ticket start:#33715,#33790,#94425 iOS: Decimal place changes on iPad POS page.by rupesh
                if (Language == "ar" || Language == "ar-kw" || Language == "en-kw")
                {
                    culture.NumberFormat.CurrencyDecimalSeparator = Settings.SymbolForDecimalSeperatorForNonDot;
                    culture.NumberFormat.NumberDecimalSeparator = Settings.SymbolForDecimalSeperatorForNonDot;
                    culture.NumberFormat.PercentDecimalSeparator = Settings.SymbolForDecimalSeperatorForNonDot;
                }
                //Ticket end:#33715,#33790,#94425 .by rupesh
                //Ticket end:#26913 .by rupesh
                CrossMultilingual.CurrentCultureInfo = culture;
                HikePOS.Resx.AppResources.Culture = CrossMultilingual.CurrentCultureInfo;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        public static string GetDeviceInfo()
        {
            string deviceInformation = string.Empty;

            try
            {

                DeviceInformation deviceInfo = new DeviceInformation()
                {
                    Model = DeviceInfo.Model,
                    Manufacturer = DeviceInfo.Manufacturer,
                    DeviceName = DeviceInfo.Name,
                    Version = DeviceInfo.VersionString,
                    Platform = DeviceInfo.Platform.ToString(),
                    Idiom = DeviceInfo.Idiom.ToString(),
                    DeviceType = DeviceInfo.DeviceType.ToString(),
                };

                deviceInformation = JsonConvert.SerializeObject(deviceInfo);


            }
            catch (Exception ex)
            {
                deviceInformation = "Exception : " + ex.Message;
            }

            return deviceInformation;
        }

        //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
        public static bool IsFeatureAccessible(this Color buttonColor)
        {

            if (buttonColor == Color.FromRgb(68, 80, 91))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreFeatureNotAvailable"), Colors.Red, Colors.White);

                return false;
            }
            return true;
        }
        //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
        //Start #61832  iPad:Create text file for invoice log. Pratik
        public static Microsoft.Maui.Controls.Shapes.Rectangle GetAbsoluteBounds(this View element)
        {
            Element looper = element;

            var absoluteX = element.X + element.Margin.Top;
            var absoluteY = element.Y + element.Margin.Left;

            // TODO: add logic to handle titles, headers, or other non-view bars

            while (looper.Parent != null)
            {
                looper = looper.Parent;
                if (looper is View v)
                {
                    absoluteX += v.X + v.Margin.Top;
                    absoluteY += v.Y + v.Margin.Left;
                }
            }

            var s = new Microsoft.Maui.Controls.Shapes.Rectangle();
            s.AnchorX = absoluteX;
            s.AnchorY = absoluteY;
            s.WidthRequest = element.Width;
            s.HeightRequest = element.Height;

            return s;
        }

        public static Rect ToSystemRectangle(this Microsoft.Maui.Controls.Shapes.Rectangle rect) =>
            new Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

        //End #61832 Pratik


        public static ReceiptTemplateDto GetReceiptTemplate(this int? id)
        {
            if (!id.HasValue)
                return null;

            ApiService<IOutletApi> OutletApiService = new ApiService<IOutletApi>();
            var outletServices = new OutletServices(OutletApiService);
            ReceiptTemplateDto templateDto = null;
            templateDto = outletServices.GetReceiptTemplateById(id.Value);
            return templateDto;
        }
        //Start #84438 iOS : FR :add discount offers on product tag by Pratik
        public static List<ProductTagDto> TagsJsonToDto(this string tags, ProductServices productServices)
        {
            var tagObject = new List<ProductTagDto>();
            if (!string.IsNullOrEmpty(tags))
            {
                var strtags = tags;
                if (!tags.ToLower().Contains("id"))
                {
                    var ids = JsonConvert.DeserializeObject<List<int>>(strtags);
                    if (ids != null && ids.Count > 0)
                    {
                        tagObject = productServices.GetLocalProductTagByIds(ids);
                    }
                }
                else
                    tagObject = JsonConvert.DeserializeObject<List<ProductTagDto>>(strtags);
            }
            return tagObject;
        }
        //End #84438 by Pratik

        //Ticket start:#90943 iOS:FR Display Barcode or SKU instead of Product Name .by Pratik
        public static ObservableCollection<PrintInvoiceLineiItemGroup> SetDisplayTitle(ObservableCollection<InvoiceLineItemDto> collection, ReceiptTemplateDto receiptTemplate, bool displayDiscount = true)
        {
            if (receiptTemplate.ReplaceProductNameWithSKU)
            {
                collection.ForEach(a => a.ProductTitle = displayDiscount ? a.ProductSKUTitleWithQuantity : (a.InvoiceItemType == InvoiceItemType.Discount ? a.ProductSKUTitleWithQuantity : a.SKUWithLabel));
            }
            else if (!receiptTemplate.ReplaceProductNameWithSKU && receiptTemplate.PrintSKU)
            {
                collection.ForEach(a => a.ProductTitle = displayDiscount ? a.ProductTitleWithQuantity : a.ProductTitleWithSku);
            }
            else
            {
                collection.ForEach(a => a.ProductTitle = displayDiscount ? a.ProductTitleWithQuantity : a.Title);
            }
            var results = GetInvoiceGroupData(collection, displayDiscount);
            return results;
        }

        public static ObservableCollection<PrintInvoiceLineiItemGroup> GetInvoiceGroupData(ObservableCollection<InvoiceLineItemDto> invoiceLines, bool displayDiscount = true)
        {
            try
            {
                var value = invoiceLines.ToList();
                if (!displayDiscount)
                    value = invoiceLines.Where(x => x.InvoiceItemType != Enums.InvoiceItemType.Discount).ToList();
                ObservableCollection<PrintInvoiceLineiItemGroup> groupLineiItemGroupList = null;
                if (!Settings.StoreGeneralRule.ShowGroupProductsByCategory)
                {
                    var items = value;
                    groupLineiItemGroupList = new ObservableCollection<PrintInvoiceLineiItemGroup>(items.Select(a => new PrintInvoiceLineiItemGroup()
                    {
                        Title = "",
                        InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>() { a }
                    }));

                    return groupLineiItemGroupList;
                }
                if (value != null && value is IList)
                {
                    groupLineiItemGroupList = new ObservableCollection<PrintInvoiceLineiItemGroup>();
                    var items = value;
                    var groupeditemsList = items.GroupBy(u => u.categoryId).Select(grp => grp.ToList()).ToList();
                    foreach (var groupedItem in groupeditemsList)
                    {
                        var invoiceLineiItemGroup = new PrintInvoiceLineiItemGroup();
                        var firstItem = groupedItem.FirstOrDefault();
                        if (firstItem.CategoryDtos == null)
                        {
                            invoiceLineiItemGroup.Title = "NONE";
                            invoiceLineiItemGroup.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();

                        }
                        else
                        {
                            var productCategories = JsonConvert.DeserializeObject<List<CategoryDto>>(firstItem.CategoryDtos);
                            if (productCategories.Count == 0)
                            {
                                invoiceLineiItemGroup.Title = "NONE";
                            }
                            else
                            {
                                invoiceLineiItemGroup = new PrintInvoiceLineiItemGroup { Title = productCategories.FirstOrDefault().Name?.ToUpper() };

                            }
                            invoiceLineiItemGroup.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();

                        }
                        foreach (var item in groupedItem)
                            invoiceLineiItemGroup.InvoiceLineItems.Add(item);
                        groupLineiItemGroupList.Add(invoiceLineiItemGroup);
                    }

                }

                return groupLineiItemGroupList;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
        }
        //Ticket end:#90943 by Pratik

        //Start #90944 iOS:FR Gift cards expiry date by Pratik
        public static string GetGiftCardExpiredMsg()
        {
            string msg = string.Empty;
            if (Helpers.Settings.StoreGeneralRule.GiftcardExpiration != null && Helpers.Settings.StoreGeneralRule.GiftcardExpiration.validityPeriod > 0)
            {
                int cnt = Helpers.Settings.StoreGeneralRule.GiftcardExpiration.validityPeriod;
                switch (Helpers.Settings.StoreGeneralRule.GiftcardExpiration.validityTime.ToLower())
                {
                    case "days":
                        msg = LanguageExtension.Localize("ThisGiftCardWillExpireOn") + " " + StringDateFormat(DateTime.UtcNow.ToStoreTime().AddDays(cnt));
                        break;
                    case "months":
                        msg = LanguageExtension.Localize("ThisGiftCardWillExpireOn") + " " + StringDateFormat(DateTime.UtcNow.ToStoreTime().AddMonths(cnt));
                        break;
                    case "years":
                        msg = LanguageExtension.Localize("ThisGiftCardWillExpireOn") + " " + StringDateFormat(DateTime.UtcNow.ToStoreTime().AddYears(cnt));
                        break;
                    default:
                        msg = string.Empty;
                        break;
                }
            }
            return msg;
        }

        public static string StringDateFormat(DateTime dt)
        {
            string suffix;
            if (new[] { 11, 12, 13 }.Contains(dt.Day))
            {
                suffix = "th";
            }
            else if (dt.Day % 10 == 1)
            {
                suffix = "st";
            }
            else if (dt.Day % 10 == 2)
            {
                suffix = "nd";
            }
            else if (dt.Day % 10 == 3)
            {
                suffix = "rd";
            }
            else
            {
                suffix = "th";
            }

            return string.Format("{0}{1} {2:MMM} {2:yyyy}", dt.Day, suffix, dt);
        }
        //End #90944 by Pratik
        public static string GetBaseUrl(this Models.Enum.AppEnvironment environment)
        {
            return environment switch
            {
                Models.Enum.AppEnvironment.Live or Models.Enum.AppEnvironment.Test
                    => ServiceConfiguration.LiveProtocol + ServiceConfiguration.LivePrefix + ServiceConfiguration.LiveBaseUrl,

                Models.Enum.AppEnvironment.DesignerTest
                    => ServiceConfiguration.DesignerProtocol + ServiceConfiguration.DesignerBaseUrl,

                Models.Enum.AppEnvironment.HConnectTest
                    => ServiceConfiguration.HConnectProtocol + ServiceConfiguration.HConnectBaseUrl,

                Models.Enum.AppEnvironment.StagingTest
                    => ServiceConfiguration.StagingProtocol + ServiceConfiguration.StagingBaseUrl,

                _ => ServiceConfiguration.AsyProtocol + ServiceConfiguration.AsyBaseUrl
            };
        }
    }

}

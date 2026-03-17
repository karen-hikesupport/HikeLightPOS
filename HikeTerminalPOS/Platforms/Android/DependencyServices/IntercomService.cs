/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IO.Intercom.Android;
using IO.Intercom.Android.Identity;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HikePOS.Droid.DependencyServices;
using HikePOS.Services;
using HikePOS.Helpers;
using Java.Util;

[assembly: Dependency(typeof(IntercomService))]
namespace HikePOS.Droid.DependencyServices
{
    class IntercomService : IIntercomService
    {   // TODO: Uncomment After adding Intercom SDK(For Android)
        public void LoginUser()
        {
            //Intercom.Initialize(MainActivity.activity.Application, "android_sdk-6a39789c5d1c58bdf9650073a0ec2e8bbd3e4e11", "rjkebmxr");

            if (Settings.CurrentUser != null && !string.IsNullOrEmpty(Settings.TenantName) && !string.IsNullOrEmpty(Settings.CurrentUser.EmailAddress))
            {
                Intercom.Client().RegisterIdentifiedUser(Registration.Create().WithUserId(Settings.TenantName + "-" + Settings.CurrentUser.UserName));
                // Intercom.RegisterUserWithEmail(Settings.CurrentUser.EmailAddress);
                var dic = new Dictionary<string, object>();
                dic.Add("hide_default_launcher", new Java.Lang.Boolean(true));
                dic.Add("custom_launcher_selector", "#hike_intercom");
                dic.Add("StoreName", Settings.TenantName);
                dic.Add("version", "Hike-V3");

                if (Settings.CurrentUser != null)
                {
                    dic.Add("store_user_id", Settings.TenantId + "-" + Settings.CurrentUser.Id);

                    if (!string.IsNullOrEmpty(Settings.CurrentUser.UserName))
                    {
                        dic.Add("user_id", Settings.TenantName + "-" + Settings.CurrentUser.UserName);
                    }

                    if (!string.IsNullOrEmpty(Settings.CurrentUser.Name))
                    {
                        dic.Add("firstname", Settings.CurrentUser.Name);
                    }

                    if (!string.IsNullOrEmpty(Settings.CurrentUser.EmailAddress))
                    {
                        dic.Add("email", Settings.CurrentUser.EmailAddress);
                    }
                }

                if (Settings.CurrentUser != null && Settings.CurrentUser.Roles != null && Settings.CurrentUser.Roles.Count > 0 && Settings.CurrentUser.Roles.First().RoleName != null)
                {
                    dic.Add("UserRole", Settings.CurrentUser.Roles.First().RoleName);
                }

                if (Settings.Subscription != null)
                {
                    if (!string.IsNullOrEmpty(Settings.Subscription.StripePlanId))
                    {
                        dic.Add("UserPlanId", Settings.Subscription.StripePlanId);
                    }

                    if (!string.IsNullOrEmpty(Settings.Subscription.StripeCustomerId))
                    {
                        dic.Add("stripe_id", Settings.Subscription.StripeCustomerId);
                    }
                }

                if (Settings.StoreShopDto != null)
                {
                    dic.Add("industry", Settings.StoreShopDto.IndustryType.ToString());
                    dic.Add("businessDescription", Settings.StoreShopDto.SellerBy.ToString());
                    if (!string.IsNullOrEmpty(Settings.StoreShopDto.ContactPhone))
                    {
                        dic.Add("phone", Settings.StoreShopDto.ContactPhone);
                    }

                    if (!string.IsNullOrEmpty(Settings.StoreShopDto.ContactMobile))
                    {
                        dic.Add("mobile", Settings.StoreShopDto.ContactMobile);
                    }
                }

                //dic.Add(new NSString("address"), new NSString(Settings.StoreShopDto.Address.City + "," + Settings.StoreShopDto.Address.State + ", " + Settings.StoreShopDto.Address.CountryName));
                // dic.Add(new NSString("isOwner"), new NSString(Settings.CurrentUser.IsOwner));
                var userAttributes = new UserAttributes.Builder().WithCustomAttributes(dic).Build();

                Intercom.Client().UpdateUser(userAttributes);
            }
            else
            {
                Intercom.Client().RegisterUnidentifiedUser();
            }
        }

        public void OpenMessenger()
        {
            Intercom.Client().DisplayMessenger();
            //Intercom.PresentMessenger();
        }

        public void CloseMessenger()
        {
            Intercom.Client().HideIntercom();
            //Intercom.HideMessenger();
        }
    }
}*/
using System;
using HikePOS.Models.Enum;

namespace HikePOS.Services
{
    public class ServiceConfiguration
    {
        //Development Branch
        public const string AsyProtocol = "http://";
        // public const string AsyBaseUrl = "my.asy.io:82/";
        // public const string AsyBaseUrl = "asy.io:82/";
        public const string AsyBaseUrl = "my.asy.io";

        //public const string AsyBaseUrl = "hikeasytestlinkly.asy.io:82/";

        //https://hikeasytestlinkly.asy.io:82

        public static AppEnvironment AppEnvironment = AppEnvironment.Live;

        //Live Branch
        public const string LiveProtocol = "https://";
        //public const string LivePrefix = "iPadV3.";

        //public const string LivePrefix = "iPadV3.";
        //public const string LiveBaseUrl = "Hikeup.com";

        /*
            Changed by : Hingraj
            Date : 03042019 Ver 2.2.1(1.0.8)
            Note: Added below url for ably change
        */
        public const string LivePrefix = "iPadV31.";
        public const string LiveBaseUrl = "Hikeup.com";

        //Ben Design
        public const string DesignerProtocol = "http://";
        public const string DesignerBaseUrl = "192.168.1.19/";

        //Hconnect
        public const string HConnectProtocol = "https://";
        public const string HConnectBaseUrl = "hconnect.hikeup.com";

        //Staging
        public const string StagingProtocol = "https://";
        public const string StagingBaseUrl = "staging.hikeup.com";

        public const int retryCount = 1;
        public const int ServiceTimeoutSeconds = 1200;

        //https://iclient.tyro.com
        //public const string TyroTestUrl = "https://iclient.test.tyro.com";
        public const string TyroTestUrl = "https://iclientsimulator.test.tyro.com";
        public const string TyroLiveUrl = "https://iclient.tyro.com";

        public const string PaypalLiveUrl = "http://hikeup.herokuapp.com/toPayPal/live?returnTokenOnQueryString=true";
        public const string PaypalSandboxUrl = "http://hikeup.herokuapp.com/toPayPal/sandbox?returnTokenOnQueryString=true";

        public const string iZettleUrl = "";
        public const bool tyro_integratedReceipt = false;

        public const string PosProductVendor = "HikePOS pty Ltd";
        public const string PosProductName = "HikePOS";
        public const string PosProductVersion = "1.0";
        public const string TyroApiKey = "AexzqnGi0zKz3tJfqqT4B8s2cubHSV";
        public const bool IntegratedReceipt = true;
        public const string ProviderNumber = "2147661H";


        #region Support links
        public const string AllReceiptDescriptionLink = "https://support.hikeup.com/hc/en-us/articles/115004834707-Allreceipt-Function-Star-Micronics";
        public const string PrinterDescriptionLink = "https://hikeup.com/pos-hardware/";
        public const string PaymentDescriptionLink = "https://support.hikeup.com/hc/en-us/sections/115001458488-Card-Payment-Processors";

        public const string TermsAndConditionLink = "https://hikeup.com/terms-of-use/";
        public const string PrivacyPolicyLink = "https://hikeup.com/privacy-policy/";

        #endregion

        public const string strGooglePlaceAPILey = "AIzaSyBmxBSIWowZ8nG6ne-qmdz1dLYbGg1fE-M";
        public const string strPlacesAutofillUrl = "https://maps.googleapis.com/maps/api/place/";

        // Mint Configurations 
        //Sandox details
        public const string MINT_INTEGRATOR_API_KEY = "vhZEkIM4X0VIljrFSvEJ";
        public const string MINT_INTEGRATOR_SECRET_KEY = "cm3LKO0FQPpyDWUHLdM2";
        public const string MINT_INTEGRATOR_REGION_CODE = "au_mintmpos";


        //Production detail
        //public const string MINT_INTEGRATOR_API_KEY = "vhZEkIM4X0VIljrFSvEJ";
        //public const string MINT_INTEGRATOR_SECRET_KEY = "vhZEkIM4X0VIljrFSvEJ";
        //public const string MINT_INTEGRATOR_REGION_CODE = "au_mintmpos";


        //Vantiv Configuration
        //public const string Vantiv_AccountID = "1027028";
        //public const string Vantiv_AcceptorID = "874766999";
        //public const string Vantiv_AccountToken = "3CF03CE7B3B4D3B8FFBA8D44A1364CBD14ADB0CB66A85EBA08F1FE697F7C1E66EB518D01";

        public const string Vantiv_ApplicationID = "8662";
        public const string Vantiv_ApplicationName = "Hike POS";
        public const string Vantiv_ApplicationVersion = "0.0.0";


        //#37229 iPad: Login session expired

        // Note : While doing the changes in Android find the referece for below field and do the require changes

        //For Live and HConnect server
        public const string AccessToken_Client_Id = "iPad-39c91f32a3";
        public const string AccessToken_Client_Secret = "efd6add56c0e4d7ab7a39288d66f5c14";

        // for asy server
        public const string ASYAccessToken_Client_Id = "IpadStagingApp-0d77edb54b";
        public const string ASYAccessToken_Client_Secret = "beefe07feec94cd1a20eda113af11e0e";

        public const string ASY_To_GetAccessToken_VerifyKey = "aad836f57ece258774c6178a24805162";

        //#37229 iPad: Login session expired

        //Simple payment integration key
        public const string DeviceApiKey = "3TGxBItHM5BoBjImGEeohulSCqjZB0uc";

        public const string ZOHO_APP_KEY = "u1dbZOWg5XxjCNCY5TU6ofqzeshyscc2QZee7FI%2Byf1TpPVXn9aP0dC1lFMohlSG";
        public const string ZOHO_ACCESS_KEY_IOS = "kJAK%2BP%2Bd30kE5Ae9c0uQIS3c5V31OepY5lbKY9GamMFsIa3fNl3JV1DTiHMKz0iX4k7Af46tumJacSxqQ8fgiVkZYIezf0DAFe6ZQfyv17Q%3D";
        public const string ZOHO_ACCESS_KEY_ANDROID = "kJAK%2BP%2Bd30ndfaUEZiq53TAWiBENjaPNucHNBtf5UVKIk9v4oOu%2F2kBjzAYeGDRyCI0J4TCdCAaRi1d9WFEL%2BXvIDaDEYa67sV9gxLfbu9o%3D";

        public const string iZettle_ClientId_IOS = "740a9886-f34c-46cd-9906-7456efcdc694";
        public const string iZettle_CallbackUrl_IOS = "hike-up://login.callback";

        public const string iZettle_ClientId_ANDROID = "a8343989-e022-44c0-b6f3-6434300b9445";
        public const string iZettle_CallbackUrl_ANDROID = "hike-pos-register://login.callback";

        public const string Fiska_ApiKey = "f1712e21-f175-4b02-affe-e635049f8d76";

    }
}


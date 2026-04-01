using System;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using HikePOS.Enums;
using HikePOS.Models;
using HikePOS.ViewModels;
using Newtonsoft.Json;
using HikePOS.Models.Payment;
using System.Web;
using System.Security.Cryptography;

namespace HikePOS.Helpers
{
    public class CommonMethods
    {
        public static async Task<bool> ReachableCheck(string url)
        {
            bool isReachable = false; // await CrossConnectivity.Current.IsReachable(url);
            return isReachable;
        }

        //Start #81159 Pratik
        public static string GetProductName(string productName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productName))
                    return string.Empty;

                var parts = productName
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                    return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";

                return productName.Length >= 2
                    ? productName[..2].ToUpperInvariant()
                    : productName.ToUpperInvariant();
            }
            catch(Exception ex)
            {
                return productName?.Take(1).ToString().ToUpperInvariant();
            }
        }
        //End #81159 Pratik

        public static string GetHtmlData(Label lbl, string txt)
        {
            if (txt == null)
                txt = string.Empty;

            //Ticket start:#30306 iOS: Receipt printed for ipad does not print the Footer text correctly.by rupesh
            string htmlData = "";
            if (lbl.HorizontalOptions.Alignment == LayoutAlignment.Start && !lbl.HorizontalOptions.Expands)
                htmlData = "<span style=\"font-size:" + lbl.FontSize + "\">" + txt + "</span>";
            else
                htmlData = "<span style=\"font-size:" + lbl.FontSize + ";text-align:center\">" + txt + "</span>";
            //Ticket end:#30306.by rupesh
            return htmlData;
        }

        public static string GetHtmlData(int size, string txt)
        {
            if (txt == null)
                txt = string.Empty;
            string htmlData = "<span style=\"font-size:" + size + "\">" + txt + "</span>";
            return htmlData;
        }


        public static string GetDeviceIdentifierWithVersion()
        {
                    string deviceID = "UnknownDeviceID";

                    try
                    {
                            #if ANDROID
                                    var context = Android.App.Application.Context;
                                    if (context?.ContentResolver != null)
                                    {
                                        deviceID = Android.Provider.Settings.Secure.GetString(
                                            context.ContentResolver,
                                            Android.Provider.Settings.Secure.AndroidId
                                        ) ?? "UnknownAndroidID";
                                    }
                            #elif IOS
                                    var identifier = UIKit.UIDevice.CurrentDevice.IdentifierForVendor;
                                    deviceID = identifier?.AsString() ?? "UnknowniOSID";
                            #endif
                    }
                    catch (Exception ex)
                    {
                        // Optionally log the error for diagnostics (e.g., App Center, Sentry, etc.)
                        System.Diagnostics.Debug.WriteLine($"[DeviceID Error]: {ex}");
                    }

                    try
                    {
                        var version = AppInfo.Current.VersionString;
                        deviceID += $" {version}";
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[App Version Error]: {ex}");
                    }

                    return deviceID;
        }

        #region HELPER PRINT FUNCTIONS 
        // ESC/POS control bytes
        public static string ESC = "\u001B";
        public static string GS = "\u001D";

        public static string BoldText(string text, bool isutf = false)
        {
            if (!isutf)
            {
                return ESC + "E" + (char)1 + text + ESC + "E" + (char)0;
            }
            else
            {
                // string boldOn = "\x1B\x45\x01";
                // string boldOff = "\x1B\x45\x00";
                // string printData = boldOn + text + boldOff;
                return text;
            }
        }

        public static string DoubleSizeText(string text, bool isutf = false) => isutf ? text : GS + "!" + (char)0x11 + text + GS + "!" + (char)0x00;

        public static string NormalText(string text, bool isutf = false) => isutf ? text : GS + "!" + (char)0x00 + text;

        public static string CenterText_Old(string text, int Imin_LINE_WIDTH, double fontsize = 2, bool isutf = false)
        {
             if (string.IsNullOrEmpty(text)) return "\n";
             
            int nameWidth = fontsize == 4 ? Imin_LINE_WIDTH / 2 : Imin_LINE_WIDTH;
            string sb = string.Empty;

            while (text.Length > nameWidth)
            {
                if (fontsize <= 2)
                {
                    sb += NormalText(text.Substring(0, nameWidth) + "\n", isutf);
                }
                else
                {
                    sb += DoubleSizeText(text.Substring(0, nameWidth) + "\n", isutf);
                }

                text = text.Substring(nameWidth);
            }

            double leftPadding = (nameWidth - text.Length) / 2;
            if(fontsize <= 2)
            {
                sb += NormalText(new string(' ', Convert.ToInt32(leftPadding)) + text + "\n", isutf);
            }
            else
            {
                sb += DoubleSizeText(new string(' ', Convert.ToInt32(leftPadding)) + text + "\n", isutf);
            }
            return sb;
        }

        public static string CenterText(
            string text,
            int Imin_LINE_WIDTH,
            double fontsize = 2,
            bool isutf = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "\n";

            int lineWidth = fontsize == 4
                ? Imin_LINE_WIDTH / 2
                : Imin_LINE_WIDTH;

            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                if (word.Length > lineWidth)
                {
                    if (!string.IsNullOrWhiteSpace(currentLine))
                    {
                        lines.Add(currentLine.TrimEnd());
                        currentLine = "";
                    }

                    // Split long word safely
                    int index = 0;
                    while (index < word.Length)
                    {
                        int len = Math.Min(lineWidth, word.Length - index);
                        lines.Add(word.Substring(index, len));
                        index += len;
                    }
                }
                else if ((currentLine + word).Length > lineWidth)
                {
                    lines.Add(currentLine.TrimEnd());
                    currentLine = word + " ";
                }
                else
                {
                    currentLine += word + " ";
                }
            }

            if (!string.IsNullOrWhiteSpace(currentLine))
                lines.Add(currentLine.TrimEnd());

            // ---- CENTER OUTPUT ----
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                int leftPadding = Math.Max(0, (lineWidth - line.Length) / 2);
                string centered = new string(' ', leftPadding) + line + "\n";

                if (fontsize <= 2)
                    sb.Append(NormalText(centered, isutf));
                else
                    sb.Append(DoubleSizeText(centered, isutf));
            }

            return sb.ToString();
        }



        public static string RightWrapItemLine(string itemName, string priceStr, int Imin_LINE_WIDTH, bool isutf = false)
        {
            if (string.IsNullOrEmpty(itemName))
                return string.Empty;
            int priceWidth = Imin_LINE_WIDTH > 32 ? 16 : 14;

            if (priceStr.Length > priceWidth)
                priceStr = priceStr.Substring(0, priceWidth - 3) + "..";

            int nameWidth = Imin_LINE_WIDTH - priceWidth;
            string sb = string.Empty;

            var asd = priceWidth - priceStr.Length;
            string appentblank = "";
            for (int i = 0; i < asd; i++)
            {
                appentblank += " ";
            }
            appentblank = appentblank + priceStr;

            bool priceonfirstline = false;
            while (itemName.Length > nameWidth)
            {
                if (!priceonfirstline)
                {
                    priceonfirstline = true;
                    sb += NormalText(itemName.Substring(0, nameWidth) + NormalText(appentblank, isutf), isutf) + "\n";
                }
                else
                {
                    sb += NormalText(itemName.Substring(0, nameWidth) + new string(' ', priceWidth), isutf) + "\n";
                }
                itemName = itemName.Substring(nameWidth);
            }
            if (itemName.Length <= nameWidth)
            {
                if (priceonfirstline)
                {
                    sb += NormalText(new string(' ', Math.Max(0, nameWidth - itemName.Length)) + itemName + new string(' ', priceWidth), isutf) + "\n";
                }
                else
                {

                    sb += NormalText(new string(' ', Math.Max(0, nameWidth - itemName.Length)) + itemName + NormalText(appentblank, isutf), isutf) + "\n";
                }
            }
            return sb;
        }

        public static string WrapItemLine_OLD(string itemName, string priceStr, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false, string stikeAmount = "")
        {
            stikeAmount = "";

            if (priceStr.Length > Imin_Column_Width)
                priceStr = priceStr.Substring(0, Imin_Column_Width - 3) + "..";

            if (string.IsNullOrEmpty(itemName))
                return string.Empty;
            int priceWidth = Imin_Column_Width;
            int nameWidth = Imin_LINE_WIDTH - priceWidth;
            string sb = string.Empty;

            var asd = priceWidth - priceStr.Length;
            string appentblank = "";
            for (int i = 0; i < asd; i++)
            {
                appentblank += " ";
            }
            appentblank = appentblank + priceStr;

            string appentstike = "";
            var stikecnt = priceWidth - (stikeAmount.Length / 2);
            for (int i = 0; i < stikecnt; i++)
            {
                appentstike += " ";
            }
            appentstike = appentstike + stikeAmount;
            bool priceonfirstline = false;
            bool dislayStike = false;
            while (itemName.Length > nameWidth)
            {
                if (!priceonfirstline)
                {
                    if (!dislayStike && !IsStrikeAmount && !string.IsNullOrEmpty(stikeAmount) && string.IsNullOrWhiteSpace(priceStr))
                    {
                        dislayStike = true;
                        sb += NormalText(itemName.Substring(0, nameWidth) + NormalText(appentstike, isutf), isutf) + "\n";
                    }
                    else
                    {
                        sb += NormalText(itemName.Substring(0, nameWidth) + NormalText(appentblank, isutf), isutf) + "\n";
                    }
                    priceonfirstline = true;
                }
                else
                {                   
                   
                    if (!string.IsNullOrEmpty(stikeAmount) && !dislayStike)
                    {
                        dislayStike = true;
                        sb += NormalText(itemName.Substring(0, nameWidth) + new string(' ', Math.Max(0, nameWidth - itemName.Length)) + NormalText(appentstike, isutf), isutf) + "\n";
                    }
                    else
                    {
                        sb += NormalText(itemName.Substring(0, nameWidth) + new string(' ', Math.Max(0, nameWidth - itemName.Length)), isutf) + "\n";
                    }
                }
                itemName = itemName.Substring(nameWidth);
                
            }

            if (itemName.Length <= nameWidth)
            {
                if (priceonfirstline)
                {
                    if (!dislayStike && !string.IsNullOrEmpty(stikeAmount))
                    {
                        dislayStike = true;
                        sb += NormalText(itemName + new string(' ', Math.Max(0, nameWidth - itemName.Length)) + NormalText(appentstike, isutf), isutf) + "\n";
                    }
                    else
                    {
                        sb += NormalText(itemName + new string(' ', Math.Max(0, nameWidth - itemName.Length) + priceWidth), isutf) + "\n";
                    }
                }
                else
                {
                    if (!IsStrikeAmount && !string.IsNullOrEmpty(stikeAmount) && string.IsNullOrWhiteSpace(priceStr) && !dislayStike)
                    {
                        dislayStike = true;
                        sb += NormalText(itemName + new string(' ', Math.Max(0, nameWidth - itemName.Length)) + NormalText(appentstike, isutf), isutf) + "\n";
                    }
                    else
                        sb += NormalText(itemName + new string(' ', Math.Max(0, nameWidth - itemName.Length)) + NormalText(appentblank, isutf), isutf) + "\n";
                }
            }
            IsStrikeAmount = dislayStike;
            return sb;
        }

        public static string WrapItemLine(
            string itemName,
            string priceStr,
            int Imin_LINE_WIDTH,
            int Imin_Column_Width,
            bool isutf = false,
            string stikeAmount = "")
        {
             stikeAmount = "";
            if (string.IsNullOrWhiteSpace(itemName))
                return string.Empty;

            // Trim price if too long
            if (!string.IsNullOrEmpty(priceStr) && priceStr.Length > Imin_Column_Width)
                priceStr = priceStr.Substring(0, Imin_Column_Width - 3) + "..";

            int priceWidth = Imin_Column_Width;
            int nameWidth = Imin_LINE_WIDTH - priceWidth;

            string result = string.Empty;

            // Right-aligned price
            string pricePadding = new string(' ', Math.Max(0, priceWidth - priceStr.Length));
            string priceBlock = pricePadding + priceStr;

            // Right-aligned strike amount
            string strikePadding = new string(' ', Math.Max(0, priceWidth - (stikeAmount.Length / 2)));
            string strikeBlock = strikePadding + stikeAmount;

            bool priceOnFirstLine = false;
            bool displayStrike = false;

            while (itemName.Length > nameWidth)
            {
                int wrapAt = itemName.LastIndexOf(' ', nameWidth);
                if (wrapAt <= 0)
                    wrapAt = nameWidth;

                string lineText = itemName.Substring(0, wrapAt).TrimEnd();
                if(wrapAt<nameWidth)
                {
                     string padding = new string(' ', Math.Max(0, nameWidth - wrapAt));
                     lineText = lineText + padding;
                }

                
                itemName = itemName.Substring(wrapAt).TrimStart();

                if (!priceOnFirstLine)
                {
                    if (!displayStrike && !IsStrikeAmount && !string.IsNullOrEmpty(stikeAmount) && string.IsNullOrWhiteSpace(priceStr))
                    {
                        displayStrike = true;
                        result += NormalText(lineText + NormalText(strikeBlock, isutf), isutf) + "\n";
                    }
                    else
                    {
                        result += NormalText(lineText + NormalText(priceBlock, isutf), isutf) + "\n";
                    }
                    priceOnFirstLine = true;
                }
                else
                {
                    if (!displayStrike && !string.IsNullOrEmpty(stikeAmount))
                    {
                        displayStrike = true;
                        result += NormalText(
                            lineText.PadRight(nameWidth) + NormalText(strikeBlock, isutf),
                            isutf) + "\n";
                    }
                    else
                    {
                        result += NormalText(lineText, isutf) + "\n";
                    }
                }
            }

            // Last line
            if (!string.IsNullOrEmpty(itemName))
            {
                if (priceOnFirstLine)
                {
                    if (!displayStrike && !string.IsNullOrEmpty(stikeAmount))
                    {
                        displayStrike = true;
                        result += NormalText(
                            itemName.PadRight(nameWidth) + NormalText(strikeBlock, isutf),
                            isutf) + "\n";
                    }
                    else
                    {
                        result += NormalText(itemName, isutf) + "\n";
                    }
                }
                else
                {
                    if (!IsStrikeAmount && !string.IsNullOrEmpty(stikeAmount) && string.IsNullOrWhiteSpace(priceStr))
                    {
                        displayStrike = true;
                        result += NormalText(
                            itemName.PadRight(nameWidth) + NormalText(strikeBlock, isutf),
                            isutf) + "\n";
                    }
                    else
                    {
                        result += NormalText(
                            itemName.PadRight(nameWidth) + NormalText(priceBlock, isutf),
                            isutf) + "\n";
                    }
                }
            }

            IsStrikeAmount = displayStrike;
            return result;
        }

        public static string WrapExtraItemLine_OLD(string itemName, string priceStr, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false, string stikeAmount = "")
        {
            stikeAmount = "";

            if (priceStr.Length > Imin_Column_Width)
                priceStr = priceStr.Substring(0, Imin_Column_Width - 3) + "..";

            int priceWidth = Imin_Column_Width;
            int nameWidth = Imin_LINE_WIDTH - priceWidth;
            string sb = string.Empty;

            var asd = priceWidth - priceStr.Length;
            string appentblank = "";
            for (int i = 0; i < asd; i++)
            {
                appentblank += " ";
            }
            appentblank = appentblank + priceStr;

            string appentstike = "";
            var stikecnt = priceWidth - (stikeAmount.Length / 2);
            for (int i = 0; i < stikecnt; i++)
            {
                appentstike += " ";
            }
            appentstike = appentstike + stikeAmount;

            bool priceonfirstline = false;
            bool dislayStike = false;
            itemName = "  " + itemName;

            while (itemName.Length > nameWidth)
            {
                if (!priceonfirstline)
                {
                    if (!IsStrikeAmount && !string.IsNullOrEmpty(stikeAmount) && string.IsNullOrWhiteSpace(priceStr))
                    {
                        dislayStike = true;
                        sb += NormalText(itemName.Substring(0, nameWidth) + NormalText(appentstike, isutf), isutf) + "\n";
                    }
                    else
                    {
                        sb += NormalText(itemName.Substring(0, nameWidth) + NormalText(appentblank, isutf), isutf) + "\n";
                    }
                    priceonfirstline = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(stikeAmount) && !dislayStike)
                    {
                        dislayStike = true;
                        sb += NormalText(itemName.Substring(0, nameWidth) + new string(' ', Math.Max(0, nameWidth - itemName.Length)) + NormalText(appentstike, isutf), isutf) + "\n";
                    }
                    else
                    {
                        sb += NormalText(itemName.Substring(0, nameWidth) + new string(' ', Math.Max(0, nameWidth - itemName.Length)), isutf) + "\n";
                    }

                }
                itemName = "  " + itemName.Substring(nameWidth);
            }

            if (itemName.Length <= nameWidth)
            {
                if (priceonfirstline)
                {
                    if (!dislayStike && !string.IsNullOrEmpty(stikeAmount))
                    {
                        dislayStike = true;
                        sb += NormalText(itemName + new string(' ', Math.Max(0, nameWidth - itemName.Length)) + NormalText(appentstike, isutf), isutf) + "\n";
                    }
                    else
                    {
                        sb += NormalText(itemName + new string(' ', Math.Max(0, nameWidth - itemName.Length) + priceWidth), isutf) + "\n";
                    }
                }
                else
                {
                    if (!IsStrikeAmount && !string.IsNullOrEmpty(stikeAmount) && string.IsNullOrWhiteSpace(priceStr) && !dislayStike)
                    {
                        dislayStike = true;
                        sb += NormalText(itemName + new string(' ', Math.Max(0, nameWidth - itemName.Length)) + NormalText(appentstike, isutf), isutf) + "\n";
                    }
                    else
                        sb += NormalText(itemName + new string(' ', Math.Max(0, nameWidth - itemName.Length)) + NormalText(appentblank, isutf), isutf) + "\n";
                }
            }
            IsStrikeAmount = dislayStike;
            return sb;
        }

        public static string WrapExtraItemLine(
            string itemName,
            string priceStr,
            int Imin_LINE_WIDTH,
            int Imin_Column_Width,
            bool isutf = false,
            string stikeAmount = "")
        {
            stikeAmount = "";
            if (string.IsNullOrWhiteSpace(itemName))
                return string.Empty;

            // Trim price if too long
            if (!string.IsNullOrEmpty(priceStr) && priceStr.Length > Imin_Column_Width)
                priceStr = priceStr.Substring(0, Imin_Column_Width - 3) + "..";

            int priceWidth = Imin_Column_Width;
            int nameWidth = Imin_LINE_WIDTH - priceWidth;

            string result = string.Empty;

            // Right-aligned price
            string pricePadding = new string(' ', Math.Max(0, priceWidth - priceStr.Length));
            string priceBlock = pricePadding + priceStr;

            // Right-aligned strike amount
            string strikePadding = new string(' ', Math.Max(0, priceWidth - (stikeAmount.Length / 2)));
            string strikeBlock = strikePadding + stikeAmount;

            bool priceOnFirstLine = false;
            bool displayStrike = false;
            itemName = "  " + itemName;

            while (itemName.Length > nameWidth)
            {
                int wrapAt = itemName.LastIndexOf(' ', nameWidth);
                if (wrapAt <= 0)
                    wrapAt = nameWidth;

                string lineText = itemName.Substring(0, wrapAt).TrimEnd();
                if(wrapAt<nameWidth)
                {
                     string padding = new string(' ', Math.Max(0, nameWidth - wrapAt));
                     lineText = lineText + padding;
                }

                
                itemName = "  " + itemName.Substring(wrapAt).TrimStart();

                if (!priceOnFirstLine)
                {
                    if (!displayStrike && !IsStrikeAmount && !string.IsNullOrEmpty(stikeAmount) && string.IsNullOrWhiteSpace(priceStr))
                    {
                        displayStrike = true;
                        result += NormalText(lineText + NormalText(strikeBlock, isutf), isutf) + "\n";
                    }
                    else
                    {
                        result += NormalText(lineText + NormalText(priceBlock, isutf), isutf) + "\n";
                    }
                    priceOnFirstLine = true;
                }
                else
                {
                    if (!displayStrike && !string.IsNullOrEmpty(stikeAmount))
                    {
                        displayStrike = true;
                        result += NormalText(
                            lineText.PadRight(nameWidth) + NormalText(strikeBlock, isutf),
                            isutf) + "\n";
                    }
                    else
                    {
                        result += NormalText(lineText, isutf) + "\n";
                    }
                }
            }

            // Last line
            if (!string.IsNullOrEmpty(itemName))
            {
                if (priceOnFirstLine)
                {
                    if (!displayStrike && !string.IsNullOrEmpty(stikeAmount))
                    {
                        displayStrike = true;
                        result += NormalText(
                            itemName.PadRight(nameWidth) + NormalText(strikeBlock, isutf),
                            isutf) + "\n";
                    }
                    else
                    {
                        result += NormalText(itemName, isutf) + "\n";
                    }
                }
                else
                {
                    if (!IsStrikeAmount && !string.IsNullOrEmpty(stikeAmount) && string.IsNullOrWhiteSpace(priceStr))
                    {
                        displayStrike = true;
                        result += NormalText(
                            itemName.PadRight(nameWidth) + NormalText(strikeBlock, isutf),
                            isutf) + "\n";
                    }
                    else
                    {
                        result += NormalText(
                            itemName.PadRight(nameWidth) + NormalText(priceBlock, isutf),
                            isutf) + "\n";
                    }
                }
            }

            IsStrikeAmount = displayStrike;
            return result;
        }

        public static StringBuilder GiftCardItemsStringBuilder(InvoiceDto invoice, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false)
        {
            StringBuilder sb = new StringBuilder();

            #region  Prepair - Items header/ List
            sb.Append("\n");
            var column1header = string.IsNullOrEmpty(receiptTemplate.ItemTitleLabel) ? "Items" : receiptTemplate.ItemTitleLabel;
            var column2header = string.IsNullOrEmpty(receiptTemplate.QuantityTitleLabel) ? "Quantity" : receiptTemplate.QuantityTitleLabel;
            var spacecnt = Imin_LINE_WIDTH - column1header.Length - column2header.Length;
            var space = new string(' ', spacecnt);
            sb.Append(NormalText(column1header + space + column2header + "\n", isutf));
            var dsiplayline = new string('-', Imin_LINE_WIDTH);
            sb.Append(NormalText(dsiplayline, isutf) + "\n");
            var result = Extensions.SetDisplayTitle(invoice.InvoiceLineItems, receiptTemplate);
            if (result != null)
            {
                var cnt = 0;
                foreach (var groupitem in result)
                {
                    if (Settings.StoreGeneralRule.ShowGroupProductsByCategory)
                        sb.Append(NormalText(WrapItemLine(groupitem.Title, " ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                    sb.Append(PrepareItemForPrint(groupitem.InvoiceLineItems, receiptTemplate, CurrentOutlet, Imin_LINE_WIDTH, Imin_Column_Width, isutf, "GC").ToString());

                    cnt++;
                    if (cnt < result.Count)
                    {
                        //sb.Append(NormalText(WrapItemLine(" ", " ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                        sb.Append("\n");
                    }
                }
            }

            #endregion
            return sb;
        }

        public static StringBuilder DDItemsStringBuilder(InvoiceDto invoice, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n");
            var column1header = string.IsNullOrEmpty(receiptTemplate.ItemTitleLabel) ? "Items" : receiptTemplate.ItemTitleLabel;
            var column2header = string.IsNullOrEmpty(receiptTemplate.QuantityTitleLabel) ? "Quantity" : receiptTemplate.QuantityTitleLabel;
            var spacecnt = Imin_LINE_WIDTH - column1header.Length - column2header.Length;
            var space = new string(' ', spacecnt);
            sb.Append(NormalText(column1header + space + column2header + "\n", isutf));
            var dsiplayline = new string('-', Imin_LINE_WIDTH);
            sb.Append(NormalText(dsiplayline, isutf) + "\n");

            var result = Extensions.SetDisplayTitle(invoice.InvoiceLineItems, receiptTemplate);
            if (result != null)
            {
                var cnt = 0;
                foreach (var groupitem in result)
                {
                    if (Settings.StoreGeneralRule.ShowGroupProductsByCategory)
                        sb.Append(NormalText(WrapItemLine(groupitem.Title, " ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                    sb.Append(PrepareItemForPrint(groupitem.InvoiceLineItems, receiptTemplate, CurrentOutlet, Imin_LINE_WIDTH, Imin_Column_Width, isutf, "DD").ToString());

                    cnt++;
                    if (cnt < result.Count)
                    {
                        //sb.Append(NormalText(WrapItemLine(" ", " ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                        sb.Append("\n");
                    }
                }
            }

            CheckStoreFeatureConverter checkStoreFeatureConverter = new CheckStoreFeatureConverter();
            var result1 = checkStoreFeatureConverter.Convert(receiptTemplate.ShowTotalNumberOfItemsOnReceipt, null, "HikeShowTotalNumberOfItemsOnReceiptFeature", null);
            if (result1 != null && result1 is bool isShowTotalNumberOfItems && isShowTotalNumberOfItems)
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("Total items in cart: " + invoice.InvoiceLineItemsCnt + "\n", isutf));
            }
            if (!string.IsNullOrEmpty(invoice.Note) && receiptTemplate.ShowCustomerNote)
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("Note: " + invoice.Note + "\n", isutf));
            }
            return sb;
        }

        public static StringBuilder PickAndPackItemsStringBuilder(InvoiceFulfillmentDto invoiceFulfillment, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false, string column3header = "Pack-now")
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n");
            var column1header = "Item";
            var column2header = "Ordered";

            var spacecnt = Imin_LINE_WIDTH - column1header.Length - column2header.Length;
            var space = new string(' ', spacecnt);

            column1header = column1header + new string(' ', Imin_LINE_WIDTH - (column1header.Length + (Imin_Column_Width * 2)));
            column2header = new string(' ', Imin_Column_Width - column2header.Length) + column2header;
            column3header = new string(' ', Imin_Column_Width - column3header.Length) + column3header;
            sb.Append(NormalText(column1header + column2header + column3header + "\n", isutf));
            var dsiplayline = new string('-', Imin_LINE_WIDTH);
            sb.Append(NormalText(dsiplayline, isutf) + "\n");

            var cnt = 0;
            foreach (var groupitem in invoiceFulfillment.InvoiceLineItems)
            {
                column2header = new string(' ', Imin_Column_Width - groupitem.Ordered.Length) + groupitem.Ordered;
                column3header = new string(' ', Imin_Column_Width - groupitem.FulfillmentQuantity.Length) + groupitem.FulfillmentQuantity;
                sb.Append(NormalText(WrapItemLine(groupitem.ItemName, column2header + column3header, Imin_LINE_WIDTH, (Imin_Column_Width * 2), isutf), isutf));
                cnt++;
                if (cnt < invoiceFulfillment.InvoiceLineItems.Count)
                {
                    sb.Append(NormalText(WrapItemLine(" ", " ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                }
            }

            return sb;
        }

        public static StringBuilder InvoiceItemsStringBuilder(InvoiceDto invoice, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n");
            var column1header = string.IsNullOrEmpty(receiptTemplate.ItemTitleLabel) ? "Items" : receiptTemplate.ItemTitleLabel;
            var column2header = string.IsNullOrEmpty(receiptTemplate.QuantityTitleLabel) ? "Price" : receiptTemplate.PriceTitleLabel;
            var spacecnt = Imin_LINE_WIDTH - column1header.Length - column2header.Length;
            var space = new string(' ', spacecnt);
            sb.Append(NormalText(column1header + space + column2header + "\n", isutf));
            var dsiplayline = new string('-', Imin_LINE_WIDTH);
            sb.Append(NormalText(dsiplayline, isutf) + "\n");

            var result = Extensions.SetDisplayTitle(invoice.InvoiceLineItems, receiptTemplate);
            if (result != null)
            {
                var cnt = 0;
                foreach (var groupitem in result)
                {
                    if (Settings.StoreGeneralRule.ShowGroupProductsByCategory)
                        sb.Append(NormalText(WrapItemLine(groupitem.Title, " ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                    sb.Append(PrepareItemForPrint(groupitem.InvoiceLineItems, receiptTemplate, CurrentOutlet, Imin_LINE_WIDTH, Imin_Column_Width, isutf, "Invoice").ToString());

                    cnt++;
                    if (cnt < result.Count)
                    {
                        //sb.Append(NormalText(WrapItemLine(" ", " ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                        sb.Append("\n");
                    }
                }
            }

            sb.Append(string.Empty);
            sb.Append(NormalText(dsiplayline, isutf) + "\n");
            if (invoice.TotalDiscount > 0 && !receiptTemplate.HideDiscountLineOnReceipt)
            {
                sb.Append(string.Empty);
                sb.Append(RightWrapItemLine(receiptTemplate.DiscountLable, invoice.TotalDiscount.ToString("c"), Imin_LINE_WIDTH, isutf));
                if (Settings.StoreGeneralRule.ShowInvoiceLevelDiscountInPercentage)
                {
                    var disCountAsPercent = ViewModels.InvoiceCalculations.GetPercentfromValue(invoice.TotalDiscount, invoice.NetAmount + invoice.TotalDiscount);
                    disCountAsPercent = Math.Round(disCountAsPercent, 2, MidpointRounding.AwayFromZero);
                    string TotalDiscountAsPercent = $"({disCountAsPercent}%)";
                    sb.Append(RightWrapItemLine("  ", TotalDiscountAsPercent, Imin_LINE_WIDTH, isutf));
                }
                sb.Append("\n");
            }
            sb.Append(RightWrapItemLine(receiptTemplate.SubTotalLable, invoice.SubTotal.ToString("c"), Imin_LINE_WIDTH, isutf));

            

            if (invoice.TotalShippingCost > 0 && invoice.ShippingTaxAmountExclusive.HasValue)
            {
                sb.Append(string.Empty);
                sb.Append(RightWrapItemLine("Shipping (Ex. Tax)", invoice.ShippingTaxAmountExclusive.Value.ToString("c"), Imin_LINE_WIDTH, isutf));
            }

            if (invoice.RoundingAmount > 0)
            {
                sb.Append(string.Empty);
                sb.Append(RightWrapItemLine("Rounding", invoice.RoundingAmount.ToString("c"), Imin_LINE_WIDTH, isutf));
            }

            ObservableCollection<LineItemTaxDto> taxes = new ObservableCollection<LineItemTaxDto>();
            if (ViewModels.InvoiceCalculations._navigationService?.NavigatedPage != null && ViewModels.InvoiceCalculations._navigationService.NavigatedPage is PaymentPage)
                taxes = GetPrintTaxesPaymentPage(invoice, receiptTemplate);
            else
                taxes = GetPrintTaxes(invoice, receiptTemplate);

            int i = 0;
            foreach (var tax in taxes)
            {
                sb.Append(string.Empty);
                if (tax.IsGroupTax)
                {
                    sb.Append(RightWrapItemLine("----------", new string('-', Imin_LINE_WIDTH > 32 ? 16 : 14), Imin_LINE_WIDTH, isutf));
                    sb.Append(BoldText(RightWrapItemLine(tax.TaxName, tax.TaxAmount.ToString("c"), Imin_LINE_WIDTH, isutf), isutf));
                    //sb.Append(NormalText(RightWrapItemLine(tax.TaxName, tax.TaxAmount.ToString("c"), Imin_LINE_WIDTH, isutf), isutf));
                }
                else
                {
                    // if((tax.TaxName.Contains(Settings.CurrentRegister.ReceiptTemplate.TaxLable + "(")  || tax.TaxName.Contains("Tax(") || tax.TaxName.Contains("(Inc. Tax)") || tax.TaxName.Contains("(Ex. Tax)")) 
                    // && i > 0 && (!taxes[i-1].TaxName.Contains(Settings.CurrentRegister.ReceiptTemplate.TaxLable + "(")  || !taxes[i-1].TaxName.Contains("Tax(")))
                    //     sb.Append(RightWrapItemLine("----------", new string('-', Imin_LINE_WIDTH > 32 ? 16 : 14), Imin_LINE_WIDTH, isutf));
                    
                    sb.Append(RightWrapItemLine(tax.TaxName, tax.TaxAmount.ToString("c"), Imin_LINE_WIDTH, isutf));
                }
                i++;
            }

            sb.Append(string.Empty);
            sb.Append(RightWrapItemLine("----------", new string('-', Imin_LINE_WIDTH > 32 ? 16 : 14), Imin_LINE_WIDTH, isutf));
           // sb.Append(NormalText(RightWrapItemLine(receiptTemplate.TotalLable, invoice.NetAmount.ToString("c"), Imin_LINE_WIDTH, isutf), isutf));
            sb.Append(BoldText(RightWrapItemLine(receiptTemplate.TotalLable, invoice.NetAmount.ToString("c"), Imin_LINE_WIDTH, isutf), isutf));

            //Start Ticket #84441 iOS: FR :add discount on receipt (Sale Invoice Receipt Upgradation) by Pratik
            if (receiptTemplate.ShowTotalDiscountOnReciept)
            {
                var totaldis = (invoice.TotalDiscount + invoice.InvoiceLineItems.Sum(a => a.TotalDiscount));
                if (totaldis > 0)
                {
                    sb.Append(string.Empty);
                    sb.Append(RightWrapItemLine("(" + LanguageExtension.Localize("TotalSaleDiscount") + " " + totaldis.ToString("C") + ")", "  ", Imin_LINE_WIDTH, isutf));
                }
            }
            sb.Append(RightWrapItemLine("----------", new string('-', Imin_LINE_WIDTH > 32 ? 16 : 14), Imin_LINE_WIDTH, isutf));

            //End Ticket #84441 by Pratik

            if(invoice.ActiveInvoicePayments != null && invoice.ActiveInvoicePayments.Count > 0)
            {
                foreach (var payment in invoice.ActiveInvoicePayments)
                {
                    sb.Append(string.Empty);
                    var lbl = payment.PrintPaymentOptionDisplayName;
                    sb.Append(RightWrapItemLine(payment.PrintPaymentOptionDisplayName, payment.Amount.ToString("c"), Imin_LINE_WIDTH, isutf));
                    if (receiptTemplate.ShowPaymentDateOnReciept && payment.PaymentStoreDate.HasValue)
                    {
                        sb.Append(RightWrapItemLine("(" + payment.PaymentStoreDate.Value.ToString("dd MMM, yyyy hh.mmtt")+ ")", " ", Imin_LINE_WIDTH, isutf));
                    }
                    
                }
            }

            if (!string.IsNullOrEmpty(invoice.ChangeAmmountDetail))
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(new string(' ', (Imin_LINE_WIDTH - invoice.ChangeAmmountDetail.Length)) + invoice.ChangeAmmountDetail, isutf) + "\n");
            }

            sb.Append("\n");
            sb.Append(RightWrapItemLine(receiptTemplate.ToPayLable, invoice.OutstandingAmount.ToString("c"), Imin_LINE_WIDTH, isutf));

            if (invoice.Status == Enums.InvoiceStatus.OnAccount && invoice.InvoiceDueDate.HasValue && receiptTemplate.ShowInvoiceDueDateOnReciept)
            {
                sb.Append(string.Empty);
                sb.Append(RightWrapItemLine("Due on: " + invoice.InvoiceDueDate.Value.ToStoreTime().ToString("dd MMM yyyy"), "  ", Imin_LINE_WIDTH, isutf));
            }

            if (receiptTemplate.ShowOnAccountOutStadningOnReciept && !string.IsNullOrEmpty(receiptTemplate.ToPayLable) && receiptTemplate.ToPayLable.ToLower().Contains("outstanding"))
            {
                CustomFieldsResponce result2 = null;
                result2 = invoice.CustomFields != null ? JsonConvert.DeserializeObject<CustomFieldsResponce>(invoice.CustomFields) : null;
                if (invoice.OutstandingAmount > 0 && invoice.Status == InvoiceStatus.OnAccount && result2?.invoiceOutstanding != null || invoice.InvoiceOutstanding != null)
                {
                    sb.Append(string.Empty);
                    sb.Append(NormalText(dsiplayline, isutf) + "\n");
                    sb.Append(NormalText("OUTSTANDING" + "\n", isutf));
                    var data = invoice.InvoiceOutstanding != null ? invoice.InvoiceOutstanding : result2.invoiceOutstanding;
                    sb.Append(NormalText("Previous Outstanding : " + (data.previousOutstanding.HasValue ? data.previousOutstanding.Value : decimal.Zero).ToString("C") + "\n", isutf));
                    sb.Append(NormalText("Current Sale : " + (data.currentSale.HasValue ? data.currentSale.Value : decimal.Zero).ToString("C") + "\n", isutf));
                    sb.Append(NormalText("Current Outstanding : " + (data.currentOutstanding.HasValue ? data.currentOutstanding.Value : decimal.Zero).ToString("C") + "\n", isutf));
                }
            }

            if (invoice.CustomerDetail != null && invoice.CustomerDetail.AllowLoyalty && invoice.CustomerId != null && invoice.CustomerId != 0 && Settings.StoreGeneralRule.EnableLoyalty && receiptTemplate.ShowLoyaltyPointsOnReciept)
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("LOYALTY POINTS" + "\n", isutf));
                sb.Append(NormalText("Balance : " + invoice.CustomerCurrentLoyaltyPoints.ToString() + "\n", isutf));


                if (invoice.InvoiceLineItems != null)
                {
                    sb.Append(NormalText("This visit - Earned : " + (invoice.LoyaltyPoints + invoice.InvoiceLineItems.Sum(x => x.CustomerGroupLoyaltyPoints)).ToString() + "\n", isutf));
                }

                decimal LoyaltyRedeemed = 0;
                if (invoice.InvoicePayments != null && invoice.InvoicePayments.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty) && Settings.StoreGeneralRule != null)
                {
                    LoyaltyRedeemed = invoice.InvoicePayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty).Sum(x => x.Amount) * Settings.StoreGeneralRule.LoyaltyPointsValue;
                }
                sb.Append(NormalText("This visit - Redeemed : " + "-" + LoyaltyRedeemed.ToString() + "\n", isutf));
                sb.Append(NormalText("Closing balance : " + (invoice.CustomerCurrentLoyaltyPoints + invoice.LoyaltyPoints - LoyaltyRedeemed).ToString() + "\n", isutf));
            }

            if (invoice.CustomerDetail != null && invoice.CustomerId != null && invoice.CustomerId != 0 && receiptTemplate.ShowStoreCreditOnReciept && invoice.InvoicePayments != null && invoice.InvoicePayments.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Credit))
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("STORE CREDIT BALANCE" + "\n", isutf));
                var storeCreditPayments = invoice.InvoicePayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Credit);
                var storeCreditUsed = storeCreditPayments.Sum(x => x.Amount);
                sb.Append(NormalText("This visit - Used : " + string.Format("{0:C}", storeCreditUsed) + "\n", isutf));
                if (storeCreditPayments.Last().InvoicePaymentDetails.Any())
                {
                    sb.Append(NormalText("Opening Balance : " + string.Format("{0:C}", Convert.ToDecimal(storeCreditPayments.First().InvoicePaymentDetails?.First().Value)) + "\n", isutf));
                    sb.Append(NormalText("Closing balance : " + string.Format("{0:C}", Convert.ToDecimal(storeCreditPayments.Last().InvoicePaymentDetails?.Last().Value))+ "\n", isutf));
                }
            }

            var GiftCardBalanceTally = ShowUsedGiftCardBalanceTally(invoice);
            if (GiftCardBalanceTally != null && GiftCardBalanceTally.Count > 0)
            {
                foreach (var gc in GiftCardBalanceTally)
                {
                    sb.Append(string.Empty);
                    sb.Append(NormalText(dsiplayline, isutf) + "\n");
                    sb.Append(NormalText("Gift card # " + gc.Number + "\n", isutf));
                    sb.Append(NormalText("Opening balance : " + gc.OpeningBalance.ToString("c") + "\n", isutf));
                    sb.Append(NormalText("This visit - used : " + gc.UsedBalance.ToString("c") + "\n", isutf));
                    sb.Append(NormalText("Closing balance : " + gc.ClosingBalance.ToString("c") + "\n", isutf));
                }
            }

            CheckStoreFeatureConverter checkStoreFeatureConverter = new CheckStoreFeatureConverter();
            var result1 = checkStoreFeatureConverter.Convert(receiptTemplate.ShowTotalNumberOfItemsOnReceipt, null, "HikeShowTotalNumberOfItemsOnReceiptFeature", null);
            if (result1 != null && result1 is bool isShowTotalNumberOfItems && isShowTotalNumberOfItems)
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("Total Items: " + invoice.InvoiceLineItemsCnt + "\n", isutf));
            }

            if (!string.IsNullOrEmpty(invoice.Note))
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("Note: " + invoice.Note.TrimStart().TrimEnd() + "\n", isutf));
            }
            return sb;
        }

        public static List<GiftCardDetail> ShowUsedGiftCardBalanceTally(InvoiceDto invoice)
        {
            var giftCardDetails = new List<GiftCardDetail>();
            var giftCardPaymentDetails = invoice.InvoicePayments
                .Where(x => x.PaymentOptionType == PaymentOptionType.GiftCard && x.InvoicePaymentDetails != null)
                .Select(x => x.InvoicePaymentDetails)
                .ToList();

            if (giftCardPaymentDetails.Any())
            {
                foreach (var paymentDetailList in giftCardPaymentDetails)
                {
                    var number = paymentDetailList
                        .FirstOrDefault(x => x.Key == InvoicePaymentKey.GiftCardNumber)?.Value?.Trim();

                    var openingStr = paymentDetailList
                        .FirstOrDefault(x => x.Key == InvoicePaymentKey.GiftCardOpeningBalance)?.Value?.Trim();

                    var closingStr = paymentDetailList
                        .FirstOrDefault(x => x.Key == InvoicePaymentKey.GiftCardClosingBalance)?.Value?.Trim();

                    decimal.TryParse(openingStr, out decimal opening);
                    decimal.TryParse(closingStr, out decimal closing);

                    var used = opening - closing;

                    giftCardDetails.Add(new GiftCardDetail
                    {
                        Number = number,
                        OpeningBalance = opening,
                        UsedBalance = used,
                        ClosingBalance = closing
                    });
                }

            }
            return giftCardDetails;
        }

        public static new ObservableCollection<LineItemTaxDto> GetPrintTaxes(InvoiceDto Invoice, ReceiptTemplateDto receiptTemplate)
        {
            var texLabel = "Tax";
            if (receiptTemplate != null)
                texLabel = receiptTemplate.TaxLable;
            var taxes = new ObservableCollection<LineItemTaxDto>((Invoice.ReceiptTaxList != null && Invoice.ReceiptTaxList.Count > 0) ? Invoice.ReceiptTaxList : Invoice.Taxgroup);
            var taxes1 = taxes?.Copy();
            if (taxes1 != null)
                taxes = taxes1;
            if (Invoice.TotalTip.ToPositive() > 0)
            {
                var hasTax = taxes?.FirstOrDefault(x => x.TaxId == Invoice.TipTaxId);
                if (hasTax != null)
                {
                    hasTax.TaxRate += Invoice.TipTaxRate.Value;
                    hasTax.TaxAmount += Invoice.TipTaxAmount.RoundingUptoTwoDecimal();
                }
                else
                {
                    if (taxes != null && !taxes.Any(a => a.TaxId == Invoice.TipTaxId) && Invoice.TipTaxId != null && Invoice.TipTaxRate != null)
                    {
                        var tipTax = new LineItemTaxDto()
                        {
                            TaxId = Invoice.TipTaxId.Value,
                            TaxName = texLabel + "(" + Invoice.TipTaxName + ")",
                            TaxRate = Invoice.TipTaxRate.Value,
                            TaxAmount = Invoice.TipTaxAmount,
                            SubTaxes = new ObservableCollection<LineItemTaxDto>()
                        };
                        taxes.Add(tipTax);
                    }

                }
            }



            if (Invoice.TotalShippingCost.ToPositive() > 0)
            {
                var hasTax = taxes?.FirstOrDefault(x => x.TaxId == Invoice.shippingTaxId.Value);
                if (hasTax != null)
                {
                    hasTax.TaxRate += Invoice.ShippingTaxRate.Value;
                    hasTax.TaxAmount += Invoice.ShippingTaxAmount.Value.RoundingUptoTwoDecimal();
                }
                else
                {
                    if (Invoice.shippingTaxId.HasValue && taxes != null && !taxes.Any(a => a.TaxId == Invoice.shippingTaxId.Value))
                    {
                        var shipingTax = new LineItemTaxDto()
                        {
                            TaxId = Invoice.shippingTaxId.Value,
                            TaxName = texLabel + "(" + Invoice.ShippingTaxName + ")",
                            TaxRate = Invoice.ShippingTaxRate.Value,
                            TaxAmount = Invoice.ShippingTaxAmount.Value,
                            SubTaxes = new ObservableCollection<LineItemTaxDto>()
                        };
                        taxes.Add(shipingTax);
                    }
                    else if (Invoice.shippingTaxId.HasValue && taxes != null && taxes.Any(a => a.TaxId == Invoice.shippingTaxId.Value))
                    {
                        var taxdata = taxes.First(a => a.TaxId == Invoice.shippingTaxId.Value);
                        taxdata.TaxRate += Invoice.ShippingTaxRate.Value;
                        taxdata.TaxAmount += Invoice.ShippingTaxAmount.Value.RoundingUptoTwoDecimal();
                    }
                }
            }
            if (Invoice.Taxgroup != null && Invoice.Taxgroup.Count > 0 && taxes != null && taxes.Any())
            {
                foreach (var item in Invoice.Taxgroup)
                {
                    if (item.SubTaxes != null && item.SubTaxes.Count > 0 && taxes.FirstOrDefault(x => x.TaxId == item.TaxId) != null)
                    {
                        var existtax1 = taxes.FirstOrDefault(x => x.TaxId == item.TaxId);
                        existtax1.SubTaxes = item.SubTaxes;
                        var totalrate = item.SubTaxes.Sum(a => a.TaxRate);
                        foreach (var subitem in item.SubTaxes)
                        {
                            var existtax = taxes.FirstOrDefault(x => x.TaxId == subitem.TaxId && x.TaxName == subitem.TaxName);
                            if (existtax != null)
                            {
                                existtax.TaxAmount = ((subitem.TaxRate * existtax1.TaxAmount) / totalrate).RoundingUptoTwoDecimal();
                            }
                        }
                    }
                }
            }

            if (Invoice.TotalTipTaxExclusive > 0)
            {
                if (Invoice.TipTaxId == null || Invoice.TipTaxId == 0)
                {
                    taxes.Add(new LineItemTaxDto() { TaxName = receiptTemplate.tipsLable + "(Inc.Tax)", TaxAmount = Invoice.TotalTipTaxExclusive });
                }
                else
                {
                    taxes.Add(new LineItemTaxDto() { TaxName = receiptTemplate.tipsLable + "(Ex.Tax)", TaxAmount = Invoice.TotalTipTaxExclusive });
                }
            }

            return taxes;
        }

        public static new ObservableCollection<LineItemTaxDto> GetPrintTaxesPaymentPage(InvoiceDto invoice, ReceiptTemplateDto CurrentReceiptTemplate)
        {
            var texLabel = "Tax";
            if (CurrentReceiptTemplate != null)
                texLabel = CurrentReceiptTemplate.TaxLable;
            var taxes = new ObservableCollection<LineItemTaxDto>();

            var TotalEffectiveamount = invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
            decimal discountPercentValue = 0;
            if (!invoice.DiscountIsAsPercentage)
            {
                var total = (invoice.Status == InvoiceStatus.Refunded ? TotalEffectiveamount.ToPositive() : TotalEffectiveamount);
                if (total != 0)
                    discountPercentValue = (invoice.DiscountValue * 100) / total;
            }
            else
            {
                discountPercentValue = invoice.DiscountValue;
            }

            invoice.Taxgroup = ViewModels.InvoiceCalculations.GetTaxgroupForprint(invoice, discountPercentValue);

            foreach (var item in invoice.Taxgroup.OrderBy(a => a.IsGroupTax))
            {

                if (item.SubTaxes.Count > 0)
                {
                    item.TaxName = texLabel + "(" + item.TaxName + ")";
                    taxes.Add(item);
                    foreach (var item2 in item.SubTaxes)
                    {
                        taxes.Add(item2);
                    }
                }
                else
                {
                    item.TaxName = texLabel + "(" + item.TaxName + ")";
                    taxes.Add(item);
                }
            }

            var taxes1 = taxes.Copy();
            if (taxes1 != null)
                taxes = taxes1;

            if (invoice.TotalTip.ToPositive() > 0)
            {
                var hasTax = taxes.FirstOrDefault(x => x.TaxId == invoice.TipTaxId);
                if (hasTax != null)
                {
                    hasTax.TaxRate += invoice.TipTaxRate.Value;
                    hasTax.TaxAmount += invoice.TipTaxAmount.RoundingUptoTwoDecimal();
                }
                else
                {
                    if (taxes != null && !taxes.Any(a => a.TaxId == invoice.TipTaxId) && invoice.TipTaxId != null && invoice.TipTaxRate != null)
                    {
                        var tipTax = new LineItemTaxDto()
                        {
                            TaxId = invoice.TipTaxId.Value,
                            TaxName = texLabel + "(" + invoice.TipTaxName + ")",
                            TaxRate = invoice.TipTaxRate.Value,
                            TaxAmount = invoice.TipTaxAmount,
                            SubTaxes = new ObservableCollection<LineItemTaxDto>()
                        };
                        taxes.Add(tipTax);
                    }
                }
            }
            if (invoice.TotalShippingCost.ToPositive() > 0)
            {
                var hasTax = taxes.FirstOrDefault(x => x.TaxId == invoice.shippingTaxId.Value);
                if (hasTax != null)
                {
                    hasTax.TaxRate += invoice.ShippingTaxRate.Value;
                    hasTax.TaxAmount += invoice.ShippingTaxAmount.Value.RoundingUptoTwoDecimal();
                }
                else
                {
                    if (invoice.shippingTaxId.HasValue && taxes != null && !taxes.Any(a => a.TaxId == invoice.shippingTaxId.Value))
                    {
                        var shipingTax = new LineItemTaxDto()
                        {
                            TaxId = invoice.shippingTaxId.Value,
                            TaxName = texLabel + "(" + invoice.ShippingTaxName + ")",
                            TaxRate = invoice.ShippingTaxRate.Value,
                            TaxAmount = invoice.ShippingTaxAmount.Value,
                            SubTaxes = new ObservableCollection<LineItemTaxDto>()
                        };
                        taxes.Add(shipingTax);
                    }
                    else if (invoice.shippingTaxId.HasValue && taxes != null && taxes.Any(a => a.TaxId == invoice.shippingTaxId.Value))
                    {
                        var taxdata = taxes.First(a => a.TaxId == invoice.shippingTaxId.Value);
                        taxdata.TaxRate += invoice.ShippingTaxRate.Value;
                        taxdata.TaxAmount += invoice.ShippingTaxAmount.Value.RoundingUptoTwoDecimal();
                    }
                }
            }
            if (invoice.Taxgroup != null && invoice.Taxgroup.Count > 0 && taxes.Any())
            {
                foreach (var item in invoice.Taxgroup)
                {
                    if (item.SubTaxes != null && item.SubTaxes.Count > 0 && taxes.FirstOrDefault(x => x.TaxId == item.TaxId) != null)
                    {
                        var existtax1 = taxes.FirstOrDefault(x => x.TaxId == item.TaxId);
                        existtax1.SubTaxes = item.SubTaxes;
                        var totalrate = item.SubTaxes.Sum(a => a.TaxRate);
                        foreach (var subitem in item.SubTaxes)
                        {
                            var existtax = taxes.FirstOrDefault(x => x.TaxId == subitem.TaxId && x.TaxName == subitem.TaxName);
                            if (existtax != null)
                            {
                                existtax.TaxAmount = ((subitem.TaxRate * existtax1.TaxAmount) / totalrate).RoundingUptoTwoDecimal();
                            }
                        }
                    }
                }
            }

            if (invoice.TotalTipTaxExclusive > 0)
            {
                if (invoice.TipTaxId == null || invoice.TipTaxId == 0)
                {
                    taxes.Add(new LineItemTaxDto() { TaxName = CurrentReceiptTemplate.tipsLable + "(Inc.Tax)", TaxAmount = invoice.TotalTipTaxExclusive });
                }
                else
                {
                    taxes.Add(new LineItemTaxDto() { TaxName = CurrentReceiptTemplate.tipsLable + "(Ex.Tax)", TaxAmount = invoice.TotalTipTaxExclusive });
                }
            }
            return taxes;
        }

        public static bool IsStrikeAmount = false;
        static StringBuilder PrepareItemForPrint(ObservableCollection<InvoiceLineItemDto> invoiceLineItems, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf, string printname)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in invoiceLineItems)
            {
                if (item.IsExtraproduct)
                {
                    if (printname == "Invoice")
                    {
                        var StrikePrice = string.Empty;
                        if (Settings.StoreGeneralRule.DisplayLineItemDiscountOnReceipt && item.IsTotalReatilAmountVisible)
                        {
                            StrikePrice = Strike(item.TotalReatilAmount.ToString("c"), isutf);
                        }

                        if (!string.IsNullOrEmpty(item.ProductTitleWithQuantity))
                        {
                            Imin_Column_Width = item.TotalAmount.ToString("c").Length + 1;
                            sb.Append(NormalText(WrapExtraItemLine((item.ProductTitle ?? item.Title), item.TotalAmount.ToString("c"), Imin_LINE_WIDTH, Imin_Column_Width, isutf, StrikePrice), isutf));
                        }

                        string lbl = string.Empty;
                        if (!string.IsNullOrEmpty(item.SKUWithLabel) && receiptTemplate.PrintSKU && !receiptTemplate.ReplaceProductNameWithSKU)
                        {
                            lbl = item.SKUWithLabel;
                        }
                        if (!string.IsNullOrEmpty(item.SerialNumber))
                        {
                            if (string.IsNullOrEmpty(lbl))
                                lbl = item.SerialNumber;
                            else
                                lbl += "  SN: " + item.SerialNumber;
                        }

                        if (!string.IsNullOrEmpty(lbl))
                        {
                            sb.Append(NormalText(WrapExtraItemLine(lbl, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf, (IsStrikeAmount ? string.Empty : StrikePrice)), isutf));
                        }

                        if (!string.IsNullOrEmpty(item.BarcodeWithLabel) && receiptTemplate.ShowItemBarCode)
                        {
                            sb.Append(NormalText(WrapExtraItemLine(item.BarcodeWithLabel, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf, (IsStrikeAmount ? string.Empty : StrikePrice)), isutf));
                        }
                        if (!string.IsNullOrEmpty(item.CustomField) && receiptTemplate.ShowCustomField)
                        {
                            sb.Append(NormalText(WrapExtraItemLine((receiptTemplate.CustomFieldLabel ?? string.Empty) + ": " + item.CustomField, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf, (IsStrikeAmount ? string.Empty : StrikePrice)), isutf));
                        }
                        if (!string.IsNullOrEmpty(item.Description))
                        {
                            string s = item.Description.TrimStart().TrimEnd();
                            sb.Append(NormalText(WrapExtraItemLine(s, " ", Imin_LINE_WIDTH, 1, isutf, (IsStrikeAmount ? string.Empty : StrikePrice)), isutf));
                        }
                        if (!string.IsNullOrEmpty(item.OffersNote) && item.OfferDiscountPercent.HasValue)
                        {
                            string s = "->" + " " + item.OffersNote.TrimStart().TrimEnd();
                            sb.Append(NormalText(WrapExtraItemLine(s, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf, (IsStrikeAmount ? string.Empty : StrikePrice)), isutf));
                        }
                        IsStrikeAmount = false;
                    }
                    else
                    {
                        sb.Append(NormalText(WrapExtraItemLine((item.ProductTitle ?? string.Empty), item.Quantity.ToString(), Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                        if (!string.IsNullOrEmpty(item.Description) && printname == "DD")
                        {
                            string s = item.Description.TrimStart().TrimEnd();
                            sb.Append(NormalText(WrapExtraItemLine(s, "  ", Imin_LINE_WIDTH, 2, isutf), isutf));
                        }
                        if (!string.IsNullOrEmpty(item.SerialNumber) && printname == "DD")
                        {
                            sb.Append(NormalText(WrapExtraItemLine("SN: " + item.SerialNumber, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                        }
                        if (!string.IsNullOrEmpty(item.CustomField) && receiptTemplate.ShowCustomField)
                        {
                            sb.Append(NormalText(WrapExtraItemLine((receiptTemplate.CustomFieldLabel ?? string.Empty) + ": " + item.CustomField, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                        }
                    }
                }
                else
                {

                    if (printname == "Invoice")
                    {
                        var StrikePrice = string.Empty;
                        if (Settings.StoreGeneralRule.DisplayLineItemDiscountOnReceipt && item.IsTotalReatilAmountVisible)
                        {
                            StrikePrice = Strike(item.TotalReatilAmount.ToString("c"), isutf);
                        }

                        if (!string.IsNullOrEmpty(item.ProductTitleWithQuantity))
                        {
                            Imin_Column_Width = item.TotalAmount.ToString("c").Length + 1;
                            sb.Append(NormalText(WrapItemLine((item.ProductTitle ?? string.Empty), item.TotalAmount.ToString("c"), Imin_LINE_WIDTH, Imin_Column_Width, isutf, StrikePrice), isutf));
                        }

                        string lbl = string.Empty;
                        if (!string.IsNullOrEmpty(item.SKUWithLabel) && receiptTemplate.PrintSKU && !receiptTemplate.ReplaceProductNameWithSKU)
                        {
                            lbl = item.SKUWithLabel;
                        }
                        if (!string.IsNullOrEmpty(item.SerialNumber))
                        {
                            if (string.IsNullOrEmpty(lbl))
                                lbl = item.SerialNumber;
                            else
                                lbl += "  SN: " + item.SerialNumber;
                        }

                        if (!string.IsNullOrEmpty(lbl))
                        {
                            sb.Append(NormalText(WrapItemLine(lbl, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf, (IsStrikeAmount ? string.Empty : StrikePrice)), isutf));
                        }

                        if (!string.IsNullOrEmpty(item.BarcodeWithLabel) && receiptTemplate.ShowItemBarCode)
                        {
                            sb.Append(NormalText(WrapItemLine(item.BarcodeWithLabel, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf, (IsStrikeAmount ? string.Empty : StrikePrice)), isutf));
                        }


                        if (!string.IsNullOrEmpty(item.CustomField) && receiptTemplate.ShowCustomField)
                        {
                            sb.Append(NormalText(WrapItemLine((receiptTemplate.CustomFieldLabel ?? string.Empty) + ": " + item.CustomField, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf, (IsStrikeAmount ? string.Empty : StrikePrice)), isutf));
                        }
                        if (!string.IsNullOrEmpty(item.Description) && !string.IsNullOrWhiteSpace(item.Description))
                        {
                            string s = item.Description.TrimStart().TrimEnd();
                            sb.Append(NormalText(WrapItemLine(s, " ", Imin_LINE_WIDTH, 1, isutf, string.Empty), isutf));
                        }
                        if (!string.IsNullOrWhiteSpace(item.OffersNote) && item.OfferDiscountPercent.HasValue)
                        {
                            string s = " -" + " " + item.OffersNote.TrimStart().TrimEnd();
                            sb.Append(NormalText(WrapItemLine(s , "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf, (IsStrikeAmount ? string.Empty : StrikePrice)), isutf));
                        }
                        IsStrikeAmount = false;
                    }
                    else
                    {
                        sb.Append(NormalText(WrapItemLine((item.ProductTitle ?? string.Empty), item.Quantity.ToString(), Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));

                        if (!string.IsNullOrEmpty(item.Description) && printname == "DD")
                        {
                            string s = item.Description.TrimStart().TrimEnd();
                            sb.Append(NormalText(WrapItemLine(s, "  ", Imin_LINE_WIDTH, 2, isutf), isutf));
                        }
                        if (!string.IsNullOrEmpty(item.SerialNumber) && printname == "DD")
                        {
                            sb.Append(NormalText(WrapItemLine("SN: " + item.SerialNumber, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                        }
                        if (!string.IsNullOrEmpty(item.CustomField) && receiptTemplate.ShowCustomField)
                        {
                            sb.Append(NormalText(WrapItemLine((receiptTemplate.CustomFieldLabel ?? string.Empty) + ": " + item.CustomField, "  ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                        }
                    }
                }
            }
            return sb;
        }

        public static string Strike(string input, bool isutf)
        {
            return isutf ? string.Concat(input.Select(c => $"{c}\u0336")) : "\x1B\x47\x01" + input + "\x1B\x47\x00";
        }

        public static StringBuilder CommonHeaderStringBuilder(InvoiceDto invoice, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false)
        {
            StringBuilder sb = new StringBuilder();
            if (receiptTemplate != null)
            {
                if (!string.IsNullOrEmpty(receiptTemplate.StoreNameLable) && !isutf)
                {
                    sb.Append("\n");
                    sb.Append(CenterText(receiptTemplate.StoreNameLable, Imin_LINE_WIDTH, 4, isutf));
                }

                if (!string.IsNullOrEmpty(receiptTemplate.HeaderText))
                {
                    string text = HtmlToPlainText(receiptTemplate.HeaderText);
                    sb.Append(CenterText(NormalText(text, isutf), Imin_LINE_WIDTH, 2, isutf));
                }

                #region  OutletAddress
                if (receiptTemplate.ShowOutletAddress && CurrentOutlet != null)
                {
                    sb.Append("\n");
                    if (!string.IsNullOrEmpty(invoice?.OutletName))
                    {
                        sb.Append(CenterText(NormalText(invoice.OutletName, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(CurrentOutlet.Address?.Address1))
                    {
                        sb.Append(CenterText(NormalText(CurrentOutlet.Address.Address1, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(CurrentOutlet.Address?.FullAddress))
                    {
                        sb.Append(CenterText(NormalText(CurrentOutlet.Address.FullAddress, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(CurrentOutlet.Email))
                    {
                        sb.Append(CenterText(NormalText("Email:" + CurrentOutlet.Email, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(CurrentOutlet.Phone))
                    {
                        sb.Append(CenterText(NormalText("Phone:" + CurrentOutlet.Phone, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                }
                #endregion

                #region Delivery Address / Customer Details

                bool lineadd = false;
                if (receiptTemplate.ShowCustomerDeliverAddress && invoice.DeliveryAddressId != null)
                {
                    lineadd = true;
                    sb.Append("\n");
                    sb.Append(CenterText(NormalText("Delivery Address", isutf), Imin_LINE_WIDTH, 2, isutf));
                    if (!string.IsNullOrEmpty(invoice?.DeliveryAddress?.ReceiverName))
                    {
                        sb.Append(CenterText(NormalText(invoice.DeliveryAddress.ReceiverName, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(invoice?.DeliveryAddress?.ReceiverCompanyName))
                    {
                        sb.Append(CenterText(NormalText(invoice.DeliveryAddress.ReceiverCompanyName, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(invoice?.DeliveryAddress?.Address1))
                    {
                        sb.Append(CenterText(NormalText(invoice.DeliveryAddress.Address1, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(invoice?.DeliveryAddress?.City) || !string.IsNullOrEmpty(invoice?.DeliveryAddress?.State) || !string.IsNullOrEmpty(invoice?.DeliveryAddress?.PostCode))
                    {
                        string CompletAdd = "";
                        if (!string.IsNullOrEmpty(invoice?.DeliveryAddress?.City))
                            CompletAdd += invoice.DeliveryAddress.City + ",";
                        if (!string.IsNullOrEmpty(invoice?.DeliveryAddress?.State))
                            CompletAdd +=  invoice.DeliveryAddress.State + ",";
                        if (!string.IsNullOrEmpty(invoice?.DeliveryAddress?.PostCode))
                            CompletAdd +=  invoice.DeliveryAddress.PostCode + ",";
                         sb.Append(CenterText(NormalText(CompletAdd, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(invoice?.DeliveryAddress?.CountryName))
                    {
                        sb.Append(CenterText(NormalText(invoice.DeliveryAddress.CountryName, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(invoice?.DeliveryAddress?.ReceiverPhone))
                    {
                        sb.Append(CenterText(NormalText("Phone:" + invoice.DeliveryAddress.ReceiverPhone, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                }

                if (receiptTemplate.PrintCustomerTaxId && !string.IsNullOrEmpty(receiptTemplate.CustomerTaxIdLabel) && !string.IsNullOrEmpty(invoice.CustomerDetail?.CustomerTaxId))
                {
                    if (!string.IsNullOrEmpty(receiptTemplate?.CustomerTaxIdLabel) && invoice.CustomerId != null)
                    {
                        if (!lineadd)
                        {
                            lineadd = true;
                            sb.AppendLine(" ");
                        }
                        sb.Append(CenterText(NormalText(receiptTemplate.CustomerTaxIdLabel + ": " + invoice.CustomerId, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                }

                if (!string.IsNullOrEmpty(receiptTemplate.CustomerTitleLabel) &&
                receiptTemplate.ShowCustomerTitle && invoice.CustomerId != null && invoice.CustomerId > 0)
                {
                    lineadd = true;
                    sb.Append("\n");                 
                    sb.Append(CenterText(NormalText(receiptTemplate.CustomerTitleLabel, isutf), Imin_LINE_WIDTH, 2, isutf));
                }
                if (!string.IsNullOrEmpty(invoice.CustomerName) && invoice.CustomerName != "Walk in")
                {
                    if (!lineadd)
                    {
                        lineadd = true;
                        sb.Append("\n");
                    }
                    StringBuilder cus_name = new StringBuilder();
                    cus_name.Append(CenterText(NormalText(invoice.CustomerName, isutf), Imin_LINE_WIDTH, 2, isutf));
                    if (receiptTemplate.ShowCustomerAddress)
                    {
                        if (!string.IsNullOrEmpty(invoice.CustomerDetail?.CompanyName)) 
                        {
                            if (receiptTemplate != null)
                            {
                                if (receiptTemplate.ShowCompanyNameInBillingInvoice)
                                {
                                    cus_name = new StringBuilder();
                                    if (receiptTemplate.ShowCompanyName)
                                    {
                                        cus_name = new StringBuilder();
                                        cus_name.Append(CenterText(NormalText(invoice.CustomerDetail.CompanyName, isutf), Imin_LINE_WIDTH, 2, isutf));
                                        cus_name.Append(CenterText(NormalText(invoice.CustomerDetail.FullName, isutf), Imin_LINE_WIDTH, 2, isutf));
                                    }
                                    else
                                        cus_name.Append(CenterText(NormalText(invoice.CustomerDetail.FullName, isutf), Imin_LINE_WIDTH, 2, isutf));
                                }
                                else
                                {
                                    cus_name = new StringBuilder();
                                    if (receiptTemplate.ShowCompanyName)
                                    {
                                       
                                        cus_name.Append(CenterText(NormalText(invoice.CustomerDetail.FullName, isutf), Imin_LINE_WIDTH, 2, isutf));
                                        cus_name.Append(CenterText(NormalText(invoice.CustomerDetail.CompanyName, isutf), Imin_LINE_WIDTH, 2, isutf));
                                    }
                                    else
                                        cus_name.Append(CenterText(NormalText(invoice.CustomerDetail.FullName, isutf), Imin_LINE_WIDTH, 2, isutf));
                                }
                            }
                        }
                    }
                    sb.Append(cus_name.ToString());
                }

                if (receiptTemplate.ShowCustomerAddress && invoice.CustomerDetail != null)
                {
                    if (!lineadd)
                    {
                        lineadd = true;
                        sb.AppendLine(" ");
                    }
                    if (!string.IsNullOrEmpty(invoice.CustomerDetail?.Address?.Address1))
                    {
                        sb.Append(CenterText(NormalText(invoice.CustomerDetail.Address.Address1, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(invoice.CustomerDetail?.Address?.FullAddress))
                    {
                        sb.Append(CenterText(NormalText(invoice.CustomerDetail.Address.FullAddress, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(invoice.CustomerDetail?.Email))
                    {
                        sb.Append(CenterText(NormalText("Email:" + invoice.CustomerDetail.Email, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                    if (!string.IsNullOrEmpty(invoice.CustomerDetail?.Phone))
                    {
                        sb.Append(CenterText(NormalText("Phone:" + invoice.CustomerDetail.Phone, isutf), Imin_LINE_WIDTH, 2, isutf));
                    }
                }

                #endregion

                sb.Append("\n");
                if (!string.IsNullOrEmpty(receiptTemplate.InvoiceHeading))
                {
                    sb.Append(CenterText(NormalText(receiptTemplate.InvoiceHeading, isutf), Imin_LINE_WIDTH, 2, isutf));
                }
                if (!string.IsNullOrEmpty(invoice.Number) || !string.IsNullOrEmpty(receiptTemplate.InvoiceNoPrefix))
                {
                    sb.Append(CenterText(NormalText(receiptTemplate.InvoiceNoPrefix + " " + invoice.Number, isutf), Imin_LINE_WIDTH, 2, isutf));
                }
            }
            return sb;
        }

        public static StringBuilder CommonBottomStringBuilder(InvoiceDto invoice, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false, bool isDD = false)
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(invoice.Barcode) && receiptTemplate.PrintReceiptBarcode)
                sb.Append(CenterText(NormalText(invoice.Barcode, isutf), Imin_LINE_WIDTH, 2, isutf));

            if (isDD && receiptTemplate.ShowCustomerSignature)
            {
                sb.Append(CenterText(NormalText(receiptTemplate.ServedByLable + " " + invoice.ServedByName, isutf), Imin_LINE_WIDTH, 2, isutf));
                sb.Append(CenterText(NormalText(invoice.FinalizeDateStoreDate.ToString("hh.mmtt, dd MMM yyyy"), isutf), Imin_LINE_WIDTH, 2, isutf));
                sb.Append("\n");
                sb.Append("\n");
                sb.Append("\n");
                sb.Append(CenterText(NormalText("_________________", isutf), Imin_LINE_WIDTH, 2, isutf));
                sb.Append(CenterText(NormalText("Customer Signature", isutf), Imin_LINE_WIDTH, 2, isutf));

            }
            else
            {
                sb.Append(CenterText(NormalText(invoice.FinalizeDateStoreDate.ToString("hh.mmtt, dd MMM yyyy"), isutf), Imin_LINE_WIDTH, 2, isutf));
                sb.Append(CenterText(NormalText(receiptTemplate.ServedByLable + " " + invoice.ServedByName, isutf), Imin_LINE_WIDTH, 2, isutf));
            }



            if (Settings.Subscription != null && Settings.Subscription.Edition != null && Settings.Subscription.Edition.PlanType == PlanType.Trial)
            {
                var dsiplayline = new string('-', Imin_LINE_WIDTH);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(CenterText(NormalText("Thank you for trialing", isutf), Imin_LINE_WIDTH, 2, isutf));
                sb.Append(CenterText(NormalText("Hike point of sale.", isutf), Imin_LINE_WIDTH, 2, isutf));
                //sb.Append("\n");
                //sb.Append(CenterText(NormalText("hikeup.com", isutf), Imin_LINE_WIDTH, 2, isutf));
            }
            // if (!string.IsNullOrEmpty(receiptTemplate.FooterText))
            // {
            //     sb.Append("\n");
            //     string text = HtmlToPlainText(receiptTemplate.FooterText);
            //     sb.Append(CenterText(NormalText(text, isutf), Imin_LINE_WIDTH, 2, isutf));
            // }
            return sb;
        }

        public static string HtmlToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // string text = html.Replace("\r", " ").Replace("&nbsp;", " ").Replace("\n", " ").Replace("\t", " ");
            // text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            // text = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty);

            string text = html.Replace("\r", " ").Replace("&nbsp;", " ").Replace("\n", " ");
            // Convert <p> to line breaks
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"</p>\s*<p>",
                "\n\n",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // Remove remaining <p> tags
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"</?p>",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            text = System.Net.WebUtility.HtmlDecode(text);

            return text.Trim();
        }

      

        #endregion

        public class HikePayReceiptBuilder
        {
            public static string BuildReceipt(PaymentReceipt doc)
            {
                var sb = new StringBuilder();
                foreach (var line in doc.OutputContent.OutputText)
                {
                    if (line == null || string.IsNullOrWhiteSpace(line.Text))
                        continue;
        
                    // Skip specific header lines
                    if (line.Text == "key=header1" || line.Text == "key=header2")
                        continue;
        
                    try
                    {
                        // Decode URL-encoded values
                        string decoded = HttpUtility.UrlDecode(line.Text) ?? string.Empty;
        
                        // Parse key-value pairs: name=something&value=123&key=filler
                        string name = null;
                        string value = null;
                        string key = null;
        
                        var parts = decoded.Split('&', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            var kv = part.Split('=', 2);
                            if (kv.Length != 2) continue;
        
                            switch (kv[0])
                            {
                                case "name": name = kv[1]; break;
                                case "value": value = kv[1]; break;
                                case "key": key = kv[1]; break;
                            }
                        }
        
                        // Filler line → blank
                        if (key == "filler")
                        {
                            sb.AppendLine();
                            continue;
                        }
        
                        bool isBold = line.CharacterStyle == "Bold";
        
                        // ========= BOLD LINES =========
                        if (isBold)
                        {
                            if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(value))
                                sb.AppendLine(name.ToUpperInvariant());
                            else if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                                sb.AppendLine($"{name.ToUpperInvariant()}: {value}");
                            else
                                sb.AppendLine(decoded.ToUpperInvariant());
        
                            continue;
                        }
        
                        // ========= REGULAR LINES =========
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                            sb.AppendLine($"{name}: {value}");
                        else if (!string.IsNullOrEmpty(name))
                            sb.AppendLine(name);
                        else
                            sb.AppendLine(decoded);
                    }
                    catch (Exception ex)
                    {
                        // Optional: log the error
                        Console.WriteLine($"Error processing line: {ex.Message}");
                    }
                }
                return sb.ToString();
            }
        }
#if IOS
        public static class UIViewControllerExtensions
        {

            public static UIKit.UIViewController GetTopViewController()
            {
                var window = UIKit.UIApplication.SharedApplication
                    .ConnectedScenes
                    .OfType<UIKit.UIWindowScene>()
                    .SelectMany(s => s.Windows)
                    .FirstOrDefault(w => w.IsKeyWindow);

                if (window == null)
                    return null;

                var root = window.RootViewController;
                if (root == null)
                    return null;

                var presented = GetTopPresentedController(root);
                if (presented != null)
                    return presented;

                return GetVisibleControllerInside(root);
            }
            private static UIKit.UIViewController GetTopPresentedController(UIKit.UIViewController root)
            {
                var current = root;

                while (current.PresentedViewController != null)
                    current = current.PresentedViewController;

                return current == root ? null : current;
            }
            private static UIKit.UIViewController GetVisibleControllerInside(UIKit.UIViewController vc)
            {
                if (vc is UIKit.UINavigationController nav)
                    return nav.VisibleViewController ?? vc;

                if (vc is UIKit.UITabBarController tab)
                    return tab.SelectedViewController ?? vc;

                if (vc is UIKit.UIPageViewController page && page.ViewControllers?.Length > 0)
                    return page.ViewControllers[0];

                if (vc.ChildViewControllers?.Length > 0)
                    return vc.ChildViewControllers.FirstOrDefault() ?? vc;

                return vc;
            }
        }
#endif
        public static string Generate10CharsId()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = RandomNumberGenerator.GetBytes(10);

            return string.Create(10, bytes, (span, data) =>
            {
                for (int i = 0; i < span.Length; i++)
                    span[i] = chars[data[i] % chars.Length];
            });
        }

        public static StringBuilder InvoiceItemsStringBuilderSecond(InvoiceDto invoice, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false)
        {
            StringBuilder sb = new StringBuilder();
            var dsiplayline = new string('-', Imin_LINE_WIDTH);
            if (receiptTemplate.ShowTotalDiscountOnReciept)
            {
                var totaldis = (invoice.TotalDiscount + invoice.InvoiceLineItems.Sum(a => a.TotalDiscount));
                if (totaldis > 0)
                {
                    sb.Append(string.Empty);
                    sb.Append(RightWrapItemLine("(" + LanguageExtension.Localize("TotalSaleDiscount") + " " + totaldis.ToString("C") + ")", "  ", Imin_LINE_WIDTH, isutf));
                }
            }
            sb.Append(RightWrapItemLine("----------", new string('-', Imin_LINE_WIDTH > 32 ? 16 : 14), Imin_LINE_WIDTH, isutf));


            if(invoice.ActiveInvoicePayments != null && invoice.ActiveInvoicePayments.Count > 0)
            {
                foreach (var payment in invoice.ActiveInvoicePayments)
                {
                    sb.Append(string.Empty);
                    var lbl = payment.PrintPaymentOptionDisplayName;
                    sb.Append(RightWrapItemLine(payment.PrintPaymentOptionDisplayName, payment.Amount.ToString("c"), Imin_LINE_WIDTH, isutf));
                    if (receiptTemplate.ShowPaymentDateOnReciept && payment.PaymentStoreDate.HasValue)
                    {
                        sb.Append(RightWrapItemLine("(" + payment.PaymentStoreDate.Value.ToString("dd MMM, yyyy hh.mmtt")+ ")", " ", Imin_LINE_WIDTH, isutf));
                    }
                    
                }
            }

            if (!string.IsNullOrEmpty(invoice.ChangeAmmountDetail))
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(new string(' ', (Imin_LINE_WIDTH - invoice.ChangeAmmountDetail.Length)) + invoice.ChangeAmmountDetail, isutf) + "\n");
            }

            sb.Append("\n");
            sb.Append(RightWrapItemLine(receiptTemplate.ToPayLable, invoice.OutstandingAmount.ToString("c"), Imin_LINE_WIDTH, isutf));

            if (invoice.Status == Enums.InvoiceStatus.OnAccount && invoice.InvoiceDueDate.HasValue && receiptTemplate.ShowInvoiceDueDateOnReciept)
            {
                sb.Append(string.Empty);
                sb.Append(RightWrapItemLine("Due on: " + invoice.InvoiceDueDate.Value.ToStoreTime().ToString("dd MMM yyyy"), "  ", Imin_LINE_WIDTH, isutf));
            }

            if (receiptTemplate.ShowOnAccountOutStadningOnReciept && !string.IsNullOrEmpty(receiptTemplate.ToPayLable) && receiptTemplate.ToPayLable.ToLower().Contains("outstanding"))
            {
                CustomFieldsResponce result2 = null;
                result2 = invoice.CustomFields != null ? JsonConvert.DeserializeObject<CustomFieldsResponce>(invoice.CustomFields) : null;
                if (invoice.OutstandingAmount > 0 && invoice.Status == InvoiceStatus.OnAccount && result2?.invoiceOutstanding != null || invoice.InvoiceOutstanding != null)
                {
                    sb.Append(string.Empty);
                    sb.Append(NormalText(dsiplayline, isutf) + "\n");
                    sb.Append(NormalText("OUTSTANDING" + "\n", isutf));
                    var data = invoice.InvoiceOutstanding != null ? invoice.InvoiceOutstanding : result2.invoiceOutstanding;
                    sb.Append(NormalText("Previous Outstanding : " + (data.previousOutstanding.HasValue ? data.previousOutstanding.Value : decimal.Zero).ToString("C") + "\n", isutf));
                    sb.Append(NormalText("Current Sale : " + (data.currentSale.HasValue ? data.currentSale.Value : decimal.Zero).ToString("C") + "\n", isutf));
                    sb.Append(NormalText("Current Outstanding : " + (data.currentOutstanding.HasValue ? data.currentOutstanding.Value : decimal.Zero).ToString("C") + "\n", isutf));
                }
            }

            if (invoice.CustomerDetail != null && invoice.CustomerDetail.AllowLoyalty && invoice.CustomerId != null && invoice.CustomerId != 0 && Settings.StoreGeneralRule.EnableLoyalty && receiptTemplate.ShowLoyaltyPointsOnReciept)
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("LOYALTY POINTS" + "\n", isutf));
                sb.Append(NormalText("Balance : " + invoice.CustomerCurrentLoyaltyPoints.ToString() + "\n", isutf));


                if (invoice.InvoiceLineItems != null)
                {
                    sb.Append(NormalText("This visit - Earned : " + (invoice.LoyaltyPoints + invoice.InvoiceLineItems.Sum(x => x.CustomerGroupLoyaltyPoints)).ToString() + "\n", isutf));
                }

                decimal LoyaltyRedeemed = 0;
                if (invoice.InvoicePayments != null && invoice.InvoicePayments.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty) && Settings.StoreGeneralRule != null)
                {
                    LoyaltyRedeemed = invoice.InvoicePayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty).Sum(x => x.Amount) * Settings.StoreGeneralRule.LoyaltyPointsValue;
                }
                sb.Append(NormalText("This visit - Redeemed : " + "-" + LoyaltyRedeemed.ToString() + "\n", isutf));
                sb.Append(NormalText("Closing balance : " + (invoice.CustomerCurrentLoyaltyPoints + invoice.LoyaltyPoints - LoyaltyRedeemed).ToString() + "\n", isutf));
            }

            if (invoice.CustomerDetail != null && invoice.CustomerId != null && invoice.CustomerId != 0 && receiptTemplate.ShowStoreCreditOnReciept && invoice.InvoicePayments != null && invoice.InvoicePayments.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Credit))
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("STORE CREDIT BALANCE" + "\n", isutf));
                var storeCreditPayments = invoice.InvoicePayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Credit);
                var storeCreditUsed = storeCreditPayments.Sum(x => x.Amount);
                sb.Append(NormalText("This visit - Used : " + string.Format("{0:C}", storeCreditUsed) + "\n", isutf));
                if (storeCreditPayments.Last().InvoicePaymentDetails.Any())
                {
                    sb.Append(NormalText("Opening Balance : " + string.Format("{0:C}", Convert.ToDecimal(storeCreditPayments.First().InvoicePaymentDetails?.First().Value)) + "\n", isutf));
                    sb.Append(NormalText("Closing balance : " + string.Format("{0:C}", Convert.ToDecimal(storeCreditPayments.Last().InvoicePaymentDetails?.Last().Value))+ "\n", isutf));
                }
            }

            var GiftCardBalanceTally = ShowUsedGiftCardBalanceTally(invoice);
            if (GiftCardBalanceTally != null && GiftCardBalanceTally.Count > 0)
            {
                foreach (var gc in GiftCardBalanceTally)
                {
                    sb.Append(string.Empty);
                    sb.Append(NormalText(dsiplayline, isutf) + "\n");
                    sb.Append(NormalText("Gift card # " + gc.Number + "\n", isutf));
                    sb.Append(NormalText("Opening balance : " + gc.OpeningBalance.ToString("c") + "\n", isutf));
                    sb.Append(NormalText("This visit - used : " + gc.UsedBalance.ToString("c") + "\n", isutf));
                    sb.Append(NormalText("Closing balance : " + gc.ClosingBalance.ToString("c") + "\n", isutf));
                }
            }

            CheckStoreFeatureConverter checkStoreFeatureConverter = new CheckStoreFeatureConverter();
            var result1 = checkStoreFeatureConverter.Convert(receiptTemplate.ShowTotalNumberOfItemsOnReceipt, null, "HikeShowTotalNumberOfItemsOnReceiptFeature", null);
            if (result1 != null && result1 is bool isShowTotalNumberOfItems && isShowTotalNumberOfItems)
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("Total Items: " + invoice.InvoiceLineItemsCnt + "\n", isutf));
            }

            if (!string.IsNullOrEmpty(invoice.Note))
            {
                sb.Append(string.Empty);
                sb.Append(NormalText(dsiplayline, isutf) + "\n");
                sb.Append(NormalText("Note: " + invoice.Note.TrimStart().TrimEnd() + "\n", isutf));
            }
            return sb;
        }
    
        public static StringBuilder InvoiceItemsStringBuilderFirst(InvoiceDto invoice, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, int Imin_LINE_WIDTH, int Imin_Column_Width, bool isutf = false)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n");
            var column1header = string.IsNullOrEmpty(receiptTemplate.ItemTitleLabel) ? "Items" : receiptTemplate.ItemTitleLabel;
            var column2header = string.IsNullOrEmpty(receiptTemplate.QuantityTitleLabel) ? "Price" : receiptTemplate.PriceTitleLabel;
            var spacecnt = Imin_LINE_WIDTH - column1header.Length - column2header.Length;
            var space = new string(' ', spacecnt);
            sb.Append(NormalText(column1header + space + column2header + "\n", isutf));
            var dsiplayline = new string('-', Imin_LINE_WIDTH);
            sb.Append(NormalText(dsiplayline, isutf) + "\n");

            var result = Extensions.SetDisplayTitle(invoice.InvoiceLineItems, receiptTemplate);
            if (result != null)
            {
                var cnt = 0;
                foreach (var groupitem in result)
                {
                    if (Settings.StoreGeneralRule.ShowGroupProductsByCategory)
                        sb.Append(NormalText(WrapItemLine(groupitem.Title, " ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                    sb.Append(PrepareItemForPrint(groupitem.InvoiceLineItems, receiptTemplate, CurrentOutlet, Imin_LINE_WIDTH, Imin_Column_Width, isutf, "Invoice").ToString());

                    cnt++;
                    if (cnt < result.Count)
                    {
                        //sb.Append(NormalText(WrapItemLine(" ", " ", Imin_LINE_WIDTH, Imin_Column_Width, isutf), isutf));
                        sb.Append("\n");
                    }
                }
            }

            sb.Append(string.Empty);
            sb.Append(NormalText(dsiplayline, isutf) + "\n");
            if (invoice.TotalDiscount > 0 && !receiptTemplate.HideDiscountLineOnReceipt)
            {
                sb.Append(string.Empty);
                sb.Append(RightWrapItemLine(receiptTemplate.DiscountLable, invoice.TotalDiscount.ToString("c"), Imin_LINE_WIDTH, isutf));
                if (Settings.StoreGeneralRule.ShowInvoiceLevelDiscountInPercentage)
                {
                    var disCountAsPercent = ViewModels.InvoiceCalculations.GetPercentfromValue(invoice.TotalDiscount, invoice.NetAmount + invoice.TotalDiscount);
                    disCountAsPercent = Math.Round(disCountAsPercent, 2, MidpointRounding.AwayFromZero);
                    string TotalDiscountAsPercent = $"({disCountAsPercent}%)";
                    sb.Append(RightWrapItemLine("  ", TotalDiscountAsPercent, Imin_LINE_WIDTH, isutf));
                }
                sb.Append("\n");
            }

            sb.Append(RightWrapItemLine(receiptTemplate.SubTotalLable, invoice.SubTotal.ToString("c"), Imin_LINE_WIDTH, isutf));

           

            if (invoice.TotalShippingCost > 0 && invoice.ShippingTaxAmountExclusive.HasValue)
            {
                sb.Append(string.Empty);
                sb.Append(RightWrapItemLine("Shipping (Ex. Tax)", invoice.ShippingTaxAmountExclusive.Value.ToString("c"), Imin_LINE_WIDTH, isutf));
            }

            if (invoice.RoundingAmount > 0)
            {
                sb.Append(string.Empty);
                sb.Append(RightWrapItemLine("Rounding", invoice.RoundingAmount.ToString("c"), Imin_LINE_WIDTH, isutf));
            }
           
            return sb;
        }
    
    }

}
using System;
using HikePOS.Resx;
using Microsoft.Maui.Controls.Shapes;
using HikePOS.UserControls;

namespace HikePOS.Resources
{
    public static class Styles
    {
        public static Style ActivityIndicatorStyle = new Style(typeof(ActivityIndicator))
        {
            Setters = {
                new Setter { Property = ActivityIndicator.ColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = ActivityIndicator.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = ActivityIndicator.HorizontalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = ActivityIndicator.MarginProperty, Value = 10}
            }
        };

        public static Style WaitProgressTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Label.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style ProgressTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 28 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style LoginSignupButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = Colors.White },
                new Setter { Property = Button.BackgroundColorProperty, Value = AppColors.HikeColor },
                new Setter { Property = Button.CornerRadiusProperty, Value = 5 },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.FontSizeProperty, Value = 22 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Button.HeightRequestProperty, Value= 60}
            }
        };

        public static Style SetUpScreenTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.HikeColor },
                new Setter { Property = Label.FontSizeProperty, Value = 36 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style LoginSignupLinkStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 17 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HeightRequestProperty, Value = 40}
            }
        };

        public static Style CheckboxLableStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 20},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
            }
        };


        public static Style CategoryTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 15 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start}
            }
        };

        public static Style CustomerNameLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 15 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start}
            }
        };

        public static Style CustomerNameGridStyle = new Style(typeof(Grid))
        {
            Setters = {
                new Setter { Property = Grid.PaddingProperty, Value = 0},
                new Setter { Property = Grid.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Grid.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand}
            }
        };

        public static Style CustomerGroupNameLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 13 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start}
            }
        };

        public static Style InvoiceItemBackgroundLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = Colors.White },
                new Setter { Property = Label.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style InvoiceItemQuantityLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = Colors.White },
                new Setter { Property = Label.FontSizeProperty, Value = 10 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeBoldFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Center}
            }
        };

        public static Style InvoiceItemProductNameLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation}
            }
        };

        public static Style InvoiceItemProductRetailPriceLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 10 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                //Ticket #9749 Start: Product price showing wrong in cart. By Nikhil.
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center}
                //Ticket #9749 End:By Nikhil.  
            }
        };

        public static Style InvoiceItemProductDescriptionLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.HikeColor },
                new Setter { Property = Label.FontSizeProperty, Value = 10 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation}
            }
        };

        public static Style InvoiceItemProductOfferNoteLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 10 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeAwesomeFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation}
            }
        };

        public static Style InvoiceItemProductTotalAmountLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                //Ticket #9749 Start: Product price showing wrong in cart. By Nikhil.
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.End},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center}
                //Ticket #9749 End:By Nikhil.
            }
        };

        public static Style InvoiceItemProductTotalRetailAmountLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 12 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                //Ticket #9749 Start: Product price showing wrong in cart. By Nikhil.
                 new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.End},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center}
                //Ticket #9749 Start:By Nikhil.
            }
        };

        public static Style InvoiceRestockCheckBoxLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Start}
            }
        };

        //FontSize="14" FontFamily="{x:Static resources:Fonts.HikeDefaultFont}" VerticalOptions="Center"
        public static Style InvoiceNoteEntryStyle = new Style(typeof(Entry))
        {
            Setters = {
                new Setter { Property = Entry.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Entry.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Entry.FontFamilyProperty, Value = Fonts.HikeLightFont}
            }
        };

        public static Style InvoiceSummaryTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.StartAndExpand},
                new Setter { Property = Label.HeightRequestProperty, Value= 40}
            }
        };

        public static Style InvoiceSummaryValueLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.End},
                new Setter { Property = Label.HeightRequestProperty, Value= 40}
            }
        };

        public static Style InvoiceSummaryTitleButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.HikeColor },
                new Setter { Property = Button.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = Button.HeightRequestProperty, Value= 40}
            }
        };

        public static Style InvoicePayTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.BackgroundColor },
                new Setter { Property = Label.FontSizeProperty, Value = 28 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Start}
            }
        };

        public static Style InvoicePayValueLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.BackgroundColor },
                new Setter { Property = Label.FontSizeProperty, Value = 28 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.EndAndExpand}
            }
        };


        public static Style InvoiceOpenRegisterButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.BackgroundColor },
                new Setter { Property = Button.BackgroundColorProperty, Value = AppColors.HikeColor},
                new Setter { Property = Button.FontSizeProperty, Value = 28 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand}
            }
        };

        public static Style SubCategoryProductCountLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 12 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Center}
            }
        };

        public static Style EntersaleItemTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 12 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap}
            }
        };

        public static Style EntersaleItemImagePlaceholderLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = Colors.White },
                new Setter { Property = Label.FontSizeProperty, Value = 46 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style EntersaleOfferImagePlaceholderLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = Colors.White },
                new Setter { Property = Label.FontSizeProperty, Value = 46 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeAwesomeFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style EntersaleItemOfferLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Label.FontSizeProperty, Value = 12 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeAwesomeFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };
        public static Style SaveDenominationButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Button.FontSizeProperty, Value = 12 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeAwesomeFont}
            }
        };


        public static Style EntersaleSearchCustomerButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Button.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeAwesomeFont},
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.CenterAndExpand}
            }
        };


        public static Style PaymentGroupHeaderLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 18 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style PaymentTenderedAmountEntryStyle = new Style(typeof(Entry))
        {
            Setters = {
                new Setter { Property = Entry.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Entry.PlaceholderColorProperty, Value = AppColors.PlaceholderColor },
                new Setter { Property = Entry.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Entry.FontAttributesProperty, Value = FontAttributes.Bold },
                new Setter { Property = Entry.FontSizeProperty, Value = 36 + App.FontIncrAmount},
                new Setter { Property = Entry.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Entry.KeyboardProperty, Value = Keyboard.Numeric},
                new Setter { Property = Entry.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Entry.HeightRequestProperty, Value = 60}
            }
        };

        public static Style PaymentChangeAmountLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold },
                new Setter { Property = Label.FontSizeProperty, Value = 36 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };
        public static Style PaymentOutstandingAmountLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PlaceholderColor },
                new Setter { Property = Label.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style PaymentTypeButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = Colors.White },
                new Setter { Property = Button.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.BackgroundColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 0 }
            }
        };

        //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
        public static Style PaymentTypeLblStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = Colors.White },
                new Setter { Property = Label.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.HorizontalTextAlignmentProperty , Value = TextAlignment.Center},
                new Setter { Property = Label.MaxLinesProperty , Value = 2}
            }
        };
        //End ticket #73190 By Pratik

        public static Style QuickCashOptionButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Button.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 0 }
            }
        };


        public static Style PaymentPrintButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Button.FontSizeProperty, Value = 18 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 0 }
            }
        };

        public static Style PaymentGiftChargeButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = Colors.White },
                new Setter { Property = Button.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.BackgroundColorProperty, Value = AppColors.HikeColor },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 0 }
            }
        };

        public static Style PaymentGiftNumberEntryStyle = new Style(typeof(Entry))
        {
            Setters = {
                new Setter { Property = Entry.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Entry.PlaceholderColorProperty, Value = AppColors.PlaceholderColor },
                new Setter { Property = Entry.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Entry.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Entry.FontFamilyProperty, Value = Fonts.HikeLightFont}
            }
        };

        public static Style PaymentPartialyPaidHeaderLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.HikeColor },
                new Setter { Property = Label.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style PaymentPartialyPaidValueLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PlaceholderColor },
                new Setter { Property = Label.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style PaymentAddNewSaleButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = Colors.White },
                new Setter { Property = Button.FontSizeProperty, Value = 18 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.BackgroundColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 5 },
                new Setter { Property = Button.WidthRequestProperty, Value = 180 }
            }
        };

        public static Style PaymentEmailButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Button.FontSizeProperty, Value = 18 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 0 }
            }
        };

        public static Style FilterByStatusButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.FontSizeProperty, Value = 16 + App.FontIncrAmount },
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont },
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.Start }
            }
        };


        public static Style PageTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Label.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                //new Setter { Property = Label.MarginProperty, Value = new Thickness(0,23,0,5)}
            }
        };

        public static Style PageBackLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.BackButtonBlueColor },
                new Setter { Property = Label.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeAwesomeFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start},
                new Setter { Property = Label.MarginProperty, Value = new Thickness(0,10,0,5)},
                new Setter { Property = Label.TranslationXProperty, Value = -3}
            }
        };


        public static Style PageBackButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.BackButtonBlueColor },
                new Setter { Property = Button.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Button.FontAttributesProperty, Value = FontAttributes.None},
                new Setter { Property = Button.MarginProperty, Value = new Thickness(0,10,0,5)},
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent},
                new Setter { Property = Button.BorderWidthProperty, Value = 0},
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.Start}
            }
        };


        public static Style TableHeaderLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 15 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start}
            }
        };

        public static Style TableHeaderIconLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeAwesomeFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.End}
            }
        };

        public static Style TableHeaderIconButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Button.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeAwesomeFont},
            }
        };

        public static Style TableItemLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style SalesFilterStatusOptionButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Button.BackgroundProperty, Value = Colors.White },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 0 },
                new Setter { Property = Button.FontSizeProperty, Value = 14 + App.FontIncrAmount },
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter { Property = Button.HeightRequestProperty, Value = 44 }

            }
        };

        public static Style SalesFilterStatusOptionRangeDateLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount },
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand }
            }
        };

        public static Style SalesFilterStatusOptionRangeDatePickerStyle = new Style(typeof(CustomDatePicker))
        {
            Setters = {
                new Setter { Property = CustomDatePicker.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = CustomDatePicker.FontSizeProperty, Value = 14 + App.FontIncrAmount },
                new Setter { Property = CustomDatePicker.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = CustomDatePicker.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter { Property = CustomDatePicker.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand }
            }
        };

        public static Style PopUpCloseButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.LightBordersColor},
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = App.FontIncrAmount == 0 ? 40 : 43 },
                new Setter { Property = Button.HeightRequestProperty, Value = (80 + (App.FontIncrAmount * 2))},
                new Setter { Property = Button.WidthRequestProperty, Value = (80 + (App.FontIncrAmount * 2))},
                //new Setter { Property = Button.TextProperty, Value = AppResources.CloseIconUnicode },
                new Setter { Property = Button.PaddingProperty, Value = new Thickness(22) },
                new Setter { Property = Button.ImageSourceProperty, Value = AppImages.CloseIcon },
                //new Setter { Property = Button.FontSizeProperty, Value = 50 }
            }
        };

        public static Style PopUpTextMenuButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.FontSizeProperty, Value = 16 + App.FontIncrAmount },
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont },
                new Setter { Property = Button.CornerRadiusProperty, Value = App.FontIncrAmount == 0 ? 40 : 43 },
                new Setter { Property = Button.HeightRequestProperty, Value = (80 + (App.FontIncrAmount * 2))},
                new Setter { Property = Button.WidthRequestProperty, Value = (80 + (App.FontIncrAmount * 2))},
#if ANDROID
                new Setter { Property = Button.LineBreakModeProperty, Value = LineBreakMode.WordWrap }
#endif
            }
        };

        public static Style PopUpTextMenuButtonStylePhone = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.NavigationBarBackgroundColor },
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.FontSizeProperty, Value = 16 + App.FontIncrAmount },
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont },
                new Setter { Property = Button.HeightRequestProperty, Value = 40},
                new Setter { Property = Button.MinimumWidthRequestProperty, Value = 80},
                new Setter { Property = Button.PaddingProperty, Value =  new Thickness(14,0,14,0)},

#if ANDROID
                new Setter { Property = Button.LineBreakModeProperty, Value = LineBreakMode.WordWrap }
#endif
            }
        };

        public static Style PopupTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 18 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = Label.MarginProperty, Value = new Thickness(40)}
            }
        };

        public static Style PopupSaleDetailCustomerTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PlaceholderColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Start}
            }
        };

        public static Style PopupSaleDetailCustomerNameLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 18 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Start}
            }
        };

        public static Style PopupSaleDetailCustomerNameButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Button.FontSizeProperty, Value = 18 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.Start}
            }
        };

        public static Style ExpandViewTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeBoldFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start}
            }
        };


        public static Style FormTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.End},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.EndAndExpand},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.StartAndExpand},
            }
        };

        public static Style FormEntryStyle = new Style(typeof(Entry))
        {
            Setters = {
                new Setter { Property = Entry.TextColorProperty, Value = AppColors.PrimaryTextColor},
                new Setter { Property = Entry.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Entry.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Entry.PlaceholderColorProperty, Value = AppColors.PlaceholderColor},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.HeightRequestProperty, Value = 50}
            }
        };

        public static Style FormEditorStyle = new Style(typeof(Editor))
        {
            Setters = {
                new Setter { Property = Editor.TextColorProperty, Value = AppColors.PrimaryTextColor},
                new Setter { Property = Editor.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Editor.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Editor.MarginProperty, Value = new Thickness(10,10,10,10)}
            }
        };

        public static Style TabButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.FontSizeProperty, Value = 16 + App.FontIncrAmount },
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont },
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.Start },
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.EndAndExpand }
            }
        };

        public static Style IncreaseDescreaseButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 5 },
                new Setter { Property = Button.FontSizeProperty, Value = 35 + App.FontIncrAmount },
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = Button.HeightRequestProperty, Value = 50 },
                new Setter { Property = Button.WidthRequestProperty, Value = 50 }
            }
        };


        public static Style QuantityEntryStyle = new Style(typeof(Entry))
        {
            Setters = {
                new Setter { Property = Entry.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Entry.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Entry.FontSizeProperty, Value = 28 + App.FontIncrAmount },
                new Setter { Property = Entry.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = Entry.HorizontalTextAlignmentProperty, Value = TextAlignment.Center }
            }
        };

        public static Style WhiteButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 0 },
                new Setter { Property = Button.FontSizeProperty, Value = 14 + App.FontIncrAmount },
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand }
            }
        };


        public static Style SliderButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.SliderTextColor },
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Button.BorderColorProperty, Value = Color.FromRgb(68, 84, 100) },
                new Setter { Property = Button.BorderWidthProperty, Value = 0.5 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 4 },
                new Setter { Property = Button.FontSizeProperty, Value = 12 + App.FontIncrAmount },
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand }
            }
        };

        public static Style SliderListLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.SliderTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start}
            }
        };

        public static Style SliderIndicatorLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.SliderTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 12 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style PrinterPreferencesOptionLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryLightTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start}
            }
        };

        public static Style FormBigEntryStyle = new Style(typeof(Entry))
        {
            Setters = {
                new Setter { Property = Entry.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Entry.PlaceholderProperty, Value = AppColors.PlaceholderColor },
                new Setter { Property = Entry.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Entry.FontSizeProperty, Value = 16 + App.FontIncrAmount },
                new Setter { Property = Entry.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = Entry.HorizontalTextAlignmentProperty, Value = TextAlignment.Start },
                new Setter { Property = Entry.HeightRequestProperty, Value = 60 },
                new Setter { Property = Entry.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter { Property = Entry.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand }
            }
        };

        //like store address entry type of label
        public static Style FormBigEntrySubLableStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Label.FontSizeProperty, Value = 16 + App.FontIncrAmount },
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
                new Setter { Property = Label.HeightRequestProperty, Value = 60 },
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.End },
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand }
            }
        };

        //Like forgot password
        public static Style FormBigEntrySubButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Button.FontSizeProperty, Value = 16 + App.FontIncrAmount },
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont },
                new Setter { Property = Button.HeightRequestProperty, Value = 60 },
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.End },
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand }
            }
        };

        public static Style FormErrorLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.ProductRedColor },
                new Setter { Property = Label.FontSizeProperty, Value = 14 + App.FontIncrAmount},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.End}
            }
        };

        public static Style FormRoundBorderStyle = new Style(typeof(Border))
        {
            Setters = {
                new Setter { Property = Border.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Border.StrokeShapeProperty, Value =  new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5, 5, 5, 5)
                }},
                new Setter { Property = Border.StrokeProperty, Value = AppColors.LightBordersColor},
                //new Setter { Property = Border.ShadowProperty, Value = false},
                new Setter { Property = Border.HeightRequestProperty, Value = 50},
                new Setter { Property = Border.PaddingProperty, Value = 0}
            }
        };

        //Start #92768 Pratik
        public static Style EmailListRoundBorderStyle = new Style(typeof(Border))
        {
            Setters = {
                new Setter { Property = Border.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Border.StrokeShapeProperty, Value =  new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5, 5, 5, 5)
                }},
                new Setter { Property = Border.StrokeProperty, Value = AppColors.LightBordersColor},
                //new Setter { Property = Border.ShadowProperty, Value = false},
                new Setter { Property = Border.PaddingProperty, Value = 0}
            }
        };
        //End #92768 Pratik


        public static Style SettingsMenuButtonLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start},
                new Setter { Property = Label.MarginProperty, Value = new Thickness(40,10,10,10)}
            }
        };

        public static Style SettingsGeneralRoundBorderStyle = new Style(typeof(Border))
        {
            Setters = {
                new Setter { Property = Border.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Border.StrokeShapeProperty, Value =  new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5, 5, 5, 5)
                }},
               // new Setter { Property = Border.ShadowProperty, Value = false},
                new Setter { Property = Border.HeightRequestProperty, Value = 70},
                new Setter { Property = Border.MinimumHeightRequestProperty, Value = 70},
                new Setter { Property = Border.PaddingProperty, Value = 0},
                new Setter { Property = Border.VerticalOptionsProperty, Value = LayoutOptions.Start}
            }
        };

        public static Style SettingsAutoHeightRoundBorderStyle = new Style(typeof(Border))
        {
            Setters = {
                new Setter { Property = Border.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Border.StrokeShapeProperty, Value =  new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5, 5, 5, 5)
                }},
                //new Setter { Property = Border.ShadowProperty, Value = false},
                new Setter { Property = Border.PaddingProperty, Value = 0},
                new Setter { Property = Border.VerticalOptionsProperty, Value = LayoutOptions.Start}
            }
        };

        public static Style SettingsOptionTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center}
            }
        };

        public static Style SettingsOptionDescriptionLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 12 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start},
                new Setter { Property = Label.MinimumHeightRequestProperty, Value = 50},
                new Setter { Property = Label.MarginProperty, Value = new Thickness(20,10,20,10)}
            }
        };

        public static Style SignupTermAndConditionLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = Label.FontSizeProperty, Value = 12 + App.FontIncrAmount},
                new Setter { Property = Label.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start}
            }
        };


        public static Style SettingsDataSyncButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.TextColorProperty, Value = Colors.White },
                new Setter { Property = Button.FontSizeProperty, Value = 16 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.BackgroundColorProperty, Value = AppColors.HikeColor },
                new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                new Setter { Property = Button.CornerRadiusProperty, Value = 5 },
                new Setter { Property = Button.WidthRequestProperty, Value = 120 },
                new Setter { Property = Button.HeightRequestProperty, Value = 40 },
                new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.End },
                new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.Center }
            }
        };

        public static Style AutoLockButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.BackgroundColorProperty, Value = AppColors.ProductWhiteColor },
                new Setter { Property = Button.BorderWidthProperty, Value = 0.5},
                new Setter { Property = Button.WidthRequestProperty, Value = 56 },
                new Setter { Property = Button.HeightRequestProperty, Value = 56},
                new Setter { Property = Button.CornerRadiusProperty, Value = 28},
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor},
                new Setter { Property = Button.FontSizeProperty, Value = 20},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = Button.PaddingProperty, Value = 2},
            }
        };

         public static Style ClearButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = Button.BorderWidthProperty, Value = 0},
                new Setter { Property = Button.WidthRequestProperty, Value = (60 + (App.FontIncrAmount * 10))},
                new Setter { Property = Button.PaddingProperty, Value = 0 },
                new Setter { Property = Button.TextColorProperty, Value = AppColors.TitlebarTextColor},
                new Setter { Property = Button.FontSizeProperty, Value = 16},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeBoldFont},

            }
        };


        public static Style DicountTipTitleLabelStyle = new Style(typeof(Label))
        {
            Setters = {
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = Label.BackgroundColorProperty, Value = Colors.White },
                new Setter { Property = Label.FontSizeProperty, Value = 16 + App.FontIncrAmount}
            }
        };


        public static Style DicountTipButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.BackgroundColorProperty, Value = AppColors.ProductWhiteColor },
                new Setter { Property = Button.TextColorProperty, Value = AppColors.PrimaryTextColor},
                new Setter { Property = Button.FontSizeProperty, Value = 26 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = Button.CornerRadiusProperty, Value =  0 }
            }
        };

        public static Style DicountTipEntryStyle = new Style(typeof(CustomEntry))
        {
            Setters = {
                new Setter { Property = CustomEntry.TextColorProperty, Value = AppColors.PrimaryTextColor },
                new Setter { Property = CustomEntry.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = CustomEntry.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = CustomEntry.HeightRequestProperty, Value = 42 },
                new Setter { Property = CustomEntry.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                new Setter { Property = CustomEntry.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
                new Setter { Property = CustomEntry.BackgroundColorProperty, Value = Colors.White},
                new Setter { Property = CustomEntry.BorderWidthProperty, Value = 0}
            }
        };


        public static Style AmountPercentageButtonStyle = new Style(typeof(Button))
        {
            Setters = {
                new Setter { Property = Button.BackgroundColorProperty, Value = AppColors.ItemBackgroundColor },
                new Setter { Property = Button.TextColorProperty, Value = Colors.White},
                new Setter { Property = Button.FontSizeProperty, Value = 20 + App.FontIncrAmount},
                new Setter { Property = Button.FontFamilyProperty, Value = Fonts.HikeLightFont},
                new Setter { Property = CustomEntry.HeightRequestProperty, Value = 40 },
                new Setter { Property = Button.CornerRadiusProperty, Value =  0 },
                new Setter { Property = Button.HorizontalOptionsProperty, Value =  LayoutOptions.FillAndExpand }
            }
        };

        public static Style DropDownImageStyle = new Style(typeof(Image))
        {
            Setters = {
                new Setter { Property = Image.SourceProperty, Value = "down_popup" },
                new Setter { Property = Image.AspectProperty, Value = Aspect.AspectFit },
                new Setter { Property = Image.HeightRequestProperty, Value = 25},
                new Setter { Property = Image.WidthRequestProperty, Value = 30},
                new Setter { Property = Image.MarginProperty, Value = new Thickness(0,0,10,0)},
                new Setter { Property = Image.VerticalOptionsProperty, Value =  LayoutOptions.Center },
                new Setter { Property = Image.HorizontalOptionsProperty, Value =  LayoutOptions.End }
            }
        };

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public static Style DatePickerLabelStyle = new Style(typeof(DatePicker))
        {
            Setters = {
                new Setter { Property = DatePicker.TextColorProperty, Value = AppColors.HikeColor },
                new Setter { Property = DatePicker.FontSizeProperty, Value = 18 + App.FontIncrAmount},
                new Setter { Property = DatePicker.FontFamilyProperty, Value = Fonts.HikeDefaultFont},
                new Setter { Property = DatePicker.HorizontalOptionsProperty, Value = LayoutOptions.Start},
                new Setter { Property = DatePicker.HeightRequestProperty, Value= 40}
            }
        };
        //End ticket #76208 by Pratik
    }
}

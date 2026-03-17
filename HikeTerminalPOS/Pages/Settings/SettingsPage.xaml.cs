using System;
using System.Collections;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using HikePOS.Services;
using HikePOS.Enums;
using HikePOS.UserControls;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS
{
	public partial class SettingsPage : BaseContentPage<SettingsViewModel>
    {
		public SettingsPage()
		{
			try
			{
				InitializeComponent();
                Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping("MyCustomizationSwitch", (handler, view) =>
                {
                    if (handler != null && view is CustomSwitch)
                    {
#if IOS
                handler.PlatformView.OnTintColor =  UIKit.UIColor.FromRGB(50, 189, 185);
#endif
                    }
                });
                NavigationPage.SetHasNavigationBar(this, false);
			}
			catch (Exception ex)
			{
				ex.Track();
			}
        }

        void BackHandle_Clicked(object sender, System.EventArgs e)
        {
            BackCommandEvent();
        }

        void BackCommandEvent()
        {
            if (ViewModel.SelectedMenuItem == "GENERAL" && GeneralAutoLockTab.TranslationX == 0)
            {
                HideAutoLockOptionHandle_Tapped(true);
            }
            else if (ViewModel.SelectedMenuItem == "PRINTER" && HardwarePrinterTab.TranslationX == 0)
            {
                HidePrinterOptionHandle_Tapped(true);
            }
            else if (ViewModel.SelectedMenuItem == "PRINTER" && HardwareDocketNumberRangeTab.TranslationX == 0)
            {
                HideDocketNumberRangeOptionHandle_Tapped(true);
            }
            else if (ViewModel.SelectedMenuItem == "PRINTER" && HardwarePrinterNoOfCopiesTab.TranslationX == 0)
            {
                HideNoOfCopiesOptionHandle_Tapped(true);
            }
        }

        async void ShowAutoLockOptionHandle_Tapped(object sender, System.EventArgs e)
        {
            var viewWidth = GeneralTab.Width + GeneralTab.Padding.Left + GeneralTab.Padding.Right;
            await GeneralAutoLockTab.TranslateTo(viewWidth, 0, 0);
            await Task.WhenAll(
                      GeneralTab.TranslateTo(viewWidth * -1, 0, 180, Easing.Linear),
                GeneralAutoLockTab.TranslateTo(0, 0, 180, Easing.Linear)
					);
        }

        void BackAutoLock_Click (object sender, System.EventArgs e)
        {
            HideAutoLockOptionHandle_Tapped(true);
        }

		async void HideAutoLockOptionHandle_Tapped(bool animation = false)
		{
            if (GeneralTab.X != 0)
            {
                var viewWidth = GeneralTab.Width + GeneralTab.Padding.Left + GeneralTab.Padding.Right;
                if (animation)
                {
                    await Task.WhenAll(
                        GeneralAutoLockTab.TranslateTo(viewWidth, 0, 180, Easing.Linear),
                        GeneralTab.TranslateTo(0, 0, 180, Easing.Linear)
					);
                }
                else
                {
                    //await GeneralAutoLockTab.TranslateTo(viewWidth, 0, 0);
                    //await GeneralTab.TranslateTo(0, 0, 0);
					await Task.WhenAll(
					  GeneralAutoLockTab.TranslateTo(viewWidth, 0, 0),
					  GeneralTab.TranslateTo(0, 0, 0)
					);
                }
            }
        }

		async void ShowPrinterOptionHandle_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
		{
            ViewModel.SelectedPrinter = (Printer)e.Parameter;
			var viewWidth = HardwareTab.Width + HardwareTab.Padding.Left + HardwareTab.Padding.Right;
			await HardwarePrinterTab.TranslateTo(viewWidth, 0, 0);
			await Task.WhenAll(
					  HardwareTab.TranslateTo(viewWidth * -1, 0, 180, Easing.Linear),
					  HardwarePrinterTab.TranslateTo(0, 0, 180, Easing.Linear)
				);
        }

		async void HidePrinterOptionHandle_Tapped(bool animation = false)
		{
            if (HardwareTab.X != 0)
			{
				var viewWidth = HardwareTab.Width + HardwareTab.Padding.Left + HardwareTab.Padding.Right;
				if (animation)
				{
					await Task.WhenAll(
						HardwarePrinterTab.TranslateTo(viewWidth, 0, 180, Easing.Linear),
						HardwareTab.TranslateTo(0, 0, 180, Easing.Linear)
					);
				}
				else
				{
					await Task.WhenAll(
					  HardwarePrinterTab.TranslateTo(viewWidth, 0, 0),
					  HardwareTab.TranslateTo(0, 0, 0)
					);
				}
			}
        }

		private void DocketNumberListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e?.CurrentSelection != null && e.CurrentSelection.Count > 0 && e.CurrentSelection.Last() is AutoLockDurationModel autoLockDurationModel)
			{
                ViewModel.ChangeDocketRangeTapped(autoLockDurationModel);
				autoLockDurationModel.IsSelected = true;
			}
            DocketNumberListView.SelectedItem = null;
			HideDocketNumberRangeOptionHandle_Tapped(true);
		}

        async void ShowDocketNumberRangeOptionHandle_Tapped(object sender, System.EventArgs e)
        {
            ViewModel.UpdateDocketRange();
			var viewWidth = HardwareTab.Width + HardwareTab.Padding.Left + HardwareTab.Padding.Right;
			await HardwareDocketNumberRangeTab.TranslateTo(viewWidth, 0, 0);
			await Task.WhenAll(
			    HardwareTab.TranslateTo(viewWidth * -1, 0, 180, Easing.Linear),
			    HardwareDocketNumberRangeTab.TranslateTo(0, 0, 180, Easing.Linear)
			);
        }

		async void HideDocketNumberRangeOptionHandle_Tapped(bool animation = false)
		{
            if (HardwareTab.X != 0)
			{
			  var viewWidth = HardwareTab.Width + HardwareTab.Padding.Left + HardwareTab.Padding.Right;
			  if (animation)
			  {
			      await Task.WhenAll(
			          HardwareDocketNumberRangeTab.TranslateTo(viewWidth, 0, 180, Easing.Linear),
			          HardwareTab.TranslateTo(0, 0, 180, Easing.Linear)
			      );
			  }
			  else
			  {
			      await Task.WhenAll(
			        HardwareDocketNumberRangeTab.TranslateTo(viewWidth, 0, 0),
			        HardwareTab.TranslateTo(0, 0, 0)
			      );
			  }
			}
        }

		async void ShowNoOfCopiesOptionHandle_Tapped(object sender, System.EventArgs e)
		{
            ViewModel.UpdateNoOfCopiesByValue();
			var viewWidth = HardwarePrinterTab.Width + HardwarePrinterTab.Padding.Left + HardwarePrinterTab.Padding.Right;
			await HardwarePrinterNoOfCopiesTab.TranslateTo(viewWidth, 0, 0);
			await Task.WhenAll(
					  HardwarePrinterTab.TranslateTo(viewWidth * -1, 0, 180, Easing.Linear),
				      HardwarePrinterNoOfCopiesTab.TranslateTo(0, 0, 180, Easing.Linear)
				);
        }

		async void HideNoOfCopiesOptionHandle_Tapped(bool animation = false)
		{
            if (HardwarePrinterTab.X != 0)
			{
				var viewWidth = HardwarePrinterTab.Width + HardwarePrinterTab.Padding.Left + HardwarePrinterTab.Padding.Right;
				if (animation)
				{
					await Task.WhenAll(
						HardwarePrinterNoOfCopiesTab.TranslateTo(viewWidth, 0, 180, Easing.Linear),
						HardwarePrinterTab.TranslateTo(0, 0, 180, Easing.Linear)
					);
				}
				else
				{
					await Task.WhenAll(
					  HardwarePrinterNoOfCopiesTab.TranslateTo(viewWidth, 0, 0),
					  HardwarePrinterTab.TranslateTo(0, 0, 0)
					);
				}
			}
        }

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
            BackCommandEvent();

            WeakReferenceMessenger.Default.Unregister<Messenger.AllReceiptRegisteredMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.EpsonPrinterFindMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.PosDeviceStatusMessenger>(this);
        }

		#region GeneralTab

		void AutoLockDuration_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e?.CurrentSelection != null && e.CurrentSelection.Any())
            {
                ViewModel.ChangeAutoLockDurationTapped((AutoLockDurationModel)e.CurrentSelection[0]);
                ((AutoLockDurationModel)e.CurrentSelection[0]).IsSelected = true;
                AutoLockListView.SelectedItem = null;
                HideAutoLockOptionHandle_Tapped(true);

            }
        }

		#endregion

		#region PrinterTab

		void NoOfCopies_ItemTapped(object sender, SelectionChangedEventArgs e)
		{
			if (e?.CurrentSelection != null && e.CurrentSelection.Count > 0 && e.CurrentSelection.Last() is AutoLockDurationModel autoLockDurationModel)
			{
                ViewModel.ChangeNoOfCopiesTapped(autoLockDurationModel);
				autoLockDurationModel.IsSelected = true;
			}
            NoOfCopiesListView.SelectedItem = null;
            HideNoOfCopiesOptionHandle_Tapped(true);
		}

        void CustomSwitch_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
        {
        }
        #endregion
        //Ticket starts #70775:The client wants to connect  usb scanner to mc3 print in ipad.by rupesh
        void UpdatePrinterFromSwitchHandle_Toggled(object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
        {
            ViewModel.UpdateSelectedPrinter();
        }
        //Ticket ends #70775.by rupesh

    }
}

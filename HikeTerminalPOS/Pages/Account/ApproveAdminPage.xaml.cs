using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS
{

    public partial class ApproveAdminPage : PopupBasePage<ApproveAdminViewModel>
    {

        AutoLockPage autolockpage;
        public EventHandler<UserListDto> SelectedUser;
        public EventHandler<bool> ClosedPopUp;

        public ApproveAdminPage()
        {
            Initialize();
            //Title = LanguageExtension.Localize("AllUserPageTitle");
            NavigationPage.SetHasNavigationBar(this, false);

        }

        protected override void Initialize()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.LoadData();

            if (autolockpage == null)
            {
                autolockpage = new AutoLockPage();
                //autolockpage.RequiredUpdateAccessToken = false;

                autolockpage.RequiredUpdateAccessToken &= Settings.StoreGeneralRule.SwitchUserAfterEachSale != false;

                autolockpage.AuthennticationSuccessed += async (object sender1, bool e1) =>
                {
                    try
                    {
                        await autolockpage.Close();
                        if (e1)
                        {
                            // Zoho ticket : 2084 : user should be change during sale when
                            // "Settings.StoreGeneralRule.SwitchUserAfterEachSale" is true.

                            if (e1 && sender1 != null && sender1 is AutoLockPage)
                            {
                                var Autolockpage = (AutoLockPage)sender1;
                                await Autolockpage.Close();
                                // Settings.CurrentUser = Autolockpage.ViewModel.CurrentUser;
                                // Settings.CurrentUserEmail = Autolockpage.ViewModel.CurrentUser.EmailAddress;
                                // Settings.GrantedPermissionNames = Autolockpage.ViewModel.CurrentUser.GrantedPermissionNames;
                                // EnterSalePage.ServedBy = Autolockpage.ViewModel.CurrentUser;
                                // WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("All"));
                                // WeakReferenceMessenger.Default.Send(new Messenger.UpdateShopDataMessenger(true));
                            }

                            SelectedUser?.Invoke(this, autolockpage.ViewModel.CurrentUser);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                };
                autolockpage.HasBackButtton = true;
                autolockpage.HasSwitchButtton = false;
            }
        }

        async void CloseHandle_Clicked(object sender, System.EventArgs e)
        {
           await Close();
           ClosedPopUp?.Invoke(this, true);
        }


        public async Task Close()
        {
            try
            {
                if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
                    await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        async void UserSelectHandle_ItemTapped(System.Object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (e?.CurrentSelection?.LastOrDefault() != null)
                {
                    autolockpage.ViewModel.CurrentUser = (UserListDto)e.CurrentSelection.LastOrDefault();
                    await Navigation.PushModalAsync(autolockpage);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }
    }
}


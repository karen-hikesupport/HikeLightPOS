
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using HikePOS.Models;
using HikePOS.Resources;
using HikePOS.Services;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using HikePOS.Helpers;
using HikePOS.UserControls;

namespace HikePOS.ViewModels
{
    public class RestaurantFloorPlanViewModel : BaseViewModel
    {
        
        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        SaleServices saleService;
        ApiService<IRestaurantApi> RestaurantApiService = new ApiService<IRestaurantApi>();
        RestaurantService RestaurantService;

        DisplayInfo displayInfo = DeviceDisplay.Current.MainDisplayInfo;

        public static bool FromRestaurant = false;
        public CanvanceLayoutResponse SelectedFloorLayout;

        public RestaurantFloorPlanPage RestaurantFloorPlanPage { get; set; }

        public ObservableCollection<OccupideTableDto> OccupiedTables { get; set; }
         public InvoiceDto Invoice { get; set; }

        ObservableCollection<FloorDto> _floorList { get; set; }
        public ObservableCollection<FloorDto> FloorList { get { return _floorList; } set { _floorList = value; SetPropertyChanged(nameof(FloorList)); } }
        AbsoluteLayout _tableCanvas;
        public AbsoluteLayout TableCanvas
        {
            get
            {
                return _tableCanvas;

            }
            set
            {
                _tableCanvas = value;
                SetPropertyChanged(nameof(TableCanvas));
            }
        }

        bool _isFloorVisible = true;
        public bool IsFloorVisible
        {
            get
            {
                return _isFloorVisible;

            }
            set
            {
                _isFloorVisible = value;
                SetPropertyChanged(nameof(IsFloorVisible));
            }
        }


        private bool IsTableClicked;

        public ICommand BackPageCommand => new Command(BackHandle_Clicked);

        public ICommand FloorTappedCommand => new Command<FloorDto>(FloorSelected);


        public RestaurantFloorPlanViewModel()
        {
            saleService = new SaleServices(saleApiService);
            RestaurantService = new RestaurantService(RestaurantApiService);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            IsTableClicked = false;
            if (FloorList == null || (FloorList != null && FloorList.Count <= 0))
            {
                var floors = RestaurantService.GetLocalFloors(Settings.SelectedOutletId);
                FloorList = new ObservableCollection<FloorDto>(floors);
            }
            //Table layout is not set up yet. Please add a floor with tables in the layout to continue with the sale.
            FromRestaurant = false;
            if (FloorList != null && FloorList.Count > 0)
            {
                IsFloorVisible = true;
                Settings.SelectedFloorID = Settings.SelectedFloorID == 0 ? FloorList.First().Id : Settings.SelectedFloorID;
                var fdata = FloorList.FirstOrDefault(a => a.Id == Settings.SelectedFloorID);
                if (fdata != null)
                    FloorSelected(fdata);
            }
            else
            {
                IsFloorVisible = false;
            }
        }

        public void SetTableLayout()
        {
            if (SelectedFloorLayout == null)
                return;
            // Get screen size
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            double screenWidth = displayInfo.Width / displayInfo.Density - 48;
            double screenHeight = displayInfo.Height / displayInfo.Density - 200;
            double canvasWidth = SelectedFloorLayout.CanvasWidth ?? 0;
            double canvasHeight = SelectedFloorLayout.CanvasHeight ?? 0;

            if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                screenWidth = displayInfo.Height / displayInfo.Density - 200;
                screenHeight = displayInfo.Width / displayInfo.Density - 20;
            }
                

            // Scaling
            double scaleX = canvasWidth > 0 ? screenWidth / canvasWidth : 0;
            double scaleY = canvasHeight > 0 ? screenHeight / canvasHeight : 0;
            double scale = Math.Min(scaleX, scaleY);  // DeviceInfo.Idiom == DeviceIdiom.Tablet ? Math.Min(scaleX, scaleY) : Math.Max(scaleX, scaleY);

            // Create layout
            TableCanvas = new AbsoluteLayout
            {
                WidthRequest = canvasWidth * scale,
                HeightRequest = canvasHeight * scale,
                Rotation = DeviceInfo.Idiom == DeviceIdiom.Phone ? 90 : 0,
                BackgroundColor = Color.FromRgba("#eee")
            };

            // Add tables scaled
            foreach (var table in SelectedFloorLayout.Objects)
            {
                var timerLabel = new Label { Text = table.ElapsedTime, MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation, FontSize = DeviceInfo.Idiom == DeviceIdiom.Phone ? 10 : 12, FontFamily = Fonts.HikeDefaultFont, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };
                timerLabel.BindingContext = table;
                timerLabel.SetBinding(Label.TextProperty, "ElapsedTime");
                timerLabel.SetBinding(Label.IsVisibleProperty, "ElapsedVisible");

                var border = new Border
                {
                    // Stroke = Colors.Black, // border outline
                    // StrokeThickness = 1,
                    BackgroundColor = AppColors.HikeColor,
                    StrokeShape = (table.TableType == "round" || table.TableType == "circle")
                                    ? new RoundRectangle { CornerRadius = new CornerRadius(((table.RenderedWidth ?? 0) * scale) / 2) } // circle
                                    : new RoundRectangle { CornerRadius = new CornerRadius(10) }, // rounded rectangle

                    Content = new VerticalStackLayout
                    {

                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label { Text = table.Name, FontSize = DeviceInfo.Idiom == DeviceIdiom.Phone ? 14 : 16, LineBreakMode = LineBreakMode.TailTruncation, FontFamily = Fonts.HikeDefaultFont, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, MaxLines = 1 },
                            timerLabel
                        }
                    }
                };

                var tablebooked = saleService.GetLocalInvoiceByTableId(table.TableId ?? 0);
                if (tablebooked != null && (tablebooked.Status == Enums.InvoiceStatus.OnGoing || tablebooked.Status == Enums.InvoiceStatus.initial || tablebooked.Status == Enums.InvoiceStatus.Pending))
                {
                    border.BackgroundColor = Color.FromArgb("#ff6868");
                    table.StartTime = tablebooked.InvoiceFloorTable.AssignedDateTime ?? DateTime.Now;
                    table.StartTimer();
                }
                else if (table.TableId.HasValue && table.TableId > 0 && OccupiedTables != null && OccupiedTables.Count > 0 && OccupiedTables.Any(a=> a.tableId.HasValue && a.tableId > 0 && a.tableId == table.TableId.Value))
                {
                    var tbl = OccupiedTables.First(a => a.tableId == table.TableId.Value);
                    border.BackgroundColor = Color.FromArgb("#ff6868");
                    table.StartTime = tbl.assignedDateTime ?? DateTime.Now;
                    table.StartTimer();
                }

                var tapGesture = new TapGestureRecognizer();
                tapGesture.CommandParameter = table;
                tapGesture.Tapped += async (s, e) =>
                {
                    if (IsTableClicked)
                        return;
                    IsTableClicked = true;
                    _ = Task.Run(() =>
                    {
                        Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? 2000 : 1000).Wait();
                        IsTableClicked = false;
                    });
                    if (s is Border tappedBorder && e.Parameter is CanvanceTableLayout table)
                    {
                        if (Invoice != null && OccupiedTables != null && OccupiedTables.Any(a => a.tableId == table.TableId.Value)
                        && Invoice.InvoiceFloorTable == null && Invoice?.InvoiceLineItems?.Count > 0)
                        { 
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ItemWithOccupideTableMsg"), Colors.Red, Colors.White);
                            return;
                        }

                        if (NavigationService != null && NavigationService.NavigationStack != null && NavigationService.NavigationStack.Count > 0)
                        {
                            FromRestaurant = true;
                            await NavigationService.PopAsync();
                            RestaurantFloorPlanPage.ClosedPaged?.Invoke(this, table);
                        }
                    }
                };
                border.GestureRecognizers.Add(tapGesture);


                double layoutX, layoutY, widthReq, heightReq;

                // if (DeviceInfo.Idiom == DeviceIdiom.Phone)
                // {
                //     // Rotate 270° clockwise (or 90° CCW)
                //     layoutX = table.Y * scale;
                //     layoutY = (screenHeight - table.X - table.Width) * scale;
                //     widthReq = table.Height * scale;
                //     heightReq = table.Width * scale;
                // }
                // else
                {
                    // Normal layout
                    layoutX = (table.Left ?? 0) * scale;
                    layoutY = (table.Top ?? 0) * scale;
                    widthReq = (table.RenderedWidth ?? 0) * scale;
                    heightReq = (table.RenderedHeight ?? 0) * scale;
                }

                border.WidthRequest = widthReq;
                border.HeightRequest = heightReq;
                AbsoluteLayout.SetLayoutBounds(border, new Rect(layoutX, layoutY, widthReq, heightReq));
                AbsoluteLayout.SetLayoutFlags(border, AbsoluteLayoutFlags.None);

                TableCanvas.Children.Add(border);
            }

        }


        public void ResetRoomSelection()
        {
            if (FloorList != null)
            {
                FloorList.Where(x => x.IsSelected).ForEach(a => a.IsSelected = false);
                Settings.SelectedFloorID = 0;
            }
        }

        private void FloorSelected(FloorDto floor)
        {
            ResetRoomSelection();
            floor.IsSelected = true;
            Settings.SelectedFloorID = floor.Id;
            SelectedFloorLayout = floor.CanvanceLayout;
            SetTableLayout();
        }


        async private void BackHandle_Clicked()
        {
            try
            {
                if (NavigationService != null && NavigationService.NavigationStack != null && NavigationService.NavigationStack.Count > 0)
                {
                    FromRestaurant = true;
                    await NavigationService.PopAsync();
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        public override void OnDisappearing()
        {
            foreach (var table in SelectedFloorLayout.Objects)
            {
                table.StopTimer();
            }

            base.OnDisappearing();
        }
    }
}
using System;
using System.Collections.Generic;
using HikePOS.ViewModels;
namespace HikePOS
{
    public partial class ProductSortMenuView : ContentView
    {
        public ProductSortMenuView()
        {
            InitializeComponent();
        }
        void CreationTimeOldest_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = this.BindingContext as EnterSaleViewModel;
            viewModel.SelectSortingFilterMenu("CreationTimeOldest");
        }
        void CreationTimeNewest_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = this.BindingContext as EnterSaleViewModel;
            viewModel.SelectSortingFilterMenu("CreationTimeNewest");
        }
        void SKUAtoZ_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = this.BindingContext as EnterSaleViewModel;
            viewModel.SelectSortingFilterMenu("SKUAtoZ");
        }
        void SKUZtoA_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = this.BindingContext as EnterSaleViewModel;
            viewModel.SelectSortingFilterMenu("SKUZtoA");
        }
        void ProductNameAtoZ_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = this.BindingContext as EnterSaleViewModel;
            viewModel.SelectSortingFilterMenu("ProductNameAtoZ");
        }
        void ProductNameZtoA_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = this.BindingContext as EnterSaleViewModel;
            viewModel.SelectSortingFilterMenu("ProductNameZtoA");
        }
        void RetailPriceHighToLow_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = this.BindingContext as EnterSaleViewModel;
            viewModel.SelectSortingFilterMenu("RetailPriceHighToLow");
        }
        void RetailPriceLowToHigh_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = this.BindingContext as EnterSaleViewModel;
            viewModel.SelectSortingFilterMenu("RetailPriceLowToHigh");
        }

    }
}

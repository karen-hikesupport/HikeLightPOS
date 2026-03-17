using System;
using System.Collections.ObjectModel;
using HikePOS.Models;

namespace HikePOS.ViewModels
{
	public class ExtraProductViewModel : BaseViewModel
	{
		ObservableCollection<ProductDto_POS> _Products { get; set; }
		public ObservableCollection<ProductDto_POS> Products { get { return _Products; } set { _Products = value; SetPropertyChanged(nameof(Products)); } }

		public ExtraProductViewModel()
		{
		}
	}
}

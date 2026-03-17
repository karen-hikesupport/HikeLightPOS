using System;
using HikePOS.Enums;
using HikePOS.Resources;

namespace HikePOS.ViewModels
{
	public class MenuViewModel : BaseViewModel
	{

		string _SelectedMenu { get; set; }
		public string SelectedMenu { get { return _SelectedMenu; } set { _SelectedMenu = value; SetPropertyChanged(nameof(SelectedMenu)); } }

		bool _isClockInOutVisible { get; set; }
		public bool IsClockInOutVisible { get { return _isClockInOutVisible; } set { _isClockInOutVisible = value; SetPropertyChanged(nameof(IsClockInOutVisible)); } }



		Color _buttonTextColor { get; set; }

		public Color ButtonTextColor { get { return _buttonTextColor; } set { _buttonTextColor = value; SetPropertyChanged(nameof(ButtonTextColor)); } }

		public MenuViewModel()
		{
			IsClockInOutVisible = true;
			ButtonTextColor = Colors.Transparent;
		}

		

		

	}
}

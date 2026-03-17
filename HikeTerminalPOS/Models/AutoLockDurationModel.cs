using System;
namespace HikePOS.Models
{
	public class AutoLockDurationModel : BaseModel
	{
		public string Title { get; set; }
        public double Value { get; set; }
        bool _IsSelected { get; set; } = false;
		public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; SetPropertyChanged(nameof(IsSelected)); } }
        public bool IsNotLast { get; set; } = true;
	}
}

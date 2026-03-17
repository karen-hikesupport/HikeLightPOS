using System.Windows.Input;

namespace HikePOS.Models
{
	
	public class ProductModel{

		public string Icon { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public double NewAmount { get; set; }
		public double OldAmount { get; set; }
	}

	public class PurchaseProductModel : FullAuditedPassiveEntityDto
	{
		public PurchaseProductModel() { 
			AddQuantityCommand = new Command(AddQuantity);
			RemoveQuantityCommand = new Command(RemoveQuantity);
		
		}
		public int id { get; set; }
		public string Icon { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public double Amount { get; set;}
		double _TotalAmount { get; set;}
		public double TotalAmount { get
			{
				return _TotalAmount;
			} }
		double _Quantity { get; set; }
		public double Quantity { get { return _Quantity;} set
			{
				_Quantity = value;
				_TotalAmount = Quantity * Amount;
				SetPropertyChanged(nameof(Quantity));
				SetPropertyChanged(nameof(TotalAmount));
			
			} }

		public double ActualQuantity { get; set; }

		public ICommand AddQuantityCommand { get; }

		void AddQuantity()
		{
			if (ActualQuantity > Quantity)
			{
				Quantity++;
			}
		}

		public ICommand RemoveQuantityCommand { get; }

		void RemoveQuantity()
		{
			if (Quantity > 0)
			{
				Quantity--;
			}
		}

	}
}

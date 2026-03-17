namespace HikePOS.Enums
{
	public enum ProductType : int
	{
		Standard,
		Service,
		Category,
		//Ticket start:#22898 Composite sale not working properly.by rupesh
		Composite = 5
		//Ticket end:#22898 .by rupesh
	}
}

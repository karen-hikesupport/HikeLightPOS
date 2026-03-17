namespace HikePOS.Models
{
	public interface IFullAuditedPassiveEntityDto 
	{
        int Id { get; set; }
        bool IsActive { get; set; }

    }

    public class FullAuditedPassiveEntityDto :  BaseModel
    {
		int _Id { get; set; }
        public int Id { get { return _Id; } set { _Id = value; SetPropertyChanged(nameof(Id)); } }

		bool _isActive { get; set; }
		public bool IsActive { get { return _isActive; } set { _isActive = value; SetPropertyChanged(nameof(IsActive)); } }
		//public bool IsActive { get; set; } = true;
	}
}

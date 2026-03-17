using Realms;
namespace HikePOS.Models
{
	public partial class ServiceUserDto : IRealmObject
	{
        [PrimaryKey]
        public int UserId { get; set; }
	}
}

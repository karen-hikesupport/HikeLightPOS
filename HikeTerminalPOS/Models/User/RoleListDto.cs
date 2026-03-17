using System;
using Realms;

namespace HikePOS.Models
{
	public partial class RoleListDto : IRealmObject
	{
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
		public string DisplayName { get; set; }
		public bool IsStatic { get; set; }
		public bool IsDefault { get; set; }
		public DateTimeOffset CreationTime { get; set; }
	}
}

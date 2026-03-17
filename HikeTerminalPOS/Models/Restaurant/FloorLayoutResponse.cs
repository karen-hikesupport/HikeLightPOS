using System;
using HikePOS.Helpers;
using Newtonsoft.Json;

//#94565
namespace HikePOS.Models
{
    public class RoomModel : FullAuditedPassiveEntityDto
    {
        public int RoomId { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        [JsonIgnore]
        bool _isSelected;

        [JsonIgnore]
        public bool IsSelected { get { return _isSelected; } set { _isSelected = value; SetPropertyChanged(nameof(IsSelected)); } }

    }

    public class TableModel
    {
        public int TableId { get; set; }
        public string Name { get; set; }
        public int Seats { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Area { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }

    public class FloorLayoutResponse  : FullAuditedPassiveEntityDto
    {
        RoomModel _room;
        public RoomModel Room { get { return _room; } set { _room = value; SetPropertyChanged(nameof(Room)); } }

        public List<TableModel> Tables { get; set; }
    }
}
//#94565

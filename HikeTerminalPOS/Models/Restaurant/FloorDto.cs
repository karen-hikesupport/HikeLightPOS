
using Microsoft.Maui.Dispatching;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
	public class FloorTable : FullAuditedPassiveEntityDto
	{
		public int FloorId { get; set; }
		public string Name { get; set; }
		public object Properties { get; set; }
	}

	public class FloorDto : FullAuditedPassiveEntityDto
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("outletId")]
		public int? OutletId { get; set; }

		[JsonProperty("outletName")]
		public string OutletName { get; set; }

		[JsonProperty("naming")]
		public string Naming { get; set; }

		[JsonProperty("numberOfTables")]
		public int? NumberOfTables { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }

		[JsonProperty("layoutJson")]
		public string LayoutJson { get; set; }

		[JsonIgnore]
		public CanvanceLayoutResponse CanvanceLayout
		{
			get
			{
				if (string.IsNullOrEmpty(LayoutJson))
				{
					return null;
				}
				else
				{
					try
					{
						var response = JsonConvert.DeserializeObject<CanvanceLayoutResponse>(LayoutJson);
						return response;
					}
					catch (Exception ex)
					{
						return null;
					}
				}
			}
		}

		[JsonProperty("floorTables")]
		public List<FloorTable> FloorTables { get; set; }
		[JsonIgnore]
		private bool _isSelected;
		[JsonIgnore]
		public bool IsSelected { get { return _isSelected; } set { _isSelected = value; SetPropertyChanged(nameof(IsSelected)); } }
		public FloorDB ToModel()
		{
			FloorDB groupPriceDB = new FloorDB
			{
				Id = Id,
				IsActive = IsActive,
				Name = Name,
				LayoutJson = LayoutJson,
				Label = Label,
				Naming = Naming,
				OutletId = OutletId,
				OutletName = OutletName,
				NumberOfTables = NumberOfTables
			};

			return groupPriceDB;
		}

		public static FloorDto FromModel(FloorDB model)
		{
			if (model == null)
				return null;

			FloorDto dto = new FloorDto
			{
				Id = model.Id,
				IsActive = model.IsActive,
				Name = model.Name,
				LayoutJson = model.LayoutJson,
				Label = model.Label,
				Naming = model.Naming,
				OutletId = model.OutletId,
				OutletName = model.OutletName,
				NumberOfTables = model.NumberOfTables
			};
			return dto;
		}
	}

	#region DB
	public partial class FloorDB : IRealmObject
	{
		[PrimaryKey]
		public int Id { get; set; }

		public bool IsActive { get; set; }

		public string Name { get; set; }

		public int? OutletId { get; set; }

		public string OutletName { get; set; }

		public string Naming { get; set; }

		public int? NumberOfTables { get; set; }

		public string Label { get; set; }

		public string LayoutJson { get; set; }
	}
	#endregion


	public class CanvanceTableLayout : BaseModel
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; }

		[JsonProperty("originX")]
		public string OriginX { get; set; }

		[JsonProperty("originY")]
		public string OriginY { get; set; }

		[JsonProperty("left")]
		public double? Left { get; set; }

		[JsonProperty("top")]
		public double? Top { get; set; }

		[JsonProperty("width")]
		public double? Width { get; set; }

		[JsonProperty("height")]
		public double? Height { get; set; }

		[JsonProperty("fill")]
		public string Fill { get; set; }

		[JsonProperty("stroke")]
		public string Stroke { get; set; }

		[JsonProperty("strokeWidth")]
		public int? StrokeWidth { get; set; }

		[JsonProperty("strokeDashArray")]
		public object StrokeDashArray { get; set; }

		[JsonProperty("strokeLineCap")]
		public string StrokeLineCap { get; set; }

		[JsonProperty("strokeDashOffset")]
		public int? StrokeDashOffset { get; set; }

		[JsonProperty("strokeLineJoin")]
		public string StrokeLineJoin { get; set; }

		[JsonProperty("strokeUniform")]
		public bool? StrokeUniform { get; set; }

		[JsonProperty("strokeMiterLimit")]
		public int? StrokeMiterLimit { get; set; }

		[JsonProperty("scaleX")]
		public double? ScaleX { get; set; }

		[JsonProperty("scaleY")]
		public double? ScaleY { get; set; }

		[JsonProperty("angle")]
		public double? Angle { get; set; }

		[JsonProperty("flipX")]
		public bool? FlipX { get; set; }

		[JsonProperty("flipY")]
		public bool? FlipY { get; set; }

		[JsonProperty("opacity")]
		public double? Opacity { get; set; }

		[JsonProperty("shadow")]
		public object Shadow { get; set; }

		[JsonProperty("visible")]
		public bool? Visible { get; set; }

		[JsonProperty("backgroundColor")]
		public string BackgroundColor { get; set; }

		[JsonProperty("fillRule")]
		public string FillRule { get; set; }

		[JsonProperty("paintFirst")]
		public string PaintFirst { get; set; }

		[JsonProperty("globalCompositeOperation")]
		public string GlobalCompositeOperation { get; set; }

		[JsonProperty("skewX")]
		public double? SkewX { get; set; }

		[JsonProperty("skewY")]
		public double? SkewY { get; set; }

		[JsonProperty("tableId")]
		public int? TableId { get; set; }

		[JsonIgnore]
		private string _name;

		[JsonProperty("name")]
		public string Name { get { return _name; } set { _name = value; SetPropertyChanged(nameof(Name)); } }

		[JsonProperty("seats")]
		public int? Seats { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("tableType")]
		public string TableType { get; set; }

		[JsonProperty("objects")]
		public List<CanvanceTableLayout> Objects;

		[JsonProperty("renderedWidth")]
		public double? RenderedWidth { get; set; }

		[JsonProperty("renderedHeight")]
		public double? RenderedHeight { get; set; }

		[JsonProperty("rx")]
		public double? Rx { get; set; }

		[JsonProperty("ry")]
		public double? Ry { get; set; }

		[JsonProperty("fontFamily")]
		public string FontFamily { get; set; }

		[JsonProperty("fontWeight")]
		public string FontWeight { get; set; }

		[JsonProperty("fontSize")]
		public int? FontSize { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("underline")]
		public bool? Underline { get; set; }

		[JsonProperty("overline")]
		public bool? Overline { get; set; }

		[JsonProperty("linethrough")]
		public bool? Linethrough { get; set; }

		[JsonProperty("textAlign")]
		public string TextAlign { get; set; }

		[JsonProperty("fontStyle")]
		public string FontStyle { get; set; }

		[JsonProperty("lineHeight")]
		public double? LineHeight { get; set; }

		[JsonProperty("textBackgroundColor")]
		public string TextBackgroundColor { get; set; }

		[JsonProperty("charSpacing")]
		public object CharSpacing { get; set; }

		[JsonProperty("direction")]
		public string Direction { get; set; }

		[JsonProperty("path")]
		public object Path { get; set; }

		[JsonProperty("pathStartOffset")]
		public object PathStartOffset { get; set; }

		[JsonProperty("pathSide")]
		public string PathSide { get; set; }

		[JsonProperty("pathAlign")]
		public string PathAlign { get; set; }

		[JsonProperty("minWidth")]
		public double? MinWidth { get; set; }

		[JsonProperty("splitByGrapheme")]
		public bool? SplitByGrapheme { get; set; }

		[JsonIgnore]
		public DateTime StartTime;
		[JsonIgnore]
		private string _elapsedTime;
		[JsonIgnore]
		public string ElapsedTime
		{
			get { return _elapsedTime; }
			set
			{
				_elapsedTime = value;
				SetPropertyChanged(nameof(ElapsedTime));
				if (string.IsNullOrEmpty(ElapsedTime))
					ElapsedVisible = false;
				else
					ElapsedVisible = true;
			}
		}

		private bool _elapsedVisible;
		[JsonIgnore]
		public bool ElapsedVisible { get { return _elapsedVisible; } set { _elapsedVisible = value; SetPropertyChanged(nameof(ElapsedVisible)); } }

		[JsonIgnore]
		private bool _timerRunning;

		public void StartTimer()
		{
			_timerRunning = true;
			Dispatcher.GetForCurrentThread().StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				if (!_timerRunning)
					return false; // Stop the timer

				// This runs every second on the UI thread
				var elapsed = DateTime.Now - StartTime.ToLocalTime();
				ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
				return true; // return true to keep repeating, false to stop
			});
		}

		public void StopTimer()
		{
			_timerRunning = false;
		}

	}

	public class CanvanceLayoutResponse
	{
		[JsonProperty("canvasWidth")]
		public double? CanvasWidth { get; set; }

		[JsonProperty("canvasHeight")]
		public double? CanvasHeight { get; set; }

		[JsonProperty("objects")]
		public List<CanvanceTableLayout> Objects { get; set; }
	}
	public class GetFloorInput
	{
		public string Name { get; set; }
		public string Filter { get; set; }
		public string sorting { get; set; }
		public int MaxResultCount { get; set; }
		public int SkipCount { get; set; }
	}

	public class OccupideTableDto
    {
        public int invoiceId { get; set; }
        public int? floorId { get; set; }
        public int? tableId { get; set; }
        public DateTime? assignedDateTime { get; set; }
        public DateTime? releasedDateTime { get; set; }
        public string tableName { get; set; }
        public string floorName { get; set; }
        public int id { get; set; }
    }

}

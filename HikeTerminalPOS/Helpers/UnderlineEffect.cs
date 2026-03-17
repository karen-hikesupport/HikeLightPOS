namespace HikePOS.Helpers
{
	public class UnderlineEffect : RoutingEffect
	{
		public const string EffectNamespace = "HikePOS.Helpers";

		public UnderlineEffect() : base($"{EffectNamespace}.{nameof(UnderlineEffect)}") { }
	}

	public class LineThroughEffect : RoutingEffect
	{
		public const string EffectNamespace = "HikePOS.Helpers";

		public LineThroughEffect() : base("HikePOS.Helpers.PlatformLineThroughEffect") { }
	}
}

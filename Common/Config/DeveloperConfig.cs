using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Common.Config;

public sealed class DeveloperConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[DefaultValue(true)]
	public bool DrawUIBorders { get; set; } = true;

	[DefaultValue(true)]
	public bool SaveSubworlds { get; set; }

#if DEBUG
	/// <summary>
	///		When enabled, every time the item filter rejects a drop a chat line is printed showing the
	///		filter, item, and which condition failed. Compiled out entirely in release builds since
	///		end-users have no use for this and the visible spam would be noisy.
	/// </summary>
	[DefaultValue(false)]
	public bool LogFilterRejections { get; set; }
#endif

	/// <summary>
	///		Static accessor that mirrors <see cref="LogFilterRejections"/> in debug and is hard-coded
	///		to <c>false</c> in release. Lets call sites read the flag without sprinkling
	///		<c>#if DEBUG</c> into gameplay code.
	/// </summary>
	public static bool ShouldLogFilterRejections
	{
		get
		{
#if DEBUG
			return ModContent.GetInstance<DeveloperConfig>()?.LogFilterRejections ?? false;
#else
			return false;
#endif
		}
	}
}
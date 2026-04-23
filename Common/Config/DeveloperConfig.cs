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
}
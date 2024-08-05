using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Common.Config;

public sealed class DeveloperConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Header("Graphics")]
	[DefaultValue(true)]
	public bool DrawUIBorders { get; set; } = true;
}
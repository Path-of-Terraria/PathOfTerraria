using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Common.Config;

public sealed class UIConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Header("UI")]
	[DefaultValue(false)]
	public bool PreventExpBarClicking { get; set; } = false;
}
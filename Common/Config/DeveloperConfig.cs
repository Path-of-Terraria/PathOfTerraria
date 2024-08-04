using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Common.Config;
public class DeveloperConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
	
	[LabelKey("$Mods.PathOfTerraria.DeveloperConfig.DrawGuiBorders.Label")]
	[TooltipKey("$Mods.PathOfTerraria.DeveloperConfig.DrawGuiBorders.Tooltip")]
	[DefaultValue(true)]
	public bool DrawGuiBorders;
}
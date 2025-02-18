using PathOfTerraria.Common.Subworlds.MappingAreas;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class GrovetenderPlayer : ModPlayer
{
	public override void PreUpdateMovement()
	{
		if (SubworldSystem.Current is ForestArea && !Player.dead && Player.Center.Y > (Main.maxTilesY - 45) * 16)
		{
			string text = Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.Grovetender.BottomDeath." + Main.rand.Next(3), Player.name);
			var reason = PlayerDeathReason.ByCustomReason(text);
			Player.KillMe(reason, 9999, 0, false);
		}
	}
}

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
			var reason = PlayerDeathReason.ByCustomReason(NetworkText.FromKey($"Mods.{PoTMod.ModName}.NPCs.Grovetender.BottomDeath." + Main.rand.Next(3), Player.name));
			Player.KillMe(reason, 9999, 0, false);
		}
	}
}

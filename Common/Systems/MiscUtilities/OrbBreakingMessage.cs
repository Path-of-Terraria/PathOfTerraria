using System.Collections.Generic;
using PathOfTerraria.Common.Systems.VanillaModifications;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.MiscUtilities;

public class OrbPreventer : GlobalTile
{
	private static int orbMessageCooldown = 0;
	private static void TrySendMessage(string text, Color color)
	{
		if (orbMessageCooldown == 0 && Main.netMode != NetmodeID.Server)
		{
			Main.NewText(text, color);
			orbMessageCooldown = 60; // 1 second cooldown
		}
	}

	public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
	{
		if (type == TileID.ShadowOrbs && PlayerIsMining() && DisableOrbBreaking.BreakableOrbSystem.CanBreakOrb == false)
		{
			TrySendMessage("Something tells me I shouldn't break this yet.", Color.MediumPurple);
			return false;
		}
		return base.CanKillTile(i, j, type, ref blockDamaged);
	}

	public override bool CanExplode(int i, int j, int type)
	{
		if (type == TileID.ShadowOrbs && DisableOrbBreaking.BreakableOrbSystem.CanBreakOrb == false)
		{
			TrySendMessage("Something tells me I shouldn't break this yet.", Color.OrangeRed);
			return false;
		}
		return base.CanExplode(i, j, type);
	}
	
	private bool PlayerIsMining()
	{
		Player player = Main.LocalPlayer;

		// Only trigger if left-click is held and player has a hammer tool out
		return Main.mouseLeft && player.HeldItem != null && (player.HeldItem.hammer > 0);
	}
	
	public partial class OrbMessageHandler : ModSystem
	{
		public override void PostUpdateEverything()
		{
			if (orbMessageCooldown > 0)
				orbMessageCooldown--;
		}
	}
}
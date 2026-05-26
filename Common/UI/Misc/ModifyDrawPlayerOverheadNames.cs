using MonoMod.Cil;
using PathOfTerraria.Common.Classing;
using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.GameContent.UI;

namespace PathOfTerraria.Common.UI.Misc;

internal class ModifyDrawPlayerOverheadNames : ILoadable
{
	public void Load(Mod mod)
	{
		IL_NewMultiplayerClosePlayersOverlay.Draw += ModifyPlayerNames;
	}

	private void ModifyPlayerNames(ILContext il)
	{
		ILCursor c = new(il);

		// Find the name,
		if (!c.TryGotoNext(MoveType.After, x => x.MatchLdfld<Player>("name")))
		{
			return;
		}

		int local = -1;

		// Get the local index of the Player
		if (!c.TryGotoPrev(MoveType.Before, x => x.MatchLdloc(out local)))
		{
			return;
		}

		if (local == -1)
		{
			return;
		}

		// Return to the name
		if (!c.TryGotoNext(MoveType.After, x => x.MatchLdfld<Player>("name")))
		{
			return;
		}

		// Emit the player, modify name
		c.EmitLdloc(local);
		c.EmitDelegate(ModifyName);
	}

	public static string ModifyName(string name, Player player)
	{
		StarterClass starterClass = player.GetModPlayer<ClassingPlayer>().Class;

		if (starterClass == StarterClass.None)
		{
			return name;
		}

		int id = StarterClasses.GetInfo(starterClass).WeaponItemId;
		return name + $" ({ClassingPlayer.GetClassNoun(player, player.whoAmI % 2)} - {player.GetModPlayer<ExpModPlayer>().Level})";
	}

	public void Unload()
	{
	}
}

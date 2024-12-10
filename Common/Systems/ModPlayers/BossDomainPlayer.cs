using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class BossDomainPlayer : ModPlayer
{
	public bool InDomain = false;

	private int _livesLeft = 0;
	private int _deadTime = 0;

	public override void Load()
	{
		On_Player.Ghost += UpdateGhost;
	}

	private static void UpdateGhost(On_Player.orig_Ghost orig, Player self)
	{
		self.GetModPlayer<BossDomainPlayer>().ResetEffects();

		orig(self);

		if (self.GetModPlayer<BossDomainPlayer>().InDomain)
		{
			self.respawnTimer++;

			if (++self.GetModPlayer<BossDomainPlayer>()._deadTime > 360)
			{
				self.GetModPlayer<BossDomainPlayer>()._deadTime = 0;
				SubworldSystem.Exit();
			}
		}
	}

	public override void ResetEffects()
	{
		bool inDomain = SubworldSystem.Current is BossDomainSubworld;

		if (inDomain && !InDomain)
		{
			SetInDomain();
		}

		if (!inDomain && InDomain)
		{
			ExitDomain();
		}

		InDomain = inDomain;
	}

	public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
	{
		if (InDomain && --_livesLeft > 0)
		{
			Player.statLife = Player.statLifeMax2;
			Player.Teleport(new Vector2(Main.spawnTileX, Main.spawnTileY).ToWorldCoordinates(), TeleportationStyleID.RodOfDiscord);
			return false;
		}

		if (InDomain)
		{
			Player.ghost = true;
		}

		return true;
	}

	private void ExitDomain()
	{
		if (Player.dead || Player.ghost)
		{
			Player.Spawn(PlayerSpawnContext.ReviveFromDeath);
			Player.ghost = false;
			Player.deadForGood = false;
		}

		_livesLeft = 0;
	}

	private void SetInDomain()
	{
		_livesLeft = GetLivesPerPlayer();
	}

	public static int GetLivesPerPlayer()
	{
		if (SubworldSystem.Current is WallOfFleshDomain)
		{
			return 1;
		}

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			return 6;
		}

		int plr = Main.CurrentFrameFlags.ActivePlayersCount;

		return plr switch
		{
			1 => 6,
			2 => 3,
			3 => 2,
			_ => 1
		};
	}
}

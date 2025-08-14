using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;

internal class BossDomainLivesPlayer : ModPlayer
{
	public bool InDomain = false;
	public int LivesLeft = 0;

	private int _deadTime = 0;

	public override void Load()
	{
		On_Player.Ghost += UpdateGhost;
	}

	private static void UpdateGhost(On_Player.orig_Ghost orig, Player self)
	{
		self.GetModPlayer<BossDomainLivesPlayer>().ResetEffects();

		orig(self);

		if (self.GetModPlayer<BossDomainLivesPlayer>().InDomain)
		{
			self.respawnTimer++;

			if (++self.GetModPlayer<BossDomainLivesPlayer>()._deadTime > 360)
			{
				self.GetModPlayer<BossDomainLivesPlayer>()._deadTime = 0;
				SubworldSystem.Exit();
			}
		}
	}

	public override void ResetEffects()
	{
		bool inDomain = SubworldSystem.Current is MappingWorld;

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

	public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
	{
		return !InDomain || !Player.ghost;
	}

	public override bool CanBeHitByProjectile(Projectile proj)
	{
		return !InDomain || !Player.ghost;
	}

	public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
	{
		if (InDomain && --LivesLeft > 0)
		{
			Player.statLife = Player.statLifeMax2;
			Player.Teleport(new Vector2(Main.spawnTileX, Main.spawnTileY).ToWorldCoordinates(), TeleportationStyleID.RodOfDiscord);
			Player.velocity = Vector2.Zero;

			for (int i = 0; i < Player.buffTime.Length; ++i)
			{
				if (Player.buffType[i] != 0 && Main.debuff[Player.buffType[i]])
				{
					Player.DelBuff(i);
					i--;
				}
			}

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

		LivesLeft = 0;
	}

	private void SetInDomain()
	{
		LivesLeft = GetLivesPerPlayer();
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

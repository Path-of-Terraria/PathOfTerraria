using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;

public interface IPreDomainRespawnPlayer
{
	private static readonly HookList<ModPlayer> OnRespawnHook = PlayerLoader.AddModHook(HookList<ModPlayer>.Create(i => ((IPreDomainRespawnPlayer)i).OnDomainRespawn));

	public void OnDomainRespawn();

	public static void Invoke(Player player)
	{
		foreach (IPreDomainRespawnPlayer g in OnRespawnHook.Enumerate(player.ModPlayers))
		{
			g.OnDomainRespawn();
		}
	}
}

internal class BossDomainLivesPlayer : ModPlayer
{
	public bool InDomain = false;
	public int LivesLost = 0;
	public int LivesGained = 0;
	public Point16 ActiveMapDevicePosition { get; private set; } = new(-1, -1);

	private int _deadTime = 0;
	private int _maxPlayersWitnessed;
	private bool _closeMapDeviceOnExit;

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

	public override void OnEnterWorld()
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
		if (InDomain)
		{
			LivesLost++;
		}

		if (InDomain && GetLivesLeft() > 0)
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

			IPreDomainRespawnPlayer.Invoke(Player);

			return false;
		}

		if (InDomain)
		{
			CloseTrackedMapDevicePortal();
			Player.ghost = true;
		}

		return true;
	}

	/// <summary>
	/// Called when the player exits a domain. Local player only.
	/// </summary>
	private void ExitDomain()
	{
		if (Player.dead || Player.ghost)
		{
			Player.ghost = false;
			Player.deadForGood = false;
			Player.Spawn(PlayerSpawnContext.ReviveFromDeath);

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				RequestCheckSectionHandler.Send(Player.Center);
			}
		}

		LivesLost = 0;
		LivesGained = 0;
		_maxPlayersWitnessed = 0;
		TryCloseTrackedMapDevicePortal();
		ActiveMapDevicePosition = new Point16(-1, -1);
		_closeMapDeviceOnExit = false;
	}

	private void SetInDomain()
	{
		LivesLost = 0;
		LivesGained = 0;
		_maxPlayersWitnessed = 0;
	}

	public void SetActiveMapDevice(Point16 position)
	{
		ActiveMapDevicePosition = position;
		_closeMapDeviceOnExit = false;
	}

	private bool HasActiveMapDevice()
	{
		return ActiveMapDevicePosition.X >= 0 && ActiveMapDevicePosition.Y >= 0;
	}

	private void CloseTrackedMapDevicePortal()
	{
		if (!HasActiveMapDevice())
		{
			return;
		}

		_closeMapDeviceOnExit = true;
	}

	private void TryCloseTrackedMapDevicePortal()
	{
		if (!_closeMapDeviceOnExit || !HasActiveMapDevice())
		{
			return;
		}

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			if (SubworldSystem.Current is not null)
			{
				ModPacket packet = Networking.GetPacket<CloseMapDevicePortalHandler>(5);
				packet.Write(ActiveMapDevicePosition.X);
				packet.Write(ActiveMapDevicePosition.Y);
				Networking.SendPacketToMainServer(packet);
			}
			else
			{
				CloseMapDevicePortalHandler.Send(ActiveMapDevicePosition);
			}
		}
		else if (SubworldSystem.Current == null)
		{
			CloseMapDevicePortalHandler.TryClosePortal(ActiveMapDevicePosition);
		}
	}

	public int GetLivesLeft()
	{
		_maxPlayersWitnessed = Math.Max(_maxPlayersWitnessed, Main.CurrentFrameFlags.ActivePlayersCount);

		return Math.Max(0, GetLivesPerPlayer(_maxPlayersWitnessed) - LivesLost) + LivesGained;
	}

	public static int GetLivesPerPlayer(int playerCount)
	{
		if (SubworldSystem.Current is WallOfFleshDomain)
		{
			return 1;
		}

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			return 6;
		}

		return playerCount switch
		{
			1 => 6,
			2 => 3,
			3 => 2,
			_ => 1
		};
	}
}

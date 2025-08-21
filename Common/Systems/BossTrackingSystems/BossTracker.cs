using PathOfTerraria.Common.NPCs.GlobalNPCs;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.BossTrackingSystems;

internal class BossTracker : ModSystem
{
	/// <summary>
	/// Tracks the bosses that were killed in a <see cref="BossDomainSubworld"/>, so we can delay the effects to the main world. 
	/// This can also be used for non-universal kills per subworld, i.e.<br/>
	/// <c>if (SubworldSystem.Current is BossDomainSubworld &amp;&amp; CachedBossesDowned.Contains(id)</c><br/>
	/// you know you are in a subworld where a boss was slain.
	/// </summary>
	public static HashSet<int> CachedBossesDowned = [];

	/// <summary>
	/// Tracks all bosses that have ever been downed. If they're in this set, they've been killed at least once before by any player.
	/// </summary>
	public static HashSet<int> TotalBossesDowned = [];

	public static BitsByte DownedFlags;
	public static bool SkipWoFBox;

	private static readonly MethodInfo DoDeathEventsInfo = typeof(NPC).GetMethod("DoDeathEvents", BindingFlags.Instance | BindingFlags.NonPublic);

	public static bool DownedEaterOfWorlds { get => DownedFlags[0]; set => DownedFlags[0] = value; }
	public static bool DownedBrainOfCthulhu { get => DownedFlags[1]; set => DownedFlags[1] = value; }

	public override void Load()
	{
		On_NPC.DoDeathEvents += HijackDeathEffects;
		On_NPC.CreateBrickBoxForWallOfFlesh += StopBrickBox;
	}

	public static void AddDowned(int id, bool fromSync = false, bool setPlayerValues = false, bool dontCache = false)
	{
		if (!dontCache)
		{
			CachedBossesDowned.Add(id);
		}
		
		TotalBossesDowned.Add(id);

		if (setPlayerValues)
		{
			foreach (Player player in Main.ActivePlayers)
			{
				player.GetModPlayer<BossTrackingPlayer>().CachedBossesDowned.Add(id);
			}
		}

		if (Main.netMode != NetmodeID.SinglePlayer && !fromSync)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.RequestWorldData, -1, -1, null, Main.myPlayer);
			}
			else
			{
				NetMessage.SendData(MessageID.WorldData);
			}

			ModContent.GetInstance<SyncPlayerBossDownedHandler>().Send(id);

			//if (SubworldSystem.Current is not null)
			//{
			//	ModPacket packet = Networking.Networking.GetPacket(Networking.Networking.Message.SyncBossDowned);
			//	packet.Write(id);

			//	Networking.Networking.SendPacketToMainServer(packet);
			//}
		}
	}

	public static bool DownedInDomain<T>(int id) where T : BossDomainSubworld
	{
		return CachedBossesDowned.Contains(id) && SubworldSystem.Current is T;
	}

	public static bool DownedInDomain<T>(params int[] ids) where T : BossDomainSubworld
	{
		if (SubworldSystem.Current is not T)
		{
			return false;
		}

		foreach (int id in ids)
		{
			if (!CachedBossesDowned.Contains(id))
			{
				return false;
			}
		}

		return true;
	}

	private void StopBrickBox(On_NPC.orig_CreateBrickBoxForWallOfFlesh orig, NPC self)
	{
		if (SkipWoFBox)
		{
			return;
		}

		orig(self);
	}

	private void HijackDeathEffects(On_NPC.orig_DoDeathEvents orig, NPC self, Player closestPlayer)
	{
		if (SubworldSystem.Current is BossDomainSubworld && self.boss)
		{
			self.type = NPCID.None;
			self.boss = false;
		}
		else
		{
			OnDeathNPC.OnDeathEffects(self);
		}

		orig(self, closestPlayer);
	}

	public override void PreUpdateEntities()
	{
		if (SubworldSystem.Current is null && CachedBossesDowned.Count > 0)
		{
			foreach (int type in CachedBossesDowned)
			{
				var npc = new NPC();
				npc.SetDefaults(type);
				npc.Center = Main.player[0].Center;

				if (npc.type == NPCID.EaterofWorldsHead)
				{
					npc.boss = true;
				}

				SkipWoFBox = true;
				DoDeathEventsInfo.Invoke(npc, [Main.player[0]]);
				SkipWoFBox = false;
			}

			CachedBossesDowned.Clear();
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add(nameof(DownedFlags), (byte)DownedFlags);
		tag.Add(nameof(CachedBossesDowned), (int[])[.. CachedBossesDowned]);
		tag.Add(nameof(TotalBossesDowned), (int[])[.. TotalBossesDowned]);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		CachedBossesDowned.Clear();
		DownedFlags = tag.GetByte(nameof(DownedFlags));
		CachedBossesDowned = [.. tag.GetIntArray(nameof(CachedBossesDowned))];
		TotalBossesDowned = [.. tag.GetIntArray(nameof(TotalBossesDowned))];
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)DownedFlags);
		writer.Write(CachedBossesDowned.Count);

		foreach (int item in CachedBossesDowned)
		{
			writer.Write(item);
		}

		writer.Write(TotalBossesDowned.Count);

		foreach (int item in TotalBossesDowned)
		{
			writer.Write(item);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		DownedFlags = reader.ReadByte();
		CachedBossesDowned.Clear();
		TotalBossesDowned.Clear();

		int count = reader.ReadInt32();
		
		for (int i = 0; i < count; ++i)
		{
			CachedBossesDowned.Add(reader.ReadInt32());
		}

		count = reader.ReadInt32();

		for (int i = 0; i < count; ++i)
		{
			TotalBossesDowned.Add(reader.ReadInt32());
		}
	}

	public class BossTrackerNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.type == NPCID.EaterofWorldsHead && NPC.CountNPCS(NPCID.EaterofWorldsBody) == 0)
			{
				// EoW is dumb and won't register as a boss before death so this is the workaround
				DownedEaterOfWorlds = true;

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}

			if (!npc.boss)
			{
				return;
			}

			if (npc.type == NPCID.BrainofCthulhu)
			{
				DownedBrainOfCthulhu = true;

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}
	}
}

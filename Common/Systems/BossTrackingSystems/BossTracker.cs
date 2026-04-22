using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PathOfTerraria.Common.NPCs.GlobalNPCs;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Content.Swamp.NPCs.SwampBoss;
using SubworldLibrary;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.BossTrackingSystems;

internal sealed class BossTracker : ModSystem
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

	public static bool SkipWoFBox;
	public static bool AlwaysSkipWoFBox = true;

	private static readonly MethodInfo DoDeathEventsInfo = typeof(NPC).GetMethod("DoDeathEvents", BindingFlags.Instance | BindingFlags.NonPublic);

	public override void Load()
	{
		On_NPC.DoDeathEvents += HijackDeathEffects;
		On_NPC.CreateBrickBoxForWallOfFlesh += StopBrickBox;
	}

	public static void AddDowned(int id, bool fromSync = false, bool setBossDowned = false, bool dontCache = false)
	{
		if (!dontCache)
		{
			CachedBossesDowned.Add(id);
		}
		
		TotalBossesDowned.Add(id);

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

			if (SubworldSystem.Current is not null && setBossDowned)
			{
				ModPacket packet = Networking.GetPacket<SyncBossDownedHandler>(5);
				packet.Write(id);
				Networking.SendPacketToMainServer(packet);
			}
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

	private static void StopBrickBox(On_NPC.orig_CreateBrickBoxForWallOfFlesh orig, NPC self)
	{
		if (!SkipWoFBox && !AlwaysSkipWoFBox)
		{
			orig(self);
		}
	}

	private static void HijackDeathEffects(On_NPC.orig_DoDeathEvents orig, NPC self, Player closestPlayer)
	{
		bool isBoss = ContentSamples.NpcsByNetId[self.netID].boss || NPCID.Sets.ShouldBeCountedAsBoss[self.type];
		bool isPillar = self.type is NPCID.LunarTowerNebula or NPCID.LunarTowerSolar or NPCID.LunarTowerStardust or NPCID.LunarTowerVortex;
		int type = self.type;
		bool oldBoss = self.boss;

		if (SubworldSystem.Current is BossDomainSubworld or IExplorationWorld && isBoss && !isPillar)
		{
			// Spawns the Wall of Flesh's box around itself, which is overriden by this method
			OnDeathNPC.OnDeathEffects(self);

			if (CheckSpecialConditions(self, isBoss))
			{
				// Automatically add/send the boss downed cache/packet
				AddDowned(self.netID, false, true, false);

				// Sends both bosses for consistency for the Twins
				if (self.netID == NPCID.Retinazer)
				{
					AddDowned(NPCID.Spazmatism, false, true, false);
				}
				else if (self.netID == NPCID.Spazmatism)
				{
					AddDowned(NPCID.Retinazer, false, true, false);
				}
			}

			if (SubworldSystem.Current is not IExplorationWorld)
			{
				self.type = NPCID.None;
				self.boss = false;
			}
		}

		orig(self, closestPlayer);
	}

	private static bool CheckSpecialConditions(NPC self, bool isBoss)
	{
		int type = self.type;

		if (type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail or NPCID.EaterofWorldsBody) // EoW should only count once every other EoW is dead
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.whoAmI != self.whoAmI && self.type == NPCID.EaterofWorldsBody)
				{
					return false;
				}
			}

			return true;
		}

		// Only count as downed when there's 1 mossmother left
		if (type == ModContent.NPCType<Mossmother>() && NPC.CountNPCS(ModContent.NPCType<Mossmother>()) > 1)
		{
			return false;
		}

		if (!isBoss)
		{
			return false;
		}

		if (type == NPCID.Spazmatism) // Spazmatism/Retinazer only count if the other is defeated
		{
			return !NPC.AnyNPCs(NPCID.Retinazer);
		}
		else if (type == NPCID.Retinazer)
		{
			return !NPC.AnyNPCs(NPCID.Spazmatism);
		}

		return isBoss;
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

				try
				{
					SkipWoFBox = true;
					DoDeathEventsInfo.Invoke(npc, [Main.player[0]]);
				}
				finally
				{
					SkipWoFBox = false;
				}
			}

			CachedBossesDowned.Clear();
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add(nameof(CachedBossesDowned), (int[])[.. CachedBossesDowned]);
		tag.Add(nameof(TotalBossesDowned), (int[])[.. TotalBossesDowned]);
	}
	public override void LoadWorldData(TagCompound tag)
	{
		CachedBossesDowned.Clear();
		CachedBossesDowned = [.. tag.GetIntArray(nameof(CachedBossesDowned))];
		TotalBossesDowned = [.. tag.GetIntArray(nameof(TotalBossesDowned))];

		// Load legacy data.
		if (tag.ContainsKey("DownedFlags"))
		{
			BitsByte oldMask = tag.GetByte("DownedFlags");

			// These calls are counted as "fromSync" since they don't need to sync - incoming players will have the result synced for them, and
			// at the time this is loaded players can't be joined to the server already
			if (oldMask[0]) { EventTracker.CompleteEvent(EventFlags.DefeatedEaterOfWorlds, fromSync: true); }
			if (oldMask[1]) { EventTracker.CompleteEvent(EventFlags.DefeatedBrainOfCthulhu, fromSync: true); }
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
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
		CachedBossesDowned.Clear();
		TotalBossesDowned.Clear();

		for (int i = 0, count = reader.ReadInt32(); i < count; ++i)
		{
			CachedBossesDowned.Add(reader.ReadInt32());
		}

		for (int i = 0, count = reader.ReadInt32(); i < count; ++i)
		{
			TotalBossesDowned.Add(reader.ReadInt32());
		}
	}

	public static void WriteConsistentInfo(TagCompound tag)
	{
		tag.Add("total", (int[])[.. TotalBossesDowned]);
		tag.Add("cached", (int[])[.. CachedBossesDowned]);
	}
	public static void ReadConsistentInfo(TagCompound tag)
	{
		TotalBossesDowned = [.. tag.GetIntArray("total")];
		CachedBossesDowned = [.. tag.GetIntArray("cached")];
	}

	public class BossTrackerNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.type == NPCID.EaterofWorldsHead && NPC.CountNPCS(NPCID.EaterofWorldsBody) == 0)
			{
				// EoW is dumb and won't register as a boss before death so this is the workaround
				EventTracker.CompleteEvent(EventFlags.DefeatedEaterOfWorlds, gameEventId: GameEventClearedID.DefeatedEaterOfWorldsOrBrainOfChtulu);

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
				EventTracker.CompleteEvent(EventFlags.DefeatedBrainOfCthulhu, gameEventId: GameEventClearedID.DefeatedEaterOfWorldsOrBrainOfChtulu);

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}
	}
}

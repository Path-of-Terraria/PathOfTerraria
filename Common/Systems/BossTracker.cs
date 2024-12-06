using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems;

internal class BossTracker : ModSystem
{
	public static HashSet<int> CachedBossesDowned = [];
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
		if (SubworldSystem.Current is BossDomainSubworld)
		{
			if (self.type == NPCID.WallofFlesh)
			{
				int x = (int)self.Center.X / 16;
				int y = (int)self.Center.Y / 16;
				int width = self.width / 2 / 16 + 1;

				for (int i = x - width; i <= x + width; i++)
				{
					for (int j = y - width; j <= y + width; j++)
					{
						Tile tile = Main.tile[i, j];

						if (i == x - width || i == x + width || j == y - width || j == y + width)
						{
							if (!tile.HasTile)
							{
								tile.TileType = (ushort)(WorldGen.crimson ? 347u : 140u);
								tile.HasTile = true;
							}
						}

						tile.LiquidType = LiquidID.Water;
						tile.LiquidAmount = 0;

						if (Main.netMode == NetmodeID.Server)
						{
							NetMessage.SendTileSquare(-1, i, j);
						}
						else
						{
							WorldGen.SquareTileFrame(i, j);
						}
					}
				}
			}

			self.type = NPCID.None;
			self.boss = false;
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
	}

	public override void LoadWorldData(TagCompound tag)
	{
		DownedFlags = tag.GetByte(nameof(DownedFlags));
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)DownedFlags);
	}

	public override void NetReceive(BinaryReader reader)
	{
		DownedFlags = reader.ReadByte();
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

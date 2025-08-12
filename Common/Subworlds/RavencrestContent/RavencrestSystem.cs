using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.BoCDomain;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Common.Systems.StructureImprovementSystem;
using PathOfTerraria.Common.Systems.VanillaModifications;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Tiles.BossDomain;
using ReLogic.Graphics;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.Subworlds.RavencrestContent;

public class RavencrestSystem : ModSystem
{
	public readonly HashSet<string> HasOverworldNPC = [];

	internal static readonly Dictionary<string, ImprovableStructure> Structures = [];

	public bool SpawnedRaven = false;
	public bool SpawnedScout = false;
	public Point16 EntrancePosition;
	public bool OneTimeCheckDone = false;
	public Point16? SpawnedMorvenPos = null;

	public override void Load()
	{
		Structures.Add("Lodge", new ImprovableStructure(2)
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Lodge_",
			Position = new Point(259, 95),
		});

		Structures.Add("Forge", new ImprovableStructure(2)
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Forge_",
			Position = new Point(195, 109)
		});

		Structures.Add("Burrow", new ImprovableStructure(2)
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Burrow_",
			Position = new Point(800, 129)
		});
    
		Structures.Add("Observatory", new ImprovableStructure(2)
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Observatory_",
			Position = new Point(107, 122)
		});

		Structures.Add("Library", new ImprovableStructure(2)
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Library_",
			Position = new Point(604, 94)
		});

		MiscOverlayUI.DrawOverlay += DrawDistantMorvenDialogue;
	}

	private void DrawDistantMorvenDialogue(SpriteBatch spriteBatch)
	{
		if (!SpawnedMorvenPos.HasValue || !Main.tile[SpawnedMorvenPos.Value].HasTile || Main.GameUpdateCount % 300 > 270)
		{
			return;
		}

		Vector2 position = SpawnedMorvenPos.Value.ToWorldCoordinates() + new Vector2(0, -40);
		float opacity = 1 - MathHelper.Clamp(position.Distance(Main.LocalPlayer.Center) / (225 * 16), 0, 1);

		if (opacity <= 0)
		{
			return;
		}

		DynamicSpriteFont font = FontAssets.MouseText.Value;
		int talkId = (int)(Main.GameUpdateCount % 900 / 300);
		string text = Language.GetTextValue("Mods.PathOfTerraria.NPCs.MorvenNPC.Stuck." + talkId);
		Vector2 size = ChatManager.GetStringSize(font, text, Vector2.One);
		Vector2 bufferSize = size * new Vector2(1, 2f);
		position = Vector2.Clamp(position, Main.screenPosition + bufferSize, Main.screenPosition + Main.ScreenSize.ToVector2() - bufferSize);

		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, position - Main.screenPosition, Color.White * opacity, 0f, size / 2f, new(1.2f));
	}

	public override void PreUpdateTime()
	{
		if (NPC.downedBoss2 && !DisableOrbBreaking.CanBreakOrb)
		{
			DisableOrbBreaking.CanBreakOrb = true;
		}

		if (!OneTimeCheckDone && Main.CurrentFrameFlags.ActivePlayersCount > 0)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				if (SubworldSystem.Current is RavencrestSubworld)
				{
					RavencrestOneTimeChecks();
				}
				else if (SubworldSystem.Current is null)
				{
					OverworldOneTimeChecks();
				}

				ModContent.GetInstance<BoCDomainSystem>().OneTimeCheck();
			}

			TavernManager.OneTimeCheck();

			OneTimeCheckDone = true;
		}
	}

	private void OverworldOneTimeChecks()
	{
		if (NPC.downedBoss1 && SpawnedMorvenPos is null && !WorldGen.crimson && !BossTracker.DownedBrainOfCthulhu)
		{
			const int MaxAttempts = 10000;

			for (int attempt = 0; attempt < MaxAttempts; attempt++)
			{
				int x = Main.rand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);
				int checkY = Main.rand.Next((int)Main.worldSurface, Main.maxTilesY / 2);

				while (checkY < Main.maxTilesY - 10 && !WorldGen.SolidTile(x, checkY))
				{
					checkY++;
				}

				if (checkY >= Main.maxTilesY - 10)
				{
					continue;
				}

				if (!Main.tile[x, checkY].HasTile || Main.tile[x, checkY].TileType != TileID.Ebonstone)
				{
					continue;
				}

				int placeY = checkY - 2;
				if (!WorldGen.InWorld(x, placeY, 15))
				{
					continue;
				}

				if (!WorldGen.EmptyTileCheck(x - 1, x + 1, placeY - 3, placeY, 3))
				{
					continue;
				}

				if (!WorldGen.PlaceObject(x, placeY, ModContent.TileType<MorvenStuck>()))
				{
					continue;
				}

				SpawnedMorvenPos = new Point16(x, placeY);
				break;
			}
			
			if (SpawnedMorvenPos is null)
			{
				Main.NewText($"Reached max attempts trying to spawn Morven. Please report this to the mod developers.");
			}
		}

		int oldMan = NPC.FindFirstNPC(NPCID.OldMan);

		if (oldMan != -1 && NPC.downedBoss3)
		{
			Main.npc[oldMan].Transform(NPCID.Clothier);
		}
	}

	private void RavencrestOneTimeChecks()
	{
		foreach (ImprovableStructure structure in Structures.Values)
		{
			structure.Place();
		}

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			for (int j = 0; j < Main.maxTilesY; ++j)
			{
				WorldGen.TileFrame(i, j, true);
				int sign = Sign.ReadSign(i, j, true);

				if (sign != -1)
				{
					string text = i switch
					{
						< 220 => "the Iron Anvil",
						< 360 => "The Lodge",
						_ => "<-- Lodge, Forge\nLibrary, Hovel -->"
					};

					Sign.TextSign(sign, text);
				}
			}
		}

		GenerationUtilities.ManuallyPopulateChests();

		foreach (string npcName in HasOverworldNPC)
		{
			int type = ModContent.Find<ModNPC>(npcName).Type;

			if (!NPC.AnyNPCs(type))
			{
				var pos = new Point16(Main.spawnTileX, Main.spawnTileY);
				NPC.NewNPC(Entity.GetSource_TownSpawn(), pos.X * 16, pos.Y * 16, type);
			}
		}

		foreach (ISpawnInRavencrestNPC npc in ModContent.GetContent<ISpawnInRavencrestNPC>())
		{
			if (!npc.CanSpawn(false, NPC.AnyNPCs(npc.Type)))
			{
				continue;
			}

			int x = npc.TileSpawn.X * 16;
			int y = npc.TileSpawn.Y * 16;
			NPC.NewNPC(Entity.GetSource_TownSpawn(), x, y, npc.Type);
		}
	}

	public static void UpgradeBuilding(string name, int level = -1)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ModContent.GetInstance<RavencrestBuildingIndex>().Send(name, level);
			return;
		}

		ImprovableStructure structure = Structures[name];
		level = level == -1 ? structure.StructureIndex + 1 : level;
		structure.Change(level);
	}

	public override void PostUpdateEverything()
	{
		if (SubworldSystem.Current is not RavencrestSubworld)
		{
			SpawnedScout = false;
		}
	}

	public override void LoadWorldData(TagCompound tag)
	{
		if (tag.TryGet("morven", out Point16 morven))
		{
			SpawnedMorvenPos = morven;
		}

		SpawnedRaven = tag.GetBool("spawnedRaven");
		EntrancePosition = tag.Get<Point16>("entrance");

		foreach (KeyValuePair<string, ImprovableStructure> structure in Structures)
		{
			if (tag.TryGet(structure.Key + "Name", out byte index))
			{
				structure.Value.Change(index);
			}
		}

		int count = tag.GetShort("overworldNPCCount");

		for (int i = 0; i < count; ++i)
		{
			HasOverworldNPC.Add(tag.GetString("overworldNPC" + i));
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		if (SpawnedMorvenPos is not null)
		{
			tag.Add("morven", SpawnedMorvenPos.Value);
		}

		tag.Add("spawnedRaven", SpawnedRaven);
		tag.Add("entrance", EntrancePosition);

		foreach (KeyValuePair<string, ImprovableStructure> structure in Structures)
		{
			tag.Add(structure.Key + "Name", (byte)structure.Value.StructureIndex);
		}

		int npcNum = 0;
		tag.Add("overworldNPCCount", (short)HasOverworldNPC.Count);

		foreach (string npc in HasOverworldNPC)
		{
			tag.Add("overworldNPC" + npcNum++, npc);
		}
	}

	public override void ClearWorld()
	{
		HasOverworldNPC.Clear();
		OneTimeCheckDone = false;
		SpawnedMorvenPos = null;

		foreach (KeyValuePair<string, ImprovableStructure> item in Structures)
		{
			item.Value.Change(0);
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((short)HasOverworldNPC.Count);

		foreach (string npc in HasOverworldNPC)
		{
			writer.Write(npc);
		}

		writer.WriteVector2(EntrancePosition.ToVector2());
		writer.Write(SpawnedMorvenPos is not null);

		if (SpawnedMorvenPos is not null)
		{
			writer.Write(SpawnedMorvenPos.Value.X);
			writer.Write(SpawnedMorvenPos.Value.Y);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		HasOverworldNPC.Clear();
		short count = reader.ReadInt16();

		for (int i = 0; i < count; ++i)
		{
			HasOverworldNPC.Add(reader.ReadString());
		}

		EntrancePosition = reader.ReadVector2().ToPoint16();

		if (reader.ReadBoolean())
		{
			SpawnedMorvenPos = new Point16(reader.ReadInt16(), reader.ReadInt16());
		}
	}
}

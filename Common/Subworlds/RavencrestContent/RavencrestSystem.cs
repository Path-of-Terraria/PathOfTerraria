using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Subworlds.BossDomains.BoCDomain;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.Systems.StructureImprovementSystem;
using PathOfTerraria.Common.Systems.VanillaModifications;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Content.NPCs.Town;
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

	private static readonly Dictionary<string, ImprovableStructure> structures = [];

	public bool SpawnedRaven = false;
	public bool SpawnedScout = false;
	public Point16 EntrancePosition;
	public bool OneTimeCheckDone = false;
	public Point16? SpawnedMorvenPos = null;

	public override void Load()
	{
		structures.Add("Lodge", new ImprovableStructure(2)
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Lodge_",
			Position = new Point(259, 134),
		});

		structures.Add("Forge", new ImprovableStructure(2)
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Forge_",
			Position = new Point(195, 148)
		});

		structures.Add("Burrow", new ImprovableStructure(2)
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Burrow_",
			Position = new Point(943, 173)
		});
    
		structures.Add("Observatory", new ImprovableStructure(2)
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Observatory_",
			Position = new Point(107, 161)
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

		if (Main.netMode != NetmodeID.MultiplayerClient && !OneTimeCheckDone && Main.CurrentFrameFlags.ActivePlayersCount > 0)
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
			ModContent.GetInstance<TavernManager>().OneTimeCheck();

			OneTimeCheckDone = true;
		}
	}

	private void OverworldOneTimeChecks()
	{
		if (NPC.downedBoss1 && SpawnedMorvenPos is null && !WorldGen.crimson)
		{
			while (true)
			{
				int x = Main.rand.Next(Main.maxTilesX / 5, Main.maxTilesX / 5 * 4);
				int y = Main.rand.Next((int)Main.worldSurface, Main.maxTilesY / 2);
				int checkY = y;

				while (!WorldGen.SolidTile(x, checkY))
				{
					checkY++;
				}

				if (!Main.tile[x, y].HasTile || Main.tile[x, y].TileType != TileID.Ebonstone)
				{
					continue;
				}

				y = checkY - 2;
				WorldGen.PlaceObject(x, y, ModContent.TileType<MorvenStuck>());

				if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<MorvenStuck>())
				{
					SpawnedMorvenPos = new Point16(x, y);
					break;
				}
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
		foreach (ImprovableStructure structure in structures.Values)
		{
			structure.Place();
		}

		foreach (string npcName in HasOverworldNPC)
		{
			int type = ModContent.Find<ModNPC>(npcName).Type;

			if (!NPC.AnyNPCs(type))
			{
				var pos = new Point16(Main.spawnTileX, Main.spawnTileY);
				NPC.NewNPC(Entity.GetSource_TownSpawn(), pos.X * 16, pos.Y * 16, type);
			}
		}
	}

	public static void UpgradeBuilding(string name, int level = -1)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			RavencrestBuildingIndex.Send(name, level);
			return;
		}

		ImprovableStructure structure = structures[name];
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

		foreach (KeyValuePair<string, ImprovableStructure> structure in structures)
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

		foreach (KeyValuePair<string, ImprovableStructure> structure in structures)
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

		foreach (KeyValuePair<string, ImprovableStructure> item in structures)
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
	}
}

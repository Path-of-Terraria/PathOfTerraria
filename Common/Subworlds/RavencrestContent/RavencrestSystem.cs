using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.Systems.StructureImprovementSystem;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.RavencrestContent;

public class RavencrestSystem : ModSystem
{
	private readonly Dictionary<string, ImprovableStructure> structures = [];

	public bool SpawnedScout = false;
	public Point16 EntrancePosition;
	public bool ReplacedBuildings = false;

	public override void OnWorldLoad()
	{
		ReplacedBuildings = false;
	}

	public override void PreUpdateTime()
	{
		if (SubworldSystem.Current is not RavencrestSubworld)
		{
			return;
		}

		if (Main.netMode != NetmodeID.MultiplayerClient && !ReplacedBuildings && Main.CurrentFrameFlags.ActivePlayersCount > 0)
		{
			foreach (ImprovableStructure structure in structures.Values)
			{
				structure.Place();
			}

			ReplacedBuildings = true;

			if (!NPC.AnyNPCs(ModContent.NPCType<GarrickNPC>()) && AnyClassQuestDone())
			{
				// Spawn in the same place as the Hunter
				var hunterNPC = ModContent.GetInstance<HunterNPC>() as ISpawnInRavencrestNPC;
				Point16 pos = hunterNPC.TileSpawn;
				NPC.NewNPC(Entity.GetSource_TownSpawn(), pos.X * 16, pos.Y * 16, ModContent.NPCType<GarrickNPC>());
			}
		}
	}

	private static bool AnyClassQuestDone()
	{
		return ModContent.GetInstance<BlacksmithStartQuest>().Completed || ModContent.GetInstance<HunterStartQuest>().Completed
			|| ModContent.GetInstance<WitchStartQuest>().Completed || ModContent.GetInstance<WizardStartQuest>().Completed;
	}

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
			Position = new Point(673, 182)
		});
	}

	public static void UpgradeBuilding(string name, int level = -1)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			RavencrestBuildingIndex.Send(name, level);
			return;
		}

		ImprovableStructure structure = ModContent.GetInstance<RavencrestSystem>().structures[name];
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
		EntrancePosition = tag.Get<Point16>("entrance");

		foreach (KeyValuePair<string, ImprovableStructure> structure in structures)
		{
			if (tag.TryGet(structure.Key + "Name", out byte index))
			{
				structure.Value.Change(index);
			}
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add("entrance", EntrancePosition);

		foreach (KeyValuePair<string, ImprovableStructure> structure in structures)
		{
			tag.Add(structure.Key + "Name", (byte)structure.Value.StructureIndex);
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.WriteVector2(EntrancePosition.ToVector2());
	}

	public override void NetReceive(BinaryReader reader)
	{
		EntrancePosition = reader.ReadVector2().ToPoint16();
	}
}

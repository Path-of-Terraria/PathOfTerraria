using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.BoCDomain;

internal class BoCDomainSystem : ModSystem
{
	public bool HasLloyd = false;
	public float DomainAtmosphere = 1;
	public Vector2 LLoydReturnPos = Vector2.Zero;

	internal static bool AnyActivePlayerNeedsLloyd()
	{
		foreach (Player player in Main.ActivePlayers)
		{
			if (Quest.PlayerHasQuest<BoCQuest>(player.whoAmI))
			{
				return true;
			}
		}

		return false;
	}

	public override void SaveWorldData(TagCompound tag)
	{
		if (HasLloyd)
		{
			tag.Add("hasLloyd", HasLloyd);
		}

		tag.Add("lloydReturnPosX", LLoydReturnPos.X);
		tag.Add("lloydReturnPosY", LLoydReturnPos.Y);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		HasLloyd = tag.ContainsKey("hasLloyd");
		LLoydReturnPos = new Vector2(tag.GetFloat("lloydReturnPosX"), tag.GetFloat("lloydReturnPosY"));
	}

	public override void PreUpdateTime()
	{
		if (SubworldSystem.Current is BrainDomain domain)
		{
			float strength = NPC.downedBoss2 ? 0.3f : 1;

			if (domain.FightTracker.Completed)
			{
				strength *= 0.5f;
			}

			DomainAtmosphere = MathHelper.Lerp(DomainAtmosphere, strength, 0.02f);
		}
	}

	public override void PreUpdateWorld()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		UpdateOverworldLloyd();
	}

	internal void OneTimeCheck()
	{
		if (SubworldSystem.Current is not null || !HasLloyd)
		{
			return;
		}

		NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (int)LLoydReturnPos.X, (int)LLoydReturnPos.Y, ModContent.NPCType<LloydNPC>());
		HasLloyd = false;
	}

	private void UpdateOverworldLloyd()
	{
		if (SubworldSystem.Current is not null || HasLloyd)
		{
			return;
		}

		bool lloydShouldExist = NPC.downedBoss1 && WorldGen.crimson && (!NPC.downedBoss2 || AnyActivePlayerNeedsLloyd());
		NPC existingLloyd = null;

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.type == ModContent.NPCType<LloydNPC>())
			{
				existingLloyd = npc;
				break;
			}
		}

		bool lloydExists = existingLloyd != null;
		if (lloydExists == lloydShouldExist)
		{
			return;
		}

		if (lloydShouldExist)
		{
			SpawnLloydNearCrimson();
		}
		else
		{
			existingLloyd.active = false;
			existingLloyd.netUpdate = true;
		}
	}

	private void SpawnLloydNearCrimson()
	{
		HashSet<int> types = [TileID.Crimstone, TileID.CrimsonGrass, TileID.Crimsand];

		for (int attempt = 0; attempt < 1000; attempt++)
		{
			int x = Main.rand.Next(Main.maxTilesX / 6, Main.maxTilesX / 6 * 5);
			int y = (int)(Main.worldSurface * 0.35f);

			while (y < Main.maxTilesY - 10 && !WorldGen.SolidTile(x, y))
			{
				y++;
			}

			Tile tile = Main.tile[x, y];

			if (!tile.HasTile || !types.Contains(tile.TileType))
			{
				continue;
			}

			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), x * 16, y * 16, ModContent.NPCType<LloydNPC>());
			break;
		}
	}
}

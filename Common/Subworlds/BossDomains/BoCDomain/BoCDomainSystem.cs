using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.BoCDomain;

internal class BoCDomainSystem : ModSystem
{
	public bool HasLloyd = false;
	public bool DontSpawnLloyd = false;
	public byte LloydAttempts = 0;
	public float DomainAtmosphere = 1;
	public Vector2 LLoydReturnPos = Vector2.Zero;

	public override void SaveWorldData(TagCompound tag)
	{
		if (HasLloyd)
		{
			tag.Add("hasLloyd", HasLloyd);
		}

		if (DontSpawnLloyd)
		{
			tag.Add("spawnLloyd", DontSpawnLloyd);
		}

		tag.Add("lloydReturnPosX", LLoydReturnPos.X);
		tag.Add("lloydReturnPosY", LLoydReturnPos.Y);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		HasLloyd = tag.ContainsKey("hasLloyd");
		DontSpawnLloyd = tag.ContainsKey("spawnLloyd");
	}

	public override void PreUpdateTime()
	{
		if (SubworldSystem.Current is BrainDomain domain)
		{
			float strength = NPC.downedBoss2 ? 0.3f : 1;

			if (domain.BossSpawned && !NPC.AnyNPCs(NPCID.BrainofCthulhu))
			{
				strength *= 0.5f;
			}

			DomainAtmosphere = MathHelper.Lerp(DomainAtmosphere, strength, 0.02f);
		}
	}

	internal void OneTimeCheck()
	{
		if (SubworldSystem.Current is null && NPC.downedBoss1 && !DontSpawnLloyd && WorldGen.crimson && !NPC.downedBoss2)
		{
			HashSet<int> types = [TileID.Crimstone, TileID.CrimsonGrass, TileID.Crimsand];

			while (true)
			{
				int x = Main.rand.Next(Main.maxTilesX / 6, Main.maxTilesX / 6 * 5);
				int y = (int)(Main.worldSurface * 0.35f);

				while (!WorldGen.SolidTile(x, y))
				{
					y++;
				}

				Tile tile = Main.tile[x, y];

				if (!tile.HasTile || !types.Contains(tile.TileType))
				{
					continue;
				}

				NPC.NewNPC(Entity.GetSource_NaturalSpawn(), x * 16, y * 16, ModContent.NPCType<LloydNPC>());
				DontSpawnLloyd = true;
				break;
			}
		}

		if (SubworldSystem.Current is null && !SubworldSystem.IsActive<BrainDomain>())
		{
			if (HasLloyd)
			{
				NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (int)LLoydReturnPos.X, (int)LLoydReturnPos.Y, ModContent.NPCType<LloydNPC>());
				HasLloyd = false;
			}
		}
	}
}

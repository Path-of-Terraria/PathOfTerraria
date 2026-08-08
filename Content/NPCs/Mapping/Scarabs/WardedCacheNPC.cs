using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Systems.Scarabs;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Scarabs;

internal sealed class WardedCacheNPC : ModNPC
{
	private ref float Power => ref NPC.ai[0];
	private ref float State => ref NPC.ai[1];
	private ref float WavesSpawned => ref NPC.ai[2];
	private ref float Timer => ref NPC.ai[3];

	public override string Texture => $"Terraria/Images/NPC_{NPCID.DD2EterniaCrystal}";

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.DD2EterniaCrystal];
	}

	public override void SetDefaults()
	{
		NPC.width = 48;
		NPC.height = 80;
		NPC.lifeMax = 1000;
		NPC.damage = 0;
		NPC.defense = 999;
		NPC.dontTakeDamage = true;
		NPC.friendly = true;
		NPC.immortal = true;
		NPC.aiStyle = -1;
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		NPC.netAlways = true;
		NPC.GetGlobalNPC<NPCDespawning>().NeverDespawn = true;
	}

	public override void AI()
	{
		NPC.velocity.X = 0f;
		Lighting.AddLight(NPC.Center, new Vector3(0.85f, 0.55f, 0.15f));
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		if (State == 0f)
		{
			if (AnyPlayerNearby())
			{
				State = 1f;
				Timer = 30f;
				NPC.netUpdate = true;
			}
			return;
		}

		if (ScarabEncounterSystem.HasLivingChildren(NPC.whoAmI) || Timer-- > 0f)
		{
			return;
		}

		int totalWaves = Math.Clamp((int)Power + 1, 2, 4);
		if (WavesSpawned < totalWaves)
		{
			WavesSpawned++;
			ScarabEncounterSystem.SpawnElitePack(NPC.Center, 2 + (int)Power, Math.Max(1, (int)Power - 1), NPC.whoAmI);
			Timer = 60f;
			NPC.netUpdate = true;
			return;
		}

		DropRewards();
		ScarabEncounterSystem.MarkCacheCompleted();
		NPC.active = false;
		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
		}
	}

	private bool AnyPlayerNearby()
	{
		foreach (Player player in Main.ActivePlayers)
		{
			if (!player.dead && player.DistanceSQ(NPC.Center) < 520f * 520f) { return true; }
		}
		return false;
	}

	private void DropRewards()
	{
		using (SmartLoot.Begin())
		{
			foreach (ItemDatabase.ItemRecord record in MapChestLoot.RollMobDrops(Math.Clamp((int)Power + 1, 2, 4)))
			{
				if (record != ItemDatabase.InvalidItem)
				{
					Item.NewItem(NPC.GetSource_Death(), NPC.Center, MapChestLoot.BuildChestItem(record));
				}
			}
		}

		if (Main.rand.NextFloat() < 0.15f)
		{
			int scarab = ScarabCatalog.RollDropType(Common.Subworlds.MappingWorld.MapTier, allowUnique: false);
			if (scarab > 0) { Item.NewItem(NPC.GetSource_Death(), NPC.Center, scarab); }
		}
	}
}

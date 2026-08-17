using PathOfTerraria.Common.Systems.Sigils;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Content.Buffs.ShrineBuffs;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Sigils;

internal sealed class ConsecratedShrineNPC : ModNPC
{
	private ref float Power => ref NPC.ai[0];
	private ref float State => ref NPC.ai[1];
	private ref float Timer => ref NPC.ai[2];

	public override string Texture => $"Terraria/Images/NPC_{NPCID.DD2EterniaCrystal}";

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.DD2EterniaCrystal];
	}

	public override void SetDefaults()
	{
		NPC.width = 42;
		NPC.height = 72;
		NPC.lifeMax = 1000;
		NPC.damage = 0;
		NPC.defense = 999;
		NPC.dontTakeDamage = true;
		NPC.friendly = true;
		NPC.immortal = true;
		NPC.aiStyle = -1;
		NPC.netAlways = true;
		NPC.GetGlobalNPC<NPCDespawning>().NeverDespawn = true;
	}

	public override void AI()
	{
		NPC.velocity.X = 0f;
		Lighting.AddLight(NPC.Center, new Vector3(0.75f, 0.72f, 0.35f));
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		if (State == 0f && AnyPlayerNearby())
		{
			State = 1f;
			Timer = 45f;
			SigilEncounterSystem.SpawnElitePack(NPC.Center, 3 + (int)Power, Math.Max(1, (int)Power - 1), NPC.whoAmI);
			NPC.netUpdate = true;
			return;
		}

		if (State != 1f || Timer-- > 0f || SigilEncounterSystem.HasLivingChildren(NPC.whoAmI))
		{
			return;
		}

		int[] buffs =
		[
			ModContent.BuffType<GodlikeBuff>(),
			ModContent.BuffType<HardinessBuff>(),
			ModContent.BuffType<HasteBuff>(),
			ModContent.BuffType<RestorationBuff>(),
			ModContent.BuffType<UnstoppableBuff>(),
		];
		int duration = Power switch
		{
			2 => 25 * 60,
			>= 3 => 30 * 60,
			_ => 20 * 60,
		};
		int buff = Main.rand.Next(buffs);
		foreach (Player player in Main.ActivePlayers)
		{
			if (!player.dead && player.DistanceSQ(NPC.Center) < 900f * 900f)
			{
				player.AddBuff(buff, duration);
			}
		}

		SigilEncounterSystem.MarkShrineCompleted();
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
}

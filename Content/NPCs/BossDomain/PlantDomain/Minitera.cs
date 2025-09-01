using NPCUtils;
using PathOfTerraria.Common;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using SubworldLibrary;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.PlantDomain;

internal class Minitera : ModNPC
{
	private ref float Timer => ref NPC.ai[0];
	private ref float SpawningTimer => ref NPC.ai[2];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 4;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Plantera);
		NPC.lifeMax = NPC.life = 3000;
		NPC.Size = new Vector2(60);
		NPC.defense /= 2;
		NPC.aiStyle = -1;
		NPC.value = Item.buyPrice(0, 2, 0, 0);
		NPC.boss = false;
		NPC.Opacity = 0.5f;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Plantera_Pink, 4));
			c.AddDust(new(DustID.Plantera_Pink, 15, NPCHitEffects.OnDeath));

			for (int i = 0; i < 6; ++i)
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_" + i, 1, NPCHitEffects.OnDeath));
			}
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Jungle");
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return SpawningTimer > 120;
	}

	public override void AI()
	{
		NPC.TargetClosest();
		SpawningTimer++;

		if (SpawningTimer <= 120)
		{
			NPC.Opacity = SpawningTimer / 120f * 0.5f + 0.5f;
			NPC.scale = SpawningTimer / 120f * 0.5f + 0.5f;
			NPC.dontTakeDamage = true;
			return;
		}

		NPC.dontTakeDamage = false;

		if (NPC.ai[1] == 0)
		{
			NPC.ai[1] = 1;

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int i = 0; i < 3; ++i)
				{
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<MiniteraHook>(), 0, NPC.whoAmI);
				}
			}
		}

		Player player = Main.player[NPC.target];

		Timer++;
		NPC.velocity = NPC.SafeDirectionTo(player.Center) * (MathF.Sin(Timer * 0.04f) * 1 + 2.5f);

		if (SpawningTimer < 180f)
		{
			NPC.velocity *= (SpawningTimer - 120f) / 60f;
		}

		NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

		if (Timer % 100 == 0 && Main.netMode != NetmodeID.MultiplayerClient && SubworldSystem.Current is not MoonLordDomain)
		{
			for (int i = 0; i < 3; ++i)
			{
				Vector2 velocity = NPC.DirectionTo(player.Center) * 12 * WorldGen.genRand.NextFloat(0.6f, 1.1f);
				int damage = ModeUtils.ProjectileDamage(40, 60, 90, 120);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity.RotatedByRandom(0.3f), ProjectileID.SeedPlantera, damage, 0, Main.myPlayer);
			}
		}
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter++;
		NPC.frame.Y = (int)(NPC.frameCounter * 0.1f % 4) * frameHeight;

		if (NPC.IsABestiaryIconDummy)
		{
			NPC.Opacity = 1f;
		}
	}
}

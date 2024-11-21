using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.DeerDomain;

public sealed class SkullApparition : ModNPC
{
	private ref float Timer => ref NPC.ai[0];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 1;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Crimera);
		NPC.aiStyle = -1;
		NPC.dontTakeDamage = true;
		NPC.Size = new Vector2(168, 172);
		NPC.damage = 60;
		NPC.Opacity = 0;
		NPC.noTileCollide = true;
		NPC.boss = true;

		NPC.TryEnableComponent<NPCHitEffects>(c => 
		{
			c.AddDust(new(DustID.Blood, 4));
			c.AddDust(new(DustID.Blood, 15, NPCHitEffects.OnDeath));
		});

		Music = MusicID.Eerie;
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return Timer > 240;
	}

	public override void AI()
	{
		Timer++;

		if (Main.rand.NextBool(8 + (int)(100 * (1 - NPC.Opacity))))
		{
			int dust = Dust.NewDust(NPC.position + new Vector2(56, 86), 48, 86, DustID.PortalBoltTrail, 0, 0, Main.rand.Next(50, 150), Scale: 2);

			Main.dust[dust].noGravity = true;
		}

		if (Timer <= 240)
		{
			NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0.25f, 0.05f);
			NPC.velocity *= 0.98f;
			return;
		}

		NPC.TargetClosest(true);
		NPC.rotation = NPC.velocity.X * 0.02f;

		bool fadeIn = true;

		if (NPC.HasValidTarget)
		{
			NPC.velocity += NPC.DirectionTo(Main.player[NPC.target].Center) * 0.2f;

			if (Main.player[NPC.target].ZoneOverworldHeight && PlayerInTheOpen(Main.player[NPC.target]))
			{
				NPC.Opacity *= 0.95f;
				fadeIn = false;

				if (NPC.Opacity < 0.075f)
				{
					NPC.active = false;

					for (int i = 0; i < 30; ++i)
					{
						Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Phantasmal, 0, 0, 150);
					}
				}
			}
		}

		if (fadeIn)
		{
			NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 1f, 0.05f);
		}

		if (NPC.velocity.LengthSquared() > 9 * 9)
		{
			NPC.velocity = Vector2.Normalize(NPC.velocity) * 9;
		}
	}

	private bool PlayerInTheOpen(Player player)
	{
		Point pos = player.Top.ToTileCoordinates();
		Point dif = pos;

		while (!WorldGen.SolidOrSlopedTile(dif.X, dif.Y) && WorldGen.InWorld(dif.X, dif.Y, 10))
		{
			dif.Y--;
		}

		return pos.Y - dif.Y > 50;
	}

	public override Color? GetAlpha(Color drawColor)
	{
		return Color.White with { A = 150 };
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;

		for (int i = 7; i >= 0; --i)
		{
			Color color = Color.White with { A = 150 } * NPC.Opacity;
			float sine = MathF.Sin(i + Main.GameUpdateCount * 0.03f);
			Vector2 pos = NPC.Center - screenPos + new Vector2(i * 1.7f, 0).RotatedBy(sine * MathHelper.Pi
				* (i % 2 == 0 ? -1 : 1));

			Main.EntitySpriteDraw(tex, pos, null, color * (i / 15f), NPC.rotation, tex.Size() / 2f, 1f, SpriteEffects.None);
		}

		return false;
	}
}
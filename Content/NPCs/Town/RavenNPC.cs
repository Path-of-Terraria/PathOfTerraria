using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Town;

public sealed class RavenNPC : ModNPC
{
	private readonly Vector2[] _offsets = new Vector2[3];
	private readonly Vector2[] _targets = new Vector2[3];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 5;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Bird);
		NPC.noTileCollide = true;
		NPC.width = 74;
		NPC.width = 74;
		NPC.dontTakeDamage = true;
		NPC.dontTakeDamageFromHostiles = true;
		NPC.netAlways = true;
		NPC.netUpdate2 = true;
		NPC.Opacity = 0f;
		NPC.aiStyle = -1;
		NPC.noGravity = true;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Ghost, 2));
			c.AddDust(new(DustID.Ghost, 6, NPCHitEffects.OnDeath));
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void AI()
	{
		NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0.45f, 0.05f);
		NPC.TargetClosest(true);

		Player target = Main.player[NPC.target];
		Vector2 entrancePosition = ModContent.GetInstance<RavencrestSystem>().EntrancePosition.ToVector2() * 16;
		bool nearEntrance = entrancePosition.DistanceSQ(NPC.Center) < 600 * 600;
		int dir = Math.Sign(entrancePosition.X - NPC.Center.X);

		if (!nearEntrance && (target.dead || !target.active || target.DistanceSQ(NPC.Center) > 500 * 500 
			|| Math.Abs(NPC.Center.X - entrancePosition.X) < Math.Abs(target.Center.X - entrancePosition.X + dir * 200)))
		{
			int tileX = (int)(NPC.Center.X / 16f);
			int tileY = (int)(NPC.Center.Y / 16f);

			while (!WorldGen.SolidOrSlopedTile(tileX, tileY))
			{
				tileY++;

				if (!WorldGen.InWorld(tileX, tileY, 20))
				{
					break;
				}
			}

			if (tileY - NPC.Center.Y / 16f > 15)
			{
				NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 6f, 0.1f);
			}
			else
			{
				SlowDown();
			}
		}
		else
		{
			NPC.spriteDirection = dir;
			Vector2 targetPosition = target.Center + new Vector2(dir * 180, -120);

			if (nearEntrance)
			{
				targetPosition = entrancePosition + new Vector2(60, 200);
			}

			if (targetPosition.DistanceSQ(NPC.Center) > 30 * 30)
			{
				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(targetPosition) * (nearEntrance ? 6 : 9), 0.2f);
			}
			else
			{
				SlowDown();
			}
		}

		if (NPC.velocity.LengthSquared() == 0)
		{
			NPC.spriteDirection = NPC.direction;
		}

		if (Main.rand.NextBool(800) && Main.netMode != NetmodeID.Server)
		{
			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/RavenCaw"), NPC.Center);
		}
	}

	private void SlowDown()
	{
		NPC.velocity *= 0.85f;

		if (NPC.velocity.LengthSquared() < 0.05f * 0.05f)
		{
			NPC.velocity *= 0;
		}
	}

	public override void FindFrame(int frameHeight)
	{
		if (NPC.velocity.LengthSquared() > 0 || NPC.IsABestiaryIconDummy)
		{
			NPC.frameCounter += 0.15f;
			NPC.frame.Y = frameHeight * (int)(NPC.frameCounter % 4);
		}
		else
		{
			NPC.frame.Y = frameHeight * 4;
		}
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override bool NeedSaving()
	{
		return true;
	}

	public override Color? GetAlpha(Color drawColor)
	{
		return Color.Lerp(drawColor, Color.White, 0.3f);
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		Vector2 pos = NPC.Center - screenPos;
		Rectangle frame = NPC.frame with { Width = 80 };
		SpriteEffects effect = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		for (int i = 0; i < _offsets.Length; ++i)
		{
			_offsets[i] = Vector2.Lerp(_offsets[i], _targets[i], 0.35f);

			if (_offsets[i].DistanceSQ(_targets[i]) < 0.5f * 0.5f)
			{
				_targets[i] = Main.rand.NextVector2CircularEdge(9, 9);
			}

			Vector2 drawPos = pos + _offsets[i];
			Main.EntitySpriteDraw(tex, drawPos, frame, drawColor * NPC.Opacity * 0.5f, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect, 0);
		}

		Main.EntitySpriteDraw(tex, pos, frame, drawColor * NPC.Opacity, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect, 0);
		Main.EntitySpriteDraw(tex, pos, frame with { X = 82 }, drawColor, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect, 0);
		return false;
	}
}
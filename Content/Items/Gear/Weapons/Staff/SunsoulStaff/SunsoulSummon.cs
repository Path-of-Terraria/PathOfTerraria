using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.World.Generation;
using System.Collections.Generic;
using System.IO;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff.SunsoulStaff;

public class SunsoulSummon : ModProjectile
{
	private Player Owner => Main.player[Projectile.owner];

	private ref float Factor => ref Projectile.ai[0];
	private ref float BezierSlot => ref Projectile.ai[1];

	private bool Moving
	{
		get => Projectile.ai[2] == 1;
		set => Projectile.ai[2] = value ? 1 : 0;
	}

	private ref float Speed => ref Projectile.localAI[0];

	private Vector2[] bezier = null;

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(60);
		Projectile.tileCollide = false;
		Projectile.timeLeft = 2;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 12;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.penetrate = -1;
		Projectile.Opacity = 0f;
		Projectile.hide = true;
	}

	public override void DrawBehind(int index, List<int> bNaT, List<int> bN, List<int> bP, List<int> overPlayers, List<int> oWUI)
	{
		overPlayers.Add(index);
	}

	public override void AI()
	{
		if (!Owner.GetModPlayer<AltUsePlayer>().AltFunctionActive)
		{
			Projectile.Kill();
			return;
		}

		if (Main.rand.NextBool())
		{
			Vector2 dVel = Projectile.velocity;
			Vector2 dPos = Projectile.position + new Vector2(12);
			Dust.NewDust(dPos, Projectile.width - 24, Projectile.height - 24, DustID.Torch, dVel.X, dVel.Y, Scale: Main.rand.NextFloat(1, 2));
		}

		int timeLeft = Owner.GetModPlayer<AltUsePlayer>().AltFunctionActiveTimer;

		if (timeLeft < 30)
		{
			Projectile.Opacity = timeLeft / 30f;
		}
		else
		{
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1, 0.1f);
		}

		Lighting.AddLight(Projectile.Center, TorchID.Orange);

		Projectile.timeLeft++;
		Projectile.rotation += 0.05f;

		if (Moving)
		{
			Projectile.rotation += Speed;
			Factor += Speed;

			while (Factor > 1)
			{
				BezierSlot++;
				Factor--;
			}

			if (BezierSlot >= bezier.Length)
			{
				Moving = false;
				bezier = [];
				Projectile.netUpdate = true;
			}
			else
			{
				int slot = (int)BezierSlot;
				Vector2 pos = slot == bezier.Length - 1 ? bezier[^1] : Vector2.Lerp(bezier[slot], bezier[slot + 1], Factor);
				Projectile.Center = Owner.Center + pos;
			}
		}

		if (Main.myPlayer != Projectile.owner)
		{
			return;
		}

		if (!Moving)
		{
			Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center + Owner.DirectionTo(Main.MouseWorld) * 30, 0.2f);

			if (Main.mouseLeft)
			{
				Moving = true;
				Factor = 0;
				BezierSlot = 0;

				Vector2 target = Main.MouseWorld - Projectile.Center;
				Vector2 offset = Vector2.Normalize(target).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-120, 120);
				var midPoint = Vector2.Lerp(Vector2.Zero, target, 0.5f);

				bezier = Spline.InterpolateXY([Vector2.Zero, midPoint + offset, target, midPoint + offset.RotatedBy(MathHelper.Pi), Vector2.Zero], 16);
				Speed = Math.Max(0.08f, Math.Abs(100 / target.Length() + 0.25f));

				Projectile.netUpdate = true;
			}
		}

		return;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write((Half)Speed);
		writer.Write(bezier is null);

		if (bezier is null)
		{
			return;
		}

		writer.Write((byte)bezier.Length);

		foreach (Vector2 pos in bezier)
		{
			writer.WriteVector2(pos);
		}
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		Speed = (float)reader.ReadHalf();

		if (reader.ReadBoolean())
		{
			return;
		}

		int count = reader.ReadByte();
		bezier = new Vector2[count];

		for (int i = 0; i < count; ++i)
		{
			bezier[i] = reader.ReadVector2();
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Owner.MinionAttackTargetNPC = target.whoAmI;
		Owner.UpdateMinionTarget();
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
		if (!Moving)
		{
			modifiers.FinalDamage *= 0.5f;
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;

		for (int k = 0; k < Projectile.oldPos.Length; k++)
		{
			Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + tex.Size() / 2f + new Vector2(0f, Projectile.gfxOffY);
			Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
			DrawSelf(color, tex, drawPos);
		}

		Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY);
		DrawSelf(lightColor, tex, position);

		return false;
	}

	private void DrawSelf(Color lightColor, Texture2D tex, Vector2 position)
	{
		SpriteEffects effect = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
			float scale = 1.4f - i * 0.3f;
			Color color = Projectile.GetAlpha(lightColor) * (0.1f + i * 0.3f) * Projectile.Opacity;
			Main.EntitySpriteDraw(tex, position, null, color, rotation, tex.Size() / 2f, scale * Projectile.scale, effect, 0);
		}
	}
}

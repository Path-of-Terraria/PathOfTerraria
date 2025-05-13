using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;

public sealed class Lightray : ModProjectile
{
	public const int LifeTime = 60;

	public ref float Timer => ref Projectile.ai[0];
	public ref float FloorY => ref Projectile.ai[1];

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.tileCollide = false;

		Projectile.width = 80;
		Projectile.height = 80;
		Projectile.timeLeft = LifeTime;
		Projectile.hide = true;
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		overPlayers.Add(index);
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		return false;
	}

	public override void AI()
	{
		Timer++;
		Projectile.scale = MathF.Min(1, Timer / 30f);
		Projectile.Opacity = Projectile.scale;

		if (Timer > LifeTime - 10)
		{
			Projectile.Opacity = 1 - (Timer - (LifeTime - 10)) / 10f;
		}

		if (Projectile.timeLeft == 5 && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(3) && NPC.CountNPCS(ModContent.NPCType<WormLightning>()) < 3)
		{
			NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)FloorY, ModContent.NPCType<WormLightning>());
		}
	}

	public override bool CanHitPlayer(Player target)
	{
		return Timer > 30;
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		return Math.Abs(targetHitbox.Left - projHitbox.Center.X) < 18 || Math.Abs(targetHitbox.Right - projHitbox.Center.X) < 18;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);
		Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
		Vector2 origin = frame.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);
		SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		Color color = Color.Yellow with { B = 222 } * 0.7f;
		var scale = new Vector2(Projectile.scale, 15000);

		Main.EntitySpriteDraw(texture, position, frame, Projectile.GetAlpha(color), Projectile.rotation, origin, scale, effects);

		for (int i = 0; i < 5; ++i)
		{
			Vector2 drawPos = position + new Vector2(Main.rand.NextFloat(-5, 5), 0);
			Main.EntitySpriteDraw(texture, drawPos, frame, Projectile.GetAlpha(color) * 0.4f, Projectile.rotation, origin, scale, effects);
		}

		return false;
	}
}
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

public sealed class SunspotAura : ModProjectile
{
	public const int LifeTime = 60 * 8;

	/// <summary>
	///		Gets or sets the index of the <see cref="Player"/> instance the projectile is homing towards. Shorthand for <c>Projectile.ai[1]</c>.
	/// </summary>
	public ref float Index => ref Projectile.ai[1];

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.tileCollide = false;

		Projectile.Size = new Vector2(434);
		Projectile.timeLeft = LifeTime;
		Projectile.Opacity = 0.3f;
		Projectile.hide = true;
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		overPlayers.Add(index);
	}

	public override bool CanHitPlayer(Player target)
	{
		return false;
	}

	public override void AI()
	{
		Projectile.rotation += 0.01f;
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.Black * Projectile.Opacity;
	}

	//public override bool PreDraw(ref Color lightColor)
	//{
	//	lightColor = new Color(235, 97, 52, 0);

	//	DrawProjectile(in lightColor);

	//	return false;
	//}

	//private void DrawProjectile(in Color lightColor)
	//{
	//	Texture2D texture = TextureAssets.Projectile[Type].Value;
	//	Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);
	//	Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
	//	Vector2 origin = frame.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);
	//	SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

	//	Main.EntitySpriteDraw(texture, position, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effects);
	//}
}
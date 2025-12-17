using PathOfTerraria.Common.Systems;
using ReLogic.Content;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

// Wanted to do a fancy shader for this but it took too long, whoops! - Gabe
internal class ColdAura : ModProjectile
{
	public const int MaxTimeLeft = 120;

	private bool Initialized
	{
		get => Projectile.ai[0] == 1f;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	public Vector2 ShaderPosition
	{
		get => new(Projectile.ai[1], Projectile.ai[2]);
		set => (Projectile.ai[1], Projectile.ai[2]) = (value.X, value.Y);
	}

	public Vector2 TargetPosition
	{
		get => new(Projectile.localAI[0], Projectile.localAI[1]);
		set => (Projectile.localAI[0], Projectile.localAI[1]) = (value.X, value.Y);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.timeLeft = MaxTimeLeft;
		Projectile.Opacity = 1f;
		Projectile.Size = new Vector2(PoTMod.NearbyDistance);
		Projectile.tileCollide = false;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		if (!Initialized)
		{
			Initialized = true;
			ShaderPosition = new Vector2(Main.rand.Next(200), Main.rand.Next(200));
		}

		Projectile.Opacity = MathF.Min(Projectile.timeLeft / 30f, 1f) * 0.25f;

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (Projectile.DistanceSQ(npc.Center) < PoTMod.NearbyDistanceSq / 4f && npc.CanBeChasedBy())
			{
				npc.GetGlobalNPC<SlowDownNPC>().AddSlowDown(0.33f, true);
			}
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return lightColor with { A = 0 } * Projectile.Opacity;
	}

	//public override bool PreDraw(ref Color lightColor)
	//{
	//	var topLeft = Vector2.Lerp(ShaderPosition, TargetPosition, Projectile.timeLeft / (float)MaxTimeLeft);
	//	ColdAuraBatching.Auras.Enqueue(new ColdAuraBatching.Aura(Projectile.Center, topLeft, Vector2.Zero));
	//	return false;
	//}
}

/// <summary>
/// Handles batching <see cref="ColdAura"/> draw calls to save a bit of performance. Unused so I don't burn too much paid time. - Gabe
/// </summary>
internal class ColdAuraBatching : ModSystem
{
	public readonly record struct Aura(Vector2 Position, Vector2 TopLeft, Vector2 Size);

	public static Queue<Aura> Auras = new();

	public static Asset<Texture2D> Perlin;
	public static Asset<Texture2D> Mask;
	private static Asset<Effect> AuraEffect;

	public override void Load()
	{
		On_Main.DrawProjectiles += Draw;

		Perlin = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/PerlinNoise");
		Mask = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/AuraCover");
		AuraEffect = ModContent.Request<Effect>("PathOfTerraria/Assets/Effects/ColdAura");
	}

	private void Draw(On_Main.orig_DrawProjectiles orig, Main self)
	{
		orig(self);

		Draw(Auras);
	}

	private static void Draw(Queue<Aura> auras)
	{
		if (auras.Count <= 0)
		{
			return;
		}

		Texture2D texture = Perlin.Value;
		Matrix trans = Main.GameViewMatrix.TransformationMatrix;
		Effect effect = AuraEffect.Value;
		effect.Parameters["noiseTexture"].SetValue(Mask.Value);
		effect.Parameters["sampleSize"].SetValue(new Vector2(150) / texture.Size());
		effect.Parameters["noiseSize"].SetValue(Mask.Size());
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, effect, trans);

		while (auras.Count > 0)
		{
			Aura aura = auras.Dequeue();
			Vector2 position = aura.Position - Main.screenPosition;
			Rectangle src = new((int)aura.TopLeft.X, (int)aura.TopLeft.Y, 150, 150);
			
			Main.EntitySpriteDraw(texture, position, src, Color.White with { A = 0 }, 0, texture.Size() / 2f, 1f, SpriteEffects.None);
		}

		Main.spriteBatch.End();
	}
}
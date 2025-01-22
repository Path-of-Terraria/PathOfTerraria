using ReLogic.Content;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class RunestoneBurst : ModProjectile
{
	public override string Texture => "Terraria/Images/Projectile_0";

	private ref float Timer => ref Projectile.ai[0];
	private ref float DecaySpeed => ref Projectile.ai[1];

	public override void SetDefaults()
	{
		Projectile.Size = Vector2.Zero;
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.aiStyle = -1;
	}

	public override void AI()
	{
		if (DecaySpeed == 0)
		{
			DecaySpeed = 0.1f;
		}

		Timer += DecaySpeed;
		DecaySpeed *= 0.94f;

		if (DecaySpeed <= 0.001f)
		{
			Projectile.Kill();
			return;
		}

		Projectile.timeLeft--;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Vector2 position = Projectile.position - Main.screenPosition;
		float timer = Timer * 300f;
		var topLeft = new Vector3(position - new Vector2(timer), 0);
		Color color = Color.White;

		short[] indices = [0, 1, 2, 1, 3, 2];

		VertexPositionColorTexture[] vertices =
		[
			new(topLeft, color, new Vector2(0, 0)),
			new(topLeft + new Vector3(new Vector2(timer * 2, 0), 0), color, new Vector2(1, 0)),
			new(topLeft + new Vector3(new Vector2(0, timer * 2), 0), color, new Vector2(0, 1)),
			new(topLeft + new Vector3(new Vector2(timer * 2), 0), color, new Vector2(1, 1)),
		];

		Effect effect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/RunestoneRing", AssetRequestMode.ImmediateLoad).Value;
		var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
		Matrix view = Main.GameViewMatrix.TransformationMatrix;
		Matrix renderMatrix = view * projection;
		float opacity = MathF.Min(DecaySpeed / 0.05f, 1);
		Vector4 drawColor = Vector4.Lerp(new Vector4(65, 131, 224, 255), new Vector4(60, 50, 24, 255), 1 - opacity) / 255f;

		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			effect.Parameters["baseColor"].SetValue(drawColor * 0.9f * opacity);
			effect.Parameters["width"].SetValue(40 / MathF.Pow(timer * 2f, 8f));
			effect.Parameters["uWorldViewProjection"].SetValue(renderMatrix);
			pass.Apply();

			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
		}

		return false;
	}
}

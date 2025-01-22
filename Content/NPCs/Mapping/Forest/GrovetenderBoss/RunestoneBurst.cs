using ReLogic.Content;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class RunestoneBurst : ModProjectile
{
	public override string Texture => "Terraria/Images/Projectile_0";

	private ref float Timer => ref Projectile.ai[0];

	public override void SetDefaults()
	{
		Projectile.Size = Vector2.Zero;
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.aiStyle = -1;
	}

	public override void AI()
	{
		Timer++;

		Timer = 30;

		if (Timer > 60)
		{
			Projectile.Kill();
			return;
		}

		Projectile.timeLeft--;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Vector2 position = Projectile.position;
		float timer = Timer * 0.2f;

		Vector3 topLeft = new Vector3(position - new Vector2(timer), 0);
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

		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			effect.Parameters["baseColor"].SetValue(Color.White.ToVector4() * 0.54f);
			effect.Parameters["uWorldViewProjection"].SetValue(renderMatrix);
			pass.Apply();

			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
		}

		return false;
	}
}

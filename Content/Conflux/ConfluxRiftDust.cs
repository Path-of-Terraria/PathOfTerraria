namespace PathOfTerraria.Content.Dusts;

public sealed class ConfluxRiftSmoke : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.noGravity = true;
		dust.noLight = true;
		dust.rotation = Main.rand.NextFloat(6.28f);
	}

	public override bool Update(Dust dust)
	{
		dust.frame = new Rectangle(0, 0, 34, 36);
		dust.position += dust.velocity;
		dust.rotation += dust.velocity.X * 0.05f;
		dust.velocity *= 0.96f;
		dust.scale *= 1.012f;
		//dust.color *= 1.02f;

		dust.alpha += 1;
		if (dust.alpha > 255)
		{
			dust.active = false;
		}

		return false;
	}

	public override Color? GetAlpha(Dust dust, Color lightColor)
	{
		return (dust.color == default ? Color.White : dust.color) * (1f - (float)(dust.alpha / 255.0f));
	}
}
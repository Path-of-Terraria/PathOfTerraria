namespace PathOfTerraria.Content.Dusts;

internal class BrightVenomDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.frame = new Rectangle(0, 10 * Main.rand.Next(3), 8, 8);
		dust.scale *= 0.4f;
	}

	public override Color? GetAlpha(Dust dust, Color lightColor)
	{
		return Color.White;
	}

	public override bool Update(Dust dust)
	{
		dust.position += dust.velocity;
		dust.velocity *= 0.9f;
		dust.scale -= 0.02f;

		if (dust.scale < 0.05f)
		{
			dust.active = false;
		}
		
		return false;
	}
}

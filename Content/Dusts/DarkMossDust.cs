namespace PathOfTerraria.Content.Dusts;

internal class DarkMossDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.frame = new Rectangle(0, 10 * Main.rand.Next(3), 8, 8);
	}

	public override bool Update(Dust dust)
	{
		dust.position += dust.velocity;
		dust.velocity *= 0.9f;
		dust.scale -= 0.01f;

		if (dust.scale < 0.05f)
		{
			dust.active = false;
		}

		return false;
	}
}

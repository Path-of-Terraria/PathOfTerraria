namespace PathOfTerraria.Content.NPCs.Mapping.Forest;

public class EntDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.noGravity = true;
		dust.frame = new Rectangle(0, 8 * Main.rand.Next(3), 6, 6);
	}

	public override bool Update(Dust dust)
	{
		dust.velocity *= 0.99f;
		dust.alpha += 2;
		dust.rotation += 0.2f;

		if (dust.alpha >= 254)
		{
			dust.active = false;
		}

		dust.position += dust.velocity;

		if (!dust.noLight)
		{
			Lighting.AddLight(dust.position, new Vector3(251, 242, 54) / 400f * (1 - dust.alpha / 255f));
		}

		return false;
	}
}
namespace PathOfTerraria.Content.Dusts;

public sealed class SparkleDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.noGravity = true;
		dust.noLight = true;
		
		dust.scale *= 1.5f;
		dust.velocity *= 0.4f;
	}

	public override bool Update(Dust dust)
	{
		dust.position += dust.velocity;
		dust.rotation += dust.velocity.X * 0.15f;
		
		dust.scale *= 0.99f;

		Lighting.AddLight(dust.position, new Vector3(0.35f * dust.scale));

		if (dust.scale < 0.5f)
		{
			dust.active = false;
		}

		return false;
	}
}
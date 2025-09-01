namespace PathOfTerraria.Content.NPCs.Mapping.Desert;

public class GhostDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.velocity *= 0.4f;
		dust.noGravity = true;
		dust.noLight = true;
		dust.scale *= 1.5f;
		dust.color.A = (byte)Main.rand.Next(20, 230);
		dust.frame = new Rectangle(0, 8 * Main.rand.Next(3), 6, 6);
	}

	public override bool Update(Dust dust)
	{
		dust.position += dust.velocity;
		dust.rotation += dust.velocity.X * 0.15f;
		dust.scale *= 0.98f;

		if (Collision.SolidCollision(dust.position, 6, 6))
		{
			dust.scale *= 0.7f;
		}

		if (dust.scale < 0.3f)
		{
			dust.active = false;
		}

		return false;
	}
}

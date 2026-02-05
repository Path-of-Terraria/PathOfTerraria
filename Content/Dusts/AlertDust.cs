namespace PathOfTerraria.Content.Dusts;

internal class AlertDust : ModDust
{
	private static ref int Timer(Dust dust)
	{
		return ref System.Runtime.CompilerServices.Unsafe.Unbox<int>(dust.customData);
	}

	public override void OnSpawn(Dust dust)
	{
		dust.customData = 30;
		dust.frame = new Rectangle(0, 0, 12, 30);
	}

	public override bool Update(Dust dust)
	{
		dust.position += dust.velocity;
		dust.velocity *= 0.9f;

		ref int timer = ref Timer(dust);
		timer--;

		if (timer <= 0)
		{
			dust.alpha += 12;

			if (dust.alpha > 255)
			{
				dust.active = false;
			}
		}

		return false;
	}
}

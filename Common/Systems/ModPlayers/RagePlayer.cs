namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class RagePlayer : ModPlayer
{
	public float Rage { get; private set; }

	public AddableFloat MaxRage = new();

	private int _rageDecay = 0;

	public override void ResetEffects()
	{
		_rageDecay--;

		if (_rageDecay <= 0)
		{
			Rage = MathHelper.Lerp(Rage, 0, 0.05f);

			if (Rage <= 0.5f)
			{
				Rage = 0;
			}
		}

		Player.GetDamage(DamageClass.Ranged) += Rage * 0.01f;
		Player.GetDamage(DamageClass.Melee) += Rage * 0.01f;

		MaxRage = new();
		MaxRage += 20;
	}

	public void AddRage(float add = 1)
	{
		Rage = MathHelper.Min(Rage + add, MaxRage.Value);
		_rageDecay = 60;
	}
}

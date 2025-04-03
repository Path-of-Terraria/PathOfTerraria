namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class OverhealthPlayer : ModPlayer
{
	public int Overhealth { get; private set; } = 0;

	public void SetOverhealth(int value)
	{
		if (Overhealth < value)
		{
			Overhealth = value;
		}
	}

	public override void ModifyHurt(ref Player.HurtModifiers modifiers)
	{
		modifiers.ModifyHurtInfo += HitOverhealth;
	}

	private void HitOverhealth(ref Player.HurtInfo info)
	{
		if (Overhealth <= 0)
		{
			return;
		}

		int delta = Math.Min(Overhealth, info.Damage - 1);
		Overhealth -= delta;
		info.Damage -= delta;

		CombatText.NewText(Player.Hitbox, Color.Purple, delta);
	}
}

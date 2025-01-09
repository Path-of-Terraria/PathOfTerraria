namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class AttributesPlayer : ModPlayer
{
	public int LifeBoost => Strength / 10 * 5;
	public float UseSpeedBoost => Dexterity / 1000f;
	public int ManaBoost => Intelligence / 10 * 5;

	public int Strength;
	public int Dexterity;
	public int Intelligence;

	public override void ResetEffects()
	{
		// Apply buffs
		Player.statLifeMax2 += LifeBoost;
		Player.GetAttackSpeed(DamageClass.Melee) += UseSpeedBoost;
		Player.statManaMax2 += ManaBoost;
		
		// Reset
		Strength = Dexterity = Intelligence = 0;
	}
}

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class AttributesPlayer : ModPlayer
{
	public int LifeBoost => (int)(Strength / 10f) * 5;
	public float UseSpeedBoost => Dexterity / 1000f;
	public int ManaBoost => (int)(Intelligence / 10f) * 5;

	public float Strength;
	public float Dexterity;
	public float Intelligence;

	public override void ResetEffects()
	{
		// Apply buffs
		Player.statLifeMax2 += LifeBoost;
		Player.GetAttackSpeed(DamageClass.Generic) += UseSpeedBoost;
		Player.statManaMax2 += ManaBoost;
		
		// Reset
		Strength = Dexterity = Intelligence = 0f;
	}
}

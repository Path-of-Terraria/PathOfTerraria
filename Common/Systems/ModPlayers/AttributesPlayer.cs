namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class AttributesPlayer : ModPlayer
{
	public int Strength { get; private set; }
	public int Dexterity { get; private set; }
	public int Intelligence { get; private set; }

	public override void ResetEffects()
	{
		// Apply buffs
		Player.statLifeMax2 += Strength / 10 * 5;
		Player.GetAttackSpeed(DamageClass.Melee) += Dexterity / 10;
		Player.statManaMax2 += Intelligence / 10 * 5;
		
		// Reset
		Strength = Dexterity = Intelligence = 0;
	}
}

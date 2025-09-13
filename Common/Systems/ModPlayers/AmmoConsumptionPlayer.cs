namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class AmmoConsumptionPlayer : ModPlayer
{
	public float AmmoSaveChance { get; set; }

	public override void ResetEffects()
	{
		AmmoSaveChance = 0f;
	}

	public override bool CanConsumeAmmo(Item weapon, Item ammo)
	{
		// Return false to prevent ammo consumption based on the chance
		return Main.rand.NextFloat() >= AmmoSaveChance;
	}

}
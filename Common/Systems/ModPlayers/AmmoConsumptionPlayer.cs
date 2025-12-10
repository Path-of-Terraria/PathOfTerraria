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
		return Main.rand.NextFloat() >= AmmoSaveChance + Player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.AmmoReservationChance.Value;
	}
}
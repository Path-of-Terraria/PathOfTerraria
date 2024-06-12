using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.ModPlayers;
internal class UniversalBuffingPlayer : ModPlayer
{
	public EntityModifier UniversalModifier;

	public override void PostUpdateEquips()
	{
		(Player.inventory[0].ModItem as Gear)?.ApplyAffixes(UniversalModifier);

		UniversalModifier.ApplyTo(Player);

		Player.statLifeMax = Math.Min(400, Player.statLifeMax2);
	}

	public override void ResetEffects()
	{
		UniversalModifier = new EntityModifier();
	}
}

using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class ChanceToConsumeAmmoPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		// Get or create a custom ModPlayer to handle ammo consumption
		var ammoPlayer = player.GetModPlayer<AmmoConsumptionPlayer>();
		ammoPlayer.AmmoSaveChance += (Value / 100f) * Level;
	}
}
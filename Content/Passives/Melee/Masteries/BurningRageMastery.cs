using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class BurningRageMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		RagePlayer ragePlayer = player.GetModPlayer<RagePlayer>();
		player.statLifeMax2 = (int)(player.statLifeMax2 * (1 + Value / 100f * ragePlayer.Rage));

		if (ragePlayer.Rage >= ragePlayer.MaxRage.Value)
		{
			player.GetModPlayer<ElementalPlayer>().Container.AddElementalValues((ElementType.Fire, 0, 1));
		}
	}
}

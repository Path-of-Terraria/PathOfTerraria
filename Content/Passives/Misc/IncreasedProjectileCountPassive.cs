using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedProjectileCountPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		var universalPlayer = player.GetModPlayer<UniversalBuffingPlayer>();
		float bonus = Value * Level;
		universalPlayer.UniversalModifier.ProjectileCount.Flat += bonus;
	}

}

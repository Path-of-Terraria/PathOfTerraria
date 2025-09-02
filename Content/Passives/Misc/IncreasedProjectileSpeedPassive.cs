using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedProjectileSpeedPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		var universalPlayer = player.GetModPlayer<UniversalBuffingPlayer>();
		float bonus = (Value / 100f) * Level;
		universalPlayer.UniversalModifier.ProjectileSpeed += bonus;
	}
}

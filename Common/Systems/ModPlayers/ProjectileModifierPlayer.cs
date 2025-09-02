using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class ProjectileModifierPlayer : ModPlayer
{
	public StatModifier ProjectileSpeedMultiplier { get; set; }
	public StatModifier ProjectileCountModifier { get; set; }

	public override void ResetEffects()
	{
		ProjectileSpeedMultiplier = StatModifier.Default;
		ProjectileCountModifier = StatModifier.Default;
	}

	public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		if (ProjectileSpeedMultiplier != StatModifier.Default)
		{
			velocity *= ProjectileSpeedMultiplier.ApplyTo(1f);
		}
	}
}
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class QuickExplosionsPassive : Passive
{
	internal class QuickExplosionProjectile : GlobalProjectile 
	{
		public override bool PreAI(Projectile projectile)
		{
			if (ProjectileID.Sets.Explosive[projectile.type] && projectile.TryGetOwner(out Player player) 
				&& player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<QuickExplosionsPassive>(out float value) && projectile.timeLeft % (int)value == 0)
			{
				projectile.timeLeft--;
			}

			return true;
		}
	}

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(((1f / Value) * 100f).ToString("0#"));
}
using Humanizer;
using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class WhipReachMastery : Passive
{
	public const float RangeIncrease = 0.5f;
	public const float UseSpeedDecrease = 0.25f;

	public override string DisplayTooltip => Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(RangeIncrease), MathUtils.Percent(UseSpeedDecrease));

	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.ShootEvent += CreateExtension;
	}

	public override void BuffPlayer(Player player)
	{
		player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) -= UseSpeedDecrease;
	}

	private bool CreateExtension(Player player, Item item, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (ProjectileID.Sets.IsAWhip[type] && player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(Name) != 0)
		{
			Projectile.NewProjectileDirect(null, position, velocity, type, damage, knockback, player.whoAmI).WhipSettings.RangeMultiplier *= (1 + RangeIncrease);
			return false;
		}

		return true;
	}
}
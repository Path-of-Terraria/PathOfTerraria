using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class DemolitionsExpertMastery : Passive
{
	internal class DemolitionsExpertProjectile : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (ProjectileID.Sets.Explosive[projectile.type] && source is EntitySource_ItemUse_WithAmmo ammo && ammo.Player.GetModPlayer<PassiveTreePlayer>().HasNode<DemolitionsExpertMastery>())
			{
				projectile.damage = (int)(projectile.damage * (1 + DamageBoost / 100f));
			}
		}
	}

	public const float DamageBoost = 15;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, DamageBoost);

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ExplosionSize += Value / 100f;
	}
}
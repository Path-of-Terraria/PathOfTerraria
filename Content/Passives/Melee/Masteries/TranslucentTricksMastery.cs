using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class TranslucentTricksMastery : Passive
{
	public sealed class TranslucentTricksPlayer : ModPlayer
	{
		public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
		{
			if (ItemID.Sets.Yoyo[item.type] && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<TranslucentTricksMastery>(out float value))
			{
				damage *= 1 + value / 100f;
			}
		}
	}

	public sealed class TranslucentYoyoProjectile : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo { Item: Item i, Player: Player p } && ItemID.Sets.Yoyo[i.type] && p.GetModPlayer<PassiveTreePlayer>().HasNode<TranslucentTricksMastery>())
			{
				projectile.tileCollide = false;
			}
		}

		public override Color? GetAlpha(Projectile projectile, Color lightColor)
		{
			if (projectile.aiStyle == ProjAIStyleID.Yoyo && projectile.TryGetOwner(out Player plr) && plr.GetModPlayer<PassiveTreePlayer>().HasNode<TranslucentTricksMastery>())
			{
				return lightColor * 0.2f;
			}

			return null;
		}
	}

	public override void BuffPlayer(Player player)
	{
		YoyoStatsPlayer stats = player.GetModPlayer<YoyoStatsPlayer>();
		stats.YoyoSpeed += Value / 100f;
	}
}

using PathOfTerraria.Content.Projectiles.Ranged.Javelin;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class MoltenDangpa : LeadDangpa
{
	public override Vector2 ItemSize => new(94);
	public override int DeathDustType => DustID.MinecartSpark;
	public override bool IsUnique => true;
	public override bool AutoloadProjectile => false;
	public override bool UseChargeAlt => false;
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Javelins/LeadDangpa";

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 5;
		Item.shoot = ModContent.ProjectileType<MoltenDangpaThrown>();
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<IncreasedDamageAffix>(-1, 15, 25);
		var attackSpeedAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyOnFireGearAffix>(-1, 0.05f, 0.1f);
		var healKillBurnAffix = (ItemAffix)Affix.CreateAffix<HealOnKillingBurningEnemiesAffix>(-1, 1f, 4f);
		var moltenShellAffix = (ItemAffix)Affix.CreateAffix<MoltenShellAffix>(1, 1, 1);
		return [addedDamageAffix, attackSpeedAffix, healKillBurnAffix, moltenShellAffix];
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		if (player.altFunctionUse == 2)
		{
			damage = (int)(damage * 1.5f);
		}
	}
}
using System.Collections.Generic;
using PathOfTerraria.Content.Projectiles.Melee;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes.WeaponAffixes;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class CorruptedBattleaxe : IronBattleaxe
{
	public override float DropChance => 1f;
	public override bool IsUnique => true;
	public override string AltUseDescription => "Throw the axe to deal damage to enemies";
	public override string Description => "Something doesn't feel right about this axe...";

	public override void Defaults()
	{
		base.Defaults();
		Item.width = 52;
		Item.height = 52;
	}
	
	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();

		if (!modPlayer.AltFunctionAvailable)
		{
			return false;
		}
		
		if (Main.myPlayer == player.whoAmI)
		{
			Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<CorruptedBattleaxeProjectile>(), Item.damage, 0, player.whoAmI);
		}
		
		modPlayer.SetAltCooldown(300, 180);
		return true;
	}
	
	public override bool CanUseItem(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();
		bool altFunctionActive = modPlayer.AltFunctionActive; // Prevent the item from being used if the alt function is active to spawn projectile instead

		if (!altFunctionActive)
		{
			Item.noUseGraphic = false;
			Item.noMelee = false;
			return true;
		}

		Item.noUseGraphic = true;
		Item.noMelee = true;
		
		return false;
	}
	
	public override List<ItemAffix> GenerateAffixes()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<AddedDamageAffix>();
		addedDamageAffix.MinValue = 1;
		addedDamageAffix.MaxValue = 4;
		
		var increasedDamageAffix = (ItemAffix)Affix.CreateAffix<IncreasedDamageAffix>();
		increasedDamageAffix.MaxValue = 0.1f;
		increasedDamageAffix.MinValue = 0.1f;
		
		var attackSpeedAffix = (ItemAffix)Affix.CreateAffix<IncreasedAttackSpeedAffix>();
		attackSpeedAffix.MaxValue = 0.1f;
		attackSpeedAffix.MinValue = 0.1f;
		
		var armorShredAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyArmorShredGearAffix>();
		armorShredAffix.MaxValue = 0.1f;
		armorShredAffix.MinValue = 0.05f;
		armorShredAffix.Duration = 120;
		return [increasedDamageAffix, increasedDamageAffix, attackSpeedAffix, armorShredAffix];
	}
}
using System.Collections.Generic;
using PathOfTerraria.Content.Projectiles.Melee;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes.WeaponAffixes;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class GuardianAngel : IronBattleaxe
{
	public override float DropChance => 1f;
	public override bool IsUnique => true;
	public override string AltUseDescription => "Throw the axe to deal damage to enemies";
	public override string Description => "Something feels right about this axe...";
	public override int MinDropItemLevel => 26;

	public override void Defaults()
	{
		base.Defaults();
		Item.width = 94;
		Item.height = 108;
		Item.damage = 30;
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
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<PassiveAffixes.AddedDamageAffix>();
		addedDamageAffix.MinValue = 1;
		addedDamageAffix.MaxValue = 4;
		
		var increasedDamageAffix = (ItemAffix)Affix.CreateAffix<PassiveAffixes.IncreasedDamageAffix>();
		increasedDamageAffix.Value = 0.1f;
		
		var attackSpeedAffix = (ItemAffix)Affix.CreateAffix<PassiveAffixes.IncreasedAttackSpeedAffix>();
		attackSpeedAffix.Value = 0.1f;
		
		var armorShredAffix = (ItemAffix)Affix.CreateAffix<ModifyHitAffixes.ChanceToApplyArmorShredGearAffix>();
		armorShredAffix.Value = 1f;
		armorShredAffix.Duration = 120;
		return [increasedDamageAffix, increasedDamageAffix, attackSpeedAffix, armorShredAffix];
	}
}
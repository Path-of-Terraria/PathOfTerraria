using System.Collections.Generic;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Projectiles.Melee;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class CorruptedBattleaxe : IronBattleaxe
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.IsUnique = true;
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
		staticData.Description = this.GetLocalization("Description");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 52;
		Item.height = 52;
		Item.shoot = ProjectileID.PurificationPowder; // Could be anything really
	}
	
	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();
	
		return modPlayer.AltFunctionAvailable;
	}
	
	public override bool CanUseItem(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();
		bool altFunctionActive = modPlayer.AltFunctionActive; // Prevent the item from being used if the alt function is active to spawn projectile instead

		return !altFunctionActive;
	}

	public override bool? UseItem(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();
		bool altFunctionActive = modPlayer.AltFunctionActive; // Prevent the item from being used if the alt function is active to spawn projectile instead

		if (!altFunctionActive)
		{
			Item.noUseGraphic = false;
			Item.noMelee = false;
			return null;
		}

		Item.noUseGraphic = true;
		Item.noMelee = true;
		return true;
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		if (player.altFunctionUse == 2)
		{
			type = ModContent.ProjectileType<CorruptedBattleaxeProjectile>();
			AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();
			modPlayer.SetAltCooldown(300, 180);
		}
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		return type == ModContent.ProjectileType<CorruptedBattleaxeProjectile>();	
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<AddedDamageAffix>(-1, 1, 4);
		var increasedDamageAffix = (ItemAffix)Affix.CreateAffix<IncreasedDamageAffix>(-1, 15f, 25f);
		var attackSpeedAffix = (ItemAffix)Affix.CreateAffix<IncreasedAttackSpeedAffix>(0.1f);
		var armorShredAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyArmorShredGearAffix>(-1, 0.1f, 0.2f);
		armorShredAffix.Duration = 120;
		return [increasedDamageAffix, increasedDamageAffix, attackSpeedAffix, armorShredAffix];
	}
}
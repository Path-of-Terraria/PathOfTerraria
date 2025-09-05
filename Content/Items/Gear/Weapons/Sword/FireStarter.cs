using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Projectiles.Melee;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class FireStarter : Sword, GenerateName.IItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.IsUnique = true;
		staticData.Description = this.GetLocalization("Description");
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 4;
		Item.Size = new(38);
		Item.UseSound = SoundID.Item1;
		Item.value = Item.buyPrice(0, 0, 5, 0);
	}
	
	string GenerateName.IItem.GenerateName(string defaultName)
	{
		return Language.GetTextValue("Mods.PathOfTerraria.Items.FireStarter.DisplayName");
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		TooltipLine nameTip = tooltips.First(x => x.Name == "ItemName");

		if (nameTip is not null)
		{
			nameTip.OverrideColor = Color.Red;
		}
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		var sharpAffix = (ItemAffix)Affix.CreateAffix<AddedDamageAffix>(1, 4);
		var onFireAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyOnFireGearAffix>(0.1f, 0.15f);
		var fireConvAffix = (ItemAffix)Affix.CreateAffix<FireConversionDamage>(0.1f, 0.15f);
		return [sharpAffix, onFireAffix, fireConvAffix];
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
			Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<FireStarterProjectile>(), Item.damage, 0, player.whoAmI);
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

		if (player.ownedProjectileCounts[ModContent.ProjectileType<FireStarterProjectile>()] <= 0)
		{
			Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<FireStarterProjectile>(), Item.damage, Item.knockBack, player.whoAmI);
			SoundEngine.PlaySound(SoundID.Item1, player.Center);
		}
		
		return false;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
		Vector2 velocity, int type, int damage, float knockback)
	{
		return false;
	}
}
using System.Collections.Generic;
using PathOfTerraria.Content.Projectiles.Melee;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class FireStarter : Sword, IGenerateNameItem
{
	public int ItemLevel
	{
		get => 1;
		set => this.GetInstanceData().RealLevel = value; // Technically preserves previous behavior.
	}

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 5f;
		staticData.IsUnique = true;
		staticData.Description = Language.GetTextValue("Mods.PathOfTerraria.Items.FireStarter.Description");
		staticData.AltUseDescription = Language.GetTextValue("Mods.PathOfTerraria.Items.FireStarter.AltUseDescription");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 4;
		Item.Size = new(38);
		Item.UseSound = SoundID.Item1;
	}
	
	public string GenerateName(Item item)
	{
		return $"[c/FF0000:{Language.GetTextValue("Mods.PathOfTerraria.Items.FireStarter.DisplayName")}]";
	}
	
	public override List<ItemAffix> GenerateAffixes(Item item)
	{
		var sharpAffix = (ItemAffix)Affix.CreateAffix<AddedDamageAffix>();
		sharpAffix.MinValue = 1;
		sharpAffix.MaxValue = 4;
		
		var onFireAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyOnFireGearAffix>();
		onFireAffix.MinValue = 0.1f;
		onFireAffix.MaxValue = 0.1f;
		return [sharpAffix, onFireAffix];
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
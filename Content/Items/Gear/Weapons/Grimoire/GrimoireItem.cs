using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.GrimoireSelection;
using PathOfTerraria.Content.Projectiles.Summoner;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;

internal class GrimoireItem : Gear
{
	protected override string GearLocalizationCategory => "Grimoire";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0f;
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
		staticData.Description = this.GetLocalization("Description");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 10;
		Item.width = 30;
		Item.height = 34;
		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.useTime = 40;
		Item.useAnimation = 40;
		Item.autoReuse = false;
		Item.DamageType = DamageClass.Summon;
		Item.knockBack = 8;
		Item.crit = 12;
		Item.UseSound = SoundID.Item1;
		Item.shoot = ProjectileID.PurificationPowder; // The value here is irrelevant
		Item.channel = true;
		Item.noMelee = true;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Grimoire;
	}

	public override bool AltFunctionUse(Player player)
	{
		return true;
	}

	public override bool CanUseItem(Player player)
	{
		if (player.altFunctionUse == 2)
		{
			SmartUiLoader.GetUiState<GrimoireSelectionUIState>().Toggle();
			return false;
		}

		return player.GetModPlayer<GrimoireSummonPlayer>().CurrentSummonId != -1;
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		type = player.GetModPlayer<GrimoireSummonPlayer>().CurrentSummonId;
		damage = (ContentSamples.ProjectilesByType[type].ModProjectile as GrimoireSummon).BaseDamage;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
		Main.projectile[proj].damage = damage;
		Main.projectile[proj].originalDamage = damage;
		return false;
	}

	public override bool OnPickup(Player player)
	{
		player.GetModPlayer<GrimoireSummonPlayer>().HasObtainedGrimoire = true;
		return true;
	}
}
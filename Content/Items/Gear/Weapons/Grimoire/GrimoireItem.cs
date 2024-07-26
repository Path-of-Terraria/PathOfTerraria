using PathOfTerraria.Content.GUI.GrimoireSelection;
using PathOfTerraria.Content.Projectiles.Summoner;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.ModPlayers;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;

internal class GrimoireItem : Gear
{
	public override string AltUseDescription => Language.GetTextValue("Mods.PathOfTerraria.Items.GrimoireItem.AltUseDescription");
	public override string Description => Language.GetTextValue("Mods.PathOfTerraria.Items.GrimoireItem.Description");
	protected override string GearLocalizationCategory => "Grimoire";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0f;
	}

	public override void SetDefaults()
	{
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

		ItemType = ItemType.Magic;
	}

	public override bool AltFunctionUse(Player player)
	{
		return true;
	}

	public override bool CanUseItem(Player player)
	{
		if (player.altFunctionUse == 2)
		{
			UILoader.GetUIState<GrimoireSelectionUIState>().Toggle();
			return false;
		}

		return player.GetModPlayer<GrimoireSummonPlayer>().CurrentSummonId != -1;
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		type = player.GetModPlayer<GrimoireSummonPlayer>().CurrentSummonId;
		damage = (ContentSamples.ProjectilesByType[type].ModProjectile as GrimoireSummon).BaseDamage;
	}
}
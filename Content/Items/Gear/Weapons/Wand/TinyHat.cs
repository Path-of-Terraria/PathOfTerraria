using PathOfTerraria.Common.Items;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Content.Projectiles.Magic;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Wand;

internal class TinyHat : Wand
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
		staticData.Description = this.GetLocalization("Description");
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");

		LeadingConditionRule cond = new(new Conditions.NotExpert());
		IItemDropRule oneFromOptionsRule = ItemDropRule.OneFromOptions(2, Type, ModContent.ItemType<DwarvenGreatsword>(), ModContent.ItemType<Twinbow>());
		cond.OnSuccess(oneFromOptionsRule);
		
		NPCLootDatabase.AddLoot(new NPCLootDatabase.ConditionalLoot(NPCLootDatabase.MatchId(NPCID.WallofFlesh), cond));
		ItemLootDatabase.AddItemRule(ItemID.WallOfFleshBossBag, oneFromOptionsRule);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		
		Item.damage = 44;
		Item.mana = 3;
		Item.useTime = Item.useAnimation = 25;
		Item.UseSound = SoundID.Item7;
		Item.value = Item.buyPrice(0, 15, 0, 0);
		Item.noUseGraphic = true;
	}

	public override void HoldItem(Player player)
	{
		if (player.ownedProjectileCounts[ModContent.ProjectileType<TinyAlaric>()] <= 0 && Main.myPlayer == player.whoAmI)
		{
			Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<TinyAlaric>(), Item.damage, 8, player.whoAmI);
		}
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		return false;
	}
}

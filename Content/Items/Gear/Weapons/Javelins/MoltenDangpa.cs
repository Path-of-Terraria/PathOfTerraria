using PathOfTerraria.Content.Projectiles.Ranged.Javelin;
using System.Collections.Generic;

using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;

using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class MoltenDangpa : LeadDangpa
{
	public override Vector2 ItemSize => new(94);
	public override int DeathDustType => DustID.MinecartSpark;
	public override bool AutoloadProjectile => false;
	public override bool UseChargeAlt => false;
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Weapons/Javelins/MoltenDangpa_Cold";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.IsUnique = true;
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
		staticData.Description = this.GetLocalization("Description");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.shoot = ModContent.ProjectileType<MoltenDangpaThrown>();
	}

	public override List<ItemAffix> GenerateImplicits()
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
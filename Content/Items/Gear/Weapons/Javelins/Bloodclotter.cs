using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Projectiles.Ranged.Javelin;
using System.Collections.Generic;

using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;

using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class Bloodclotter : PlatinumGlaive
{
	public override Vector2 ItemSize => new(116);
	public override int DeathDustType => DustID.Blood;
	public override bool UseChargeAlt => false;
	public override bool AutoloadProjectile => false;

	public override List<ItemAffix> GenerateImplicits()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<IncreasedDamageAffix>(-1, 15, 25);
		var moltenShellAffix = (ItemAffix)Affix.CreateAffix<BloodSiphonAffix>(1, 1, 1);
		var bloodclotAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyBloodclotItemAffix>(1, 1, 1);
		return [addedDamageAffix, moltenShellAffix, bloodclotAffix];
	}

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

		Item.shoot = ModContent.ProjectileType<BloodclotterThrown>();
	}

	public override bool CanUseItem(Player player)
	{
		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();

		if (player.altFunctionUse == 2 && altUsePlayer.AltFunctionAvailable)
		{
			player.AddBuff(ModContent.BuffType<BloodclotDebuff>(), 5 * 60);
			altUsePlayer.SetAltCooldown(20 * 60, 5 * 60);

			for (int i = 0; i < 18; ++i)
			{
				Dust.NewDust(player.Center, 1, 1, DustID.Blood, player.direction * Main.rand.NextFloat(6f, 20f), Main.rand.NextFloat(-0.8f, 0.8f));
			}

			return false;
		}

		return true;
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		if (player.HasBuff<BloodclotDebuff>())
		{
			damage = (int)(damage * 1.5f);
		}
	}
}
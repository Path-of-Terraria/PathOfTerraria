using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Projectiles.Ranged.Javelin;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class Bloodclotter : LeadDangpa
{
	public override Vector2 ItemSize => new(94);
	public override int DeathDustType => DustID.Blood;
	public override bool IsUnique => true;
	public override bool AutoloadProjectile => false;
	public override bool UseChargeAlt => false;

	public override void Defaults()
	{
		base.Defaults();

		Item.shoot = ModContent.ProjectileType<MoltenDangpaThrown>();
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<IncreasedDamageAffix>(-1, 15, 25);
		var moltenShellAffix = (ItemAffix)Affix.CreateAffix<MoltenShellAffix>(1, 1, 1);
		return [addedDamageAffix, moltenShellAffix];
	}

	public override bool CanShoot(Player player)
	{
		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();

		if (player.altFunctionUse == 2 && altUsePlayer.AltFunctionAvailable)
		{
			player.AddBuff(ModContent.BuffType<BloodclotDebuff>(), 5 * 60);
			altUsePlayer.SetAltCooldown(20 * 60, 5 * 60);
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
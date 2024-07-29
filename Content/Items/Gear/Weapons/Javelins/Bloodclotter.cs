using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class Bloodclotter : PlatinumGlaive
{
	public override Vector2 ItemSize => new(116);
	public override int DeathDustType => DustID.Blood;
	public override bool IsUnique => true;
	public override bool UseChargeAlt => false;

	public override List<ItemAffix> GenerateAffixes()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<IncreasedDamageAffix>(-1, 15, 25);
		var moltenShellAffix = (ItemAffix)Affix.CreateAffix<MoltenShellAffix>(1, 1, 1);
		var bloodclotAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyBloodclotItemAffix>(1, 1, 1);
		return [addedDamageAffix, moltenShellAffix, bloodclotAffix];
	}

	public override bool CanUseItem(Player player)
	{
		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();

		if (player.altFunctionUse == 2 && altUsePlayer.AltFunctionAvailable)
		{
			player.AddBuff(ModContent.BuffType<BloodclotDebuff>(), 5 * 60);
			altUsePlayer.SetAltCooldown(5 * 60, 5 * 60);

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
using PathOfTerraria.Content.Projectiles.Ranged.Javelin;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class Rottenbone : PlatinumGlaive
{
	public override Vector2 ItemSize => new(116);
	public override int DeathDustType => DustID.CorruptGibs;
	public override bool IsUnique => true;
	public override bool UseChargeAlt => false;
	public override bool AutoloadProjectile => false;

	public override List<ItemAffix> GenerateAffixes()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<IncreasedDamageAffix>(-1, 15, 25);
		var moltenShellAffix = (ItemAffix)Affix.CreateAffix<FetidCarapaceAffix>(1, 1, 1);
		var bloodclotAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyPoisonItemAffix>(-1, 0.05f, 0.1f);
		var poisonedStrengthAffix = (ItemAffix)Affix.CreateAffix<BuffPoisonedHitsAffix>(0.2f, 0.2f, 0.2f);
		return [addedDamageAffix, moltenShellAffix, bloodclotAffix, poisonedStrengthAffix];
	}

	public override void Defaults()
	{
		base.Defaults();

		Item.shoot = ModContent.ProjectileType<RottenboneThrown>();
	}
}
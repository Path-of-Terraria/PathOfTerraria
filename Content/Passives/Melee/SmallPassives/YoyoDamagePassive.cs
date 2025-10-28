using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class YoyoDamagePassive : Passive
{
	public const float DamageIncrease = 0.1f;

	public sealed class YoyoDamagePassivePlayer : ModPlayer
	{
		public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
		{
			if (ItemID.Sets.Yoyo[item.type] && Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(YoyoDamagePassive)) != 0)
			{
				damage *= (1 + DamageIncrease);
			}
		}
	}
}
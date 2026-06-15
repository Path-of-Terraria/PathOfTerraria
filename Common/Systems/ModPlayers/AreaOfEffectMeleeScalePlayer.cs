using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Passives;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class AreaOfEffectMeleeScalePlayer : ModPlayer
{
	public override void ModifyItemScale(Item item, ref float scale)
	{
		if (!item.DamageType.CountsAsClass(DamageClass.Melee))
		{
			return;
		}

		float passiveValue = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<BiggerExplosivesPassive>();
		scale += passiveValue / 100f;
	}
}

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

		PassiveTreePlayer passiveTreePlayer = Player.GetModPlayer<PassiveTreePlayer>();
		float passiveValue = passiveTreePlayer.GetCumulativeValue<BiggerExplosivesPassive>()
			+ passiveTreePlayer.GetCumulativeValue<IncreasedAreaOfEffectPassive>();
		scale += passiveValue / 100f;
	}
}

using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Passives;

internal class RageResistancePassive : Passive
{
	public class RageResistancePlayer : ModPlayer
	{
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (Player.HasBuff<RageStacksBuff>())
			{
				modifiers.FinalDamage -= Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<RageResistancePassive>();
			}
		}
	}
}


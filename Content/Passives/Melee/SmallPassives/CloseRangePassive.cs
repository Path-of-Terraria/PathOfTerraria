using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class CloseRangePassive : Passive
{
	internal class CloseRangePlayer : ModPlayer
	{
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			float str = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<CloseRangePassive>();

			if (str < 0)
			{
				return;
			}

			if (target.DistanceSQ(Player.Center) < 200 * 200)
			{
				modifiers.FinalDamage += str * 0.1f; 
			}
		}
	}
}


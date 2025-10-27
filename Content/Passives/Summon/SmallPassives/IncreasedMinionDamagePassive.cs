using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedMinionDamagePassive : Passive
{
	public sealed class IncreasedMinionDamagePassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			int level = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(Name);

			if (proj.minion)
			{
				modifiers.FinalDamage += level / 100f;
			}
		}
	}
}

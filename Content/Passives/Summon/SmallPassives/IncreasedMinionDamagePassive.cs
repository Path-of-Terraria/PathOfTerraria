using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedMinionDamagePassive : Passive
{
	public sealed class IncreasedMinionDamagePassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			float level = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<IncreasedMinionDamagePassive>();

			if (proj.minion)
			{
				modifiers.FinalDamage += level / 100f;
			}
		}
	}
}

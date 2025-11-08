using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class ChannelingFunctionalityPlayer : ModPlayer
{
	internal int ChannelTimer = 0;

	public override void ResetEffects()
	{
		if (Player.channel)
		{
			ChannelTimer++;

			if (Player.GetModPlayer<PassiveTreePlayer>().HasNode<SingleFocusMastery>())
			{
				Player.AddBuff(ModContent.BuffType<SingleFocusBuff>(), 2, true);
				Player.GetDamage(DamageClass.Generic) += Math.Min(ChannelTimer / 30f * 0.03f, 0.3f);
			}
		}
		else
		{
			ChannelTimer = 0;
		}
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
		if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<EnergySurgeMastery>(out float str) && ChannelTimer < 2 * 60)
		{
			modifiers.FinalDamage += str / 100f;
		}
	}
}
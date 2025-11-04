using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Passives.Utility.Masteries;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class SingleFocusPlayer : ModPlayer
{
	int _channelTimer = 0;

	public override void ResetEffects()
	{
		if (Player.channel)
		{
			_channelTimer++;

			if (Player.GetModPlayer<PassiveTreePlayer>().HasNode<SingleFocusMastery>())
			{
				Player.GetDamage(DamageClass.Generic) += (int)Math.Min(_channelTimer / 100f, 0.3f);
			}

			if (Player.GetModPlayer<PassiveTreePlayer>().HasNode<EnergySurgeMastery>() && _channelTimer < 2 * 60)
			{
				Player.GetDamage(DamageClass.Generic) += 0.3f;
			}
		}
		else
		{
			_channelTimer = 0;
		}
	}
}
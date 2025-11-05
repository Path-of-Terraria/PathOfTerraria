using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class ChannelingBurstMastery : Passive
{
	internal class ChannelingBurstPlayer : ModPlayer
	{
		public override void ResetEffects()
		{
			if (Player.channel && Player.GetModPlayer<PassiveTreePlayer>().HasNode<ChannelingBurstMastery>() && Main.myPlayer == Player.whoAmI)
			{
				float dist = MathF.Min(Player.Distance(Main.MouseWorld), 800) / 800f;
				Player.GetDamage(DamageClass.Magic) += dist;
				Player.manaCost *= 1 + dist;

				if (dist > Main.rand.NextFloat())
				{
					float mul = dist * 0.8f + 1f;
					Dust.NewDust(Player.position, Player.width, 12, DustID.ManaRegeneration, Main.rand.NextFloat(-2, 2) * mul, -6 * mul);
				}
			}
		}
	}
}

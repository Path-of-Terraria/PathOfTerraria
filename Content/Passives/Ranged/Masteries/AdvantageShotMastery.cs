using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class AdvantageShotMastery : Passive
{
	internal class AdvantageShotPlayer : ModPlayer
	{
		internal int IdleTime = 0;
		internal float TargetOpacity = 1f;

		private float _realOpacity = 1f;

		public override void ResetEffects()
		{
			if (Player.velocity.LengthSquared() < 0.1f)
			{
				IdleTime++;
			}
			else
			{
				IdleTime = 0;
			}

			_realOpacity = MathHelper.Lerp(_realOpacity, TargetOpacity, 0.1f);
			TargetOpacity = 1f;
		}

		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
		{
			if (drawInfo.shadow == 0)
			{
				(r, g, b, a) = (r * _realOpacity, g * _realOpacity, b * _realOpacity, a * _realOpacity);
			}
		}
	}

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, 40);

	public override void BuffPlayer(Player player)
	{
		if (player.GetModPlayer<AdvantageShotPlayer>().IdleTime > Value * 60)
		{
			player.GetDamage(DamageClass.Ranged) += 0.4f;
			player.GetModPlayer<AdvantageShotPlayer>().TargetOpacity = 0.3f;
		}
	}
}
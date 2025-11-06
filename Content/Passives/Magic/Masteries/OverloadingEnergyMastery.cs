using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class OverloadingEnergyMastery : Passive
{
	internal class OverloadingEnergyPlayer : ModPlayer
	{
		private float _timer = 0;

		public override void ResetEffects()
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<OverloadingEnergyMastery>(out float value))
			{
				_timer++;

				if (_timer > value * 60)
				{
					_timer = 0;

					int old = Player.statMana;
					Player.statMana = (int)MathF.Min(Player.statMana + (Player.statManaMax2 * 0.05f), Player.statManaMax2);
					int delta = Player.statMana - old;

					if (delta > 0)
					{
						Player.ManaEffect(delta);
					}
				}

				if (Player.statMana == Player.statManaMax2)
				{
					Player.GetDamage(DamageClass.Generic) += 0.15f;
				}
			}
		}
	}
}

using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class OverloadingEnergyMastery : Passive
{
	internal class OverloadingEnergyPlayer : ModPlayer
	{
		private float _timer = 0;

		public override void PostUpdateEquips()
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

				float manaPercentage = (float)Player.statMana / Player.statManaMax2;
				float damageBonus = (1f - manaPercentage) * 0.4f;
				
				Player.GetDamage(DamageClass.Generic) += damageBonus;
			}
		}
	}
}
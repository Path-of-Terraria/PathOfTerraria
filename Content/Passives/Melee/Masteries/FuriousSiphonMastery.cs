using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class FuriousSiphonMastery : Passive
{
	internal class FuriousSiphonPlayer : ModPlayer
	{
		private float _elapsedHealing = 0;
		private int _timer = 0;

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<FuriousSiphonMastery>(out float value) && (hit.DamageType.CountsAsClass(DamageClass.Melee)
				|| hit.DamageType.CountsAsClass(DamageClass.Ranged)) && Player.TryGetModPlayer(out RagePlayer rage) && rage.Rage == rage.MaxRage.Value)
			{
				_elapsedHealing += damageDone / 100 + 1;
			}
		}

		public override void PreUpdate()
		{
			_timer++;

			if (_timer > 60 && _elapsedHealing >= 1)
			{
				_timer = 0;

				Player.Heal((int)_elapsedHealing);
				_elapsedHealing -= (int)_elapsedHealing;
			}
		}
	}
}

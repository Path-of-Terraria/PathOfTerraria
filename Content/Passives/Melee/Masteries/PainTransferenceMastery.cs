using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class PainTransferenceMastery : Passive
{
	internal class PainTransferencePlayer : ModPlayer
	{
		internal int AccruedHealing = 0;

		private int _timer = 0;

		public override void PostUpdateEquips()
		{
			_timer++;

			if (_timer >= 3 * 60 && AccruedHealing > 0)
			{
				_timer = 0;

				ExplosionHitbox.VFXPackage package = new(0, 12, 0, false, 0, null, DustID.LifeDrain);
				var size = new Vector2(PoTMod.NearbyDistance * 2);
				ExplosionHitbox.QuickSpawn(Player.GetSource_FromThis(), Player, AccruedHealing, Player.whoAmI, size, ExplosionSpawnInfo.PlayerOwned(Player.whoAmI), package);
				AccruedHealing = 0;
			}
		}
	}

	public override void OnLoad()
	{
		On_Player.Heal += OnHeal;
		On_Player.UpdateLifeRegen += OnRegeneration;
	}

	private void OnRegeneration(On_Player.orig_UpdateLifeRegen orig, Player self)
	{
		int oldLife = self.statLife;

		orig(self);

		if (self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<PainTransferenceMastery>(out float value))
		{
			int amount = self.statLife - oldLife;

			if (amount > 0)
			{
				amount = Math.Max((int)(amount * value / 100f), 1);
				self.GetModPlayer<PainTransferencePlayer>().AccruedHealing += amount;
			}
		}
	}

	private void OnHeal(On_Player.orig_Heal orig, Player self, int amount)
	{
		orig(self, amount);

		if (self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<PainTransferenceMastery>(out float value))
		{
			amount = (int)(amount * value / 100f);
			self.GetModPlayer<PainTransferencePlayer>().AccruedHealing += amount;
		}
	}
}

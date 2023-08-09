using Terraria.Audio;
using Terraria.ID;

namespace FunnyExperience.Core.Systems
{
	internal class PotionSystem : ModPlayer
	{
		public int healingLeft = 3;
		public int maxHealing = 3;
		public int healPower = 30;
		public int healDelay = 300;

		public int manaLeft = 3;
		public int maxMana = 3;
		public int manaPower = 50;
		public int manaDelay = 300;

		public override void Load()
		{
			On_Player.QuickHeal += QuickHeal;
			On_Player.QuickMana += QuickMana;
		}

		private void QuickHeal(On_Player.orig_QuickHeal orig, Player self)
		{
			PotionSystem mp = self.GetModPlayer<PotionSystem>();

			if (mp.healingLeft > 0 && !self.HasBuff(BuffID.PotionSickness))
			{
				self.HealEffect(mp.healPower);
				self.statLife += mp.healPower;
				self.AddBuff(BuffID.PotionSickness, mp.healDelay);
				mp.healingLeft--;

				SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/PickupPotion"));
				SoundEngine.PlaySound(SoundID.Item3);
			}
		}

		private void QuickMana(On_Player.orig_QuickMana orig, Player self)
		{
			PotionSystem mp = self.GetModPlayer<PotionSystem>();

			if (mp.manaLeft > 0 && !self.HasBuff(BuffID.ManaSickness))
			{
				self.ManaEffect(mp.manaPower);
				self.statMana += mp.manaPower;
				self.AddBuff(BuffID.ManaSickness, mp.manaDelay);
				mp.manaLeft--;

				SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/PickupPotion"));
				SoundEngine.PlaySound(SoundID.Item3);
			}
		}

		public override void ResetEffects()
		{
			if (healingLeft > maxHealing)
				healingLeft = maxHealing;

			if (manaLeft > maxMana)
				manaLeft = maxMana;

			maxHealing = 3;
			healPower = 30;
			healDelay = 300;

			maxMana = 3;
			manaPower = 50;
			manaDelay = 300;
		}
	}

	internal class PotionGlobal : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			return entity.healLife > 0 || entity.healMana > 0;
		}

		public override bool? UseItem(Item item, Player player)
		{
			Main.NewText("Used a potion!");
			return null;
		}
	}
}

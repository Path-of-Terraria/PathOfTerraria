using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class BurstingBottlesMastery : Passive
{
	internal class BurstingBottlesPlayer : ModPlayer, PotionPlayer.IOnCustomPotionPlayer
	{
		public void OnCustomPotion(bool healing, int amount)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BurstingBottlesMastery>(out float value))
			{
				ExplosionHitbox.VFXPackage package = new(3, 4, 14, true, 0.4f, null, DustID.HealingPlus, DustID.RedTorch);
				ExplosionHitbox.QuickSpawn(Player.GetSource_Misc("PotionUse"), Player, Vector2.Zero, 10, Player.whoAmI, new Vector2(150), package, true, value / 100f);
			}
		}
	}
}

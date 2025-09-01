using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class DisableFishingFishron : ModSystem
{
	public override void Load()
	{
		On_Player.ItemCheck_CheckFishingBobber_PullBobber += HijackBaitType;
	}

	private void HijackBaitType(On_Player.orig_ItemCheck_CheckFishingBobber_PullBobber orig, Player self, Projectile bobber, int baitTypeUsed)
	{
		if (baitTypeUsed == ItemID.TruffleWorm)
		{
			baitTypeUsed = ItemID.MasterBait;

			// Catch a good fish for using a crazy good bait
			bobber.localAI[1] = Main.rand.Next(10) switch
			{
				0 => ItemID.ArmoredCavefish,
				1 => ItemID.CrimsonTigerfish,
				2 => ItemID.ChaosFish,
				3 => ItemID.GoldenCarp,
				4 => ItemID.Obsidifish,
				5 => ItemID.Hemopiranha,
				_ => ItemID.Prismite,
			};
		}

		orig(self, bobber, baitTypeUsed);
	}
}

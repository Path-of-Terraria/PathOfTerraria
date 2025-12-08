using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class JourneymanMastery : Passive
{
	internal class JourneymanPlayer : ModPlayer
	{
		public override void Load()
		{
			On_Player.GetPickaxeDamage += ModifyPickaxeDamage;
		}

		private int ModifyPickaxeDamage(On_Player.orig_GetPickaxeDamage orig, Player self, int x, int y, int pickPower, int hitBufferIndex, Tile tileTarget)
		{
			int power = orig(self, x, y, pickPower, hitBufferIndex, tileTarget);

			// Vanilla uses power == 100 for Main.tileNoFail, so I avoid that
			if (self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<JourneymanMastery>(out float value) && power != 100)
			{
				Point16 pos = (self.BottomLeft + new Vector2(4, 4)).ToTileCoordinates16();

				if ((pos.X == x || pos.X + 1 == x) && pos.Y == y)
				{
					power = (int)(power * (1 + value / 40f));
				}
			}

			return power;
		}

		public override float UseSpeedMultiplier(Item item)
		{
			if (!Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<JourneymanMastery>(out float value))
			{
				return 1f;
			}

			Point16 pos = Player.BottomLeft.ToTileCoordinates16();

			if (item.pick != -1 && Player.tileTargetY == pos.Y)
			{
				return 1 + value / 100f;
			}

			return 1f;
		}
	}

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value * 2);
}

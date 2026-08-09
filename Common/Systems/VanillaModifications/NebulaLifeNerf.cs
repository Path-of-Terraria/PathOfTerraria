using PathOfTerraria.Utilities;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications;

internal class NebulaLifeNerf : ILoadable
{
	public void Load(Mod mod)
	{
		On_Player.UpdateBuffs += ForceRejectNebulaLifeBuff;
	}

	private void ForceRejectNebulaLifeBuff(On_Player.orig_UpdateBuffs orig, Player self, int i)
	{
		int oldNebula = self.nebulaLevelLife;

		using (var _ = ValueOverride.Create(ref self.nebulaLevelLife, 0))
		{
			orig(self, i);
		}

		if (oldNebula > 0)
		{
			RecreateNebulaLifeFunctionality(self, oldNebula);
		}
	}

	private static void RecreateNebulaLifeFunctionality(Player self, int oldNebula)
	{
		int buffSlot = -1;

		for (int i = BuffID.NebulaUpLife1; i <= BuffID.NebulaUpLife3; ++i)
		{
			buffSlot = self.FindBuffIndex(BuffID.NebulaUpLife1);

			if (buffSlot != -1)
			{
				break;
			}
		}

		if (buffSlot == -1)
		{
			return;
		}

		int num3 = oldNebula;
		int num4 = (byte)(1 + self.buffType[buffSlot] - 173);
		if (num3 > 0 && num3 != num4)
		{
			if (num3 > num4)
			{
				self.DelBuff(buffSlot);
				buffSlot--;
			}
			else
			{
				for (int num5 = 0; num5 < Player.MaxBuffs; num5++)
				{
					if (self.buffType[num5] >= 173 && self.buffType[num5] <= 175 + num4 - 1)
					{
						self.DelBuff(num5);
						num5--;
					}
				}
			}
		}

		self.nebulaLevelLife = num4;

		if (self.buffTime[buffSlot] == 2 && self.nebulaLevelLife > 1)
		{
			self.nebulaLevelLife--;
			self.buffType[buffSlot]--;
			self.buffTime[buffSlot] = 480;
		}

		self.lifeRegen += 6 * self.nebulaLevelLife;
	}

	public void Unload()
	{
	}
}

using Terraria.Chat;
using Terraria.GameContent.Achievements;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

/// <summary>
/// Disables natural spawns from the Eye of Cthulhu, Destroyer, Twins and Skeletron Prime.
/// </summary>
internal class DisableRandomSpawns : ModSystem
{
	public override void Load()
	{
		On_Main.UpdateTime_StartNight += StopBossesFromSpawning;
	}

	private void StopBossesFromSpawning(On_Main.orig_UpdateTime_StartNight orig, ref bool stopEvents)
	{
		bool oldEoCDown = NPC.downedBoss1;
		NPC.downedBoss1 = true;
		WorldGen.spawnEye = true;

		orig(ref stopEvents);

		// Re-implement blood moon chance as spawnEye blocks it in vanilla
		if (Main.moonPhase != 4 && Main.rand.NextBool(Main.tenthAnniversaryWorld ? 6 : 9) && Main.netMode != NetmodeID.MultiplayerClient)
		{
			for (int m = 0; m < 255; m++)
			{
				if (Main.player[m].active && Main.player[m].ConsumedLifeCrystals > 1)
				{
					Main.bloodMoon = true;
					break;
				}
			}

			if (Main.bloodMoon)
			{
				Main.sundialCooldown = 0;
				Main.moondialCooldown = 0;
				AchievementsHelper.NotifyProgressionEvent(4);
				
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.NewText(Lang.misc[8].Value, 50, byte.MaxValue, 130);
				}
				else if (Main.netMode == NetmodeID.Server)
				{
					ChatHelper.BroadcastChatMessage(Lang.misc[8].ToNetworkText(), new Color(50, 255, 130));
				}
			}
		}

		NPC.downedBoss1 = oldEoCDown;
		WorldGen.spawnEye = false;
	}
}

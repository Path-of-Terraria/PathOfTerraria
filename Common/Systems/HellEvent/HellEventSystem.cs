using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.HellEvent;

internal class HellEventSystem : ModSystem
{
	public static bool EventOccuringInstant { get; private set; }
	public static bool EventOccuring => EventStrength > 0.1f;

	public static float EventStrength = 0;

	private int eventTimer = 0;
	private int delayTimer = 0;

	public override void PostUpdatePlayers()
	{
		EventOccuringInstant = false;

		if (Main.hardMode || SubworldSystem.Current is not null and not WallOfFleshDomain)
		{
			return;
		}

		if (++delayTimer < 60)
		{
			return;
		}

		eventTimer++;

		bool canEventOccur = GetCanOccur();

		if (!canEventOccur)
		{
			return;
		}

		if (eventTimer % (1 * 60 * 60) > 0.5f * 60 * 60)
		{
			EventOccuringInstant = true;
		}
	}

	private static bool GetCanOccur()
	{
		bool canEventOccur = false;

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.Center.Y / 16 > Main.maxTilesY - 400 && Quest.GetSingleton<WoFQuest>().Available())
			{
				canEventOccur = true;
				break;
			}
		}

		return canEventOccur;
	}

	public override void PreUpdatePlayers()
	{
		EventStrength = MathHelper.Lerp(EventStrength, EventOccuringInstant ? 1 : 0, 0.01f);
	}
}

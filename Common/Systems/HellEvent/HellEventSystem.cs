using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Systems.Questing;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.HellEvent;

internal class HellEventSystem : ModSystem
{
	public static bool EventOccuringInstant { get; private set; }
	public static bool EventOccuring => EventStrength > 0.1f;

	public static float EventStrength = 0;

	private int eventTimer = 0;

	public override void PreUpdateEntities()
	{
		EventOccuringInstant = false;

		if (Main.hardMode || SubworldSystem.Current is not null and not WallOfFleshDomain)
		{
			return;
		}

		eventTimer++;

		bool canEventOccur = false;

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.GetModPlayer<QuestChecksPlayer>().CanWoFQuest)
			{
				canEventOccur = true;
				break;
			}
		}

		if (!canEventOccur)
		{
			return;
		}

		if (eventTimer % (1 * 60 * 60) > 0.5f * 60 * 60)
		{
			EventOccuringInstant = true;
		}
	}

	public override void PreUpdatePlayers()
	{
		EventStrength = MathHelper.Lerp(EventStrength, EventOccuringInstant ? 1 : 0, 0.01f);
	}
}

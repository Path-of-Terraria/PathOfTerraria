using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Common.Systems.HellEvent;

internal class HellEventSystem : ModSystem
{
	public static bool EventOccuring { get; private set; }
	public static float EventStrength = 0;

	private int eventTimer = 0;

	public override void PreUpdateEntities()
	{
		EventStrength = MathHelper.Lerp(EventStrength, EventOccuring ? 1 : 0, 0.05f);

		eventTimer++;
		EventOccuring = false;

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
			EventOccuring = true;
		}
	}
}

using PathOfTerraria.Common.Events;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

internal class CollectCount(Func<Item, bool> includes, int count, Func<string, string> displayText) : QuestStep
{
	public CollectCount(int itemType, int count, Func<string, string> displayText) : this(
		(Item item) => item.type == itemType, count, displayText)
	{
	}

	private int _total;
	private PathOfTerrariaPlayerEvents.PostUpdateDelegate tracker;

	public override string QuestString()
	{
		return displayText(_total + "/" + count);
	}

	public override string QuestCompleteString()
	{
		return displayText(count + "/" + count);
	}

	public override void Track(Player player, Action onCompletion)
	{
		tracker = (Player p) =>
		{
			if (p == player)
			{
				_total = 0;
				foreach (Item i in p.inventory)
				{
					if (includes(i))
					{
						_total += i.stack;
					}
				}
			}

			if (_total >= count)
			{
				onCompletion();
			}
		};

		PathOfTerrariaPlayerEvents.PostUpdateEvent += tracker;
	}

	public override void UnTrack()
	{
		PathOfTerrariaPlayerEvents.PostUpdateEvent -= tracker;
	}
}
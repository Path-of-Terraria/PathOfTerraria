using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

internal class CollectCount(Func<Item, bool> includes, int count, LocalizedText name) : QuestStep
{
	public CollectCount(int itemType, int count) : this(item => item.type == itemType, count, Lang.GetItemName(itemType))
	{
	}

	private readonly LocalizedText Name = name;

	public int Total = 0;

	public override string ToString()
	{
		return QuestString();
	}

	public override string QuestString()
	{
		return "Collect " + (IsDone ? count : Total) + "/" + count + " " + Name.Value;
	}

	public override bool Track(Player player)
	{
		Total = TrackItemCount(includes, player);
		return Total >= count;
	}

	private static int TrackItemCount(Func<Item, bool> includes, Player player)
	{
		int total = 0;

		foreach (Item i in player.inventory)
		{
			if (includes(i))
			{
				total += i.stack;
			}
		}

		return total;
	}
}
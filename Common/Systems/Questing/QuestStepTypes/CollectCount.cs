using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

/// <summary>
/// Counts up that you've obtained enough items that fit the <paramref name="includes"/> condition.<br/>
/// <paramref name="itemDescription"/> should describe what you're obtaining; for example, "Items that sell for at least 20 silver."
/// </summary>
/// <param name="includes">The condition(s) for if an item is valid.</param>
/// <param name="count">How many items to get.</param>
/// <param name="itemDescription">Appended to the display string to describe what the player needs to get.</param>
internal class CollectCount(Func<Item, bool> includes, int count, LocalizedText itemDescription) : QuestStep
{
	public CollectCount(int itemType, int count) : this(item => item.type == itemType, count, Lang.GetItemName(itemType))
	{
	}

	// ModItem localization isn't loaded by here through Lang.GetItemName for quests so we need the instance itself
	public CollectCount(ModItem modItem, int count) : this(item => item.type == modItem.Type, count, modItem.DisplayName)
	{
	}

	private readonly LocalizedText ItemDescription = itemDescription;

	public int Total = 0;

	public override string ToString()
	{
		return DisplayString();
	}

	public override string DisplayString()
	{
		return "Collect " + (IsDone ? count : Total) + "/" + count + " " + ItemDescription.Value;
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
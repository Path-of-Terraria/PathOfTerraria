namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

internal class CollectCount(Func<Item, bool> includes, int count, string name) : QuestStep
{
	public CollectCount(int itemType, int count, string name) : this(
		(Item item) => item.type == itemType, count, name)
	{
	}

	private readonly string Name = name;

	private int _total = 0;

	public override string QuestString()
	{
		return "Collect " + _total + "/" + count + " " + Name;
	}

	public override string QuestCompleteString()
	{
		return "Collect " + count + "/" + count + " " + Name;
	}

	public override bool Track(Player player)
	{
		_total = 0;

		foreach (Item i in player.inventory)
		{
			if (includes(i))
			{
				_total += i.stack;
			}
		}

		return _total >= count;
	}
}
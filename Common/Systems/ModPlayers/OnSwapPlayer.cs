namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// Used for running stuff when items are swapped. 
/// At the moment, this only has an event for the main item (inventory[0]) being swapped.
/// </summary>
internal class OnSwapPlayer : ModPlayer 
{
	public delegate void SwapDelegate(Player self, Item newItem, Item oldItem);

	public static event SwapDelegate OnSwapMainItem;

	private Item _oldItem = null;

	public override void PreUpdate()
	{
		if (_oldItem != Player.inventory[0])
		{
			OnSwapMainItem.Invoke(Player, Player.inventory[0], _oldItem);
		}

		_oldItem = Player.inventory[0];
	}
}

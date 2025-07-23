namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// Used for running stuff when items are swapped. 
/// At the moment, this only has an event for the main item (inventory[0]) being swapped.
/// </summary>
internal class OnSwapPlayer : ModPlayer 
{
	public delegate void SwapDelegate(Player self, Item newItem, Item oldItem);

	/// <summary>
	/// Called when the player's main item, <see cref="Player.inventory"/>[0], is changed. Run later in <see cref="PostUpdate"/>, 
	/// useful if you need values like accessories or affixes.
	/// </summary>
	public static event SwapDelegate LateSwapMainItem;

	private Item _oldItem = null;
	private Action _runLate = null;

	public override void PreUpdate()
	{
		if (_oldItem != Player.inventory[0])
		{
			// Cache LateSwap call so we can store the value of _oldItem without having to keep clones or w/e
			_runLate = () => LateSwapMainItem.Invoke(Player, Player.inventory[0], _oldItem);
		}

		_oldItem = Player.inventory[0];
	}

	public override void PostUpdate()
	{
		if (_runLate is not null)
		{
			_runLate.Invoke();
			_runLate = null;
		}
	}
}

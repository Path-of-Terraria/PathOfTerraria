using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// Base class for currency shards. As of now, does nothing.
/// </summary>
internal abstract class CurrencyShard : ModItem
{
	public override bool CanRightClick()
	{
		if (!Main.LocalPlayer.HeldItem.TryGetGlobalItem(out PoTGlobalItem _))
		{
			return false;
		}

		return !Main.LocalPlayer.HeldItem.GetInstanceData().Corrupted;
	}
}

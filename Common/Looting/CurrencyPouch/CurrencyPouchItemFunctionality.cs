using PathOfTerraria.Core.UI;
using System.Collections.Generic;
using PathOfTerraria.Content.Achievements;
using PathOfTerraria.Content.Items.Currency;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

internal class CurrencyPouchItemFunctionality : GlobalItem
{
	public override bool ItemSpace(Item item, Player player)
	{
		return CurrencyPouchStoragePlayer.CurrencyPouchAllowed(item);
	}

	public override bool OnPickup(Item item, Player player)
	{
		if (!CurrencyPouchStoragePlayer.CurrencyPouchAllowed(item))
		{
			return true;
		}

		Dictionary<int, int> storage = player.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType;
		storage.TryAdd(item.type, 0);
		storage[item.type] = Math.Min(storage[item.type] + item.stack, item.maxStack);
		
		// ACHIEVEMENT CHECK - Mirror, Mirror
		if (Main.myPlayer == player.whoAmI && item.type == ModContent.ItemType<EchoingShard>())
		{
			var mirrorAchievement = ModContent.GetInstance<MirrorMirrorAchievement>();
			if (!mirrorAchievement.FindEchoingShardCondition.IsCompleted)
			{
				mirrorAchievement.FindEchoingShardCondition.Complete();
			}
		}

		AdvancedPopupRequest request = default;
		request.Velocity = new Vector2(0, -16);
		request.DurationInFrames = 120;
		request.Text = item.Name + $" ({item.stack})";
		request.Color = Color.LightBlue;

		PopupText.NewText(request, item.Center);

		if (Main.myPlayer == player.whoAmI && UIManager.TryGet(CurrencyPouchUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
		{
			// Force a reload
			(data.Value as CurrencyPouchUIState).Toggle();
			(data.Value as CurrencyPouchUIState).Toggle();
		}

		return false;
	}
}

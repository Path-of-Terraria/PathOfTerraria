using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.UI;
using ReLogic.Content;
using System.Runtime.CompilerServices;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class ExtraStockMastery : Passive
{
	internal class ExtraStockPlayer : ModPlayer
	{
		private readonly static Asset<Texture2D> StockTex = ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/TravelingRestockButton");

		public bool CanRestock => Player.GetModPlayer<PassiveTreePlayer>().HasNode<ExtraStockMastery>();

		private bool _restockAvailable = false;

		public override void Load()
		{
			On_Main.DrawInventory += DrawInventory_RerollButton;
			On_Main.UpdateTime_StartNight += ResetRestocksForAllPlayers;
		}

		private void ResetRestocksForAllPlayers(On_Main.orig_UpdateTime_StartNight orig, ref bool stopEvents)
		{
			orig(ref stopEvents);

			foreach (Player player in Main.ActivePlayers)
			{
				player.GetModPlayer<ExtraStockPlayer>()._restockAvailable = player.GetModPlayer<ExtraStockPlayer>().CanRestock;
			}
		}

		private static void DrawInventory_RerollButton(On_Main.orig_DrawInventory orig, Main self)
		{
			orig(self);

			ExtraStockPlayer restockPlayer = Main.LocalPlayer.GetModPlayer<ExtraStockPlayer>();

			if (!restockPlayer.CanRestock || Main.LocalPlayer.talkNPC == -1 || Main.LocalPlayer.TalkNPC.type != NPCID.TravellingMerchant)
			{
				return;
			}

			const int XOffSet = 10;
			const int YOffset = 1;

			int x = (int)(73f + (float)(XOffSet * 56) * Main.inventoryScale);
			int y = (int)((float)Main.instance.invBottom + (float)(YOffset * 56) * Main.inventoryScale) - 16;
			Rectangle bounds = new(x, y, 30, 30);
			bool hover = bounds.Contains(Main.MouseScreen.ToPoint()) && restockPlayer._restockAvailable;
			Color color = Color.White;

			if (hover)
			{
				if (restockPlayer._restockAvailable && Main.mouseLeft && Main.mouseLeftRelease)
				{
					Main.mouseLeftRelease = false;

					restockPlayer._restockAvailable = false;

					Chest.SetupTravelShop();
					OpenShop(Main.instance, 19); // Force-resets the shop (19 is the shop index for the Traveling Merchant)

					[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "OpenShop")]
					static extern void OpenShop(Main main, int index);
				}

				Tooltip.Create(new TooltipDescription()
				{
					Identifier = "ShopRestock",
					SimpleTitle = Language.GetTextValue("Mods.PathOfTerraria.Misc.RestockShop"),
					SimpleSubtitle = Language.GetTextValue("Mods.PathOfTerraria.Misc.RestockTooltip")
				});
			}

			if (!restockPlayer._restockAvailable)
			{
				color = Color.Gray;
			}

			Main.spriteBatch.Draw(StockTex.Value, new Vector2(x, y), new Rectangle(hover ? 32 : 0, 0, 30, 30), color);
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("restock", _restockAvailable);
		}

		public override void LoadData(TagCompound tag)
		{
			_restockAvailable = tag.GetBool("restock");
		}
	}
}

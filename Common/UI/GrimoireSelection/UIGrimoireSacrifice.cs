using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Pickups;
using PathOfTerraria.Content.Projectiles.Summoner;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.GrimoireSelection;

internal class UIGrimoireSacrifice : UIElement
{
	public static readonly Asset<Texture2D> EmptySlot = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Projectiles/Summoner/GrimoireSummons/Empty_Icon");

	private List<int> _temporaryItems = null;
	private int _showTime = 0;

	private readonly UIImage _currentSummon;

	public UIGrimoireSacrifice()
	{
		Width = StyleDimension.FromPixels(350);
		Height = StyleDimension.FromPixels(150);

		Append(new UIImage(ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/Grimoire/SacrificeBack"))
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill,
			HAlign = 0.5f,
			VAlign = 0.5f
		});

		Append(_currentSummon = new UIImage(EmptySlot)
		{
			Width = StyleDimension.FromPixels(32),
			Height = StyleDimension.FromPixels(32),
			HAlign = 0.5f,
			VAlign = -0.2f
		});
		RefreshSummonImage();

		Item[] parts = GrimoirePlayer.Get().StoredParts;
		for (int i = 0; i < parts.Length; i++)
		{
			var pos = GetSlotPosition(i).ToPoint();
			var slot = new UIItemSlot(parts, i, ItemSlot.Context.ChestItem)
			{
				Width = StyleDimension.FromPixels(32),
				Height = StyleDimension.FromPixels(32),
				HAlign = 0.5f,
				VAlign = 0.5f,
				Left = StyleDimension.FromPixels(pos.X),
				Top = StyleDimension.FromPixels(pos.Y),
			};

			int current = i; //Don't use i directly
			slot.OnLeftClick += (a, b) => ClickSlot(ref parts[current]);
			slot.OnUpdate += (self) =>
			{
				if (self.ContainsPoint(Main.MouseScreen))
				{
					GrimoireSelectionUIState.UpdateSlot(parts[current]);
				}
			};

			Append(slot);
		}
	}

	public static void ClickSlot(ref Item slotItem)
	{
		var summoner = GrimoirePlayer.Get();

		if (slotItem.IsAir)
		{
			if (Main.mouseItem.ModItem is GrimoirePickup) //Place the pickup in the slot
			{
				slotItem = Main.mouseItem.Clone();
				Main.mouseItem.TurnToAir();
			}

			return;
		}

		if (ItemSlot.ShiftInUse)
		{
			summoner.Storage.Add(slotItem.Clone());
			slotItem.TurnToAir();
		}
		else
		{
			Item oldMouseItem = Main.mouseItem;

			Main.mouseItem = slotItem.Clone();
			slotItem = oldMouseItem.Clone();
		}

		GrimoireSelectionUIState.RefreshStorage();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		_showTime--;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (_showTime > 0 && _temporaryItems is not null)
		{
			Vector2 center = GetDimensions().ToRectangle().Center();

			for (int i = 0; i < _temporaryItems.Count; i++)
			{
				int type = _temporaryItems[i];
				Main.DrawItemIcon(spriteBatch, ContentSamples.ItemsByType[type], center + GetSlotPosition(i), Color.White * (_showTime / 180f), 26);
			}
		}
	}

	public void SetHint(GrimoireSummonLoader.Requirement showItems)
	{
		_temporaryItems = showItems.Types;
		_showTime = 180;
	}

	public void RefreshSummonImage()
	{
		int id = GrimoirePlayer.Get().CurrentSummonId;
		_currentSummon.SetImage((id == -1) ? EmptySlot : GrimoireSummon.IconsById[id]);
		_currentSummon.Recalculate();
	}

	public static Vector2 GetSlotPosition(int slot)
	{
		return slot switch
		{
			0 => new Vector2(0, 50),
			1 => new Vector2(80, 20),
			2 => new Vector2(-80, 20),
			3 => new Vector2(52, -34),
			_ => new Vector2(-52, -34)
		};
	}
}
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.Items.Pickups;
using PathOfTerraria.Content.Projectiles.Summoner;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.GrimoireSelection;

internal class GrimoireSelectionUIState : CloseableSmartUi, IMutuallyExclusiveUI
{
	public static readonly Point MainPanelSize = new(900, 550);

	protected override bool IsCentered => true;

	public static Asset<Texture2D> EmptySummonTexture = null;
	public static Asset<Texture2D> MorganaHelp = null;
	public static Asset<Texture2D> HelpBack = null;

	public static Item EmptyItem
	{
		get 
		{
			var item = new Item(ItemID.None);
			item.TurnToAir();
			return item;
		}
	}

	private static UIGrid _storageGrid = null;
	private static UIGrid _summonGrid = null;
	private static UIGrimoireSacrifice _sacrificePanel = null;
	private static Item _trashItem = null;
	private static UIItemIcon _trashSlot = null;
	private static bool _helpOpen = false;
	private static bool _helpHover = false;
	private static bool _heyHelp = false;
	private static float _helpOpacity = 0;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(x => x.Name == "Vanilla: Hotbar");
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		if (IsVisible && !Main.playerInventory)
		{
			Toggle();
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		float invSize = Main.inventoryScale;
		Main.inventoryScale = 0.8f;
		base.Draw(spriteBatch);
		Main.inventoryScale = invSize;

		DrawMorganaHelp(spriteBatch);
	}

	private void DrawMorganaHelp(SpriteBatch spriteBatch)
	{
		if (Main.LocalPlayer.GetModPlayer<GrimoirePlayer>().FirstOpenMenagerie)
		{
			_heyHelp = true;
		}

		Main.LocalPlayer.GetModPlayer<GrimoirePlayer>().FirstOpenMenagerie = false;

		CalculatedStyle panelSize = Panel.GetDimensions();
		Vector2 topLeft = panelSize.Position();
		Vector2 pos = topLeft + new Vector2(66, 14);
		Rectangle rect = new((int)pos.X, (int)pos.Y, 34, 34);
		bool hover = rect.Contains(Main.MouseScreen.ToPoint());
		Rectangle src = new(36 * hover.ToInt(), 36 * _helpOpen.ToInt(), 34, 34);
		spriteBatch.Draw(MorganaHelp.Value, pos, src, Color.White);

		if (_heyHelp)
		{
			if (_helpOpen)
			{
				_heyHelp = false;
			}

			string heyText = Language.GetTextValue("Mods.PathOfTerraria.UI.Grimoire.ClickMe");
			Vector2 heyPos = topLeft + new Vector2(52, -6 + MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 4);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, heyText, heyPos, Color.White, -0.2f, Vector2.Zero, Vector2.One);
		}

		if (hover)
		{
			if (Main.mouseLeft && Main.mouseLeftRelease)
			{
				_helpOpen = !_helpOpen;
			}

			Tooltip.Create(new TooltipDescription
			{
				Identifier = GetType().Name,
				SimpleTitle = "Help",
			});
		}

		if (_helpHover != hover)
		{
			SoundEngine.PlaySound(SoundID.MenuTick, Main.LocalPlayer.Center);
		}

		_helpHover = hover;
		_helpOpacity = MathHelper.Lerp(_helpOpacity, _helpOpen ? 1 : 0, 0.08f);

		if (_helpOpacity < 0.01f)
		{
			return;
		}

		spriteBatch.Draw(HelpBack.Value, topLeft, Color.White * _helpOpacity);
		DynamicSpriteFont font = FontAssets.ItemStack.Value;
		string text = Language.GetTextValue("Mods.PathOfTerraria.UI.Grimoire.Help.Materials");
		DrawHelpText(spriteBatch, topLeft + new Vector2(30, panelSize.Y * 1.5f), font, text, 320);

		text = Language.GetTextValue("Mods.PathOfTerraria.UI.Grimoire.Help.Summons");
		DrawHelpText(spriteBatch, topLeft + new Vector2(430, 250), font, text, 420);

		text = Language.GetTextValue("Mods.PathOfTerraria.UI.Grimoire.Help.Ritual");
		DrawHelpText(spriteBatch, topLeft + new Vector2(446, 544), font, text, 420);
	}

	private static void DrawHelpText(SpriteBatch spriteBatch, Vector2 position, DynamicSpriteFont font, string text, int maxWidth)
	{
		Vector2 size = ChatManager.GetStringSize(font, text, Vector2.One, maxWidth);
		position.Y += 28;
		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, position, Color.White * _helpOpacity, 0f, size * new Vector2(0f, 1f), Vector2.One, maxWidth);
	}

	public void Toggle()
	{
		RemoveAllChildren();

		_helpOpen = false;

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		EmptySummonTexture ??= ModContent.Request<Texture2D>("PathOfTerraria/Assets/Projectiles/Summoner/GrimoireSummons/Empty_Icon");
		MorganaHelp ??= ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/Grimoire/MorganaHelp");
		HelpBack ??= ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/Grimoire/HelpBack");
		IsVisible = !IsVisible;

		if (!IsVisible)
		{
			_storageGrid.Clear();
			_storageGrid = null;
			return;
		}

		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<GrimoireSelectionUIState>();

		Main.playerInventory = true;

		CreateMainPanel(false, MainPanelSize, false, true);
		Panel.VAlign = 0.7f;

		BuildSummonSelect(Panel);
		BuildStorage(Panel);

		Panel.Append(_sacrificePanel = new UIGrimoireSacrifice()
		{
			HAlign = 0.86f,
			VAlign = 0.9f
		});

		CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton"));
		CloseButton.Left.Set(PointsAndExitPadding, 0);
		CloseButton.Top.Set(10, 0f);
		CloseButton.Width.Set(38, 0);
		CloseButton.Height.Set(38, 0);
		CloseButton.OnLeftClick += (a, b) =>
		{
			Toggle();
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		CloseButton.SetVisibility(1, 1);
		Panel.Append(CloseButton);

		Recalculate();
	}

	private static void BuildSummonSelect(UICloseablePanel panel)
	{
		var mainPanel = new UIPanel()
		{
			Width = StyleDimension.FromPixels(498),
			Height = StyleDimension.FromPixels(300),
			HAlign = 1f,
		};
		panel.Append(mainPanel);

		mainPanel.Append(new UIImageFramed(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Grimoire/GrimoireButton"), new Rectangle(0, 0, 64, 64))
		{
			VAlign = -0.24f,
			HAlign = 0.5f,
			Width = StyleDimension.FromPixels(64),
			Height = StyleDimension.FromPixels(64),
		});

		var gridPanel = new UIPanel()
		{
			Width = StyleDimension.FromPixelsAndPercent(-24, 1),
			Height = StyleDimension.FromPixelsAndPercent(-20, 1),
			VAlign = 1f
		};
		mainPanel.Append(gridPanel);

		_summonGrid = new()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill
		};
		_summonGrid.OnUpdate += SpamRecalculate;
		gridPanel.Append(_summonGrid);

		var scrollBar = new UIScrollbar()
		{
			Width = StyleDimension.FromPixels(20),
			Height = StyleDimension.FromPixelsAndPercent(-20, 1f),
			HAlign = 1f,
			VAlign = 1f
		};
		mainPanel.Append(scrollBar);
		_summonGrid.SetScrollbar(scrollBar);

		foreach (int item in ModContent.GetInstance<GrimoireSummonLoader>().SummonIds)
		{
			var summon = ContentSamples.ProjectilesByType[item].ModProjectile as GrimoireSummon;

			var summonIcon = new UIColoredImageButton(GrimoireSummon.IconsById[summon.Type])
			{
				Width = StyleDimension.FromPixels(36),
				Height = StyleDimension.FromPixels(36)
			};
			_summonGrid.Add(summonIcon);

			summonIcon.OnUpdate += (self) => UpdateSummonIcon(self as UIColoredImageButton, summon);
			summonIcon.OnLeftClick += (a, b) => ClickSummon(summon);
		}

		_summonGrid.Recalculate();
	}

	private static void ClickSummon(GrimoireSummon summon)
	{
		if (GrimoirePlayer.Get().CanUseSummon(summon, out GrimoireSummonLoader.Requirement requirements))
		{
			ToggleSummon(summon, requirements);
		}
		else
		{
			_sacrificePanel.SetHint(requirements);
			SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.5f });
		}

		RefreshStorage();

		static void ToggleSummon(GrimoireSummon summon, GrimoireSummonLoader.Requirement items)
		{
			var summoner = GrimoirePlayer.Get();
			bool deactivate = summoner.CurrentSummonId == summon.Type;

			summoner.CurrentSummonId = deactivate ? -1 : summon.Type;
			_sacrificePanel.RefreshSummonImage();
			RefreshStorage();

			SoundEngine.PlaySound((deactivate ? SoundID.MenuClose : SoundID.MenuOpen) with { Pitch = 0.5f });
		}
	}

	private static void UpdateSummonIcon(UIColoredImageButton self, GrimoireSummon item)
	{
		self.SetColor(GrimoirePlayer.Get().CanUseSummon(item, out _) ? Color.White : new Color(100, 100, 100));

		if (!self.GetDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
		{
			return;
		}

		string tooltip = Language.GetTextValue($"Mods.{item.Mod.Name}.Projectiles.{item.Name}.Description");
		tooltip += "\n" + Language.GetText($"Mods.{item.Mod.Name}.UI.BaseDamage").Format(item.BaseDamage);
		Tooltip.Create(new TooltipDescription
		{
			Identifier = "SummonIcon",
			SimpleTitle = item.DisplayName.Value,
			SimpleSubtitle = tooltip,
		});
	}

	private static void BuildStorage(UICloseablePanel panel)
	{
		var mainPanel = new UIPanel()
		{
			Width = StyleDimension.FromPixels(398),
			Height = StyleDimension.Fill,
		};
		panel.Append(mainPanel);

		mainPanel.Append(new UIText(Language.GetTextValue("Mods.PathOfTerraria.UI.Grimoire.Storage"), 1, true)
		{
			HAlign = 0.5f,
		});

		var storagePanel = new UIPanel()
		{
			Height = StyleDimension.FromPixelsAndPercent(-54, 1f),
			Width = StyleDimension.FromPixelsAndPercent(-24, 1),
			VAlign = 1f
		};
		mainPanel.Append(storagePanel);

		_storageGrid = new UIGrid()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill,
		};
		storagePanel.Append(_storageGrid);

		var scrollBar = new Terraria.ModLoader.UI.Elements.FixedUIScrollbar(SmartUiLoader.GetUiState<GrimoireSelectionUIState>().UserInterface)
		{
			Width = StyleDimension.FromPixels(20),
			Height = StyleDimension.FromPixelsAndPercent(-54, 1f),
			HAlign = 1f,
			VAlign = 1f,
		};
		_storageGrid.SetScrollbar(scrollBar);
		_storageGrid.OnUpdate += SpamRecalculate;
		_storageGrid.OnLeftClick += (a, b) => StoreItem(Main.mouseItem);

		mainPanel.Append(scrollBar);

		RefreshStorage();

		if (_trashItem is null || _trashItem.IsAir)
		{
			_trashItem = new Item(ItemID.TrashCan);
		}

		BuildTrashSlot(false);
	}

	/// <summary>
	/// Used on both <see cref="_storageGrid"/> and <see cref="_summonGrid"/> since they don't use scroll properly otherwise.
	/// </summary>
	private static void SpamRecalculate(UIElement affectedElement)
	{
		affectedElement.Recalculate();
	}

	internal static void RefreshStorage()
	{
		if (_storageGrid is null)
		{
			return;
		}
		
		_storageGrid.Clear();

		int summonId = GrimoirePlayer.Get().CurrentSummonId;
		Dictionary<int, int> parts = null;

		if (summonId > ProjectileID.None)
		{
			var summon = ContentSamples.ProjectilesByType[summonId].ModProjectile as GrimoireSummon;
			parts = summon.GetRequiredParts();
		}

		foreach (Item item in Main.LocalPlayer.GetModPlayer<GrimoirePlayer>().Storage)
		{
			if (parts != null && !parts.ContainsKey(item.type))
			{
				continue;
			}

			var storageIcon = new UIItemIcon(item, false)
			{
				Width = StyleDimension.FromPixels(31),
				Height = StyleDimension.FromPixels(31)
			};
			_storageGrid.Add(storageIcon);

			storageIcon.OnLeftClick += (_, self) => ClickStorageItem(item);
			storageIcon.OnUpdate += (self) =>
			{
				if (self.ContainsPoint(Main.MouseScreen))
				{
					UpdateSlot(item);
				}
			};
		}

		_storageGrid.Recalculate();
		_storageGrid.Recalculate();
	}

	private static void ClickStorageItem(Item item)
	{
		var summoner = GrimoirePlayer.Get();
		if (ItemSlot.ShiftInUse)
		{
			summoner.Storage.Remove(item);

			for (int i = 0; i < summoner.StoredParts.Length; ++i)
			{
				if (summoner.StoredParts[i].IsAir)
				{
					summoner.Storage.Remove(item);
					summoner.StoredParts[i] = item.Clone();
					RefreshStorage();

					break;
				}
			}

			summoner.StoredParts.CopyTo(summoner.StoredParts, 0);
			return;
		}

		if (Main.mouseItem.IsAir)
		{
			summoner.Storage.Remove(item);
			Main.mouseItem = item.Clone();
		}

		RefreshStorage();
	}

	private static void StoreItem(Item item)
	{
		if (item.ModItem is not GrimoirePickup)
		{
			return;
		}

		GrimoirePlayer storagePlayer = Main.LocalPlayer.GetModPlayer<GrimoirePlayer>();

		storagePlayer.Storage.Add(item.Clone());
		item.TurnToAir();

		RefreshStorage();
	}

	private static void BuildTrashSlot(bool remove)
	{
		UICloseablePanel panel = SmartUiLoader.GetUiState<GrimoireSelectionUIState>().Panel;

		if (remove)
		{
			panel.RemoveChild(_trashSlot);
		}

		_trashSlot = new UIItemIcon(_trashItem, false)
		{
			Width = StyleDimension.FromPixels(31),
			Height = StyleDimension.FromPixels(31),
			Top = StyleDimension.FromPixelsAndPercent(34, 0.5f),
			Left = StyleDimension.FromPixelsAndPercent(-38, 0.5f)
		};

		_trashSlot.OnLeftClick += (a, b) => PutTrashBackInStorage();
		panel.Append(_trashSlot);
	}

	private static void PutTrashBackInStorage()
	{
		if (_trashItem.IsAir || _trashItem.type == ItemID.TrashCan)
		{
			return;
		}

		Main.LocalPlayer.GetModPlayer<GrimoirePlayer>().Storage.Add(_trashItem.Clone());
		_trashItem = new Item(ItemID.TrashCan);

		BuildTrashSlot(true);
		RefreshStorage();
	}

	public static void UpdateSlot(Item item)
	{
		if (item.IsAir || item.type == ItemID.None)
		{
			return;
		}

		if (ItemSlot.ShiftInUse)
		{
			Main.cursorOverride = 9;
		}

		Tooltip.Create(new TooltipDescription
		{
			Identifier = "GrimoireSelection",
			AssociatedItem = item,
			Lines = ItemTooltipBuilder.BuildTooltips(item, Main.LocalPlayer),
		});
	}
}
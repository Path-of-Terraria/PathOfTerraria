using PathOfTerraria.Common.Looting.VirtualBagUI;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Content.Items.Currency.Runebound;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

internal sealed class CurrencyPouchItemSlot(Asset<Texture2D> backgroundTexture, UIImageItemSlot.SlotWrapper itemHandler, int context)
	: UIHoverImageItemSlot(backgroundTexture, Asset<Texture2D>.Empty, itemHandler, null, context)
{
	public int StoredAmount { get; set; }
	public Color CountColor { get; set; } = Color.Gray;

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		CalculatedStyle dimensions = GetDimensions();
		const float Scale = 0.75f;
		string text = StoredAmount.ToString();
		Vector2 size = FontAssets.ItemStack.Value.MeasureString(text) * Scale;
		Vector2 position = new(dimensions.X + dimensions.Width - size.X - 3f, dimensions.Y + dimensions.Height - size.Y - 2f);
		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text, position,
			CountColor, 0f, Vector2.Zero, new Vector2(Scale));
	}
}

internal sealed class CurrencyPouchCraftingSlot(Asset<Texture2D> backgroundTexture, Asset<Texture2D> iconTexture,
	UIImageItemSlot.SlotWrapper itemHandler, int context, float iconScalingSize)
	: UIImageItemSlot(backgroundTexture, iconTexture, itemHandler, context, null, true, iconScalingSize)
{
	private const ulong EffectDuration = 36;
	private ulong _effectStartedAt = ulong.MaxValue;
	private Color _effectColor;

	public void PlayCraftingEffect(Color color)
	{
		_effectStartedAt = Main.GameUpdateCount;
		_effectColor = color;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (_effectStartedAt == ulong.MaxValue)
		{
			return;
		}

		ulong elapsed = Main.GameUpdateCount - _effectStartedAt;
		if (elapsed >= EffectDuration)
		{
			_effectStartedAt = ulong.MaxValue;
			return;
		}

		float progress = elapsed / (float)EffectDuration;
		float opacity = MathF.Pow(1f - progress, 1.5f);
		Vector2 center = GetDimensions().Center();
		Texture2D pixel = TextureAssets.MagicPixel.Value;
		Rectangle pixelSource = new(0, 0, 1, 1);

		for (int i = 0; i < 12; i++)
		{
			float rotation = MathHelper.TwoPi * i / 12f + progress * 0.35f;
			float radius = MathHelper.Lerp(10f, 22f, progress);
			float length = MathHelper.Lerp(8f, 2f, progress);
			Vector2 direction = rotation.ToRotationVector2();
			Vector2 position = center + direction * radius;

			spriteBatch.Draw(pixel, position, pixelSource, _effectColor * opacity, rotation, new Vector2(0f, 0.5f),
				new Vector2(length, 2f), SpriteEffects.None, 0f);
		}
	}
}

internal class CurrencyPouchUIState : UIState, IMutuallyExclusiveUI
{
	public const int SlotContext = ItemSlot.Context.ChestItem;
	public const string Identifier = "Currency Pouch UI";
	private const string CurrencyTab = "currency";
	private const string RunestonesTab = "runestones";
	private const int GridColumns = 9;
	private const int MinimumGridRows = 3;
	private const int GridRowHeight = 52;
	private const int GridStartY = 8;
	private const int CraftingSlotSize = 62;
	private const int RunestoneFamilyRows = 5;
	private const int RunestoneFamilyRowStep = 54;
	private const int RunestoneGradeStep = 50;
	private const ulong CraftingFeedbackDuration = 180;

	private UIElement _backdrop = null;
	private CurrencyPouchBackUI _placedItemTooltip = null;

	private static ref Item SlottedItem => ref Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().SlottedItem;

	private CurrencyPouchCraftingSlot _modifyItemSlot = null;
	private UIPanelTab _currencyTab = null;
	private UIPanelTab _runestonesTab = null;
	private string _activeTab = CurrencyTab;
	private readonly HashSet<string> _changedModifierTexts = [];
	private readonly List<string> _removedModifierTexts = [];
	private ulong _craftingFeedbackUntil;
	private Color _craftingFeedbackColor = Color.White;

	public override void OnActivate()
	{
		const int GridBuffer = 50;
		const int TabBarHeight = 32;
		const string Path = "Mods.PathOfTerraria.UI.CurrencyPouch.";
		int[] availableTypes = GetAvailableTypes();
		bool runestoneLayout = _activeTab == RunestonesTab;
		int gridRows = runestoneLayout
			? RunestoneFamilyRows
			: Math.Max(MinimumGridRows, (availableTypes.Length + GridColumns - 1) / GridColumns);
		int extraGridHeight = (gridRows - MinimumGridRows) * GridRowHeight;
		int craftingSlotTop = runestoneLayout ? 92 : GridStartY + gridRows * GridRowHeight + 14;
		int tooltipTop = runestoneLayout
			? GridStartY + RunestoneFamilyRows * RunestoneFamilyRowStep + 8
			: craftingSlotTop + CraftingSlotSize + 30;

		RemoveAllChildren();

		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<CurrencyPouchUIState>();

		UIPanel panel = new();
		panel.SetDimensions((0f, 0), (0, 0), (0, 540), (0, 342 + TabBarHeight + extraGridHeight));
		panel.VAlign = 0.5f;
		panel.HAlign = 0.5f;
		panel.OnUpdate += BlockClicks;
		panel.AddComponent(new UIMouseDrag("CurrencyPouch_MainWindow", canMove: true, canResize: false));
		Append(panel);

		panel.Append(new UIText(Language.GetText(Path + "Title"), 0.6f, true) { Top = StyleDimension.FromPixels(6), Left = StyleDimension.FromPixels(6) });

		var close = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")) { HAlign = 1f };
		close.SetVisibility(1, 0.6f);
		close.SetDimensions((0, 0), (0, 0), (0, 38), (0, 38));
		close.OnLeftClick += Close;
		panel.Append(close);

		_currencyTab = new UIPanelTab(CurrencyTab, Language.GetText(Path + "TabCurrency"), 0.85f);
		_currencyTab.SetPadding(6);
		_currencyTab.Top = StyleDimension.FromPixels(GridBuffer);
		_currencyTab.BackgroundColor.A = 255;
		_currencyTab.OnLeftClick += (_, _) => SetActiveTab(CurrencyTab);
		panel.Append(_currencyTab);

		_runestonesTab = new UIPanelTab(RunestonesTab, Language.GetText(Path + "TabRunestones"), 0.85f);
		_runestonesTab.SetPadding(6);
		_runestonesTab.Top = StyleDimension.FromPixels(GridBuffer);
		_runestonesTab.BackgroundColor.A = 255;
		_runestonesTab.OnLeftClick += (_, _) => SetActiveTab(RunestonesTab);
		panel.Append(_runestonesTab);

		_currencyTab.Recalculate();
		_runestonesTab.Left = StyleDimension.FromPixels(_currencyTab.GetDimensions().Width + 8);
		_runestonesTab.Recalculate();
		UpdateTabStyles();

		_backdrop = new UIElement();
		_backdrop.SetDimensions((0, 0), (0, GridBuffer + TabBarHeight), (1, 0), (1, -(GridBuffer + TabBarHeight)));
		panel.Append(_backdrop);

		BuildItemSlots(_backdrop, availableTypes);

		UIElement container = _backdrop.AddElement(new UIElement(), x => x.SetDimensions((0.5f, -31), (0, craftingSlotTop), (0, CraftingSlotSize), (0, CraftingSlotSize)));

		var wrapper = new UIImageItemSlot.SlotWrapper(() => SlottedItem, x => SlottedItem = x);
		Asset<Texture2D> back = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/ItemSlot", AssetRequestMode.ImmediateLoad);
		Asset<Texture2D> icon = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CurrencyWeaponIcon", AssetRequestMode.ImmediateLoad);
		_modifyItemSlot = new CurrencyPouchCraftingSlot(back, icon, wrapper, SlotContext, 48);
		// Tab changes rebuild this UI after the state has already completed its normal
		// initialization pass, so initialize the newly-created slot before its first update.
		_modifyItemSlot.Initialize();
		_modifyItemSlot.SetDimensions((0, 0), (0, 0), (1, 0), (1, 0));
		_modifyItemSlot.HAlign = 0.5f;
		_modifyItemSlot.VAlign = 0.5f;
		_modifyItemSlot.Background.ImageScale = 1.28f;
		_modifyItemSlot.Icon.ImageScale = 1.12f;
		_modifyItemSlot.OnUpdate += DisplayConstantTooltipIfSlotted;
		container.Append(_modifyItemSlot);

		UIText craftingHint = new(Language.GetText(Path + "CraftingHint"), 0.72f)
		{
			HAlign = 0.5f,
			Top = StyleDimension.FromPixels(craftingSlotTop + CraftingSlotSize + 2),
			TextColor = Color.LightGray,
		};
		_backdrop.Append(craftingHint);

		_placedItemTooltip = new CurrencyPouchBackUI();
		_placedItemTooltip.SetDimensions((0.5f, -200), (0, tooltipTop), (0, 400), (0, 1));
		_backdrop.Append(_placedItemTooltip);
	}

	private void DisplayConstantTooltipIfSlotted(UIElement affectedElement)
	{
		var slot = affectedElement as UIImageItemSlot;
		
		if (!slot.Item.IsAir)
		{
			List<DrawableTooltipLine> lines = ItemTooltipBuilder.BuildTooltips(slot.Item, Main.LocalPlayer);
			ApplyCraftingFeedback(lines);
			_placedItemTooltip.SetTooltips(lines);
		}
		else
		{
			_placedItemTooltip.SetTooltips(null);
		}
	}

	private int[] GetAvailableTypes()
	{
		IEnumerable<int> availableTypesQuery = ContentSamples.ItemsByType
			.Where(pair => pair.Key > ItemID.None && BelongsToActiveTab(pair.Value))
			.Select(pair => pair.Key);

		return (_activeTab == RunestonesTab
			? availableTypesQuery.OrderBy(type => type)
			: availableTypesQuery.OrderBy(type => ContentSamples.ItemsByType[type].Name))
			.ToArray();
	}

	private void BuildItemSlots(UIElement backdrop, int[] availableTypes)
	{
		if (_activeTab == RunestonesTab)
		{
			BuildRunestoneSlots(backdrop, availableTypes);
			return;
		}

		for (int i = 0; i < availableTypes.Length; i++)
		{
			Vector2 position = new(23 + i % GridColumns * 56, GridStartY + i / GridColumns * GridRowHeight);
			backdrop.Append(BuildSingleSlot(availableTypes[i], position));
		}
	}

	private void BuildRunestoneSlots(UIElement backdrop, int[] availableTypes)
	{
		var familyGroups = availableTypes
			.Where(type => ContentSamples.ItemsByType[type].ModItem is Runestone)
			.GroupBy(type => ((Runestone)ContentSamples.ItemsByType[type].ModItem).Family)
			.OrderBy(group => group.Key)
			.ToArray();

		for (int familyIndex = 0; familyIndex < familyGroups.Length; familyIndex++)
		{
			bool rightSide = familyIndex >= RunestoneFamilyRows;
			int row = familyIndex % RunestoneFamilyRows;
			float startX = rightSide ? 376f : 14f;
			int[] familyTypes = familyGroups[familyIndex]
				.OrderBy(type => ((Runestone)ContentSamples.ItemsByType[type].ModItem).Grade)
				.ToArray();

			for (int gradeIndex = 0; gradeIndex < familyTypes.Length; gradeIndex++)
			{
				Vector2 position = new(startX + gradeIndex * RunestoneGradeStep, GridStartY + row * RunestoneFamilyRowStep);
				backdrop.Append(BuildSingleSlot(familyTypes[gradeIndex], position));
			}
		}

		int[] specialTypes = availableTypes
			.Where(type => ContentSamples.ItemsByType[type].ModItem is not Runestone)
			.OrderBy(type => ContentSamples.ItemsByType[type].Name)
			.ToArray();
		float specialStartX = (540f - specialTypes.Length * RunestoneGradeStep) * 0.5f;

		for (int i = 0; i < specialTypes.Length; i++)
		{
			backdrop.Append(BuildSingleSlot(specialTypes[i], new Vector2(specialStartX + i * RunestoneGradeStep, 180f)));
		}
	}

	private bool BelongsToActiveTab(Item item)
	{
		return item.ModItem is CurrencyShard
			&& (_activeTab == RunestonesTab ? item.ModItem is IRunestoneItem : item.ModItem is not IRunestoneItem);
	}

	private void SetActiveTab(string tab)
	{
		if (_activeTab == tab)
		{
			return;
		}

		_activeTab = tab;
		OnActivate();
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	private void UpdateTabStyles()
	{
		_currencyTab.TextColor = _activeTab == CurrencyTab ? Color.Yellow : Color.White;
		_runestonesTab.TextColor = _activeTab == RunestonesTab ? Color.Yellow : Color.White;
	}

	public UIImageItemSlot BuildSingleSlot(int type, Vector2 position)
	{
		Item item = ContentSamples.ItemsByType[type].Clone();
		var wrapper = new UIImageItemSlot.SlotWrapper(() => item, _ => { });
		Asset<Texture2D> slotTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/ItemSlot", AssetRequestMode.ImmediateLoad);
		var storageSlot = new CurrencyPouchItemSlot(slotTexture, wrapper, SlotContext)
		{
			Left = StyleDimension.FromPixels(position.X),
			Top = StyleDimension.FromPixels(position.Y),
			IsLocked = _ => true,
			InactiveScale = 1f,
			ActiveScale = 1.08f,
			DrawStack = false,
		};

		storageSlot.Initialize();
		storageSlot.OnUpdate += self =>
		{
			CurrencyPouchStoragePlayer pouch = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>();
			int amount = pouch.StorageByType.TryGetValue(type, out int stored) ? stored : 0;
			storageSlot.StoredAmount = amount;
			storageSlot.CountColor = amount == Item.CommonMaxStack ? Color.Red : amount > 0 ? Color.White : Color.Gray;
			storageSlot.ItemDrawColor = amount > 0 ? Color.White : new Color(150, 150, 150, 180);
			storageSlot.Background.Color = amount > 0 ? Color.White : new Color(175, 175, 175);

			if (amount > 0 && self.ContainsPoint(Main.MouseScreen))
			{
				UpdateSlot(item);
			}
		};

		storageSlot.OnLeftClick += (_, _) => LeftClickItem(item);
		storageSlot.OnRightClick += (_, _) => RightClickItem(item);

		return storageSlot;
	}

	private void LeftClickItem(Item item)
	{
		bool isControlHeld = Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl);
		bool isShiftHeld = Main.keyState.PressingShift();

		if (isShiftHeld && !isControlHeld)
		{
			if (!TryStoreAllMatchingInventoryItems(item.type, out int stored))
			{
				return;
			}

			SoundEngine.PlaySound(SoundID.Grab);
			return;
		}

		if (!isControlHeld)
		{
			return;
		}

		int amountToExtract = isShiftHeld ? Item.CommonMaxStack : 1;

		if (!TryExtractItem(item, amountToExtract, out int extracted))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.Grab);

		if (extracted > 0 && !Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType.ContainsKey(item.type))
		{
			Toggle();
			Toggle();
		}
	}

	private void RightClickItem(Item item)
	{
		if (item.ModItem is not CurrencyShard { SupportsPouchCrafting: true } shard)
		{
			return;
		}

		if (!shard.CanUseInPouch(SlottedItem, out _))
		{
			return;
		}

		Dictionary<int, int> storage = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType;
		
		// Check if player actually has any of this currency type
		if (!storage.TryGetValue(item.type, out int currentAmount) || currentAmount <= 0)
		{
			return;
		}

		List<DrawableTooltipLine> before = ItemTooltipBuilder.BuildTooltips(SlottedItem, Main.LocalPlayer);
		storage[item.type] = Math.Max(storage[item.type] - 1, 0);
		shard.ApplyToItem(SlottedItem);

		List<DrawableTooltipLine> after = ItemTooltipBuilder.BuildTooltips(SlottedItem, Main.LocalPlayer);
		Color effectColor = GetCraftingEffectColor(item.type);
		CaptureCraftingFeedback(before, after, effectColor);
		_modifyItemSlot.PlayCraftingEffect(effectColor);
		SoundEngine.PlaySound(SoundID.Item4, Main.LocalPlayer.Center);
	}

	private void CaptureCraftingFeedback(List<DrawableTooltipLine> before, List<DrawableTooltipLine> after, Color color)
	{
		List<string> beforeModifiers = before.Where(IsModifierLine).Select(line => line.Text).ToList();
		List<string> afterModifiers = after.Where(IsModifierLine).Select(line => line.Text).ToList();

		_changedModifierTexts.Clear();
		foreach (string text in ExceptWithCounts(afterModifiers, beforeModifiers))
		{
			_changedModifierTexts.Add(text);
		}

		_removedModifierTexts.Clear();
		_removedModifierTexts.AddRange(ExceptWithCounts(beforeModifiers, afterModifiers));
		_craftingFeedbackColor = color;
		_craftingFeedbackUntil = Main.GameUpdateCount + CraftingFeedbackDuration;
	}

	private void ApplyCraftingFeedback(List<DrawableTooltipLine> lines)
	{
		if (Main.GameUpdateCount >= _craftingFeedbackUntil)
		{
			_changedModifierTexts.Clear();
			_removedModifierTexts.Clear();
			return;
		}

		float remaining = (_craftingFeedbackUntil - Main.GameUpdateCount) / (float)CraftingFeedbackDuration;
		float pulse = MathF.Sin(Main.GameUpdateCount * 0.25f) * 0.15f + 0.75f;
		Color highlight = Color.Lerp(Color.White, _craftingFeedbackColor, pulse * remaining);

		for (int i = 0; i < lines.Count; i++)
		{
			DrawableTooltipLine line = lines[i];
			if (!_changedModifierTexts.Contains(line.Text))
			{
				continue;
			}

			var highlightedLine = new TooltipLine(PoTMod.Instance, line.Name, line.Text);
			lines[i] = new DrawableTooltipLine(highlightedLine, i, 0, 0, highlight)
			{
				BaseScale = line.BaseScale,
			};
		}

		foreach (string removedText in _removedModifierTexts)
		{
			string text = Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.ModifierRemoved", removedText);
			var removedLine = new TooltipLine(PoTMod.Instance, "CraftingChangeRemoved", text);
			lines.Add(new DrawableTooltipLine(removedLine, lines.Count, 0, 0, Color.Lerp(Color.Gray, Color.IndianRed, remaining))
			{
				BaseScale = new Vector2(0.85f),
			});
		}
	}

	private static bool IsModifierLine(DrawableTooltipLine line)
	{
		return line.Name.Contains("Affix", StringComparison.Ordinal)
			|| line.Name.Contains("Socket", StringComparison.Ordinal)
			|| line.Name.Contains("Implicit", StringComparison.Ordinal);
	}

	private static List<string> ExceptWithCounts(IEnumerable<string> source, IEnumerable<string> valuesToRemove)
	{
		var counts = valuesToRemove.GroupBy(text => text).ToDictionary(group => group.Key, group => group.Count());
		var result = new List<string>();

		foreach (string text in source)
		{
			if (counts.TryGetValue(text, out int count) && count > 0)
			{
				counts[text] = count - 1;
			}
			else
			{
				result.Add(text);
			}
		}

		return result;
	}

	private static Color GetCraftingEffectColor(int itemType)
	{
		float hue = itemType * 0.61803398875f % 1f;
		return Main.hslToRgb(hue, 0.8f, 0.62f);
	}

	private static bool TryExtractItem(Item item, int requestedAmount, out int extracted)
	{
		extracted = 0;

		Dictionary<int, int> storage = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType;

		if (!storage.TryGetValue(item.type, out int currentAmount) || currentAmount <= 0)
		{
			return false;
		}

		int remainingToInsert = Math.Min(requestedAmount, currentAmount);
		Player player = Main.LocalPlayer;

		for (int i = 0; i < Main.InventoryItemSlotsCount && remainingToInsert > 0; i++)
		{
			ref Item invItem = ref player.inventory[i];

			if (invItem.type != item.type || invItem.stack >= invItem.maxStack)
			{
				continue;
			}

			int moved = Math.Min(invItem.maxStack - invItem.stack, remainingToInsert);
			invItem.stack += moved;
			remainingToInsert -= moved;
			extracted += moved;
		}

		for (int i = 0; i < Main.InventoryItemSlotsCount && remainingToInsert > 0; i++)
		{
			ref Item invItem = ref player.inventory[i];

			if (!invItem.IsAir)
			{
				continue;
			}

			invItem = item.Clone();
			invItem.stack = Math.Min(item.maxStack, remainingToInsert);
			remainingToInsert -= invItem.stack;
			extracted += invItem.stack;
		}

		if (extracted <= 0)
		{
			return false;
		}

		storage[item.type] -= extracted;

		if (storage[item.type] <= 0)
		{
			storage.Remove(item.type);
		}

		return true;
	}

	private static bool TryStoreAllMatchingInventoryItems(int itemType, out int stored)
	{
		stored = 0;

		Player player = Main.LocalPlayer;
		Dictionary<int, int> storage = player.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType;
		storage.TryAdd(itemType, 0);

		for (int i = 0; i < Main.InventoryItemSlotsCount; i++)
		{
			ref Item invItem = ref player.inventory[i];

			if (invItem.type != itemType || invItem.IsAir)
			{
				continue;
			}

			int storageSpace = invItem.maxStack - storage[itemType];

			if (storageSpace <= 0)
			{
				break;
			}

			int moved = Math.Min(invItem.stack, storageSpace);
			storage[itemType] += moved;
			invItem.stack -= moved;
			stored += moved;

			if (invItem.stack <= 0)
			{
				invItem.TurnToAir();
			}
		}

		if (storage[itemType] <= 0)
		{
			storage.Remove(itemType);
		}

		return stored > 0;
	}

	private void Close(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		UIManager.TryToggleOrRegister(Identifier, "Vanilla: Mouse Text", new VirtualBagUIState(), 0, InterfaceScaleType.UI);
	}

	private void BlockClicks(UIElement affectedElement)
	{
		if (affectedElement.ContainsPoint(Main.MouseScreen))
		{
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public override void OnDeactivate()
	{
		RemoveAllChildren();
	}

	public static void UpdateSlot(Item item)
	{
		if (item.IsAir || item.type == ItemID.None)
		{
			return;
		}

		List<DrawableTooltipLine> lines = ItemTooltipBuilder.BuildTooltips(item, Main.LocalPlayer);

		if (!SlottedItem.IsAir && item.ModItem is CurrencyShard { SupportsPouchCrafting: true } shard)
		{
			string text = Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.Apply");
			Color fadeColor = ItemTooltips.Colors.Positive;

			if (!shard.CanUseInPouch(SlottedItem, out string failKey))
			{
				fadeColor = ItemTooltips.Colors.Negative;
				text = Language.GetTextValue("Mods.PathOfTerraria.Misc.ShardInvalidations." + failKey);
			}

			float factor = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.5f + 0.5f;
			var color = Color.Lerp(Color.White, fadeColor, factor);
			var baseTip = new TooltipLine(PoTMod.Instance, "ApplyNotice", text);
			int stack = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType[item.type];
			lines.Add(new DrawableTooltipLine(baseTip, lines.Count, 0, 0, color) { BaseScale = new(0.9f) });
		}

		lines.Add(new DrawableTooltipLine(
			new TooltipLine(PoTMod.Instance, "ExtractOne", Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.ExtractOne")),
			lines.Count, 0, 0, ItemTooltips.Colors.Positive) { BaseScale = new(0.9f) });
		lines.Add(new DrawableTooltipLine(
			new TooltipLine(PoTMod.Instance, "ExtractAll", Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.ExtractAll")),
			lines.Count, 0, 0, ItemTooltips.Colors.Positive) { BaseScale = new(0.9f) });
		lines.Add(new DrawableTooltipLine(
			new TooltipLine(PoTMod.Instance, "StoreAll", Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.StoreAll")),
			lines.Count, 0, 0, ItemTooltips.Colors.Positive) { BaseScale = new(0.9f) });

		Tooltip.Create(new TooltipDescription
		{
			Identifier = "VirtualBagTooltip",
			AssociatedItem = item,
			Lines = lines,
		});
	}

	public void Toggle()
	{
		UIManager.TryToggleOrRegister(Identifier, "Vanilla: Mouse Text", new VirtualBagUIState(), 0, InterfaceScaleType.UI);
	}

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		float oldScale = Main.inventoryScale;
		Main.inventoryScale = 1f;
		base.DrawChildren(spriteBatch);
		Main.inventoryScale = oldScale;
	}
}

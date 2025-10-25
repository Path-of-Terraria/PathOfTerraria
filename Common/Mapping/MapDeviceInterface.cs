using System.Collections.Generic;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Tiles.Furniture;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

#nullable enable
#pragma warning disable IDE0053 // Use expression body for lambda expression

namespace PathOfTerraria.Common.Mapping;

internal sealed class MapDeviceInterface : ModSystem
{
	// All layers but these are hidden when this interface is opened.
	private static readonly HashSet<string> elementWhitelist =
	[
		"Vanilla: Cursor",
		"Vanilla: Mouse Text",
		"Vanilla: Mouse Over",
		"Vanilla: Mouse Item / NPC Head",
		"Vanilla: Player Chat",
		"Vanilla: Interface Logic 1",
		"Vanilla: Interface Logic 2",
		"Vanilla: Interface Logic 3",
		"Vanilla: Interface Logic 4",
		$"{nameof(PathOfTerraria)}: {typeof(Tooltip).FullName}",
		$"{nameof(PathOfTerraria)}: {typeof(MapDeviceState).FullName}", //MapDeviceState.Identifier,
	];

	public static bool Active { get; private set; }
	public static MapDeviceEntity? Entity { get; private set; }
	public static MapDeviceState State { get; private set; } = null!;

	public override void Load()
	{
		if (!Main.dedServ)
		{
			//State = new MapDeviceState();
			//UIManager.Register(MapDeviceState.Identifier, "Vanilla: Mouse Text", State, offset: -1);

			UIManager.PostModifyInterfaceLayers += PostModifyInterfaceLayers;
		}
	}
	public override void PostSetupContent()
	{
		if (!Main.dedServ)
		{
			State = SmartUiLoader.GetUiState<MapDeviceState>();
		}
	}

	private void PostModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		if (!Active) { return; }

		foreach (GameInterfaceLayer layer in layers)
		{
			if (!elementWhitelist.Contains(layer.Name))
			{
				layer.Active = false;
			}
		}
	}

	public override void UpdateUI(GameTime gameTime)
	{
		if (Active && (!Main.playerInventory || Main.LocalPlayer.dead || Entity == null || !TileEntity.ByID.ContainsKey(Entity.ID)))
		{
			Close();
		}
	}

	public static void OpenOrClose(MapDeviceEntity mapDevice)
	{
		if (!Active || Entity != mapDevice)
		{
			Open(mapDevice);
		}
		else
		{
			Close();
		}
	}

	public static void Open(MapDeviceEntity mapDevice)
	{
		Main.LocalPlayer.SetTalkNPC(-1);

		Entity = mapDevice;
		Active = true;
		Main.playerInventory = true;
		SoundEngine.PlaySound(SoundID.MenuOpen);
		State.Visible = true; //UIManager.TryEnable(MapDeviceState.Identifier);
		State.Rebuild();
	}

	public static void Close(bool fromEntity = false)
	{
		if (!fromEntity)
		{
			Entity?.TryClosingInterface();
		}

		Entity = null;
		Active = false;
		if (Main.playerInventory) { SoundEngine.PlaySound(SoundID.MenuClose); }

		State.RemoveAllChildren();
	}
}

internal sealed class MapDeviceState : SmartUiState //UIState
{
	//public static string Identifier { get; } = $"{nameof(PathOfTerraria)}/{nameof(MapDeviceState)}";

	private float openAnimation;

	public UIElement? Window { get; private set; }

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return Math.Max(0, layers.FindIndex(l => l.Name == "Vanilla: Mouse Text") - 1);
	}

	public override void OnInitialize()
	{

	}

	public override void SafeUpdate(GameTime gameTime)
	{
		//base.Update(gameTime);

		openAnimation = Math.Clamp(openAnimation + ((MapDeviceInterface.Active ? 1f : -1f) / 20f), 0f, 1f);

		if (openAnimation == 0f)
		{
			Visible = false; //UIManager.TryDisable(Identifier);
		}

		Recalculate();
	}

	public override void Draw(SpriteBatch sb)
	{
		float alpha = (1f - MathF.Pow(1f - openAnimation, 5f)) * 0.6f;
		Color color = Color.Black.MultiplyRGBA(new Color(Vector4.One * alpha));
		sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(-8, -8, Main.screenWidth + 16, Main.screenHeight + 16), color);

		base.Draw(sb);
	}

	public void Rebuild()
	{
		RemoveAllChildren();

		Asset<Texture2D> backgroundTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/MapDeviceUI", AssetRequestMode.ImmediateLoad);

		UIElement window = Window = this.AddElement(new UIImage(backgroundTexture), e =>
		{
			Vector2 texSize = backgroundTexture.Size();
			e.SetDimensions(x: (0.5f, -(texSize.X * 0.5f)), y: (0.5f, -(texSize.Y * 0.4f)), width: (0.0f, +texSize.X), height: (0f, +texSize.Y));
		});

		if (MapDeviceInterface.Entity is not { } entity) { return; }

		// Open/Close portal button.
		window.AddElement(new UIButton<string>(""), e =>
		{
			e.SetDimensions(x: (0.5f - (0.5f * 0.5f), +0), y: (1.00f, -48), width: (0.5f, +0), height: (0f, +32));
			e.AddComponent(new UIDynamicText(_ => Language.GetTextValue($"Mods.{nameof(PathOfTerraria)}.UI.MapDevice.{(entity.PortalActive ? "Deactivate" : "Activate")}Portal")));

			e.OnLeftClick += OpenOrClosePortalClick;
		});

		// BankItem is used for new slots, as it acts like a chest, but does not cause unwanted network packets to be sent.
		const int CustomSlotContext = ItemSlot.Context.BankItem;

		// Map slot
		var mapSlot = new UIImageItemSlot.SlotWrapper(() => entity.StoredMap, value => entity.StoredMap = value);
		Asset<Texture2D> itemFrame = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/AugmentFrame", AssetRequestMode.ImmediateLoad);
		window.AddElement(new UIHoverImageItemSlot(itemFrame, Asset<Texture2D>.Empty, mapSlot, null, context: CustomSlotContext), e =>
		{
			e.SetDimensions(x: (0.5f, -25), y: (0.0f, +58), width: (0f, +42), height: (0f, +42));
			e.Initialize();

			(e.InactiveScale, e.ActiveScale) = (1.00f, 1.00f);
			e.Predicate = (newItem, oldItem) => newItem is { ModItem: Map };
			e.IsLocked = _ => MapDeviceInterface.Entity is { PortalActive: true };
			e.OnModifyItem += OnModifyMapItem;
		});

		// Storage slots
		UIElement storagePanel = this.AddElement(new UIPanel(), e =>
		{
			var size = new Vector2(300, 256);
			e.SetDimensions(x: (0.5f, -(size.X * 0.5f) + 500), y: (0.5f, -(size.Y * 0.5f) + 100), width: (0f, +300), height: (0f, +256));
		});
		UIGrid storageGrid = storagePanel.AddElement(new UIGrid(), e =>
		{
			e.SetDimensions(x: (0.0f, +0), y: (0.0f, +0), width: (1f, +0), height: (1f, +0));
		});

		for (int i = 0; i < entity.Storage.Length; i++)
		{
			int indexCopy = i;
			var slot = new UIImageItemSlot.SlotWrapper(() => (entity.Storage, indexCopy));
			storageGrid.AddElement(new UIHoverImageItemSlot(itemFrame, Asset<Texture2D>.Empty, slot, null, context: CustomSlotContext), e =>
			{
				e.Initialize();
				e.Width.Set(+44, 0f);
				e.Height.Set(+44, 0f);

				(e.InactiveScale, e.ActiveScale) = (1.00f, 1.10f);
				e.OnModifyItem += (e, oldItem, newItem) => OnModifyStorageItem(e, oldItem, newItem, indexCopy);
			});
		}

		// Inventory slots
		UIElement inventoryPanel = this.AddElement(new UIPanel(), e =>
		{
			var size = new Vector2(520, 300);
			e.SetDimensions(x: (0.5f, -(size.X * 0.5f) - 500), y: (0.5f, -(size.Y * 0.5f) + 100), width: (0f, +size.X), height: (0f, +size.Y));
		});
		UIGrid inventoryGrid = inventoryPanel.AddElement(new UIGrid(), e =>
		{
			// Disable sorting.
			e.ManualSortMethod = _ => { };

			e.SetDimensions(x: (0.0f, +0), y: (0.0f, +0), width: (1f, +0), height: (1f, +0));
		});

		for (int i = 0; i < 50; i++)
		{
			int indexCopy = i;
			var slot = new UIImageItemSlot.SlotWrapper(() => (Main.LocalPlayer.inventory, indexCopy));
			inventoryGrid.AddElement(new UIHoverImageItemSlot(itemFrame, Asset<Texture2D>.Empty, slot, null, ItemSlot.Context.InventoryItem), e =>
			{
				e.OnInitialize();
				e.Width.Set(+44, 0f);
				e.Height.Set(+44, 0f);

				(e.InactiveScale, e.ActiveScale) = (1.00f, 1.10f);
			});
		}

		Recalculate();
	}

	private static void OnModifyMapItem(UIElement element, Item oldItem, Item newItem)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient || MapDeviceInterface.Entity is not { } device) { return; }

		MapDeviceSync.Send(device.ID, MapDeviceSync.Flags.Map, null);
	}

	private static void OnModifyStorageItem(UIElement element, Item oldItem, Item newItem, int slotIndex)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient || MapDeviceInterface.Entity is not { } device) { return; }

		MapDeviceSync.Send(device.ID, MapDeviceSync.Flags.Storage, [slotIndex]);

		_ = (element, oldItem, newItem);
	}

	private static void OpenOrClosePortalClick(UIMouseEvent evt, UIElement element)
	{
		if (MapDeviceInterface.Entity is not { } entity) { return; }

		if (!entity.PortalActive)
		{
			if (MapDeviceInterface.Entity?.TryOpeningPortal() == true)
			{
				MapDeviceInterface.Close();
			}
		}
		else
		{
			MapDeviceInterface.Entity?.TryClosingPortal();
		}
	}
}

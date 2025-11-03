using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Tiles.Furniture;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

#nullable enable
#pragma warning disable IDE0042 // Deconstruct variable declaration
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
			On_Player.ToggleInv += ToggleInvHook;
		}
	}

	// Hijack attempts to close the inventory while this UI is open, preventing the sound from playing.
	private static void ToggleInvHook(On_Player.orig_ToggleInv orig, Player player)
	{
		if (!Active)
		{
			orig(player);
			return;
		}

		Close();
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
		State.Show();
		// The inventory must be "open" so that mouseItems do not auto-drop.
		Main.playerInventory = true;

		SoundEngine.PlaySound(new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/MapDevice/Open")
		{
			PitchVariance = 0.1f,
		});
	}

	public static void Close(bool fromEntity = false)
	{
		if (!fromEntity)
		{
			Entity?.TryClosingInterface();
			return;
		}

		Entity = null;
		Active = false;
		State.OnClosing();

		if (fromEntity)
		{
			Main.playerInventory = false;
			SoundEngine.PlaySound(new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/MapDevice/Close")
			{
				PitchVariance = 0.1f,
			});
		}
	}
}

internal sealed class MapDeviceState : SmartUiState //UIState
{
	// Drawers have to be UI elements to render in time for scissoring areas to function.
	public sealed class DrawerElement(Asset<Texture2D> texture, float middleFactor, float horizontalExtents) : UIElement
	{
		/// <summary> The texture to use. </summary>
		public Asset<Texture2D> Texture { get; set; } = texture;
		/// <summary> The ratio of the middle portion of the drawer, the part that is scaled. </summary>
		public float MiddleFactor { get; set; } = middleFactor;

		protected override void DrawSelf(SpriteBatch sb)
		{
			if (Texture is not { IsLoaded: true, Value: Texture2D texture }) { return; }

			CalculatedStyle elemCalc = GetDimensions();
			Vector2 srcSize = texture.Size();
			Vector2 fullSize = new(elemCalc.Width + (horizontalExtents * 2), srcSize.Y);
			Vector2 dstBase = elemCalc.Center() - (fullSize * 0.5f);
			// Calculate factors at which to split the rendering.
			float middleSrcFactor = MiddleFactor;
			float middleDstFactor = (fullSize.X - (srcSize.X * (1.0f - middleSrcFactor))) / fullSize.X;
			float middleSrcExtent = middleSrcFactor / 2;
			float middleDstExtent = middleDstFactor / 2;
			float[] srcSteps = [0f, 0.5f - middleSrcExtent, 0.5f + middleSrcExtent, 1f];
			float[] dstSteps = [0f, 0.5f - middleDstExtent, 0.5f + middleDstExtent, 1f];

			// Renders as 3 parts.
			for (int j = 0; j < 3; j++)
			{
				Vector4 srcF = new(srcSteps[j] * srcSize.X, 0f, srcSteps[j + 1] * srcSize.X, srcSize.Y);
				Vector4 dstF = new(dstSteps[j] * fullSize.X, 0f, dstSteps[j + 1] * fullSize.X, fullSize.Y);
				dstF += new Vector4(dstBase.X, dstBase.Y, dstBase.X, dstBase.Y);
				(int X, int Y, int Z, int W) srcI = ((int)srcF.X, (int)srcF.Y, (int)srcF.Z, (int)srcF.W);
				(int X, int Y, int Z, int W) dstI = ((int)dstF.X, (int)dstF.Y, (int)dstF.Z, (int)dstF.W);

				Rectangle srcRect = new(srcI.X, srcI.Y, srcI.Z - srcI.X, srcI.W - srcI.Y);
				Rectangle dstRect = new(dstI.X, dstI.Y, dstI.Z - dstI.X, dstI.W - dstI.Y);

				sb.Draw(texture, dstRect, srcRect, Color.White, 0f, default, 0, 0f);
			}
		}
	}

	public sealed class WindowElement : UIElement
	{
		/// <summary> The texture to use. </summary>
		public Asset<Texture2D> Texture { get; set; }

		public WindowElement(Asset<Texture2D> texture)
		{
			Texture = texture;
			OverrideSamplerState = SamplerState.PointClamp;
		}

		protected override void DrawSelf(SpriteBatch sb)
		{
			if (Texture is not { IsLoaded: true, Value: Texture2D texture }) { return; }

			CalculatedStyle dimensions = GetDimensions();
			var center = dimensions.Center().ToPoint().ToVector2();

			sb.Draw(texture, center, null, Color.White, 0f, texture.Size() * 0.5f, 1f, 0, 0f);

			(Color? Injection, Color? Burst, SpriteFrame Frame) = MapDeviceInterface.State.activationEffect;

			// Render injection and portal opening animations.
			// These use a 1x1 texture, so must be rendered at 2x2 with PointClamp sampler.
			if ((Injection ?? Burst).HasValue)
			{
				var effectCenter = (center + new Vector2(-1f, -24f)).ToPoint().ToVector2();

				if (Injection is { } injectionColor)
				{
					Texture2D injectionTexture = ModContent.Request<Texture2D>($"{BasePath}/MapDeviceBase_Injection", AssetRequestMode.ImmediateLoad).Value;
					Rectangle injectionSrcRect = Frame.GetSourceRectangle(injectionTexture);
					sb.Draw(injectionTexture, effectCenter, injectionSrcRect, injectionColor, 0f, injectionSrcRect.Size() * 0.5f, 2f, 0, 0f);
				}
				if (Burst is { } burstColor)
				{
					Texture2D burstTexture = ModContent.Request<Texture2D>($"{BasePath}/MapDeviceBase_Burst", AssetRequestMode.ImmediateLoad).Value;
					Rectangle burstSrcRect = Frame.GetSourceRectangle(burstTexture);
					sb.Draw(burstTexture, effectCenter, burstSrcRect, burstColor, 0f, burstSrcRect.Size() * 0.5f, 2f, 0, 0f);
				}
			}
		}
	}

	private static readonly string BasePath = $"{PoTMod.ModName}/Assets/UI/MapDevice";
	private static readonly string SoundPath = $"{PoTMod.ModName}/Assets/Sounds/MapDevice";
	private static readonly SoundStyle GearSound = new($"{SoundPath}/GearLoop")
	{
		Volume = 0.09f,
		Pitch = -0.25f,
		IsLooped = true,
	};
	private static SlotId gearSoundHandle;

	private bool forceUpdateAnimation;
	private float openingAnimation;
	private float closingAnimation;
	private (float Speed, float Rotation, Vector2 Offset) gear;
	private SpriteFrame buttonLockFrame = new(1, 18);
	private (Color? Injection, Color? Burst, SpriteFrame Frame) activationEffect = (null, null, new(4, 3));
	private (StyleDimension X, StyleDimension Y) storagePositionSrc;
	private (StyleDimension X, StyleDimension Y) storagePositionDst;
	private (StyleDimension X, StyleDimension Y) inventoryPositionSrc;
	private (StyleDimension X, StyleDimension Y) inventoryPositionDst;

	public WindowElement? Window { get; private set; }
	public UIElement? ActionButton { get; private set; }
	public UIGrid? Storage { get; private set; }
	public UIGrid? Inventory { get; private set; }
	public UIElement? StoragePanel { get; private set; }
	public UIElement? InventoryPanel { get; private set; }

	public new bool Visible { get => base.Visible; private set => base.Visible = value; }

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return Math.Max(0, layers.FindIndex(l => l.Name == "Vanilla: Mouse Text") - 1);
	}

	public MapDeviceState()
	{
		OverrideSamplerState = SamplerState.PointClamp;
	}

	internal void OnClosing()
	{
		// Prevent interactions when the UI begins to close.
		IgnoresMouseInteraction = true;
	}

	internal void Show()
	{
		if (Visible) { return; }

		Visible = true;
		OverrideSamplerState = SamplerState.PointClamp;
		// Allow interactions once again.
		IgnoresMouseInteraction = false;

		Refresh();
	}

	internal void Hide()
	{
		if (!Visible) { return; }

		Visible = false;
		openingAnimation = 0f;
		closingAnimation = 0f;
		IgnoresMouseInteraction = true;
		SoundUtils.StopAndInvalidateSoundSlot(ref gearSoundHandle);
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		// Shared animations.
		bool? isButtonAvailable = MapDeviceInterface.Entity != null ? IsButtonAvailable() : null;
		(float oldOpening, float oldClosing) = (openingAnimation, closingAnimation);
		openingAnimation = Math.Clamp(openingAnimation + ((MapDeviceInterface.Active ? +1f : +0f) / 20f), 0f, 1f);
		closingAnimation = Math.Clamp(closingAnimation + ((MapDeviceInterface.Active ? -1f : +1f) / 20f), 0f, 1f);

		// Cogwheel.
		const float GearMaxSpeed = 60f;
		const float GearAcceleration = 1f;
		const float GearDeceleration = 1f;
		const float GearOffset = 1f;
		const float GearOffsetRate = 4f * (MathHelper.Pi / 180f);
		float oldRot = gear.Rotation;
		gear.Speed = Math.Clamp(gear.Speed + (isButtonAvailable == true ? +GearAcceleration : -GearDeceleration), 0f, GearMaxSpeed);
		gear.Rotation += MathHelper.ToRadians(gear.Speed * TimeSystem.LogicDeltaTime);
		float gearRate = gear.Speed / GearMaxSpeed;
		float gearVolume = Main.hasFocus ? gearRate : Math.Min(gearRate, 0.01f);
		float gearPitch = -0.99f + gearRate;
		SoundUtils.UpdateLoopingSound(ref gearSoundHandle, null, gearVolume, gearPitch, GearSound);

		if ((int)MathF.Floor(gear.Rotation / GearOffsetRate) > (int)MathF.Floor(oldRot / GearOffsetRate))
		{
			gear.Offset = Main.rand.NextVector2Circular(GearOffset, GearOffset);
		}
		else
		{
			gear.Offset *= 0.7f;
		}

		// Activation animations.
		const int ActivationAnimationRate = 6;
		const int ActivationAnimationFrameCount = 11;
		if ((TimeSystem.UpdateCount % ActivationAnimationRate) == 0 && (activationEffect.Injection.HasValue || activationEffect.Burst.HasValue))
		{
			ref SpriteFrame frame = ref activationEffect.Frame;
			int frameIndex = (frame.CurrentRow * frame.ColumnCount) + frame.CurrentColumn;

			if (frameIndex < ActivationAnimationFrameCount - 1)
			{
				frame.CurrentColumn = (byte)((frame.CurrentColumn + 1) % frame.ColumnCount);

				if (frame.CurrentColumn == 0)
				{
					frame.CurrentRow++;
					frameIndex++;
				}
			}
		}

		// Button animations.
		const int ButtonAnimationDelay = 4;
		const int ButtonFrameOpen = 12;
		const int ButtonFrameClosed = 0;
		if ((TimeSystem.UpdateCount % ButtonAnimationDelay) == 0 && MapDeviceInterface.Entity != null)
		{
			byte targetRow = (byte)(isButtonAvailable != false ? ButtonFrameOpen : ButtonFrameClosed);

			if (buttonLockFrame.CurrentRow != targetRow)
			{
				// Play audio.
				if (buttonLockFrame.CurrentRow == ButtonFrameOpen)
				{
					SoundEngine.PlaySound(new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/MapDevice/Lock")
					{
						PitchVariance = 0.1f,
						Volume = 0.7f,
					});
				}
				else if (buttonLockFrame.CurrentRow == ButtonFrameClosed)
				{
					SoundEngine.PlaySound(new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/MapDevice/Unlock")
					{
						Pitch = 0.1f,
						PitchVariance = 0.1f,
						Volume = 0.85f,
					});
				}

				buttonLockFrame.CurrentRow = (byte)((buttonLockFrame.CurrentRow + 1) % buttonLockFrame.RowCount);
			}
		}

		// Animate inventory and storage elements.
		if ((forceUpdateAnimation || oldOpening != openingAnimation || oldClosing != closingAnimation) && StoragePanel != null && InventoryPanel != null)
		{
			static StyleDimension LerpDimension(StyleDimension a, StyleDimension b, float t)
			{
				return new(MathHelper.Lerp(a.Precent, b.Precent, t), MathHelper.Lerp(a.Pixels, b.Pixels, t));
			}

			float sharedAnimation = MathHelper.Clamp((1f - MathF.Pow(1f - openingAnimation, 5f)) - (1f - MathF.Pow(1f - closingAnimation, 5f)), 0f, 1f);

			StoragePanel.Left = LerpDimension(storagePositionSrc.X, storagePositionDst.X, sharedAnimation);
			StoragePanel.Top = LerpDimension(storagePositionSrc.Y, storagePositionDst.Y, sharedAnimation);
			StoragePanel.Recalculate();

			InventoryPanel.Left = LerpDimension(inventoryPositionSrc.X, inventoryPositionDst.X, sharedAnimation);
			InventoryPanel.Top = LerpDimension(inventoryPositionSrc.Y, inventoryPositionDst.Y, sharedAnimation);
			InventoryPanel.Recalculate();
		}

		if (closingAnimation >= 0.99f)
		{
			Hide();
		}

		Recalculate();
	}

	protected override void DrawChildren(SpriteBatch sb)
	{
		if (Window != null && Visible) { DrawUnder(sb); }

		base.DrawChildren(sb);

		if (Window != null && Visible) { DrawOver(sb); }
	}

	public void DrawUnder(SpriteBatch sb)
	{
		if (Window == null) { return; }

		float sharedAnimation = (1f - MathF.Pow(1f - openingAnimation, 5f)) - (1f - MathF.Pow(1f - closingAnimation, 5f));

		// Dim the background.
		float dimmingAlpha = sharedAnimation * 0.75f;
		Color dimmingColor = Color.Black.MultiplyRGBA(new Color(Vector4.One * dimmingAlpha));
		sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(-8, -8, Main.screenWidth + 16, Main.screenHeight + 16), dimmingColor);

		// Acquire common data.
		CalculatedStyle windowDimensions = Window.GetDimensions();
		var windowCenter = windowDimensions.Center().ToPoint().ToVector2();

		// Render the cogwheel.
		Asset<Texture2D> gearTexture = ModContent.Request<Texture2D>($"{BasePath}/MapDeviceBase_Gear", AssetRequestMode.ImmediateLoad);
		Vector2 gearPosition = windowCenter + gear.Offset + Vector2.Lerp(new Vector2(-0f, -96f), new Vector2(-0f, -128f), sharedAnimation);
		float gearRotation = MathHelper.ToRadians(MathF.Floor(MathHelper.ToDegrees(gear.Rotation) / 4f) * 4f);
		sb.Draw(gearTexture.Value, gearPosition, null, Color.White, gearRotation, gearTexture.Size() * 0.5f, 1f, 0, 0f);
	}

	public void DrawOver(SpriteBatch sb)
	{
		if (Window == null || ActionButton == null) { return; }

		// Acquire common data.
		CalculatedStyle windowDimensions = Window.GetDimensions();
		var windowCenter = windowDimensions.Center().ToPoint().ToVector2();

		// Render animated button lock.
		var lockDstRect = ActionButton.GetDimensions().ToRectangle();
		lockDstRect.Y -= 2;
		Texture2D lockTexture = ModContent.Request<Texture2D>($"{BasePath}/MapDevice_Trigger_Locked", AssetRequestMode.ImmediateLoad).Value;
		Rectangle lockSrcRect = buttonLockFrame.GetSourceRectangle(lockTexture);
		sb.Draw(lockTexture, lockDstRect, lockSrcRect, Color.White, 0f, default, 0, 0f);
	}

	public override void Refresh()
	{
		RemoveAllChildren();

		forceUpdateAnimation = true;

		if (MapDeviceInterface.Entity is not { } entity) { return; }

		static float CalculateScreenScale(float dimension, float minSizeWithoutScaling)
		{
			const float MinScreenSize = 800f;
			return MathHelper.Clamp((minSizeWithoutScaling - Main.screenWidth) / (minSizeWithoutScaling - MinScreenSize), 0f, 1f);
		}

		// Prepare common values.
		const int CustomSlotContext = ItemSlot.Context.BankItem; // BankItem is used for new slots, as it acts like a chest, but does not cause unwanted network packets to be sent.
		Asset<Texture2D> itemFrame = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/ItemSlot", AssetRequestMode.ImmediateLoad);
		// Prepare values for the drawer elements.
		const float drawerMiddleFactor = 0.1f;
		const float drawerExtents = 48f;
		Asset<Texture2D> drawerTextureL = ModContent.Request<Texture2D>($"{BasePath}/MapDevicePullout_Left", AssetRequestMode.ImmediateLoad);
		Asset<Texture2D> drawerTextureR = ModContent.Request<Texture2D>($"{BasePath}/MapDevicePullout_Right", AssetRequestMode.ImmediateLoad);

		#region Main Window

		Asset<Texture2D> backgroundTexture = ModContent.Request<Texture2D>($"{BasePath}/MapDeviceBase", AssetRequestMode.ImmediateLoad);
		Vector2 windowAreaSize = backgroundTexture.Size();
		var uiOrigin = (Main.ScreenSize.ToVector2() * new Vector2(0.5f, 0.7f)).ToPoint().ToVector2();

		Window = this.AddElement(new WindowElement(backgroundTexture), e =>
		{
			e.SetDimensions
			(
				x: (0f, +(int)(uiOrigin.X - (windowAreaSize.X * 0.5f))),
				y: (0f, +(int)(uiOrigin.Y - (windowAreaSize.Y * 0.5f))),
				width: (0f, +windowAreaSize.X),
				height: (0f, +windowAreaSize.Y)
			);
		});

		// Open/Close portal button.
		Asset<Texture2D> buttonBackground = ModContent.Request<Texture2D>($"{BasePath}/MapDevice_Trigger", AssetRequestMode.ImmediateLoad);
		Asset<Texture2D> buttonBackgroundHover = ModContent.Request<Texture2D>($"{BasePath}/MapDevice_Trigger_Highlight", AssetRequestMode.ImmediateLoad);
		ActionButton = Window.AddElement(new UIHoverImage(buttonBackground, buttonBackgroundHover), e =>
		{
			Vector2 size = buttonBackground.Size();
			e.SetDimensions(x: (0.5f, -(int)(size.X * 0.5f)), y: (0.5f, +108), width: (0.0f, +size.X), height: (0f, +size.Y));

			e.AddElement(new UIButton<string>(""), e =>
			{
				e.SetDimensions(x: (0f, +0), y: (0f, +0), width: (1f, +0), height: (1f, +0));
				e.AddComponent(new UIDynamicText(_ => Language.GetTextValue($"Mods.{nameof(PathOfTerraria)}.UI.MapDevice.{(entity.PortalActive ? "Deactivate" : "Activate")}Portal")));

				e.DrawPanel = false;
				e.OnLeftClick += OpenOrClosePortalClick;
			});
		});

		// Map slot
		Asset<Texture2D> mapSlotTexture = ModContent.Request<Texture2D>($"{BasePath}/MapDeviceBase_Map_Slot", AssetRequestMode.ImmediateLoad);
		Asset<Texture2D> mapIconTexture = ModContent.Request<Texture2D>($"{BasePath}/MapDeviceBase_Map_Icon", AssetRequestMode.ImmediateLoad);
		var mapSlot = new UIImageItemSlot.SlotWrapper(() => entity.StoredMap, value => entity.StoredMap = value);
		Window.AddElement(new UIHoverImageItemSlot(mapSlotTexture, mapIconTexture, mapSlot, null, context: CustomSlotContext), e =>
		{
			e.Initialize();
			e.SetDimensions(x: (0.5f, -24), y: (0.5f, -148), width: (0f, +mapSlotTexture.Width() + 1), height: (0f, +mapSlotTexture.Height()));

			(e.InactiveScale, e.ActiveScale) = (1.00f, 1.00f);
			e.Predicate = (newItem, oldItem) => newItem is { ModItem: Map };
			e.IsLocked = _ => MapDeviceInterface.Entity is { PortalActive: true };
			e.OnModifyItem += OnModifyMapItem;
		});

#endregion

		#region Frags
		//TODO: Frag slots are unimplemented!
#if false
		for (int i = 0; i < 4; i++)
		{
			Item[] dummyContainer = [new Item()];
			var fragSlot = new UIImageItemSlot.SlotWrapper(() => dummyContainer[0], value => dummyContainer[0] = value);

			Asset<Texture2D> fragSlotTexture = ModContent.Request<Texture2D>($"{BasePath}/MapDevice_Frag_Slot", AssetRequestMode.ImmediateLoad);
			Asset<Texture2D> fragIconTexture = ModContent.Request<Texture2D>($"{BasePath}/MapDevice_Frag_Icon", AssetRequestMode.ImmediateLoad);
			Vector2 fragSlotSize = fragSlotTexture.Size();
			Window.AddElement(new UIHoverImageItemSlot(fragSlotTexture, fragIconTexture, fragSlot, null, context: CustomSlotContext), e =>
			{
				e.Initialize();
				float xOffset = (i is 1 or 2 ? (+32) : (+90)) * (i is 0 or 1 ? (-1) : (1));
				float yOffset = +156; //(i is 1 or 2 ? (-24) : (-24));
				e.SetDimensions(x: (0.5f, xOffset - (+fragSlotSize.X * 0.5f)), y: (0.5f, yOffset), width: (0f, +fragSlotSize.X), height: (0f, +fragSlotSize.Y));

				(e.InactiveScale, e.ActiveScale) = (1.00f, 1.00f);

				e.Predicate = (newItem, oldItem) => false;
				e.IsLocked = _ => true;
			});
		}
#endif
		#endregion

		#region Storage

		Point16 storageSlotLayout = new(5, 4);
		float storageSlotScale = MathHelper.Lerp(1.0f, 0.90f, CalculateScreenScale(Main.screenWidth, 860f));
		Vector2 storageSlotSize = new Vector2(50, 50) * storageSlotScale;
		Vector2 storageSize = storageSlotLayout.ToVector2() * storageSlotSize + Vector2.One;
		storagePositionSrc = (new(0.0f, -(storageSize.X * 1.0f) - 000 - 8), new(0.5f, -(storageSize.Y * 0.5f) + 32));
		storagePositionDst = (new(0.0f, +(storageSize.X * 0.0f) + 128 + 8), new(0.5f, -(storageSize.Y * 0.5f) + 32));

		UIElement storageScissor = this.AddElement(new UIElement(), e =>
		{
			e.SetDimensions(x: (0f, +uiOrigin.X), y: (-0.5f, +uiOrigin.Y), width: (0.5f, +0), height: (1.0f, +0));
			e.OverflowHidden = true;
		});
		StoragePanel = storageScissor.AddElement(new DrawerElement(drawerTextureR, drawerMiddleFactor, drawerExtents), e =>
		{
			(e.Left, e.Top, e.Width, e.Height) = (storagePositionSrc.X, storagePositionSrc.Y, new(+storageSize.X, 0f), new(+storageSize.Y, 0f));
		});
		Storage = StoragePanel.AddElement(new UIGrid(), e =>
		{
			e.SetDimensions(x: (0f, +0), y: (0f, +0), width: (1.0f, +0), height: (1.0f, +0));
			// No sorting, no padding, with overflow.
			e.ListPadding = 0f;
			e.OverflowHidden = false;
			e.ManualSortMethod = _ => { };
		});

		for (int i = 0; i < entity.Storage.Length; i++)
		{
			int indexCopy = i;
			var slot = new UIImageItemSlot.SlotWrapper(() => (entity.Storage, indexCopy));
			Storage.AddElement(new UIHoverImageItemSlot(itemFrame, Asset<Texture2D>.Empty, slot, null, context: CustomSlotContext), e =>
			{
				e.Initialize();
				e.Width.Set(+storageSlotSize.X, 0f);
				e.Height.Set(+storageSlotSize.Y, 0f);

				(e.InactiveScale, e.ActiveScale) = (storageSlotScale, storageSlotScale * 1.10f);
				e.OnModifyItem += (e, oldItem, newItem) => OnModifyStorageItem(e, oldItem, newItem, indexCopy);
			});
		}

		#endregion

		#region Inventory

		Point16 inventorySlotLayout = new(10, 5);
		float inventorySlotScale = MathHelper.Lerp(1.00f, 0.58f, CalculateScreenScale(Main.screenWidth, 1200f));
		Vector2 inventorySlotSize = new Vector2(46, 46) * inventorySlotScale;
		Vector2 inventorySize = inventorySlotLayout.ToVector2() * inventorySlotSize + Vector2.One;
		inventoryPositionSrc = (new(1.0f, +(inventorySize.X * 0.0f) + 000 + 8), new(0.5f, -(inventorySize.Y * 0.5f) + 32));
		inventoryPositionDst = (new(1.0f, -(inventorySize.X * 1.0f) - 128 - 8), new(0.5f, -(inventorySize.Y * 0.5f) + 32));

		UIElement inventoryScissor = this.AddElement(new UIElement(), e =>
		{
			e.SetDimensions(x: (-0.5f, +uiOrigin.X), y: (-0.5f, +uiOrigin.Y), width: (0.5f, +0), height: (1.0f, +0));
			e.OverflowHidden = true;
		});
		InventoryPanel = inventoryScissor.AddElement(new DrawerElement(drawerTextureL, drawerMiddleFactor, drawerExtents), e =>
		{
			(e.Left, e.Top, e.Width, e.Height) = (inventoryPositionSrc.X, inventoryPositionSrc.Y, new(+inventorySize.X, 0f), new(+inventorySize.Y, 0f));
		});
		Inventory = InventoryPanel.AddElement(new UIGrid(), e =>
		{
			e.SetDimensions(x: (0f, +0), y: (0f, +0), width: (1.0f, +0), height: (1.0f, +0));
			// No sorting, no padding, with overflow.
			e.ListPadding = 0f;
			e.OverflowHidden = false;
			e.ManualSortMethod = _ => { };
		});

		for (int i = 0; i < 50; i++)
		{
			int indexCopy = i;
			var slot = new UIImageItemSlot.SlotWrapper(() => (Main.LocalPlayer.inventory, indexCopy));
			Inventory.AddElement(new UIHoverImageItemSlot(itemFrame, Asset<Texture2D>.Empty, slot, null, ItemSlot.Context.InventoryItem), e =>
			{
				e.Initialize();
				e.Width.Set(+inventorySlotSize.X, 0f);
				e.Height.Set(+inventorySlotSize.Y, 0f);

				(e.InactiveScale, e.ActiveScale) = (inventorySlotScale, inventorySlotScale * 1.10f);
			});
		}

		#endregion

		// Put the main window under everything else.
		RemoveChild(Window);
		Append(Window);

		Recalculate();
		Main.QueueMainThreadAction(Recalculate);
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

	private static bool IsButtonAvailable()
	{
		if (MapDeviceInterface.Entity is not { } entity) { return true; }

		return entity.PortalActive ? entity.TryClosingPortal(evalMode: true) : entity.TryOpeningPortal(evalMode: true);
	}

	private static void OpenOrClosePortalClick(UIMouseEvent evt, UIElement element)
	{
		if (MapDeviceInterface.Entity is not { } entity) { return; }

		if (MapDeviceInterface.State is not { } state) { return; }

		if (!entity.PortalActive)
		{
			if (entity.TryOpeningPortal() == true)
			{
				//state.activationEffect.Burst = ColorUtils.FromHexRgb(0x4a433f);
				state.activationEffect.Burst = ColorUtils.FromHexRgb(0x958982);
				state.activationEffect.Injection = ColorUtils.FromHexRgb(0x13d6ff);
				state.activationEffect.Frame = state.activationEffect.Frame.With(0, 0);
			}
		}
		else
		{
			entity.TryClosingPortal();

			state.activationEffect.Burst = null;
			state.activationEffect.Burst = ColorUtils.FromHexRgb(0x958982);
			state.activationEffect.Injection = null;
			state.activationEffect.Frame = state.activationEffect.Frame.With(0, 0);
		}
	}

	[UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "OverflowHiddenRasterizerState")]
	private static extern ref readonly RasterizerState OverflowHiddenRasterizer(UIElement? _);
}

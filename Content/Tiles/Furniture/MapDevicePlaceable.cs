using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Mapping;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Placeable;
using PathOfTerraria.Core.Audio;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

#nullable enable

namespace PathOfTerraria.Content.Tiles.Furniture;

public class MapDevicePlaceable : ModTile
{
	public const int FullWidth = 18 * 5;
	public const int FullHeight = 18 * 5;

	private static Asset<Texture2D>? _portalTex;

	/// <summary> This is the portal that will appear above. </summary>
	protected virtual string PortalTexturePath => $"{PoTMod.ModName}/Assets/Items/Placeable/Portal";

	public static Point16 GetTopLeft(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		i -= tile.TileFrameX % FullWidth / 18;
		j -= tile.TileFrameY % FullHeight / 18;

		return new(i, j);
	}

	public static bool IsPointOnPortal(Point16 origin, Point16 point)
	{
		return point.Y <= origin.Y + 3;
	}

	public override void SetStaticDefaults()
	{
		Main.tileNoAttach[Type] = true;
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.InteractibleByNPCs[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
		TileObjectData.newTile.Width = 5;
		TileObjectData.newTile.Height = 5;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<MapDeviceEntity>().Hook_AfterPlacement, -1, 0, false);
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.DrawYOffset = 2;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.AnchorBottom = new AnchorData(TileObjectData.newTile.AnchorBottom.type, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.Origin = new(3, 4);
		TileObjectData.addTile(Type);

		RegisterItemDrop(ModContent.ItemType<MapDevice>());
		AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.MapDevice"));
	}

	public override bool CreateDust(int i, int j, ref int type)
	{
		return false;
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		Point16 coords = GetTopLeft(i, j);
		ModContent.GetInstance<MapDeviceEntity>().Kill(coords.X, coords.Y);
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

		if (!TryGetEntity(i, j, out MapDeviceEntity? entity))
		{
			return;
		}

		Point16 tilePoint = entity.Position;
		Vector2 worldPos = tilePoint.ToWorldCoordinates(40f, 64f);

		entity.UpdateAudio(worldPos);

		if (tile.TileFrameX / 18 % 5 == 0 || tile.TileFrameY != 0)
		{
			return;
		}

		if (!entity.PortalActive)
		{
			return;
		}

		// This is lighting-mode specific, always include this if you draw tiles manually
		var offScreen = new Vector2(Main.drawToScreen ? 0 : Main.offScreenRange);

		// Take the tile, check if it actually exists
		tile = Main.tile[tilePoint];

		if (!tile.HasTile)
		{
			return;
		}

		// Start loading the texture and wait for it.
		if ((_portalTex ??= ModContent.Request<Texture2D>(PortalTexturePath)) is not { IsLoaded: true, Value: { } texture })
		{
			return;
		}

		Color baseColor = Color.White;
		Texture2D? itemTex = null;

		if (entity.Injection is { } injection)
		{
			MapResource resource = MapResources.Get(injection);
			baseColor = resource.AccentColor;
		}
		else if (entity.StoredMap is { IsAir: false, type: int itemType })
		{
			baseColor = ColorUtils.FromHexRgb(0x_0097ff);
			itemTex = TextureAssets.Item[itemType].Value;
		}

		// Get the initial draw parameters
		int frameY = tile.TileFrameX % 90 / FullWidth; // Picks the frame on the sheet based on the placeStyle of the item
		Rectangle frame = texture.Frame(1, 1, 0, frameY);
		Vector2 origin = frame.Size() / 2f;
		Color color = Color.Lerp(Lighting.GetColor(tilePoint.X, tilePoint.Y), Color.White, 0.4f).MultiplyRGBA(baseColor);
		bool direction = tile.TileFrameY / FullHeight != 0; // This is related to the alternate tile data we registered before
		SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		// Some math magic to make it smoothly move up and down over time
		float offset = MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 5f);
		Vector2 drawPos = worldPos + offScreen - Main.screenPosition + new Vector2(0f, -36f) + new Vector2(0f, offset * 7f);

		// Draw the main texture
		for (int k = 0; k < 3; ++k)
		{
			float rotation = Main.GlobalTimeWrappedHourly * 7f * (k % 2 == 0 ? -1 : 1);
			Color drawColor = color * (1 - k * 0.2f);
			spriteBatch.Draw(texture, drawPos, frame, drawColor with { A = 150 }, rotation, origin, 1.3f - k * 0.2f, effects, 0f);
		}

		if (itemTex != null)
		{
			spriteBatch.Draw(itemTex, drawPos, null, new Color(140, 230, 255) * 0.95f, 0f, itemTex.Size() / 2f, 0.75f, effects, 0f);
		}

		// Draw the periodic glow effect
		float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 2f) * 0.3f + 0.7f;
		Color effectColor = color;
		effectColor.A = 0;
		effectColor *= 0.1f * scale;
		for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI))
		{
			spriteBatch.Draw(texture, drawPos + (MathHelper.TwoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame, effectColor, 0f, origin, 1f, effects, 0f);
		}
	}

	public override bool RightClick(int x, int y)
	{
		if (TryGetEntity(x, y, out MapDeviceEntity? entity) && Main.netMode != NetmodeID.Server)
		{
			if (IsPointOnPortal(entity.Position, new(x, y)))
			{
				entity.TryEnteringPortal();
			}
			else
			{
				entity.TryOpeningInterface();
			}
		}

		return true;
	}

	public override void MouseOver(int x, int y)
	{
		Player player = Main.LocalPlayer;
		player.noThrow = 2;

		if (TryGetEntity(x, y, out MapDeviceEntity? entity))
		{
			if (IsPointOnPortal(entity.Position, new(x, y)))
			{
				if (entity.TryEnteringPortal(evalMode: true))
				{
					player.cursorItemIconEnabled = true;
					player.cursorItemIconID = entity.StoredMap.type;
				}
			}
			else if (entity.TryOpeningInterface(evalMode: true))
			{
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = ModContent.ItemType<MapDevice>();
			}
		}
	}

	private static bool TryGetEntity(int x, int y, [NotNullWhen(true)] out MapDeviceEntity? entity)
	{
		Point16 topLeft = GetTopLeft(x, y);

		if (TileEntity.ByPosition.TryGetValue(topLeft, out TileEntity? tileEntity) && tileEntity is MapDeviceEntity mapEntity)
		{
			entity = mapEntity;
			return true;
		}

		entity = null;
		return false;
	}
}

internal class MapDeviceEntity : ModTileEntity
{
	private static SoundStyle PortalSound => new($"{nameof(PathOfTerraria)}/Assets/Sounds/MapDevice/PortalLoop")
	{
		Volume = 0.4f,
		IsLooped = true,
	};

	public static int StorageSize { get; } = 5 * 4;

	private SlotId portalSoundHandle;
	private float portalSoundVolume;

	public Item StoredMap { get; set; }
	public Item[] Storage { get; set; }
	public bool PortalActive { get; set; }
	public int PortalUsesLeft { get; set; }
	public int? InteractingPlayer { get; private set; }
	/// <summary> Which map destination resource is currently injected into the device, and what quantity of it. </summary>
	public int? Injection { get; set; }

	public MapDeviceEntity()
	{
		StoredMap = new();
		Storage = new Item[StorageSize];
		for (int i = 0; i < Storage.Length; i++) { Storage[i] = new(); }
	}

	public override bool IsTileValidForEntity(int x, int y)
	{
		return Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<MapDevicePlaceable>();
	}

	public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
	{
		var tileData = TileObjectData.GetTileData(type, style, alternate);
		int topLeftX = i - tileData.Origin.X;
		int topLeftY = j - tileData.Origin.Y;

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, tileData.Width, tileData.Height);
			NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: Type);
			return -1;
		}

		return Place(topLeftX, topLeftY);
	}

	public override void OnKill()
	{
		// Drop storage items. NewItem checks for it not being air.
		foreach (Item item in Storage)
		{
			if (item is not { IsAir: false }) { continue; }

			Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), Position.ToWorldCoordinates(), item);
		}

		// Drop the map, but only if the portal has not been activated.
		if (!PortalActive && StoredMap is { IsAir: false })
		{
			Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), Position.ToWorldCoordinates(), StoredMap);
		}
	}

	public override void Update()
	{
		// Close UI if player is dead, gone, or too far away.
		if (InteractingPlayer.HasValue && Main.player[InteractingPlayer.Value] is { } player)
		{
			var tileData = TileObjectData.GetTileData(Main.tile[Position].TileType, 1);
			var worldPos = Position.ToWorldCoordinates().ToPoint();
			var tileRect = new Rectangle(worldPos.X, worldPos.Y, tileData.Width * TileUtils.TileSizeInPixels, tileData.Height * TileUtils.TileSizeInPixels);
			Point checkPoint = tileRect.ClosestPointInRect(player.Center).ToTileCoordinates();
			if (!player.active || player.dead || !player.IsInTileInteractionRange(checkPoint.X, checkPoint.Y, TileReachCheckSettings.Simple))
			{
				TryClosingInterface();
			}
		}
	}

	public override void SaveData(TagCompound tag)
	{
		if (StoredMap is { IsAir: false })
		{
			tag.Add("item", ItemIO.Save(StoredMap));
		}

		var storage = new TagCompound();

		for (int i = 0; i < Storage.Length; i++)
		{
			if (Storage[i] is { IsAir: false } item)
			{
				storage[i.ToString()] = ItemIO.Save(item);
			}
		}

		if (storage.Count > 0) { tag.Add("storage", storage); }
	}
	public override void LoadData(TagCompound tag)
	{
		if (tag.TryGet("item", out TagCompound itemTag))
		{
			StoredMap = ItemIO.Load(itemTag);
		}

		if (tag.TryGet("storage", out TagCompound storage))
		{
			foreach (KeyValuePair<string, object> pair in storage)
			{
				if (int.TryParse(pair.Key, out int key) && key >= 0 && key < Storage.Length && ItemIO.Load((TagCompound)pair.Value) is { IsAir: false } item)
				{
					Storage[key] = item;
				}
			}
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		MapDeviceSync.WriteInto(writer, this, MapDeviceSync.Flags.FullSync, null);
	}
	public override void NetReceive(BinaryReader reader)
	{
		MapDeviceSync.ReadInto(sender: byte.MaxValue, reader, this, out _);
	}

	public void UpdateAudio(Vector2 center)
	{
		float targetVolume = PortalActive ? new ExponentialRange(0f, 1024f, 2f).DistanceFactor(Main.LocalPlayer.Distance(center)) : 0f;
		portalSoundVolume = MathUtils.StepTowards(portalSoundVolume, targetVolume, MathF.Max(0f, TimeSystem.RenderDeltaTime));

		SoundUtils.UpdateLoopingSound(ref portalSoundHandle, center, portalSoundVolume, null, PortalSound);
		TileSoundTracker.Track(Position, new()
		{
			Handle = portalSoundHandle,
			EntityType = GetType(),
		});
	}

	/// <summary>
	/// Attempts to have the local player or <paramref name="netSender"/> open the map device UI.
	/// <br/> Returns whether an attempt to perform the interaction will be made, not whether it will succeed.
	/// </summary>
	public bool TryOpeningInterface(bool evalMode = false, byte? netSender = null)
	{
		// Do not bother if already open on client.
		if (!Main.dedServ && MapDeviceInterface.Active)
		{
			return false;
		}

		// Short-circuit in evaluation mode.
		if (evalMode) { return true; }

		// Request interaction.
		if (Main.netMode == NetmodeID.MultiplayerClient && !netSender.HasValue)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.OpenInterface);
			return true;
		}

		// Apply client effect.
		if (Main.netMode != NetmodeID.Server)
		{
			MapDeviceInterface.Open(this);
		}
		
		// Apply world effect.
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			byte? playerId = Main.netMode == NetmodeID.SinglePlayer ? (byte)Main.myPlayer : netSender;
			Debug.Assert(playerId != null);

			// If not open by the sender - refuse interaction and notify of the refusal.
			if (InteractingPlayer.HasValue && InteractingPlayer != playerId)
			{
				if (Main.netMode == NetmodeID.Server)
				{
					var text = NetworkText.FromKey($"Mods.{nameof(PathOfTerraria)}.UI.MapDevice.AlreadyInUse");
					ChatHelper.SendChatMessageToClient(text, Color.PaleVioletRed, playerId.Value);
				}

				return false;
			}

			InteractingPlayer = playerId.Value;

			// Acknowledge interaction.
			if (Main.netMode == NetmodeID.Server)
			{
				MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.OpenInterface, toClient: playerId.Value);

				// Synchronize map resources just in case the world or the client have never received them.
				MapResources.RequestMessage.Send();
			}
		}

		return true;
	}

	/// <summary>
	/// Writes down that a given player has closed the interface, if one has been open.
	/// <br/> Returns whether an attempt to perform the interaction will be made, not whether it will succeed.
	/// </summary>
	public bool TryClosingInterface(byte? netSender = null)
	{
		// Request interaction.
		if (Main.netMode == NetmodeID.MultiplayerClient && !netSender.HasValue)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.CloseInterface);
			return true;
		}

		// Apply clientside effect.
		if (Main.netMode != NetmodeID.Server)
		{
			MapDeviceInterface.Close(fromEntity: true);
		}
		
		// Apply world effect.
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			// Silently refuse if no one is interacting, or if the sender exists and is not the one interacting.
			if (InteractingPlayer == null || (netSender.HasValue && InteractingPlayer != netSender.Value))
			{
				return false;
			}

			if (Main.netMode == NetmodeID.Server)
			{
				// Notify the client currently interacting.
				MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.CloseInterface, toClient: InteractingPlayer.Value);
			}

			InteractingPlayer = null;
		}

		return true;
	}

	/// <summary>
	/// Attempts to have the local player enter the portal.
	/// <br/> Returns whether an attempt to perform the interaction will be made, not whether it will succeed.
	/// </summary>
	public bool TryEnteringPortal(bool evalMode = false, byte? netSender = null)
	{
		if (!PortalActive || StoredMap is not { IsAir: false, ModItem: Map map })
		{
			return false;
		}

		// Short-circuit in evaluation mode.
		if (evalMode) { return true; }

		// Request interaction.
		if (Main.netMode == NetmodeID.MultiplayerClient && netSender == null)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.EnterPortal);
			return true;
		}

		if (Main.netMode != NetmodeID.Server)
		{
			// Finish clientside interaction.
			map.OpenMap();
		}
		else
		{
			// Acknowledge interaction.
			Debug.Assert(netSender != null);
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.EnterPortal, toClient: netSender.Value);
		}

		// Roll down uses and close the portal if needed.
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			PortalUsesLeft--;

			if (PortalUsesLeft <= 0)
			{
				TryClosingPortal();
			}
		}

		return true;
	}

	/// <summary>
	/// Attempts to open a portal for the map stored within the device.
	/// <br/> Returns whether an attempt to perform the interaction will be made, not whether it will succeed.
	/// </summary>
	public bool TryOpeningPortal(bool evalMode = false, byte? netSender = null)
	{
		if (PortalActive)
		{
			return false;
		}

		if (StoredMap is not { IsAir: false, ModItem: Map } && Injection is null)
		{
			return false;
		}

		// Short-circuit in evaluation mode.
		if (evalMode) { return true; }

		// Request interaction.
		if (Main.netMode == NetmodeID.MultiplayerClient && netSender == null)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.OpenPortal);
			return true;
		}

		// Refuse interaction if sent by someone other than the interacting player.
		if (Main.netMode == NetmodeID.Server && netSender.HasValue && InteractingPlayer != netSender)
		{
			return false;
		}

		PortalActive = true;
		PortalUsesLeft = int.MaxValue;

		if (StoredMap is { IsAir: false, ModItem: Map map })
		{
			PortalUsesLeft = map.MaxUses;
		}
		else if (Injection is { } injection)
		{
			MapResource resource = MapResources.Get(injection);
			PortalUsesLeft = resource.PortalUses;

			// Consume resource.
#if !DEBUG || false
			MapResources.AddOrRemove(injection, -resource.Cost);
#endif
		}

		// Broadcast the interaction.
		if (Main.netMode == NetmodeID.Server)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.OpenPortal);
		}

		if (!Main.dedServ)
		{
			//TODO: Replace.
			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Skills/BerserkStart")
			{
				Volume = 0.75f,
				PitchVariance = 0.2f,
			});
		}

		return true;
	}

	/// <summary>
	/// Attempts to close the currently open portal.
	/// <br/> Returns whether an attempt to perform the interaction will be made, not whether it will succeed.
	/// </summary>
	public bool TryClosingPortal(bool evalMode = false, byte? netSender = null)
	{
		if (!PortalActive)
		{
			return false;
		}

		// Short-circuit in evaluation mode.
		if (evalMode) { return true; }

		// Request interaction.
		if (Main.netMode == NetmodeID.MultiplayerClient && netSender == null)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.ClosePortal);
			return true;
		}

		// Refuse interaction if sent by someone other than the interacting player.
		if (Main.netMode == NetmodeID.Server && netSender.HasValue && InteractingPlayer != netSender)
		{
			return false;
		}

		// The map is destroyed if the portal is ever closed.
		StoredMap = new();
		PortalActive = false;
		PortalUsesLeft = 0;
		Injection = null;

		// Broadcast the interaction.
		if (Main.netMode == NetmodeID.Server)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.ClosePortal);
		}

		if (!Main.dedServ)
		{
			//TODO: Replace.
			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Skills/BerserkEnd")
			{
				Volume = 0.75f,
				PitchVariance = 0.2f,
			});
		}

		return true;
	}

	/// <summary>
	/// Attempts to "inject" a specific resource into the map device.
	/// <br/> Returns whether an attempt to perform the interaction will be made, not whether it will succeed.
	/// </summary>
	public bool TryInjectingResource(int resourceId, bool evalMode = false, byte? netSender = null)
	{
		if (PortalActive)
		{
			return false;
		}

		// Refuse if there is already a map or another injection.
		if (StoredMap is { IsAir: false } || Injection.HasValue)
		{
			return false;
		}

		// Refuse interaction if the resource ID is invalid or if there is not enough of it.
		if (!MapResources.TryGet(resourceId, out MapResource mapResources) || mapResources.Value < mapResources.Cost)
		{
			return false;
		}

		// Short-circuit in evaluation mode.
		if (evalMode) { return true; }

		// Request interaction.
		if (Main.netMode == NetmodeID.MultiplayerClient && netSender == null)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.InjectResource, arg: resourceId);
			return true;
		}

		// Refuse interaction if sent by someone other than the interacting player.
		if (Main.netMode == NetmodeID.Server && netSender.HasValue && InteractingPlayer != netSender)
		{
			return false;
		}

		Injection = resourceId;

		// Broadcast the interaction.
		if (Main.netMode == NetmodeID.Server)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.InjectResource);
		}

		return true;
	}

	/// <summary>
	/// Attempts to eject the resource currently injected into the map device.
	/// <br/> Returns whether an attempt to perform the interaction will be made, not whether it will succeed.
	/// </summary>
	public bool TryEjectingResource(bool evalMode = false, byte? netSender = null)
	{
		if (PortalActive)
		{
			return false;
		}

		// Refuse if there is no injection.
		if (!Injection.HasValue)
		{
			return false;
		}

		// Short-circuit in evaluation mode.
		if (evalMode) { return true; }

		// Request interaction.
		if (Main.netMode == NetmodeID.MultiplayerClient && netSender == null)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.EjectResource);
			return true;
		}

		// Refuse interaction if sent by someone other than the interacting player.
		if (Main.netMode == NetmodeID.Server && netSender.HasValue && InteractingPlayer != netSender)
		{
			return false;
		}

		Injection = null;

		// Broadcast the interaction.
		if (Main.netMode == NetmodeID.Server)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.EjectResource);
		}

		return true;
	}
}

internal class MapDeviceInteraction : Handler
{
	public enum Kind : byte
	{
		OpenInterface,
		CloseInterface,
		OpenPortal,
		ClosePortal,
		EnterPortal,
		InjectResource,
		EjectResource,
	}

	public static void Send(int entityId, Kind kind, int arg = 0, int toClient = -1, int ignoreClient = -1)
	{
		ModPacket packet = Networking.GetPacket<MapDeviceInteraction>();
		packet.Write((int)entityId);
		packet.Write((byte)kind);
		packet.Write7BitEncodedInt(arg);
		packet.Send(toClient, ignoreClient);
	}

	internal override void Receive(BinaryReader reader, byte sender)
	{
		int entityId = reader.ReadInt32();
		var kind = (Kind)reader.ReadByte();
		int arg = reader.Read7BitEncodedInt();

		if (TileEntity.ByID.TryGetValue(entityId, out TileEntity? tileEntity) && tileEntity is MapDeviceEntity mapEntity)
		{
			switch (kind)
			{
				case Kind.OpenInterface: mapEntity.TryOpeningInterface(netSender: sender); break;
				case Kind.CloseInterface: mapEntity.TryClosingInterface(netSender: sender); break;
				case Kind.OpenPortal: mapEntity.TryOpeningPortal(netSender: sender); break;
				case Kind.ClosePortal: mapEntity.TryClosingPortal(netSender: sender); break;
				case Kind.EnterPortal: mapEntity.TryEnteringPortal(netSender: sender); break;
				case Kind.InjectResource: mapEntity.TryInjectingResource(resourceId: arg, netSender: sender); break;
			}
		}
	}
}

internal class MapDeviceSync : Handler
{
	[Flags]
	public enum Flags : byte
	{
		Status = 1 << 0,
		Map = 1 << 1,
		Storage = 1 << 2,
		Injection = 1 << 3,
		FullSync = Status | Map | Storage | Injection,
	}

	public static void Send(int entityId, Flags flags, IEnumerable<int>? itemIndices, int toClient = -1, int ignoreClient = -1)
	{
		if (!TileEntity.ByID.TryGetValue(entityId, out TileEntity? tileEntity) || tileEntity is not MapDeviceEntity device)
		{
			return;
		}

		// Never send status from clients.
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			flags &= ~Flags.Status;
		}

		if (flags == 0)
		{
			return;
		}

		ModPacket packet = Networking.GetPacket<MapDeviceSync>();
		packet.Write((int)entityId);
		WriteInto(packet, device, flags, itemIndices);
		packet.Send(toClient, ignoreClient);
	}
	public static void WriteInto(BinaryWriter writer, MapDeviceEntity device, Flags flags, IEnumerable<int>? itemIndices)
	{
		writer.Write((byte)flags);

		if (flags.HasFlag(Flags.Status))
		{
			writer.Write((bool)device.PortalActive);
			writer.Write7BitEncodedInt(device.PortalUsesLeft);
		}

		if (flags.HasFlag(Flags.Map))
		{
			ItemIO.Send(device.StoredMap, writer, writeStack: true);
		}

		if (flags.HasFlag(Flags.Storage))
		{
			// Write preceding masks.
			Span<BitMask<byte>> masks = stackalloc BitMask<byte>[(int)MathF.Ceiling(device.Storage.Length / 8f)];
			foreach (int storageIndex in itemIndices ?? Enumerable.Range(0, device.Storage.Length))
			{
				if (device.Storage[storageIndex] is { IsAir: false })
				{
					(int div, int rem) = Math.DivRem(storageIndex, 8);
					masks[div].Set(rem);
				}
			}
			foreach (BitMask<byte> mask in masks)
			{
				writer.Write((byte)mask.Value);
			}

			// Write item data.
			for (int maskIndex = 0, storageIndex = 0; maskIndex < masks.Length; maskIndex++)
			{
				foreach (int bitIndex in masks[maskIndex])
				{
					ItemIO.Send(device.Storage[storageIndex], writer, writeStack: true);
				}
			}
		}

		if (flags.HasFlag(Flags.Injection))
		{
			writer.Write7BitEncodedInt(device.Injection ?? -1);
		}
	}

	private static bool Receive(byte sender, BinaryReader reader, out int entityId, out Flags flags)
	{
		entityId = reader.ReadInt32();

		// Try to acquire the entity. We still have to finish reading the packet whole if we fail.
		MapDeviceEntity? mapEntity = TileEntity.ByID.TryGetValue(entityId, out TileEntity? te) && te is MapDeviceEntity e ? e : null;

		ReadInto(sender, reader, mapEntity, out flags);

		return mapEntity != null;
	}
	public static void ReadInto(byte sender, BinaryReader reader, MapDeviceEntity? mapEntity, out Flags flags)
	{
		flags = (Flags)reader.ReadByte();

		if (flags.HasFlag(Flags.Status))
		{
			bool isActive = reader.ReadBoolean();
			int remainingUses = reader.Read7BitEncodedInt();

			// On servers, refuse applying status data if it is received from clients.
			if (mapEntity != null && Main.netMode != NetmodeID.Server)
			{
				mapEntity.PortalActive = isActive;
				mapEntity.PortalUsesLeft = remainingUses;
			}
		}

		if (flags.HasFlag(Flags.Map))
		{
			// On servers, refuse applying client's map item if the portal has already been opened.
			bool useDummy = (Main.netMode == NetmodeID.Server && mapEntity?.PortalActive == true) || mapEntity?.StoredMap == null;
			Item mapSlot = useDummy ? new() : mapEntity!.StoredMap;
			ItemIO.Receive(mapSlot!, reader, readStack: true);

			// Drop the received map if it went into a dummy slot.
			if (Main.netMode == NetmodeID.Server && mapSlot != mapEntity?.StoredMap && mapSlot is { IsAir: false } && Main.player[sender] is { active: true } player)
			{
				Item.NewItem(null, player.Center, mapSlot);
			}
		}

		if (flags.HasFlag(Flags.Storage))
		{
			// Read masks.
			Span<BitMask<byte>> masks = stackalloc BitMask<byte>[(int)MathF.Ceiling(MapDeviceEntity.StorageSize / 8f)];
			for (int i = 0; i < masks.Length; i++) { masks[i] = new(reader.ReadByte()); }

			// On servers, refuse applying client's storage items if they are not the one interacting with the device.
			bool forceDummy = Main.netMode == NetmodeID.Server && mapEntity?.InteractingPlayer != sender;

			// Read items.
			for (int maskIndex = 0, storageIndex = 0; maskIndex < masks.Length; maskIndex++)
			{
				foreach (int bitIndex in masks[maskIndex])
				{
					bool useDummy = forceDummy || mapEntity?.Storage[storageIndex] == null;
					Item item = useDummy ? new() : mapEntity!.Storage[storageIndex];
					ItemIO.Receive(item, reader, readStack: true);
				}
			}
		}

		if (flags.HasFlag(Flags.Injection))
		{
			int resourceId = reader.Read7BitEncodedInt();

			// Refuse applying data if it is received from clients.
			if (mapEntity != null && Main.netMode != NetmodeID.Server)
			{
				mapEntity.Injection = MapResources.TryGet(resourceId, out _) ? resourceId : null;
			}
		}
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		if (Receive(sender, reader, out int entityId, out Flags flags))
		{
			Send(entityId, flags, null, ignoreClient: sender);
		}
	}
	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		Receive(sender, reader, out _, out _);
	}
}
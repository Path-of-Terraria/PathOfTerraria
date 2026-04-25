using Mono.Cecil;
using PathOfTerraria.Common.Conflux;
using PathOfTerraria.Common.Mapping;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Placeable;
using PathOfTerraria.Core.Audio;
using PathOfTerraria.Core.Camera;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Terraria;
using SubworldLibrary;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

#nullable enable

namespace PathOfTerraria.Content.Tiles.Furniture;

[Obsolete($"Replaced by {nameof(MapDeviceTile)}", error: true)]
public sealed class MapDevicePlaceable : MapDeviceTile
{
	protected override bool IsLegacy => true;
	
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		// Override tile data with legacy parameters.
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
	}
}

public class MapDeviceTile : ModTile
{
	//private static Asset<Texture2D>? _portalTex;
	private static Asset<Texture2D>? _spikesTex;
	private static Asset<Texture2D>? _backTex;

	protected virtual bool IsLegacy => false;
	protected virtual string PortalTexturePath => $"{Texture}_Portal";
	protected virtual string SpikesTexturePath => $"{Texture}_Spikes";
	protected virtual string BackTexturePath => $"{Texture}_Background";

	public Point16 GetTopLeft(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		var tileData = TileObjectData.GetTileData((int)Type, style: 0);
		i -= (tile.TileFrameX % (tileData.Width * 18)) / 18;
		j -= (tile.TileFrameY % (tileData.Height * 18)) / 18;

		return new(i, j);
	}

	public static bool IsPointOnPortal(Point16 origin, Point16 point)
	{
		return point.Y <= origin.Y + 2;
	}

	public override void SetStaticDefaults()
	{
		Main.tileNoAttach[Type] = true;
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.InteractibleByNPCs[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
		// Visually spans [13, 7], logically [11, 6].
		TileObjectData.newTile.Width = 11; 
		TileObjectData.newTile.Height = 6;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 18];
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<MapDeviceEntity>().Hook_AfterPlacement, -1, 0, false);
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.DrawYOffset = 2;
		TileObjectData.newTile.StyleHorizontal = false;
		TileObjectData.newTile.RandomStyleRange = 0;
		TileObjectData.newTile.AnchorBottom = new AnchorData(TileObjectData.newTile.AnchorBottom.type, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.Origin = new(6, 5);
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

	public override bool PreDraw(int i, int j, SpriteBatch sb)
	{
		Tile tile = Main.tile[i, j];
		if (!TryGetEntity(i, j, out MapDeviceEntity? entity)) { return true; }
		if (!Main.tile[entity.Position].HasTile) { return true; }

		var tileData = TileObjectData.GetTileData(tile);
		Point16 tilePoint = entity.Position;
		Vector2 worldCenter = tilePoint.ToWorldCoordinates(tileData.Width * 16 * 0.5f, tileData.Height * 16 * 0.5f);
		// This is lighting-mode specific, always include this if you draw tiles manually.
		Vector2 screenPosition = Main.screenPosition - new Vector2(Main.drawToScreen ? 0 : Main.offScreenRange);

		// Only render on the top left tile.
		if (new Point16(i, j) != tilePoint) { return true; }

		if (!IsLegacy)
		{
			DrawSpikes(sb, tilePoint, worldCenter, entity, screenPosition);
		}

		return true;
	}
	public override void PostDraw(int i, int j, SpriteBatch sb)
	{
		Tile tile = Main.tile[i, j];
		if (!TryGetEntity(i, j, out MapDeviceEntity? entity)) { return; }
		if (!Main.tile[entity.Position].HasTile) { return; }

		var tileData = TileObjectData.GetTileData(tile);
		Point16 tilePoint = entity.Position;
		Vector2 worldCenter = tilePoint.ToWorldCoordinates(tileData.Width * 16 * 0.5f, tileData.Height * 16 * 0.5f);
		// This is lighting-mode specific, always include this if you draw tiles manually.
		Vector2 screenPosition = Main.screenPosition - new Vector2(Main.drawToScreen ? 0 : Main.offScreenRange);

		// Only render on the top left tile.
		if (new Point16(i, j) != tilePoint) { return; }

		entity.UpdateEffects(worldCenter);

		if (entity.PortalActive)
		{
			DrawItemTexture(sb, tilePoint, worldCenter, entity, screenPosition);
		}
	}

	private void DrawSpikes(SpriteBatch sb, Point16 tilePoint, Vector2 worldCenter, MapDeviceEntity entity, Vector2 screenPosition)
	{
		_ = (tilePoint, entity);

		float activationAnim = (1f - MathF.Pow(1f - entity.ActivationAnimation, 5.0f));
		float openingAnim = (1f - MathF.Pow(1f - entity.OpeningAnimation, 8.0f));
		float readyAnim = (1f - MathF.Pow(1f - entity.ReadyAnimation, 1.5f));

		// Shake.
		bool isActive = entity.InteractingPlayer != null || entity.PortalActive;
		bool isReady = readyAnim >= 1f;
		int shakeRate = entity.PortalActive ? 1 : (isReady && isActive ? 16 : 0);

		Texture2D backTexture = AssetUtils.ImmediateValue(BackTexturePath, ref _backTex);
		Texture2D spikesTexture = AssetUtils.ImmediateValue(SpikesTexturePath, ref _spikesTex);
		var spikesFrame = new SpriteFrame(1, 5) { PaddingX = 0, PaddingY = 0 };
		var spikesOffset = new Vector2(0, MathHelper.Lerp(6, 0, openingAnim) + MathHelper.Lerp(8, 2, MathF.Max(openingAnim, readyAnim * activationAnim)));
		int excessOffset = Math.Max(0, (int)(spikesOffset.Y) + 6);
		spikesOffset.Y += spikesFrame.GetSourceRectangle(spikesTexture).Height * 0.5f;

		Vector2 baseOffset = new(0, 8);
		Vector2 backPos = worldCenter + baseOffset - screenPosition;
		Vector2 backOrigin = backTexture.Size() * 0.5f;
		Color color = Lighting.GetColor(worldCenter.ToTileCoordinates());

		sb.Draw(backTexture, backPos, null, color, 0, backTexture.Size() * 0.5f, 1f, 0, 0f);

		for (int i = 0; i < spikesFrame.RowCount; i++)
		{
			Vector2 spikeOffset = spikesOffset;
			SpriteFrame spikeFrame = spikesFrame.With(0, (byte)i);
			Rectangle srcRect = spikeFrame.GetSourceRectangle(spikesTexture);
			srcRect.Height = Math.Max(1, srcRect.Height - excessOffset);
			Vector2 spikeOrigin = srcRect.Size() * new Vector2(0.5f, 1.0f);
			float spikeRotation = 0f;

			if (shakeRate != 0 && ((Main.GameUpdateCount + ((i * 389) >> 1)) / shakeRate) % 3 == 0)
			{
				int offInt = ((Main.GameUpdateCount + (i * 942)) / shakeRate) % 2 == 0 ? 1 : -1;
				int rotInt = ((Main.GameUpdateCount + (i * 7589 << 1)) / shakeRate) % 7 == 0 ? 1 : -1;
				spikeOffset += new Vector2(0, offInt * -1);
				spikeRotation = rotInt * -0.015f * (openingAnim > 0f ? 1 : 0);
			}

			Vector2 spikePos = worldCenter + baseOffset + spikeOffset - screenPosition;

			//additional animations to go with the new portal
			float indexMultiplier = MathHelper.Lerp(-1,1,i / 4f);
			float indexMultiplierPosY = MathHelper.Lerp(0, -7, readyAnim);
			spikeRotation += (indexMultiplier * -.2f);
			float waveProgress = MathF.Sin(Main.GameUpdateCount * 1f) *2;
			spikeRotation -= (indexMultiplier * -.4f * openingAnim);
			spikePos.Y -= indexMultiplierPosY;

			sb.Draw(spikesTexture, spikePos, srcRect, color, spikeRotation, spikeOrigin, 1f, 0, 0f);
		}
	}
	/// <summary>
	/// the portel itself is now a <see cref="PathOfTerraria.Common.Projectiles.BasePortalProjectile"/> 
	/// </summary>
	private void DrawItemTexture(SpriteBatch sb, Point16 tilePoint, Vector2 worldCenter, MapDeviceEntity entity, Vector2 screenPosition)
	{
		//Texture2D portalTexture = AssetUtils.ImmediateValue(PortalTexturePath, ref _portalTex);
		Texture2D? itemTex = null;
		Color baseColor = entity.GetPortalColor();
		Item portalItem = entity.StoredMap;

		if (portalItem is { IsAir: false, type: int itemType })
		{
			itemTex = TextureAssets.Item[itemType].Value;
		}

		// Get the initial draw parameters
		Tile tile = Main.tile[tilePoint];
		var tileData = TileObjectData.GetTileData(tile);
		int frameY = tile.TileFrameX % 90 / tileData.CoordinateFullWidth; // Picks the frame on the sheet based on the placeStyle of the item
		//Rectangle frame = portalTexture.Frame(1, 1, 0, frameY);
		//Vector2 origin = frame.Size() / 2f;
		Color color = Color.Lerp(Lighting.GetColor(tilePoint.X, tilePoint.Y), Color.White, 0.4f).MultiplyRGBA(baseColor);
		bool direction = tile.TileFrameY / tileData.CoordinateFullHeight != 0; // This is related to the alternate tile data we registered before
		SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		// Some math magic to make it smoothly move up and down over time
		float offset = MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 5f);
		Vector2 drawPos = worldCenter - screenPosition + new Vector2(0f, -65) + new Vector2(0f, offset * 7f);

		// Draw the main texture
		float baseScale = MathHelper.Lerp(0.1f, 2.0f, 1f - MathF.Pow(1f - entity.OpeningAnimation, 3f));
		for (int k = 0; k < 3; ++k)
		{
			float rotation = Main.GlobalTimeWrappedHourly * 7f * (k % 2 == 0 ? -1 : 1);
			Color drawColor = (color * (1 - (k * 0.2f)) * 0.7f) with { A = 112 };
			//sb.Draw(portalTexture, drawPos, frame, drawColor, rotation, origin, baseScale - k * 0.2f, effects, 0f);
		}

		if (itemTex != null)
		{
			float itemStep = 1f - MathF.Pow(1f - entity.OpeningAnimation, 3f);
			var itemPos = Vector2.Lerp(drawPos + new Vector2(0, 129), drawPos, itemStep);
			sb.Draw(itemTex, itemPos, null, new Color(140, 230, 255) * 0.95f, 0f, itemTex.Size() / 2f, 0.75f, effects, 0f);
		}

		// Draw the periodic glow effect
		float scale = baseScale + (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 2f) * 0.5f;
		Color effectColor = color;
		effectColor.A = 0;
		effectColor *= 0.1f * scale;
		//for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI))
		//{
		//	sb.Draw(portalTexture, drawPos + (MathHelper.TwoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame, effectColor, 0f, origin, 1f, effects, 0f);
		//}
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
					player.cursorItemIconID = entity.Injection is { } inj ? inj.Id : entity.StoredMap.type;
				}
			}
			else if (entity.TryOpeningInterface(evalMode: true))
			{
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = ModContent.ItemType<MapDevice>();
			}
		}
	}

	private bool TryGetEntity(int x, int y, [NotNullWhen(true)] out MapDeviceEntity? entity)
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
	internal class ClearInfoHandler : Handler
	{
		public static void Send(string subworldName, bool reAdd)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				return;
			}

			ModPacket packet = Networking.GetPacket<ClearInfoHandler>();
			BitsByte info = new(reAdd);
			packet.Write(info);
			packet.Write(subworldName);
			packet.Send();
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			BitsByte info = reader.ReadBitsByte();
			string subworldName = reader.ReadString();
			bool reAdd = info[0];

			ResetPersistentMapInfo(subworldName, reAdd, true);

			if (Main.netMode == NetmodeID.Server)
			{
				Send(subworldName, reAdd);
			}
		}
	}

	private static SoundStyle PortalSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/MapDevice/PortalLoop")
	{
		Volume = 0.4f,
		IsLooped = true,
		PauseBehavior = PauseBehavior.PauseWithGame,
	};
	private static SoundStyle EngineSound => new($"{nameof(PathOfTerraria)}/Assets/Sounds/MapDevice/GearLoop")
	{
		Volume = 0.03f,
		Pitch = -0.05f,
		IsLooped = true,
		PauseBehavior = PauseBehavior.PauseWithGame,
	};

	public static int StorageSize { get; } = 5 * 4;

	private SlotId portalSoundHandle;
	private SlotId engineSoundHandle;
	private float portalSoundVolume;
	private float engineSoundVolume;
	private bool portalActive = false;
	public int PortalProjWhoAmI = -1;
	public Item StoredMap { get; set; }
	public Item[] Storage { get; set; }
	public bool PortalActive { get => portalActive; set
		{
			portalActive = value;
			if (value)
			{
				SpawnPortalProjectile();
			}
			else if(PortalProjWhoAmI != -1 && Main.projectile[PortalProjWhoAmI].type == ModContent.ProjectileType<BasePortalProjectile>())
			{
				Main.projectile[PortalProjWhoAmI].ai[0] = 1; //kill animation 
			}
		}
	}
	public int PortalUsesLeft { get; set; }
	public int? InteractingPlayer { get; private set; }
	/// <summary> Which map destination resource is currently injected into the device, and what quantity of it. </summary>
	public (int Id, int Amount)? Injection { get; set; }
	public float ActivationAnimation { get; set; }
	public float OpeningAnimation { get; set; }
	public float ReadyAnimation { get; set; }
	public Vector2 ShakeOffset { get; set; }

	public MapDeviceEntity()
	{
		StoredMap = new();
		Storage = new Item[StorageSize];
		for (int i = 0; i < Storage.Length; i++) { Storage[i] = new(); }
	}

	public override bool IsTileValidForEntity(int x, int y)
	{
		Tile tile = Main.tile[x, y];
		return tile.HasTile && TileLoader.GetTile(tile.TileType) is MapDeviceTile;
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

		//Portal handling
		Projectile portal = Main.projectile[PortalProjWhoAmI];
		if (portal.type == ModContent.ProjectileType<BasePortalProjectile>())
		{
			portal.ai[0] = 1;
		}
	}

	public override void Update()
	{
		// If the local player has the map device open.
		if (InteractingPlayer.HasValue && Main.player[InteractingPlayer.Value] is { } player)
		{
			var tileData = TileObjectData.GetTileData(Main.tile[Position].TileType, 1);
			var worldPos = Position.ToWorldCoordinates(0, 0).ToPoint();
			var tileRect = new Rectangle(worldPos.X, worldPos.Y, tileData.Width * TileUtils.TileSizeInPixels, tileData.Height * TileUtils.TileSizeInPixels);
			Vector2 worldCenter = tileRect.Center();

			// Close UI if player is dead, gone, or too far away.
			Point checkPoint = tileRect.ClosestPointInRect(player.Center).ToTileCoordinates();
			if (!player.active || player.dead || !player.IsInTileInteractionRange(checkPoint.X, checkPoint.Y, TileReachCheckSettings.Simple))
			{
				TryClosingInterface();
				return;
			}
		}

		Vector2 center = Position.ToWorldCoordinates();
		foreach (Item item in Main.ActiveItems)
		{
			if (item.ModItem is Map map && item.velocity.LengthSquared() < 4f && item.WithinRange(center, 128f) && Array.FindIndex(Storage, s => s.IsAir) is >= 0 and int freeSlot)
			{
				SoundEngine.PlaySound(SoundID.Grab, item.Center);
				Storage[freeSlot] = item.Clone();
				item.active = false;
			}
		}
	}

	public override void SaveData(TagCompound tag)
	{
		// Portal state
		if (PortalActive)
		{
			tag.Add("portalActive", true);
			tag.Add("portalUsesLeft", PortalUsesLeft);
		}

		// Map
		if (StoredMap is { IsAir: false })
		{
			tag.Add("item", ItemIO.Save(StoredMap));
		}

		// Injection
		if (Injection is { } injection)
		{
			tag.Add("injection", new TagCompound
			{
				{ "id", (int)injection.Id },
				{ "amount", (int)injection.Amount },
			});
		}

		// Storage
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
		// Portal state
		PortalActive = tag.GetBool("portalActive");
		PortalUsesLeft = tag.GetInt("portalUsesLeft");

		// Map
		if (tag.TryGet("item", out TagCompound itemTag))
		{
			StoredMap = ItemIO.Load(itemTag);
		}

		// Injection
		if (tag.TryGet("injection", out TagCompound injTag))
		{
			if (injTag.GetInt("id") is int id && injTag.GetInt("amount") is >= 0 and int amount && MapResources.TryGet(id, out MapResource resource))
			{
				Injection = (id, amount);
			}
		}

		// Storage
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

	public void UpdateEffects(Vector2 center)
	{
		if (Main.dedServ) { return; }
		
		ActivationAnimation = MathUtils.StepTowards(ActivationAnimation, (InteractingPlayer.HasValue | PortalActive) ? 1 : 0, TimeSystem.RenderDeltaTime / 0.2f);
		OpeningAnimation = MathUtils.StepTowards(OpeningAnimation, PortalActive ? 1 : 0, TimeSystem.RenderDeltaTime / 0.2f);
		ReadyAnimation = MathUtils.StepTowards(ReadyAnimation, (PortalActive || TryOpeningPortal(evalMode: true)) ? 1 : 0, TimeSystem.RenderDeltaTime / 0.2f);
		// OpeningAnimation = MathUtils.StepTowards(OpeningAnimation, (PortalActive || TryOpeningPortal(evalMode: true)) ? 1 : 0, TimeSystem.RenderDeltaTime / 0.2f);

		if (PortalActive)
		{
			// Light.
			Lighting.AddLight(center, GetPortalColor().ToVector3() * 1f * OpeningAnimation);

			// Fade out music when nearby, for that sweet melodic ambience.
			ref float musicVolume = ref Main.musicFade[Main.curMusic];
			float distancePower = MathUtils.DistancePower(center.Distance(CameraSystem.ScreenCenter), 48, 256);
			float musicEffect = MathF.Pow(MathUtils.Clamp01(distancePower * OpeningAnimation), 2f);
			if (musicEffect > 0f)
			{
				float target = MathF.Min(musicVolume, MathHelper.Lerp(1f, 0.1f, musicEffect));
				musicVolume = MathHelper.Lerp(musicVolume, target, 0.1f);
			}
		}
		
		// Maintain a camera curio as long as this interface is open by the local player.
		if (InteractingPlayer.HasValue && Main.player[InteractingPlayer.Value] is { } player && player == Main.LocalPlayer)
		{
			CameraCurios.Create(new()
			{
				Identifier = nameof(MapDeviceInterface),
				Weight = 1f,
				LengthInSeconds = 1f / 6f,
				FadeInLength = 0.50f,
				FadeOutLength = 0.50f,
				Position = center + new Vector2(0, 16 + (MathF.Pow(ReadyAnimation, 2f) * +16) + (MathF.Pow(OpeningAnimation, 2f) * +8)),
				Zoom = +1f,
			});
		}

		// Audio.
		{
			float targetVolume = PortalActive ? new ExponentialRange(0, 1500, 2.5f).DistanceFactor(Main.LocalPlayer.Distance(center)) : 0f;
			portalSoundVolume = MathUtils.StepTowards(portalSoundVolume, targetVolume, MathF.Max(0f, TimeSystem.RenderDeltaTime));
			SoundUtils.UpdateLoopingSound(ref portalSoundHandle, center, portalSoundVolume, null, PortalSound);
			TileSoundTracker.Track(Position, new() { Handle = portalSoundHandle, EntityType = GetType() });
		}
		{
			float targetVolume = PortalActive ? new ExponentialRange(0, 1200, 2.5f).DistanceFactor(Main.LocalPlayer.Distance(center)) : 0f;
			engineSoundVolume = MathUtils.StepTowards(engineSoundVolume, targetVolume, MathF.Max(0f, TimeSystem.RenderDeltaTime));
			SoundUtils.UpdateLoopingSound(ref engineSoundHandle, center, engineSoundVolume, null, EngineSound);
			TileSoundTracker.Track(Position, new() { Handle = engineSoundHandle, EntityType = GetType() });
		}

		ShakeOffset *= 0.0f;
		if (PortalActive || (TryOpeningPortal(evalMode: true) && InteractingPlayer != null))
		{
			int rate = PortalActive ? 3 : 2;
			if (Main.GameUpdateCount % rate == 0) { ShakeOffset = new Vector2(Main.rand.NextBool() ? 1 : -1, 0); }
		}
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

				// Unfortunately the inventory sync here could actually cause a duping glitch if someone with enough delay abuses it:
				// MapDeviceSync.CorrectDesync(ID, netSender, MapDeviceSync.Flags.FullSync);

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
		if (!PortalActive) { return false; }
		if (StoredMap is not { IsAir: false, ModItem: Map m } && Injection == null) { return false; }

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
			if (StoredMap?.ModItem is Map map)
			{
				map.OpenMap();
			}
			else if (MapResources.TryGet(Injection!.Value.Id, out MapResource res)
			&& res.PortalDestination is { Length: > 0 })
			{
				SubworldSystem.Enter(res.PortalDestination);
			}
		}
		else
		{
			// Acknowledge interaction.
			Debug.Assert(netSender != null);
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.EnterPortal, toClient: netSender.Value);
		}

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			Player player = Main.netMode == NetmodeID.SinglePlayer ? Main.LocalPlayer : Main.player[netSender!.Value];
			player.GetModPlayer<BossDomainLivesPlayer>().SetActiveMapDevice(Position);
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

		// Ensure a newly opened portal starts from a fresh save.
		MappingWorld.DeleteSavedSubworld();

		PortalActive = true;
		PortalUsesLeft = int.MaxValue;

		if (StoredMap is { IsAir: false, ModItem: Map map })
		{
			ResetPersistentMapInfo(map.GetDestination().FullName, true);

			PortalUsesLeft = map.MaxUses;
		}
		else if (Injection is { } injection)
		{
			MapResource resource = MapResources.Get(injection.Id);
			PortalUsesLeft = resource.PortalUses;
		}

		// Broadcast the interaction.
		if (Main.netMode == NetmodeID.Server)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.OpenPortal);
		}

		// Effects.
		if (!Main.dedServ)
		{
			Main.instance.CameraModifiers.Add(new PunchCameraModifier(Position.ToWorldCoordinates(), new Vector2(1, -4), 1f, 4.5f, 75, 300, "PortalOpening"));

			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/MapDevice/PortalOpen", 3)
			{
				MaxInstances = 2,
				Volume = 0.85f,
				PitchVariance = 0.25f,
			});
		}

		return true;
	}

	private static void ResetPersistentMapInfo(string name, bool reAdd = false, bool fromNet = false)
	{
		if (!fromNet)
		{
			ClearInfoHandler.Send(name, reAdd);
		}

		MappingWorld.TimesEnteredByDomain.Remove(name);
		MappingWorld.PersistentDomainInfo.Remove(name);

		if (reAdd)
		{
			MappingWorld.TimesEnteredByDomain.Add(name, 0);
			MappingWorld.PersistentDomainInfo.Add(name, new());
		}
	}

	/// <summary>
	/// The Portal Drawing Projectile. The reason its a projectile is because dealing with shaders in tiles require a lot of effort and we could just use a projecitle which does the same thing but easier and more reliable
	/// </summary>
	private void SpawnPortalProjectile() 
	{
		Point16 tilePoint = Position;
		Vector2 worldCenter = tilePoint.ToWorldCoordinates(88, -20f);
		PortalProjWhoAmI = Projectile.NewProjectile(null, worldCenter, Vector2.Zero,ModContent.ProjectileType<BasePortalProjectile>(),0,0);
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

		if (StoredMap.ModItem is Map map)
		{
			ResetPersistentMapInfo(map.GetDestination().FullName);
		}

		// The map is destroyed if the portal is ever closed.
		StoredMap = new();
		PortalActive = false;
		PortalUsesLeft = 0;
		Injection = null;
		MappingWorld.DeleteSavedSubworld();

		// Broadcast the interaction.
		if (Main.netMode == NetmodeID.Server)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.ClosePortal);
		}

		// Effects.
		if (!Main.dedServ)
		{
			Main.instance.CameraModifiers.Add(new PunchCameraModifier(Position.ToWorldCoordinates(), new Vector2(1, -4), 1f, 2.5f, 45, 300, "PortalClosing"));
	
			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/MapDevice/PortalClose", 2)
			{
				MaxInstances = 2,
				Volume = 0.80f,
				PitchVariance = 0.25f,
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

		// Refuse if there is already a map present.
		if (StoredMap is { IsAir: false } || Injection != null)
		{
			if (!evalMode) { DebugUtils.DebugLog($"Cannot inject as a map or another injection is already present."); }
			MapDeviceSync.CorrectDesync(ID, netSender, MapDeviceSync.Flags.Injection | MapDeviceSync.Flags.Map);
			return false;
		}

		// Refuse interaction if the resource ID is invalid or if there is not enough of it.
		if (!MapResources.TryGet(resourceId, out MapResource resource))
		{
			if (!evalMode) { DebugUtils.DebugLog($"Invalid resource ID: {resourceId}"); }
			return false;
		}
		if (resource.Value < resource.Cost)
		{
			if (!evalMode) { DebugUtils.DebugLog($"Not enough of {ContentSamples.ItemsByType[resource.AssociatedItem].Name} resource, need {resource.Cost} but only got {resource.Value}."); }
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

		Injection = (resourceId, resource.Cost);
		MapResources.ModifyValue(resourceId, -resource.Cost);

		// Broadcast the interaction.
		if (Main.netMode == NetmodeID.Server)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.InjectResource, arg: resourceId);
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
		if (Injection is not { } injection)
		{
			MapDeviceSync.CorrectDesync(ID, netSender, MapDeviceSync.Flags.Injection);
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

		if (MapResources.TryGet(injection.Id, out MapResource resource))
		{
			MapResources.ModifyValue(injection.Id, +injection.Amount, discovery: ResourceDiscovery.Never);
		}

		Injection = null;

		// Broadcast the interaction.
		if (Main.netMode == NetmodeID.Server)
		{
			MapDeviceInteraction.Send(ID, MapDeviceInteraction.Kind.EjectResource);
		}

		return true;
	}

	public Color GetPortalColor()
	{
		if (Injection is { } injection)
		{
			return MapResources.Get(injection.Id).AccentColor;
		}
		else if (StoredMap is { IsAir: false, type: int itemType })
		{
			return ColorUtils.FromHexRgb(0x_0097ff);
		}

		return Color.Transparent;
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
		DebugUtils.DebugLog($"Sending interaction confirmation: {kind}");
		
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

		DebugUtils.DebugLog($"Interaction received: {kind}");

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
				case Kind.EjectResource: mapEntity.TryEjectingResource(netSender: sender); break;
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

	public static void CorrectDesync(int entityId, byte? toClient, Flags flags)
	{
		if (Main.netMode == NetmodeID.Server && toClient != null)
		{
			Send(entityId, flags, toClient: toClient.Value);
		}
	}

	public static void Send(int entityId, Flags flags, IEnumerable<int>? itemIndices = null, int toClient = -1, int ignoreClient = -1)
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
			writer.Write7BitEncodedInt(device.Injection?.Id ?? -1);
			writer.Write7BitEncodedInt(device.Injection?.Amount ?? 0);
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
			int resourceAmount = reader.Read7BitEncodedInt();

			// Refuse applying data if it is received from clients.
			if (mapEntity != null && Main.netMode != NetmodeID.Server)
			{
				mapEntity.Injection = MapResources.TryGet(resourceId, out _) ? (resourceId, Math.Max(0, resourceAmount)) : null;
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

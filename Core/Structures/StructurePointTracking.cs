using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

#nullable enable

namespace PathOfTerraria.Core.Structures;

#if DEBUG || STAGING
internal sealed class MetadataEntityPlacer : ModItem
{
	public override string Texture => $"Terraria/Images/Item_{ItemID.VileMushroom}";
	
	public override void SetDefaults()
	{
		Item.Size = new Vector2(30, 30);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.useTime = 1;
		Item.useAnimation = 1;
	}
}

internal sealed class MetadataEntityHandler : Handler
{
	private enum OpCode : byte
	{
		Place,
		Remove,
	}

	public static void Place(Point16 point, string identifier, bool fromNet = false)
	{
		int entityID = ModContent.GetInstance<MetadataTileEntity>().Place(point, identifier);
		
		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.TileEntitySharing, number: entityID);
			return;
		}

		if (Main.netMode != NetmodeID.MultiplayerClient || fromNet) { return; }

		ModPacket packet = Networking.GetPacket<MetadataEntityHandler>();
		packet.Write((byte)OpCode.Place);
		packet.Write(point.X);
		packet.Write(point.Y);
		packet.Write(identifier);
		packet.Send();
	}
	public static void Remove(Point16 point, bool fromNet = false)
	{
		TileEntityUtils.Remove(point);

		if (Main.netMode != NetmodeID.MultiplayerClient || fromNet) { return; }

		ModPacket packet = Networking.GetPacket<MetadataEntityHandler>();
		packet.Write((byte)OpCode.Remove);
		packet.Write((short)point.X);
		packet.Write((short)point.Y);
		packet.Send();
	}
	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		var op = (OpCode)reader.ReadByte();
		var point = new Point16(reader.ReadInt16(), reader.ReadInt16());

		if (op == OpCode.Place)
		{
			string identifier = reader.ReadString();
			Place(point, identifier, fromNet: true);
		}
		else if (op == OpCode.Remove && TileEntity.ByPosition.TryGetValue(point, out TileEntity? te) && te is MetadataTileEntity mte)
		{
			Remove(point, fromNet: true);
		}
	}
}

internal sealed class MetadataEntitySystem : ModSystem
{
	internal sealed class SetEntityIdCommand : ModCommand
	{
		public override string Command => "potSetEntityId";
		public override CommandType Type => CommandType.Chat;
		public override string Usage => $"/{Command} {{identifier}}";
		public override bool IsCaseSensitive => true;

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length != 1) { throw new UsageException(); }
			
			placementIdentifier = args[0];
		}
	}
	internal sealed class ClearMetaEntitiesCommand : ModCommand
	{
		public override string Command => "potClearMetaEntities";
		public override CommandType Type => CommandType.Chat;
		public override string Usage => $"/{Command}";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length != 0) { throw new UsageException(); }
			
			foreach (TileEntity entity in TileEntity.ByID.Values.ToArray())
			{
				if (entity is not MetadataTileEntity) { continue; }
				MetadataEntityHandler.Remove(entity.Position);
			}
		}
	}

	private static Asset<Texture2D>? metaEntityIcon;
	private static string placementIdentifier = "";
	
	public override void PostDrawTiles()
	{
		if (Main.LocalPlayer?.HeldItem?.ModItem is not MetadataEntityPlacer) { return; }
		if (Main.mapFullscreen) { return; }

		// Shroomman head.
		if (!AssetUtils.AsyncValue("Terraria/Images/NPC_Head_76", ref metaEntityIcon, out Texture2D iconTex))
		{
			return;
		}

		SpriteBatch sb = Main.spriteBatch;
		using SpriteBatchScope sbScope = sb.Scope(new(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix));
		Vector2 screenPos = Main.screenPosition;

		MetadataTileEntity baseEntity = ModContent.GetInstance<MetadataTileEntity>();
		bool anyHovered = false;
		
		Point16 halfTileRes = Main.ScreenSize.ToVector2().ToTileCoordinates16() + new Point16(1, 1);
		Point16 centerTile = (Main.screenPosition + (Main.ScreenSize.ToVector2() * 0.5f)).ToTileCoordinates16();
		int x1 = Math.Max(centerTile.X - halfTileRes.X, 0);
		int y1 = Math.Max(centerTile.Y - halfTileRes.Y, 0);
		int x2 = Math.Min(centerTile.X + halfTileRes.X, Main.maxTilesX - 1);
		int y2 = Math.Min(centerTile.Y + halfTileRes.Y, Main.maxTilesY - 1);
		for (int x = x1; x < x2; x++)
		{
			for (int y = y1; y < y2; y++)
			{
				// Do not use TileEntity.TryGet, as that offsets the coordinate by TileObjectData.
				if (!TileEntity.ByPosition.TryGetValue(new(x, y), out TileEntity? entity)) { continue; }
				if (entity is not MetadataTileEntity metadata) { continue; }

				Vector2 iconPos = new Point16(x, y).ToWorldCoordinates() - screenPos;
				Rectangle hoverRect = new Rectangle((int)iconPos.X, (int)iconPos.Y, 0, 0).Inflated(iconTex.Width / 2, iconTex.Height / 2);
				bool isHovered = !anyHovered && hoverRect.Contains(Main.MouseScreen.ToPoint());
				var drawColor = new Color(Vector4.One * (isHovered ? 0.9f : 0.5f));
				
				sb.Draw(iconTex, iconPos, null, drawColor, 0, iconTex.Size() * 0.5f, 1, 0, 0);

				if (isHovered)
				{
					string coordLine = $"{{X: {x}, Y: {y}}}";
					Main.instance.MouseText($"{coordLine}\n{metadata.Identifier}@{entity.ID}\nRight click to remove");
				}

				if (isHovered && (Main.mouseRight & Main.mouseRightRelease))
				{
					Main.mouseRightRelease = false;
					MetadataEntityHandler.Remove(entity.Position);
				}

				anyHovered |= isHovered;
			}
		}

		Point16 mousePoint = Main.MouseWorld.ToTileCoordinates16();
		if (!anyHovered && !TileEntity.ByPosition.ContainsKey(mousePoint))
		{
			SetEntityIdCommand cmd = ModContent.GetInstance<SetEntityIdCommand>();
			string coordLine = $"{{X: {mousePoint.X}, Y: {mousePoint.Y}}}";

			if (string.IsNullOrWhiteSpace(placementIdentifier))
			{
				Main.instance.MouseText($"{coordLine}\nUse '{cmd.Usage}'\nTo set placed identifier");
			}
			else if (Main.mouseLeft & Main.mouseLeftRelease)
			{
				Main.mouseLeftRelease = false;
				MetadataEntityHandler.Place(mousePoint, placementIdentifier);
			}
			else
			{
				Vector2 iconPos = mousePoint.ToWorldCoordinates() - screenPos;
				var drawColor = new Color(Vector4.One * 0.2f);
				sb.Draw(iconTex, iconPos, null, drawColor, 0, iconTex.Size() * 0.5f, 1, 0, 0);
				
				Main.instance.MouseText($"{coordLine}\nClick to place '{placementIdentifier}'\nChange via '{cmd.Usage}'");
			}
		}
	}
}
#endif

internal sealed class MetadataTileEntity : ModTileEntity
{
	private static readonly HashSet<MetadataTileEntity> instances = [];
	
	public string Identifier { get; set; } = "";
	
	public override bool IsTileValidForEntity(int x, int y) { return true; }

	public override int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			NetMessage.SendData(MessageID.TileEntityPlacement, number: x, number2: y, number3: Type);
			return -1;
		}

		return Place(x, y);
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(Identifier);
	}
	public override void NetReceive(BinaryReader reader)
	{
		Identifier = reader.ReadString();
	}
	public override void SaveData(TagCompound tag)
	{
		tag["identifier"] = Identifier;
	}
	public override void LoadData(TagCompound tag)
	{
		Identifier = tag.GetString("identifier");
	}

#if DEBUG || STAGING
	// Cleanup invalid entities.
	public override void Update()
	{
		if (string.IsNullOrWhiteSpace(Identifier))
		{
			Main.QueueMainThreadAction(() => MetadataEntityHandler.Remove(Position));
		}
	}
#endif

	public int Place(Point16 point, string identifier)
	{
		int id = Place(point.X, point.Y);
		var entity = (MetadataTileEntity)ByID[id];
		entity.Identifier = identifier;
		return id;
	}
}

/// <summary> Searches for <see cref="MetadataTileEntity"/> instances with the given identifier. </summary>
internal struct StructurePointTracker(string needle, Rectangle? area = null)
{
	/// <summary> The tile entity identifier to search for. </summary>
	public string Needle { get; set; } = needle;
	public Point16? Result { get; private set; }
	// public Point16 Fallback { get; private set; } = fallback;
	public (int X, int Y, int Z, int W)? SearchArea { get; private set; } = area is { } a ? (a.X, a.Y, a.Right, a.Bottom) : null;

	private uint lastSearchTick;

	public Point16? Get()
	{
		if (Result != null)
		{
			return Result.Value;
		}
		
		const int retryRateInTicks = 60;
		if (lastSearchTick == 0 || (Main.GameUpdateCount - lastSearchTick) >= retryRateInTicks)
		{
			Search();
		}

		return Result;
	}
	public void Reset()
	{
		lastSearchTick = 0;
	}
	public void Search()
	{
		Result = null;

		(int x1, int y1, int x2, int y2) = SearchArea ?? (0, 0, Main.maxTilesX, Main.maxTilesY);
		for (int x = x1; x < x2; x++)
		{
			for (int y = y1; y < y2; y++)
			{
				var point = new Point16(x, y);
				if (!TileEntity.ByPosition.TryGetValue(point, out TileEntity? entity)) { continue; }
				if (entity is not MetadataTileEntity metadata) { continue; }
				if (metadata.Identifier != Needle) { continue; }

				Result = new Point16(x, y);
				DebugUtils.DebugLog($"Found structure point '{Needle}' at position [{x}, {y}]");
				goto End;
			}
		}

		if (Result == null)
		{
			string msg = $"Could not locate structure point '{Needle}'. Please report this!";
			Main.NewText(msg, Color.Red);
			PoTMod.Instance.Logger.Warn(msg);
			Debugger.Break();
		}

		End:;
		lastSearchTick = Main.GameUpdateCount;
	}

	public void NetSend(BinaryWriter writer)
	{
		_ = Get();
		writer.Write((short)(Result?.X ?? short.MaxValue));
		writer.Write((short)(Result?.Y ?? short.MaxValue));
	}
	public void NetReceive(BinaryReader reader)
	{
		var result = new Point16(reader.ReadInt16(), reader.ReadInt16());
		Result = result.X != short.MaxValue ? result : null;
	}
}

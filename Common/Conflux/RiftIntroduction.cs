// #define DEBUG_KEYS

using System.IO;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Conflux;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Conflux;

internal sealed class RiftIntroductionNPC : GlobalNPC
{
	public override void OnKill(NPC npc)
    {
        if (npc.type != NPCID.WallofFlesh || Main.hardMode) { return; }
        
        Vector2 point = npc.Center;
        RiftIntroduction.Pending = (Countdown: 3 * 60, Position: point);
    }
}

internal sealed class RiftIntroductionHandler : Handler
{
	public static void Send(Rectangle area)
	{
		ModPacket packet = Networking.GetPacket<RiftIntroductionHandler>();
        packet.Write((ushort)area.X);
        packet.Write((ushort)area.Y);
        packet.Write((ushort)area.Width);
        packet.Write((ushort)area.Height);
		packet.Send();
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
        var area = new Rectangle(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());
        RiftIntroduction.ArenaSpawnEffects(area);
	}
}

/// <summary>
/// Spawns a rift after Wall of Flesh is defeated.
/// </summary>
internal sealed class RiftIntroduction : ModSystem
{
    public static (uint Countdown, Vector2 Position) Pending;

	public override void PostUpdateWorld()
    {
        if (Pending.Countdown > 0 && --Pending.Countdown == 0)
        {
            StartIntroduction(Pending.Position);
        }

#if DEBUG && DEBUG_KEYS
        if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.O))
        {
            Main.hardMode = false;
        }
#endif
    }

    public static void StartIntroduction(Vector2 position)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) { return; }
        
        // Announce to players.
		ChatHelper.BroadcastChatMessage(NetworkText.FromKey($"Mods.{nameof(PathOfTerraria)}.Misc.Rifts.Introduction"), Color.Magenta);

        // Place rift.
		ConfluxRift.Flags flags = ConfluxRift.Flags.Special;
		float flagsAsFloat = ConfluxRift.FlagsToFloat(flags);
        IEntitySource source = Entity.GetSource_None();
        int riftType = ModContent.ProjectileType<InfernalRift>();
        Vector2 riftPosition = position + new Vector2(40, 200);
		var rift = Projectile.NewProjectileDirect(source, riftPosition, Vector2.Zero, riftType, 0, 0f, ai0: flagsAsFloat);

        // Place arena.
        const bool largeArena = true;
		const string arenaPath = $"Assets/Structures/RiftIntroductionArena_{(largeArena ? "Large" : "Small")}";
		Point16 arenaSize = StructureTools.GetSize(arenaPath);
        Point16 arenaCenter = position.ToTileCoordinates16() + new Point16(0, 20);
		Point16 arenaPos = StructureTools.PlaceByOrigin(arenaPath, arenaCenter, new Vector2(0.5f, 0.5f));

        // Sync area.
        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendTileSquare(-1, arenaPos.X, arenaPos.Y, arenaSize.X, arenaSize.Y);
        }
        
        // Tile placement effects.
        ArenaSpawnEffects(new Rectangle(arenaPos.X, arenaPos.Y, arenaSize.X, arenaSize.Y));

        // If an exit portal exists - move it onto a platform on the side.
        Vector2 exitPosition = riftPosition + new Vector2(-130, +500);
        int exitPortalType = ModContent.ProjectileType<ExitPortal>();
        foreach (Projectile portal in Main.ActiveProjectiles)
        {
            if (portal.type != exitPortalType) { continue; }
            portal.Center = exitPosition;

            if (Main.netMode == NetmodeID.Server) { NetMessage.SendData(MessageID.SyncProjectile, number: portal.whoAmI); }
        }

        // Move all items as well.
        for (int i = 0; i < Main.maxItems; i++)
        {
            Item item = Main.item[i];
            if (!item.active) { continue; }

            item.Center = exitPosition;

            if (Main.netMode == NetmodeID.Server) { NetMessage.SendData(MessageID.SyncItem, number: i); }
        }
    }

    public static void ArenaSpawnEffects(Rectangle area)
    {
        // Non-inclusive.
        int x1 = Math.Max(area.X, 0);
        int y1 = Math.Max(area.Y, 0);
        int x2 = Math.Min(area.X + area.Width, Main.maxTilesX);
        int y2 = Math.Min(area.Y + area.Height, Main.maxTilesY);

        if (Main.netMode == NetmodeID.Server)
        {
            RiftIntroductionHandler.Send(new Rectangle(x1, y1, x2 - x1, y2 - x1));
            return;
        }

        // Play sound.
        Vector2 soundPos = area.Center.ToWorldCoordinates();
        SoundEngine.PlaySound(position: soundPos, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftArenaSpawn")
        {
            Volume = 0.9f,
        });

        for (int x = x1; x < x2; x++)
        {
            for (int y = y1; y < y2; y++)
			{
				Tile tile = Main.tile[x, y];
				if (tile.TileType != TileID.CrimtaneBrick || !tile.HasTile) { continue; }
                
				Vector2 worldPos = new Point16(x, y).ToWorldCoordinates(0, 0);
				Dust.NewDustDirect(worldPos, 16, 16, DustID.FireworkFountain_Red);
			}
		}
    }

    public static bool RiftExists()
    {
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.ModProjectile is not ConfluxRift rift) { continue; }
            if (rift.Kind != ConfluxRiftKind.Infernal) { continue; }
            if (!rift.BitFlags.HasFlag(ConfluxRift.Flags.Special)) { continue; }
            return true;
        }

        return false;
    }
}

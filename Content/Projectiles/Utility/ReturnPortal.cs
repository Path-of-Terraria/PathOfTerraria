using PathOfTerraria.Common.Projectiles;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Projectiles.Utility;

/// <summary>
/// A player-created exit portal that is saved with its subworld so it remains available when the player returns.
/// </summary>
internal sealed class ReturnPortal : ExitPortal, ISaveProjectile
{
	public override string Texture => $"{PoTMod.ModName}/Content/Projectiles/Utility/ExitPortal";

	protected override bool IsPlayerCreatedReturnPortal => true;

	public void SaveData(TagCompound tag) { }

	public void LoadData(TagCompound tag, Projectile projectile)
	{
		// Player indices are transient multiplayer connection slots. An old player-owned portal is
		// killed during subserver startup before that slot reconnects, so persisted portals belong to
		// the world instead. Older saves may still contain an "owner" field; intentionally ignore it.
		projectile.owner = Main.maxPlayers;
		projectile.netUpdate = true;
	}
}

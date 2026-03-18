using Terraria.ID;

namespace PathOfTerraria.Common.Projectiles;

[ReinitializeDuringResizeArrays]
internal class CustomProjectileSets
{
	/// <summary>
	/// Defines which minion projectiles are multisegment, such as the body/tail of the Stardust Dragon projectile.<br/>
	/// This is useful when doing actions per-minion that should avoid also occuring for each segment of a segmented minion.
	/// </summary>
	public static bool[] MultisegmentMinionProjectiles = ProjectileID.Sets.Factory.CreateNamedSet(PoTMod.Instance, "MultisegmentMinionProjectiles")
		.Description("Defines which minion projectiles are multisegment, such as the body/tail of the Stardust Dragon projectile.")
		.RegisterBoolSet(false, [ProjectileID.StardustDragon2, ProjectileID.StardustDragon3, ProjectileID.StardustDragon4]);
}

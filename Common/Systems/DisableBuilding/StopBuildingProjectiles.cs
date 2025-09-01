using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

/// <summary>
/// Used to stop certain projectiles from adding or removing liquids.
/// </summary>
internal class StopBuildingProjectiles : ILoadable
{
	public readonly record struct ProjectileAlias(int ProjectileId, int AltProjectileId, int ItemId, int AltItemId);

	public static Dictionary<int, ProjectileAlias> ProjectileAliasingsByProjectileId = [];
	public static Dictionary<int, ProjectileAlias> ProjectileAliasingsByItemId = [];

	public void Load(Mod mod)
	{
		On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += ModNewProj;

		// Some projectiles do not have items associated with them. As such, their ItemId is set to None to simply ignore it.
		ProjectileAliasingsByProjectileId.Clear();
		AddAliasing(ProjectileID.DryRocket, ProjectileID.RocketI, ItemID.DryRocket, ItemID.RocketI);
		AddAliasing(ProjectileID.DrySnowmanRocket, ProjectileID.RocketI, ItemID.None, ItemID.RocketI);
		AddAliasing(ProjectileID.WetRocket, ProjectileID.RocketI, ItemID.WetRocket, ItemID.RocketI);
		AddAliasing(ProjectileID.WetSnowmanRocket, ProjectileID.RocketI, ItemID.None, ItemID.RocketI);
		AddAliasing(ProjectileID.LavaRocket, ProjectileID.RocketI, ItemID.LavaRocket, ItemID.RocketI);
		AddAliasing(ProjectileID.LavaSnowmanRocket, ProjectileID.RocketI, ItemID.None, ItemID.RocketI);
		AddAliasing(ProjectileID.HoneyRocket, ProjectileID.RocketI, ItemID.HoneyRocket, ItemID.RocketI);
		AddAliasing(ProjectileID.HoneySnowmanRocket, ProjectileID.RocketI, ItemID.None, ItemID.RocketI);
	}

	private static void AddAliasing(short projId, short altProjId, short itemId, short altItemId)
	{
		var alias = new ProjectileAlias(projId, altProjId, itemId, altItemId);
		ProjectileAliasingsByProjectileId.Add(projId, alias);

		if (itemId != ItemID.None)
		{
			ProjectileAliasingsByItemId.Add(itemId, alias);
		}
	}

	private int ModNewProj(On_Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig, IEntitySource spawnSource, 
		float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2)
	{
		if (SubworldSystem.Current is MappingWorld and not MoonLordDomain)
		{
			if (ProjectileAliasingsByProjectileId.TryGetValue(Type, out ProjectileAlias alias))
			{
				Type = alias.AltProjectileId;
			}
		}

		return orig(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
	}

	public void Unload()
	{
	}
}

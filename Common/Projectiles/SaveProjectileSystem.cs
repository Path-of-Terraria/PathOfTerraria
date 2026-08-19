using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Projectiles;

internal class SaveProjectileSystem : ModSystem
{
	public override void SaveWorldData(TagCompound tag)
	{
		int saveCount = 0;
		TagCompound saveProjs = [];

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.ModProjectile is not ISaveProjectile saveProj)
			{
				continue;
			}

			TagCompound proj = [];
			proj.Add("name", projectile.ModProjectile.FullName);
			proj.Add("pos", projectile.Center);
			saveProj.SaveData(proj);
			saveProjs.Add("projectile" + saveCount++, proj);
		}

		saveProjs.Add("count", saveCount);
		tag.Add("saveProjectiles", saveProjs);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		TagCompound projCompound = tag.GetCompound("saveProjectiles");
		int count = projCompound.GetInt("count");

		for (int i = 0; i < count; ++i)
		{
			TagCompound proj = projCompound.GetCompound("projectile" + i);
			string name = proj.GetString("name");

			// Projectiles that have since been renamed or removed must not take the rest of the world data
			// with them. Find throws on a missing key, and that throw aborts LoadWorldData for every later
			// entry and for every other tag this system owns.
			if (!ModContent.TryFind(name, out ModProjectile saveProjectile))
			{
				PoTMod.Instance.Logger.Warn($"Skipping saved projectile '{name}': that content no longer exists.");
				continue;
			}

			Vector2 pos = proj.Get<Vector2>("pos");

			int index = Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, saveProjectile.Type, 0, 0, -1);
			if (Main.projectile[index].ModProjectile is ISaveProjectile savedProj)
			{
				savedProj.LoadData(proj, Main.projectile[index]);
			}
		}
	}
}

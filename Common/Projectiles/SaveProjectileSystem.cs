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
			proj.Add("pos", projectile.position);
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
			int type = ModContent.Find<ModProjectile>(proj.GetString("name")).Type;
			Vector2 pos = proj.Get<Vector2>("pos");

			int index = Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, type, 0, 0, -1);
			var savedProj = Main.projectile[index].ModProjectile as ISaveProjectile;
			savedProj.LoadData(proj, Main.projectile[index]);
		}
	}
}

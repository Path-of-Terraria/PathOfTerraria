using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems;

internal class PersistentBuffSystem : ModPlayer
{
	private readonly record struct SavedProjectile(int type, int time, int damage, float knockBack);

	private readonly List<SavedProjectile> _savedProjectiles = [];

	public override void SaveData(TagCompound tag)
	{
		TagCompound projectiles = [];
		int count = 0;

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.owner == Player.whoAmI && projectile.minion)
			{
				projectiles.Add("projType_" + count, projectile.type);
				projectiles.Add("projTime_" + count, projectile.timeLeft);
				projectiles.Add("projDamage_" + count, projectile.timeLeft);
				projectiles.Add("projKnockback_" + count, projectile.knockBack);
				count++;
			}
		}

		if (count > 0)
		{
			projectiles.Add("count", count);
			tag.Add("savedProjectiles", projectiles);
		}
	}

	public override void LoadData(TagCompound tag)
	{
		if (tag.TryGet("savedProjectiles", out TagCompound projectiles))
		{
			int count = projectiles.GetInt("count");

			for (int i = 0; i < count; ++i)
			{
				int type = projectiles.GetInt("projType_" + i);
				int time = projectiles.GetInt("projTime_" + i);
				int damage = projectiles.GetInt("projDamage_" + i);
				float knockBack = projectiles.GetInt("projKnockback_" + i);

				_savedProjectiles.Add(new SavedProjectile(type, time, damage, knockBack));
			}
		}
	}

	public override void OnEnterWorld()
	{
		Vector2 pos = Player.Center - new Vector2(0, 10);

		foreach (SavedProjectile item in _savedProjectiles)
		{
			int proj = Projectile.NewProjectile(Player.GetSource_FromThis(), pos, Vector2.Zero, item.type, item.damage, item.knockBack, Player.whoAmI);
			Main.projectile[proj].timeLeft = item.time;
			Main.projectile[proj].netUpdate = true;
		}

		_savedProjectiles.Clear();
	}
}

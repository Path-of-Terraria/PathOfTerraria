using PathOfTerraria.Common.Projectiles;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems;

internal class PersistentMinionsPlayer : ModPlayer
{
	private class PersistentMinionProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		internal int OriginalDamage = 0;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.minion && !CustomProjectileSets.NonPersistentProjectiles[entity.type];
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			OriginalDamage = projectile.damage;
		}
	}

	private readonly record struct SavedProjectile(int type, int time, int damage, float knockBack);

	private readonly List<SavedProjectile> _savedProjectiles = [];

	public override void SaveData(TagCompound tag)
	{
		TagCompound projectiles = [];
		int count = 0;

		if (Main.gameMenu) // Skip this on the main menu, as it can't run properly
		{
			return;
		}

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.owner == Player.whoAmI && projectile.TryGetGlobalProjectile(out PersistentMinionProjectile persist))
			{
				int damage = persist.OriginalDamage;

				projectiles.Add("projType_" + count, projectile.type);
				projectiles.Add("projTime_" + count, projectile.timeLeft);
				projectiles.Add("projDamage_" + count, damage);
				projectiles.Add("projKnockback_" + count, projectile.knockBack);
				count++;

				// Store the projectile so this works in singleplayer, as while players ARE saved between subworlds,
				// they are NOT loaded
				_savedProjectiles.Add(new SavedProjectile(projectile.type, projectile.timeLeft, damage, projectile.knockBack));
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
				float knockBack = projectiles.GetFloat("projKnockback_" + i);

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
			Main.projectile[proj].damage = item.damage;
			Main.projectile[proj].originalDamage = item.damage;
		}

		_savedProjectiles.Clear();
	}
}


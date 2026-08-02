using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems;

internal class PersistentMinionsPlayer : ModPlayer, IPreDomainRespawnPlayer
{
	private static readonly HashSet<int> MinionBuffTypes = [];

	private sealed class MinionBuffCacheSystem : ModSystem
	{
		public override void PostSetupContent()
		{
			MinionBuffTypes.Clear();

			foreach (Item item in ContentSamples.ItemsByType.Values)
			{
				if (item.buffType > 0 && item.shoot > ProjectileID.None
					&& ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile projectile) && projectile.minion)
				{
					MinionBuffTypes.Add(item.buffType);
				}
			}
		}

		public override void Unload()
		{
			MinionBuffTypes.Clear();
		}
	}

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
	private readonly record struct SavedBuff(int type, int time);

	private readonly List<SavedProjectile> _savedProjectiles = [];
	private readonly List<SavedBuff> _savedMinionBuffs = [];
	private readonly List<SavedBuff> _savedStationBuffs = [];
	private readonly List<SavedProjectile> _respawnProjectiles = [];
	private readonly List<SavedBuff> _respawnMinionBuffs = [];

	private int _domainRespawnRestoreDelay;

	public override void SaveData(TagCompound tag)
	{
		if (Main.gameMenu) // Skip this on the main menu, as it can't run properly
		{
			return;
		}

		CaptureProjectiles(_savedProjectiles);
		CaptureMinionBuffs(_savedMinionBuffs, _savedProjectiles.Count > 0);
		CaptureStationBuffs(_savedStationBuffs);
		SaveProjectiles(tag, "savedProjectiles", _savedProjectiles);
		SaveBuffs(tag, "savedMinionBuffs", _savedMinionBuffs);
		SaveBuffs(tag, "savedStationBuffs", _savedStationBuffs);
	}

	public override void LoadData(TagCompound tag)
	{
		LoadProjectiles(tag, "savedProjectiles", _savedProjectiles);
		LoadBuffs(tag, "savedMinionBuffs", _savedMinionBuffs);
		LoadBuffs(tag, "savedStationBuffs", _savedStationBuffs);
	}

	public override void OnEnterWorld()
	{
		RestoreBuffs(_savedMinionBuffs);
		RestoreBuffs(_savedStationBuffs);
		RestoreProjectiles(_savedProjectiles);

		_savedProjectiles.Clear();
		_savedMinionBuffs.Clear();
		_savedStationBuffs.Clear();
	}

	public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
	{
		if (Player.whoAmI == Main.myPlayer)
		{
			CaptureRespawnState();
		}

		return true;
	}

	public override void OnRespawn()
	{
		if (Player.whoAmI != Main.myPlayer)
		{
			return;
		}

		_domainRespawnRestoreDelay = 0;
		RestoreRespawnState();
	}

	public void OnDomainRespawn()
	{
		if (Player.whoAmI != Main.myPlayer)
		{
			return;
		}

		CaptureRespawnState();

		// Boss-domain lives cancel normal death from inside PreKill. Wait until the following update
		// so any vanilla death cleanup has finished before restoring the minions.
		_domainRespawnRestoreDelay = 2;
	}

	public override void PostUpdate()
	{
		if (Player.whoAmI != Main.myPlayer || _domainRespawnRestoreDelay <= 0 || --_domainRespawnRestoreDelay > 0)
		{
			return;
		}

		RestoreRespawnState();
	}

	private void CaptureRespawnState()
	{
		CaptureProjectiles(_respawnProjectiles);
		CaptureMinionBuffs(_respawnMinionBuffs, _respawnProjectiles.Count > 0);
	}

	private void RestoreRespawnState()
	{
		RestoreBuffs(_respawnMinionBuffs);
		RestoreProjectiles(_respawnProjectiles);
		_respawnProjectiles.Clear();
		_respawnMinionBuffs.Clear();
	}

	private void CaptureProjectiles(List<SavedProjectile> destination)
	{
		destination.Clear();

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.owner != Player.whoAmI || !projectile.TryGetGlobalProjectile(out PersistentMinionProjectile persist))
			{
				continue;
			}

			destination.Add(new SavedProjectile(projectile.type, projectile.timeLeft, persist.OriginalDamage, projectile.knockBack));
		}
	}

	private void CaptureMinionBuffs(List<SavedBuff> destination, bool hasMinions)
	{
		destination.Clear();

		if (!hasMinions)
		{
			return;
		}

		for (int i = 0; i < Player.buffType.Length; ++i)
		{
			int type = Player.buffType[i];

			if (MinionBuffTypes.Contains(type))
			{
				destination.Add(new SavedBuff(type, Player.buffTime[i]));
			}
		}
	}

	private void CaptureStationBuffs(List<SavedBuff> destination)
	{
		destination.Clear();

		int[] stationBuffTypes = [BuffID.Bewitched, BuffID.WarTable];

		foreach (int type in stationBuffTypes)
		{
			int index = Player.FindBuffIndex(type);

			if (index >= 0)
			{
				destination.Add(new SavedBuff(type, Player.buffTime[index]));
			}
		}
	}

	private void RestoreProjectiles(List<SavedProjectile> projectiles)
	{
		Dictionary<int, int> activeCounts = [];

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.owner == Player.whoAmI && projectile.TryGetGlobalProjectile(out PersistentMinionProjectile _))
			{
				activeCounts.TryGetValue(projectile.type, out int count);
				activeCounts[projectile.type] = count + 1;
			}
		}

		Vector2 position = Player.Center - new Vector2(0, 10);

		foreach (SavedProjectile item in projectiles)
		{
			if (activeCounts.TryGetValue(item.type, out int count) && count > 0)
			{
				activeCounts[item.type] = count - 1;
				continue;
			}

			int index = Projectile.NewProjectile(Player.GetSource_FromThis(), position, Vector2.Zero, item.type, item.damage, item.knockBack, Player.whoAmI);

			if (index >= Main.maxProjectiles)
			{
				continue;
			}

			Projectile projectile = Main.projectile[index];
			projectile.timeLeft = item.time;
			projectile.netUpdate = true;
			projectile.damage = item.damage;
			projectile.originalDamage = item.damage;
		}
	}

	private void RestoreBuffs(List<SavedBuff> buffs)
	{
		foreach (SavedBuff buff in buffs)
		{
			Player.AddBuff(buff.type, buff.time, quiet: false);
		}
	}

	private static void SaveProjectiles(TagCompound tag, string key, List<SavedProjectile> projectiles)
	{
		if (projectiles.Count == 0)
		{
			return;
		}

		TagCompound saved = [];
		saved.Add("count", projectiles.Count);

		for (int i = 0; i < projectiles.Count; ++i)
		{
			SavedProjectile projectile = projectiles[i];
			saved.Add("projType_" + i, projectile.type);
			saved.Add("projTime_" + i, projectile.time);
			saved.Add("projDamage_" + i, projectile.damage);
			saved.Add("projKnockback_" + i, projectile.knockBack);
		}

		tag.Add(key, saved);
	}

	private static void LoadProjectiles(TagCompound tag, string key, List<SavedProjectile> destination)
	{
		destination.Clear();

		if (!tag.TryGet(key, out TagCompound projectiles))
		{
			return;
		}

		int count = projectiles.GetInt("count");

		for (int i = 0; i < count; ++i)
		{
			destination.Add(new SavedProjectile(
				projectiles.GetInt("projType_" + i),
				projectiles.GetInt("projTime_" + i),
				projectiles.GetInt("projDamage_" + i),
				projectiles.GetFloat("projKnockback_" + i)));
		}
	}

	private static void SaveBuffs(TagCompound tag, string key, List<SavedBuff> buffs)
	{
		if (buffs.Count == 0)
		{
			return;
		}

		TagCompound saved = [];
		saved.Add("count", buffs.Count);

		for (int i = 0; i < buffs.Count; ++i)
		{
			saved.Add("buffType_" + i, buffs[i].type);
			saved.Add("buffTime_" + i, buffs[i].time);
		}

		tag.Add(key, saved);
	}

	private static void LoadBuffs(TagCompound tag, string key, List<SavedBuff> destination)
	{
		destination.Clear();

		if (!tag.TryGet(key, out TagCompound buffs))
		{
			return;
		}

		int count = buffs.GetInt("count");

		for (int i = 0; i < count; ++i)
		{
			destination.Add(new SavedBuff(buffs.GetInt("buffType_" + i), buffs.GetInt("buffTime_" + i)));
		}
	}
}

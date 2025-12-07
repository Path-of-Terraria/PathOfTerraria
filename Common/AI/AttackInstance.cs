using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Common.AI;

/// <summary> Used to continuously deal damage during an attack, while skipping entities that already received it. </summary>
internal class AttackInstance()
{
	[Flags]
	public enum EntityKind
	{
		None = 0,
		All = Player | Enemy | Friendly | Boss,
		Player = 1 << 0,
		Enemy = 1 << 1,
		Neutral = 1 << 3,
		Friendly = 1 << 4,
		Boss = 1 << 5,
	}

	public required Rectangle Aabb;
	public required Func<Player, PlayerDeathReason> DeathReason;
	public required int Damage;
	public required float Knockback;
	public required Vector2 Direction;
	public required EntityKind Filter = EntityKind.All;
	public Predicate<Entity> Predicate = static _ => true;
	public HashSet<int>? HitEntities;
	public DamageClass? DamageType;

	public void DamageEntities()
	{
		int directionSign = Direction.X >= 0f ? 1 : -1;

		foreach (Player player in Main.ActivePlayers)
		{
			int hitIndex = 0 + player.whoAmI;
			if (Filter.HasFlag(EntityKind.Player) && Predicate!(player) && Aabb.Intersects(player.Hitbox) && HitEntities?.Contains(hitIndex) != true)
			{
				player.Hurt(DeathReason(player), Damage, directionSign);
				(HitEntities ??= []).Add(hitIndex);
			}
		}

		foreach (NPC npc in Main.ActiveNPCs)
		{
			int hitIndex = 300 + npc.whoAmI;
			EntityKind kind = npc switch
			{
				_ when npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type] => EntityKind.Boss,
				_ when npc.friendly => EntityKind.Friendly,
				_ when npc.damage <= 0 => EntityKind.Neutral,
				_ => EntityKind.Enemy,
			};
			if (!npc.immortal && Filter.HasFlag(kind) && Predicate!(npc) && Aabb.Intersects(npc.Hitbox) && HitEntities?.Contains(hitIndex) != true)
			{
				npc.SimpleStrikeNPC(Damage, directionSign, false, Knockback);
				(HitEntities ??= []).Add(hitIndex);
			}
		}
	}
}

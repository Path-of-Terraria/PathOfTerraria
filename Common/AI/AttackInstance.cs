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
		LocalPlayer = 1 << 1,
		RemotePlayer = 1 << 2,
		EnemyNPC = 1 << 3,
		NeutralNPC = 1 << 4,
		FriendlyNPC = 1 << 5,
		BossNPC = 1 << 6,
		Player = LocalPlayer | RemotePlayer,
		NPC = EnemyNPC | NeutralNPC | FriendlyNPC | BossNPC,
		All = Player | NPC,
	}

	public required Rectangle Aabb;
	public required Func<Player, PlayerDeathReason> DeathReason;
	public required int Damage;
	public required float Knockback;
	public required Vector2 Direction;
	public required EntityKind Filter = EntityKind.All;
	public Predicate<Entity> Predicate = static _ => true;
	public int ExcludedEntityIndex = -1;
	//TODO: Create a concept of entity slot versions to account for sudden slot overtakes?
	public HashSet<int>? HitEntities;
	public DamageClass? DamageType;

	public Entity ExcludedEntity
	{
		init => ExcludedEntityIndex = EncodeIndex(value);
	}

	public void DamageEntities()
	{
		int directionSign = Direction.X >= 0f ? 1 : -1;

		foreach (Player player in Main.ActivePlayers)
		{
			int hitIndex = PlayerIndex(player);
			EntityKind kind = player.whoAmI == Main.myPlayer ? EntityKind.LocalPlayer : EntityKind.RemotePlayer;

			if ((Filter & kind) != 0 && hitIndex != ExcludedEntityIndex && Predicate!(player) && Aabb.Intersects(player.Hitbox) && HitEntities?.Contains(hitIndex) != true)
			{
				player.Hurt(DeathReason(player), Damage, directionSign);
				(HitEntities ??= []).Add(hitIndex);
			}
		}

		foreach (NPC npc in Main.ActiveNPCs)
		{
			int hitIndex = NPCIndex(npc);
			EntityKind kind = 0;
			kind |= (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) ? EntityKind.BossNPC : 0;
			kind |= (npc.friendly) ? EntityKind.FriendlyNPC : 0;
			kind |= (npc.damage <= 0) ? EntityKind.NeutralNPC : 0;
			kind |= (npc.damage > 0 && !npc.friendly) ? EntityKind.EnemyNPC : 0;

			if (!npc.immortal && (Filter & kind) != 0 && hitIndex != ExcludedEntityIndex && Predicate!(npc) && Aabb.Intersects(npc.Hitbox) && HitEntities?.Contains(hitIndex) != true)
			{
				npc.SimpleStrikeNPC(Damage, directionSign, false, Knockback);
				(HitEntities ??= []).Add(hitIndex);
			}
		}
	}

	public static int EncodeIndex(Entity entity)
	{
		return entity switch
		{
			Player p => PlayerIndex(p),
			NPC n => NPCIndex(n),
			_ => int.MinValue + entity.whoAmI,
		};
	}

	private static int PlayerIndex(Player p)
	{
		return p.whoAmI;
	}
	private static int NPCIndex(NPC n)
	{
		return 300 + n.whoAmI;
	}
}

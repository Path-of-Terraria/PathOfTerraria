﻿namespace PathOfTerraria.Common.Events;

public class PathOfTerrariaNpcEvents : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public delegate void PostAIDelegate(NPC NPC);

	public static event PostAIDelegate PostAIEvent;

	public override void PostAI(NPC NPC)
	{
		PostAIEvent?.Invoke(NPC);
	}

	public delegate void OnKillDelegate(NPC NPC);

	public static event OnKillDelegate OnKillEvent;

	public override void OnKill(NPC NPC)
	{
		OnKillEvent?.Invoke(NPC);
	}

	//these modify hit bys should only be used for editing the ref variables if you want them changed in a way that happens BEFORE Player on hit effects run. no extra effects will be synced in multiPlayer outside of the ref variables
	public delegate void ModifyHitByItemDelegate(NPC NPC, Player Player, Item Item, ref NPC.HitModifiers modifiers);

	public static event ModifyHitByItemDelegate ModifyHitByItemEvent;

	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		ModifyHitByItemEvent?.Invoke(npc, player, item, ref modifiers);
	}

	public delegate void ModifyHitByProjectileDelegate(NPC NPC, Projectile Projectile, ref NPC.HitModifiers modifiers);

	public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;

	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		ModifyHitByProjectileEvent?.Invoke(npc, projectile, ref modifiers);
	}

	public delegate void ModifyHitPlayerDelegate(NPC NPC, Player target, ref Player.HurtModifiers modifiers);

	public static event ModifyHitPlayerDelegate ModifyHitPlayerEvent;

	public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
	{
		ModifyHitPlayerEvent?.Invoke(npc, target, ref modifiers);
	}

	public delegate void OnKillByDelegate(NPC NPC, Player player);

	public static event OnKillByDelegate OnKillByEvent;

	public delegate void OnHitByItemDelegate(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone);

	public static event OnHitByItemDelegate OnHitByItemEvent;

	public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
	{
		if (npc.life <= 0)
		{
			OnKillByEvent?.Invoke(npc, player);
		}

		OnHitByItemEvent?.Invoke(npc, player, item, hit, damageDone);
	}

	public delegate void OnHitByProjectileDelegate(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone);

	public static event OnHitByProjectileDelegate OnHitByProjectileEvent;

	public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
	{
		if (Main.player[projectile.owner] is not null)
		{
			if (npc.life <= 0)
			{
				OnKillByEvent?.Invoke(npc, Main.player[projectile.owner]);
			}
		}

		OnHitByProjectileEvent?.Invoke(npc, projectile, hit, damageDone);
	}

	public delegate void ResetEffectsDelegate(NPC NPC);

	public static event ResetEffectsDelegate ResetEffectsEvent;

	public override void ResetEffects(NPC NPC)
	{
		ResetEffectsEvent?.Invoke(NPC);
	}

	public delegate void ModifyNPCLootDelegate(NPC npc, NPCLoot npcloot);

	public static event ModifyNPCLootDelegate ModifyNPCLootEvent;

	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		ModifyNPCLootEvent?.Invoke(npc, npcLoot);
	}

	public delegate void ModifyGlobalLootDelegate(GlobalLoot globalLoot);

	public static event ModifyGlobalLootDelegate ModifyGlobalLootEvent;

	public override void ModifyGlobalLoot(GlobalLoot globalLoot)
	{
		ModifyGlobalLootEvent?.Invoke(globalLoot);
	}

	public override void Unload()
	{
		PostAIEvent = null;
		OnKillEvent = null;
		OnKillByEvent = null;
		ModifyHitByItemEvent = null;
		ModifyHitByProjectileEvent = null;
		ModifyHitPlayerEvent = null;
		ResetEffectsEvent = null;
		ModifyNPCLootEvent = null;
		ModifyGlobalLootEvent = null;
	}
}
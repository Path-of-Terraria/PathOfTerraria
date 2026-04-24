using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using Terraria.ID;

namespace PathOfTerraria.Common.Buffs;

internal class DoTFunctionality
{
	public static void ApplyPlayerInteraction(NPC npc, Entity attacker)
	{
		if (attacker is Player player)
		{
			npc.ApplyInteraction(player.whoAmI);
		}
	}

	public static void ApplyPlayerInteraction(NPC npc, Player player)
	{
		npc.ApplyInteraction(player.whoAmI);
	}

	public static void ApplyPlayerInteraction(NPC npc, int playerWhoAmI)
	{
		if (playerWhoAmI is < 0 or >= Main.maxPlayers)
		{
			return;
		}

		Player player = Main.player[playerWhoAmI];

		if (player.active)
		{
			npc.ApplyInteraction(playerWhoAmI);
		}
	}

	public static void ApplyDoT(NPC npc, int damage, ref float elapsedDoT, Color? lightColor = null, Color? darkColor = null)
	{
		lightColor ??= Color.OrangeRed;
		darkColor ??= Color.Orange;

		// Clamp damage so we don't deal 30 damage to a 20 hp target
		if (damage > npc.life)
		{
			damage = npc.life;
		}

		if (!npc.dontTakeDamage && !npc.immortal)
		{
			if (npc.realLife == -1)
			{
				npc.life -= damage;
			}
			else
			{
				Main.npc[npc.realLife].life -= damage;
			}
		}

		elapsedDoT -= damage;

		// Vanilla is dumb, this is the easiest way to properly kill an NPC while showing gore & doing death effects,
		// that is, WITHOUT calling StrikeNPC for every hit, which causes a hit sound and forces a specific combat text
		if (npc.life <= 0 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			npc.life = 1;

			NPC.HitInfo info = default;
			info.HideCombatText = true;
			info.Damage = 1;
			info.DamageType = DamageClass.Default;
			npc.StrikeNPC(info);

			if (Main.dedServ)
			{
				// For some reason gores aren't spawned through StrikeNPC, spawn manually
				SendSpawnVFXModule.SendNPCGores(npc);
			}
		}

		CombatText.NewText(npc.Hitbox, Color.Lerp(lightColor.Value, darkColor.Value, Main.rand.NextFloat()), damage, false, true);
	}
}

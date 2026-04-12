using System.Diagnostics;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

#nullable enable

/// <summary>
/// Used to streamline calls which may take either an NPC being hit or a Player being hit. Is binary; that is, either a player is hurt or an NPC is hurt, not both.
/// </summary>
internal readonly struct HitInfoContainer
{
	public bool IsPlayerInfo => PlayerHurt is not null;
	public int DamageDealt => IsPlayerInfo ? PlayerHurt!.Value.Damage : NPCHurt!.Value.Damage;
	public bool IsCrit => IsPlayerInfo ? NPCCritFunctionality.CriticalStrikeNPC.CurrentlyCritting : NPCHurt!.Value.Crit;

	public readonly NPC.HitInfo? NPCHurt = null;
	public readonly Player.HurtInfo? PlayerHurt = null;

	public HitInfoContainer(NPC.HitInfo? hit, Player.HurtInfo? hurt)
	{
		NPCHurt = hit;
		PlayerHurt = hurt;

		Debug.Assert(NPCHurt is null || PlayerHurt is null, "Only one info may be filled, not both.");
	}
}

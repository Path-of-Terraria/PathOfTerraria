namespace PathOfTerraria.Common.Systems.ElementalDamage;

#nullable enable

/// <summary>
/// Used to streamline calls which may take either an NPC being hit or a Player being hit.
/// </summary>
internal readonly struct HitInfoContainer(NPC.HitInfo? hit, Player.HurtInfo? hurt)
{
	internal readonly NPC.HitInfo? NPCHurt = hit;
	internal readonly Player.HurtInfo? PlayerHurt = hurt;
}

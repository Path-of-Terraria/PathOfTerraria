namespace PathOfTerraria.Content.Buffs;

/// <summary>
/// Functionality is in SkillCombatPlayer.
/// </summary>
public sealed class AnticipatedStrikeBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
	}
}
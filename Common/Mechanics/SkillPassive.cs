using PathOfTerraria.Common.Systems.Skills;
using Terraria.Localization;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillPassive(SkillTree tree) : SkillNode(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/SkillPassives/" + Name;
	public override string DisplayName => Language.GetOrRegister("Mods.PathOfTerraria.SkillPassives." + Name + ".Name").Value;
	public override string DisplayTooltip => Language.GetOrRegister("Mods.PathOfTerraria.SkillPassives." + Name + ".Tooltip").Value;

	/// <summary> The effects of this skill passive. </summary>
	public virtual void PassiveEffects(ref SkillBuff buff) { }

	public override bool CanAllocate(Player player)
	{
		return base.CanAllocate(player) && Tree.Points > 0;
	}

	public override void OnAllocate(Player player)
	{
		int level = Level;
		base.OnAllocate(player);
		Tree.Points--;

		player.GetModPlayer<SkillTreePlayer>().ModifyPassive(Tree, GetType(), Level - level);
	}

	public override void OnDeallocate(Player player)
	{
		int level = Level;
		base.OnDeallocate(player);
		Tree.Points++;

		player.GetModPlayer<SkillTreePlayer>().ModifyPassive(Tree, GetType(), Level - level);
	}
}
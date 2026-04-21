using PathOfTerraria.Common.Systems.Skills;
using Terraria.Localization;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillPassive(SkillTree tree) : SkillNode(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/SkillPassives/" + Name;
	public override string DisplayName => Language.GetOrRegister("Mods.PathOfTerraria.SkillPassives." + Name + ".Name", () => GetType().Name).Value;
	public override string DisplayTooltip => Language.GetOrRegister("Mods.PathOfTerraria.SkillPassives." + Name + ".Tooltip").Format(TooltipArguments);

	public virtual object[] TooltipArguments { get; } = [];

	/// <summary> The effects of this skill passive. </summary>
	public virtual void PassiveEffects(ref SkillBuff buff) { }

	public override bool CanAllocate(Player player)
	{
		SkillTreePlayer treePlayer = player.GetModPlayer<SkillTreePlayer>();
		return base.CanAllocate(player)
			&& treePlayer.GetAvailableSkillPoints(Tree) > 0;
	}

	public override void OnAllocate(Player player)
	{
		base.OnAllocate(player);
		player.GetModPlayer<SkillTreePlayer>().ModifySkillPassive(Tree, GetType(), Level);
	}

	public override void OnDeallocate(Player player)
	{
		base.OnDeallocate(player);
		player.GetModPlayer<SkillTreePlayer>().ModifySkillPassive(Tree, GetType(), Level);
	}

	public override string ToString()
	{
		return $"{Name}: {Level}/{MaxLevel}";
	}
}

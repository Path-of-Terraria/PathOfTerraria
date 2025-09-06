using PathOfTerraria.Common.Systems.Skills;
using Terraria.Localization;

namespace PathOfTerraria.Common.Mechanics;

/// <summary> The base class for skill specializations. Only one specialization can be selected per skill tree. </summary>
public abstract class SkillSpecial(SkillTree tree) : SkillNode(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/SkillSpecials/" + Name;
	public override string DisplayName => Language.GetOrRegister("Mods.PathOfTerraria.SkillSpecials." + Name + ".Name").Value;
	public override string DisplayTooltip
	{
		get
		{
			string tooltip = Language.GetOrRegister("Mods.PathOfTerraria.SkillSpecials." + Name + ".Description").Value;
			tooltip += "\n\n" + Language.GetTextValue($"Mods.{PoTMod.Instance.Name}.SkillSpecials.OneLine");

			return tooltip;
		}
	}

	public override void OnAllocate(Player player)
	{
		base.OnAllocate(player);
		player.GetModPlayer<SkillTreePlayer>().SetSpecializationForSkill(Tree.ParentSkill, this);
		Tree.Specialization = this;
	}

	public override void OnDeallocate(Player player)
	{
		base.OnDeallocate(player);
		player.GetModPlayer<SkillTreePlayer>().SpecializationsBySkill.Remove(Tree.ParentSkill);
		Tree.Specialization = null;
	}

	/// <summary> Whether this skill specialization can be used. </summary>
	public override bool CanAllocate(Player player)
	{
		return Connections > 0 && Tree.Specialization is null;
	}

	/// <summary> Whether this skill specialization can be refunded. </summary>
	public override bool CanDeallocate(Player player)
	{
		return Connections < 2 && Tree.Specialization == this;
	}
}
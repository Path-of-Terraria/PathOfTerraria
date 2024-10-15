using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Gear.Weapons.Javelins;
using PathOfTerraria.Content.Skills.Melee;
using PathOfTerraria.Content.Skills.Ranged;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class MoltenShellAffix : ItemAffix
{
	public override void OnLoad()
	{
		OnSwapPlayer.OnSwapMainItem += EnableMoltenShellIfOpen;
	}

	private void EnableMoltenShellIfOpen(Player self, Item newItem, Item oldItem)
	{
		if (newItem.type == ModContent.ItemType<MoltenDangpa>())
		{
			SkillCombatPlayer skillCombatPlayer = self.GetModPlayer<SkillCombatPlayer>();
			skillCombatPlayer.TryAddSkill(new MoltenShield());
		}
	}
}

internal class BloodSiphonAffix : ItemAffix
{
	public override void OnLoad()
	{
		OnSwapPlayer.OnSwapMainItem += EnableBloodSiphonIfOpen;
	}

	private void EnableBloodSiphonIfOpen(Player self, Item newItem, Item oldItem)
	{
		if (newItem.type == ModContent.ItemType<Bloodclotter>())
		{
			SkillCombatPlayer skillCombatPlayer = self.GetModPlayer<SkillCombatPlayer>();
			skillCombatPlayer.TryAddSkill(new BloodSiphon());
		}
	}
}

internal class FetidCarapaceAffix : ItemAffix
{
	public override void OnLoad()
	{
		OnSwapPlayer.OnSwapMainItem += EnableFetidCarapaceIfOpen;
	}

	private void EnableFetidCarapaceIfOpen(Player self, Item newItem, Item oldItem)
	{
		if (newItem.type == ModContent.ItemType<Rottenbone>())
		{
			SkillCombatPlayer skillCombatPlayer = self.GetModPlayer<SkillCombatPlayer>();
			skillCombatPlayer.TryAddSkill(new FetidCarapace());
		}
	}
}
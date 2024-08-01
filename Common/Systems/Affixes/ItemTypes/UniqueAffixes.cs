using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Gear.Weapons.Javelins;
using PathOfTerraria.Content.Skills.Melee;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

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
			SkillPlayer skillPlayer = self.GetModPlayer<SkillPlayer>();
			skillPlayer.TryAddSkill(new MoltenShield());
		}
	}
}

using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Gear.Weapons.Javelins;
using PathOfTerraria.Content.Skills.Melee;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

internal class NoFallDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		if (player != null)
		{
			player.noFallDmg = true;
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
			SkillPlayer skillPlayer = self.GetModPlayer<SkillPlayer>();
			skillPlayer.TryAddSkill(new FetidCarapace());
		}
	}
}

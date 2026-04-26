using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Enums;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class FriendlyDefensePassive : Passive
{
	private class FriendlyDefensePlayer : ModPlayer
	{
		public override void PostUpdateEquips()
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.whoAmI != Player.whoAmI && player.GetModPlayer<PassiveTreePlayer>().HasNode<FriendlyDefensePassive>() && (player.team == (int)Team.None || player.team == Player.team))
				{
					Player.statDefense += FriendlyDefense;
				}
			}
		}
	}

	private const int FriendlyDefense = 4;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, FriendlyDefense);

	public override void BuffPlayer(Player player)
	{
		player.statDefense += (int)Value;
	}
}

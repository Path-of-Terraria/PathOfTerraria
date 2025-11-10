using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

// +{0} Increased Maximum Mana
internal class AddedManaPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statManaMax2 += Value * Level;
	}
} 

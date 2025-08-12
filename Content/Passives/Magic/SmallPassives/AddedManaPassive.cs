using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;

// +{0} Increased Maximum Mana
internal class AddedManaPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statManaMax2 += Value * Level;
	}
} 




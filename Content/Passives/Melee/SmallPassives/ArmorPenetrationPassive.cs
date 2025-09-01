using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;


internal class ArmorPenetrationPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetArmorPenetration(DamageClass.Melee) *= 1 + (Value/100.0f) * Level;
	}
}
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;


internal class IncreasedSummonDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Summon) += (Value/100.0f) * Level;
	}
}

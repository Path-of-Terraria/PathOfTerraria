using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;


internal class IncreasedWhipDamagePassive : Passive
{
}

//internal class WhipModsItem : GlobalItem
//{
//	public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
//	{
//		if (item.shoot > ProjectileID.None && ProjectileID.Sets.IsAWhip[item.shoot])
//		{
//			damage += player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(IncreasedWhipDamagePassive)) * 0.05f;
//		}
//	}
//
//	public override float UseSpeedMultiplier(Item item, Player player)
//	{
//		if (item.shoot > ProjectileID.None && ProjectileID.Sets.IsAWhip[item.shoot])
//		{
//			return 1 + player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(IncreasedWhipSpeedPassive)) * 0.05f;
//		}
//
//		return 1;
//	}
//}
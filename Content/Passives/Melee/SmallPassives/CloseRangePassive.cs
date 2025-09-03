using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;


internal class CloseRangePassive : Passive
{
	internal class CloseRangePlayer : ModPlayer
	{
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			float str = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(CloseRangePassive));

			if (str < 0)
			{
				return;
			}

			if (target.DistanceSQ(Player.Center) < 200 * 200)
			{
				modifiers.FinalDamage += str * 0.1f; 
			}
		}
	}
}


using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedMinionDamagePassive : Passive
{
	public sealed class IncreasedMinionDamagePassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			int level = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(Name);

			if (proj.minion)
			{
				modifiers.FinalDamage += (ModContent.GetInstance<IncreasedMinionDamagePassive>().Value / 100.0f) * level;
			}
		}
	}
}

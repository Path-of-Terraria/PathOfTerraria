using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddRagePassive : Passive
{
	internal class AddRagePlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<AddRagePassive>(out float value))
			{
				Player.GetModPlayer<RagePlayer>().AddRage(value);
			}
		}
	}
}


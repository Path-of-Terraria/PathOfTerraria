using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

public class IncreasedChannelingDamagePassive : Passive
{
	public sealed class IncreasedChannelingDamagePassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (!Player.channel)
			{
				return;
			}

			float bonus = Player.GetModPlayer<ChannelingModPlayer>().ChannelingDamageBonus;
			if (bonus > 0)
			{
				modifiers.FinalDamage += bonus;
			}
		}
	}

	public override void BuffPlayer(Player player)
	{
		// Store the channeling damage bonus in a ModPlayer
		player.GetModPlayer<ChannelingModPlayer>().ChannelingDamageBonus += (Value/100f) * Level;
	}
}
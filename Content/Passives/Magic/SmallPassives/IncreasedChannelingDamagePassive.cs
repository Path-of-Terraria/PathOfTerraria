using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

public class IncreasedChannelingDamagePassive : Passive
{
	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.ModifyHitNPCWithProjEvent += BuffChannelingDamage;
	}

	public override void BuffPlayer(Player player)
	{
		// Store the channeling damage bonus in a ModPlayer
		player.GetModPlayer<ChannelingModPlayer>().ChannelingDamageBonus += (Value/100f) * Level;
	}

	private void BuffChannelingDamage(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (!player.channel)
		{
			return;
		}
		
		float bonus = player.GetModPlayer<ChannelingModPlayer>().ChannelingDamageBonus;
		if (bonus > 0)
		{
			modifiers.FinalDamage += bonus;
		}
	}
}
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.MobSystem;

namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class SiphonerAffix : MobAffix
{
	public override ItemRarity MinimumRarity => ItemRarity.Rare;

	public class SiphonerNPC : GlobalNPC
	{
		public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
		{
			if (npc.GetGlobalNPC<ArpgNPC>().HasAffix<SiphonerAffix>())
			{
				target.CheckMana(hurtInfo.Damage / 2, true);
			}
		}
	}
}
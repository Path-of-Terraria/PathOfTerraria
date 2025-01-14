using PathOfTerraria.Common.Systems.MobSystem;

namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class LifeAffixes
{
	internal class DoubleLife : MobAffix
	{
		public override void PostRarity(NPC npc)
		{
			npc.lifeMax *= 2;
			npc.life = npc.lifeMax;
		}
	}

	internal class SiphonerAffix : MobAffix
	{
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
}
using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems.SkillSystem;

namespace PathOfTerraria.Content.Buffs
{
	class CustomRage() : SmartBuff("Rage", "Increased damage and greatly increased knockback", false)
	{
		public override string Texture => $"{PathOfTerraria.ModName}/Assets/Buffs/Base";

		public override void Load()
		{
			PathOfTerrariaNpcEvents.ModifyHitPlayerEvent += TakeMoreDamage;
		}

		private void TakeMoreDamage(NPC npc, Player target, ref Player.HurtModifiers modifiers)
		{
			modifiers.FinalDamage += 5;
		}

		public override void Update(Player Player, ref int buffIndex)
		{ 
			Player.GetDamage(DamageClass.Generic) += 1.5f; //150% more damage
		}
	}
}
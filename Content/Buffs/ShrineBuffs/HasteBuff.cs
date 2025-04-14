using PathOfTerraria.Common.Systems;
using System.Runtime.CompilerServices;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ShrineBuffs;

internal class HasteBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		// This allows for otherwise buff immune NPCs to have this effect
		BuffID.Sets.IsATagBuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.GetGlobalNPC<SpeedUpNPC>().ExtraAISpeed += 0.15f;

		if (!npc.noTileCollide)
		{
			npc.position += npc.velocity * 0.05f;
			UpdateCollision(npc);
		}
		else // NPCs without collision don't update their position in the same way for some reason
		{
			npc.position += npc.velocity * 0.8f;
		}

		[UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(UpdateCollision))]
		extern static void UpdateCollision(NPC npc);
	}

	public class HastePlayer : ModPlayer
	{
		public override void PostUpdateMiscEffects()
		{
			if (Player.HasBuff<HasteBuff>())
			{
				Player.runAcceleration *= 1.2f;
				Player.maxRunSpeed *= 1.2f;
				Player.moveSpeed += 0.2f;
				Player.GetAttackSpeed(DamageClass.Generic) += 0.25f;
			}
		}
	}
}

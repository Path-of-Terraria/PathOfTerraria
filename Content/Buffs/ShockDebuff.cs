using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.Affixes;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

public sealed class ShockDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (Main.rand.NextBool(35))
		{
			Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric);
		}
	}

	private sealed class ShockedNPC : GlobalNPC
	{
		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
		{
			if (npc.HasBuff<ShockDebuff>())
			{
				modifiers.FinalDamage += 0.1f * (1 + player.GetModPlayer<AffixPlayer>().StrengthOf<BuffShockedEffectAffix>() * 0.01f);
			}
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (npc.HasBuff<ShockDebuff>())
			{
				float mul = 0.1f;

				if (projectile.TryGetOwner(out Player owner))
				{
					mul *= owner.GetModPlayer<AffixPlayer>().StrengthOf<ChanceToApplyShockGearAffix>() * 0.01f;
				}

				modifiers.FinalDamage += mul;
			}
		}
	}
}
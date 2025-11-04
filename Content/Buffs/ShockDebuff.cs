using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.Affixes;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

public sealed class ShockDebuff : ModBuff
{
	public static void Apply(Player player, NPC npc, int damage)
	{
		float ailmentThreshold = npc.lifeMax;
		float modifier = player.GetModPlayer<AffixPlayer>().StrengthOf<BuffShockedEffectAffix>() * 0.01f;
		float effect = 0.5f * MathF.Pow(damage / ailmentThreshold, 0.4f) * (1 + modifier);

		if (effect <= 0.05f)
		{
			return;
		}

		npc.GetGlobalNPC<ShockedNPC>().ShockStrength = effect;
		npc.AddBuff(ModContent.BuffType<ShockDebuff>(), 2 * 60);
	}

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
}

public sealed class ShockedNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	private float shockStrength = 0;

	public float ShockStrength
	{
		get => shockStrength;
		set => shockStrength = MathHelper.Clamp(value, 0, 0.5f);
	}

	public override void ResetEffects(NPC npc)
	{
		if (!npc.HasBuff<ShockDebuff>())
		{
			shockStrength = 0;
		}
	}

	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		if (npc.HasBuff<ShockDebuff>())
		{
			modifiers.FinalDamage += ShockStrength * (1 + player.GetModPlayer<AffixPlayer>().StrengthOf<BuffShockedEffectAffix>() * 0.01f);
		}
	}

	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		if (npc.HasBuff<ShockDebuff>())
		{
			modifiers.FinalDamage += ShockStrength;
		}
	}
}
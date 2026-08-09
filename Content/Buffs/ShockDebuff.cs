using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using Terraria;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

public sealed class ShockDebuff : ModBuff
{
	/// <summary>
	/// Applies this buff to a given entity (Player or NPC). If victim is an <see cref="NPC"/>, attacker is a <see cref="Player"/>. NPCs cannot apply this buff to other NPCs at this time.
	/// </summary>
	public static void Apply(Entity attacker, Entity victim, int damage)
	{
		if (victim is NPC npc)
		{
			float ailmentThreshold = npc.lifeMax;
			float modifier = attacker is Player player ? player.GetModPlayer<AffixPlayer>().StrengthOf<BuffShockedEffectAffix>() * 0.01f : 0;
			float effect = 0.5f * MathF.Pow(damage / ailmentThreshold, 0.4f) * (1 + modifier);

			if (effect <= 0.05f)
			{
				return;
			}

			Common.Buffs.DoTFunctionality.ApplyPlayerInteraction(npc, attacker);

			npc.GetGlobalNPC<ShockedNPC>().ShockStrength = effect;
			npc.AddBuff(ModContent.BuffType<ShockDebuff>(), 2 * 60);
		}
		else if (victim is Player player)
		{
			float ailmentThreshold = player.statLifeMax2;
			float modifier = attacker is Player other ? other.GetModPlayer<AffixPlayer>().StrengthOf<BuffShockedEffectAffix>() * 0.01f : 0;
			float effect = 0.5f * MathF.Pow(damage / ailmentThreshold, 0.4f) * (1 + modifier);

			if (effect <= 0.05f)
			{
				return;
			}

			player.GetModPlayer<ShockPlayer>().ShockEffectiveness = effect;
			player.AddBuff(ModContent.BuffType<ShockDebuff>(), 4 * 60);
		}
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
			modifiers.FinalDamage += ShockStrength;
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

public class ShockPlayer : ModPlayer
{
	public float ShockEffectiveness { get; set; }

	public override void ModifyHurt(ref Player.HurtModifiers modifiers)
	{
		if (Player.HasBuff<ShockDebuff>())
		{
			modifiers.FinalDamage += ShockEffectiveness;
		}
	}
}

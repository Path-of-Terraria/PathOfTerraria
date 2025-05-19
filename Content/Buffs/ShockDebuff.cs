using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.Affixes;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

public sealed class ShockDebuff : ModBuff
{
    public enum DamageType
    {
        Fire,
        Cold,
        Lightning
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

        // Calculate debuff application chance based on damage dealt
        float damagePercent = npc.damage / (float)npc.lifeMax;
        float chance;
        if (damagePercent < 0.02f)
            chance = 0f;
        else if (damagePercent >= 0.5f)
            chance = 0.2f;
        else
            chance = damagePercent * 0.4f;

        if (Main.rand.NextFloat() <= chance)
        {
            // Apply the corresponding debuff based on damage type
            switch (GetDamageType(npc))
            {
                case DamageType.Fire:
                    npc.AddBuff(BuffID.OnFire, 300);
                    break;
                case DamageType.Cold:
                    npc.AddBuff(BuffID.Chilled, 300);
                    break;
                case DamageType.Lightning:
                    npc.AddBuff(BuffID.Electrified, 300);
                    break;
            }
        }
    }

    private DamageType GetDamageType(NPC npc)
    {
        // Placeholder logic to determine damage type
        // Replace this with actual logic to determine the damage type
        return DamageType.Lightning;
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

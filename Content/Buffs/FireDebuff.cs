using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PathOfTerraria.Content.Buffs
{
    public sealed class FireDebuff : ModBuff
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
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Fire);
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
            return DamageType.Fire;
        }

        private sealed class FireDebuffNPC : GlobalNPC
        {
            public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
            {
                if (npc.HasBuff<FireDebuff>())
                {
                    modifiers.FinalDamage += 0.1f;
                }
            }

            public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
            {
                if (npc.HasBuff<FireDebuff>())
                {
                    modifiers.FinalDamage += 0.1f;
                }
            }
        }
    }
}

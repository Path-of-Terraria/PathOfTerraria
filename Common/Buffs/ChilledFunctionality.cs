using PathOfTerraria.Common.Systems;
using Terraria.ID;

namespace PathOfTerraria.Common.Buffs;

internal class ChilledFunctionality : GlobalBuff
{
    public enum DamageType
    {
        Fire,
        Cold,
        Lightning
    }

    public override void Update(int type, NPC npc, ref int buffIndex)
    {
        if (type == BuffID.Chilled)
        {
            npc.GetGlobalNPC<SlowDownNPC>().SpeedModifier += 0.2f;

            if (Main.rand.NextBool(20))
            {
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Ice, npc.velocity.X, npc.velocity.Y);
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
    }

    private DamageType GetDamageType(NPC npc)
    {
        // Placeholder logic to determine damage type
        // Replace this with actual logic to determine the damage type
        return DamageType.Cold;
    }

    private class ChilledNPC : GlobalNPC
    {
        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            if (npc.HasBuff(BuffID.Chilled))
            {
                return Color.Lerp(drawColor, Color.LightBlue, 0.8f);
            }

            return null;
        }
    }
}

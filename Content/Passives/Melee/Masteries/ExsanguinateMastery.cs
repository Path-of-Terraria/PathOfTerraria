using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class ExsanguinateMastery : Passive
{
	internal class ExsanguinatePlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.life <= 0 && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ExsanguinateMastery>(out float value) 
				&& target.TryGetGlobalNPC(out BleedDebuffNPC bleed) && bleed.Stacks is { Length: > 0 } stacks)
			{
				float damage = 0;
				BleedStack empty = new(1, 0);

				foreach (BleedStack item in stacks)
				{
					damage += item.Damage / (float)BleedDebuff.DefaultTime * item.TimeLeft;
				}

				if (target.velocity.LengthSquared() > 0.1f)
				{
					damage *= 3;
				}

				for (int i = 0; i < stacks.Length; ++i)
				{
					bleed.SetStack(i, empty);
				}

				ExplosionHitbox.VFXPackage package = new(4, 30, 4, true, 0.6f, null, DustID.Blood, DustID.BloodWater);
				ExplosionHitbox.QuickSpawn(Player.GetSource_OnHit(target), target, (int)(damage * value / 100f), Player.whoAmI, target.Size * 1.5f, package);
			}
		}
	}
}

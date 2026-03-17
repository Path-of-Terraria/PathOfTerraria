using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class BlastProtectionPassive : Passive
{
	internal class BlastProtectionPlayer : ModPlayer 
	{
		public override bool FreeDodge(Player.HurtInfo info)
		{
			if (info.DamageSource.SourceProjectileLocalIndex == -1)
			{
				return false;
			}

			Projectile proj = Main.projectile[info.DamageSource.SourceProjectileLocalIndex];

			if (Player.GetModPlayer<PassiveTreePlayer>().HasNode<BlastProtectionPassive>() && proj.owner == Player.whoAmI && ProjectileID.Sets.Explosive[proj.type])
			{
				return true;
			}

			return false;
		}
	}
}
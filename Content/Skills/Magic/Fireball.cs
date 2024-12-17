using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.Magic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Utilities;

namespace PathOfTerraria.Content.Skills.Magic;

public class Fireball : Skill
{
	public override int MaxLevel => 3;
	
	public override List<SkillPassive> Passives =>
	[
		new SkillPassiveAnchor(this),
		new LightningNovaSkillPassive(this),
		new FireNovaSkillPassive(this),
		new IceNovaSkillPassive(this)
	];

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (15 - Level) * 60;
		ManaCost = 20 + 5 * Level;
		Duration = 0;
		WeaponType = ItemType.Magic;
	}

	public override void UseSkill(Player player)
	{
		player.statMana -= ManaCost;
		Timer = Cooldown;

		int damage = (int)(player.HeldItem.damage * (2 + 0.5f * Level));
		var source = new EntitySource_Misc("NovaSkill");
		float knockback = 2f;
		int type = ModContent.ProjectileType<FireballProj>();

		Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
	}

	public override bool CanUseSkill(Player player)
	{
		return base.CanUseSkill(player) && player.HeldItem.CountsAsClass(DamageClass.Magic);
	}

	private class FireballProj : ModProjectile
	{
		private const int TotalRadius = 300;

		public override string Texture => "Terraria/Images/NPC_0";

		private int Spread => (int)((1 - Projectile.timeLeft / 30f) * TotalRadius);

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.timeLeft = 30;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 5 * 60);
		}
	}
}
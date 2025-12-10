using Mono.Cecil;
using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.SkillPassives.FireballPassives;
using PathOfTerraria.Content.SkillPassives.SwarmPassives;
using PathOfTerraria.Content.SkillSpecials.FireballSpecials;
using PathOfTerraria.Content.SkillSpecials.NovaSpecials;
using PathOfTerraria.Content.SkillTrees;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using static System.Net.Mime.MediaTypeNames;

namespace PathOfTerraria.Content.Skills.Magic;

public class Fireball : Skill
{
	public override int MaxLevel => 3;
	
	public enum FireballType : byte
	{
		Normal,
		Inferno,
		Shadowflame,
		Frostfire,
	}

    public override SkillTags Tags()
    {
	    SkillTags tags = SkillTags.Magic | SkillTags.Projectile | SkillTags.Fire | SkillTags.AreaOfEffect;
        
        // Add additional tags based on specialization
        if (Tree.Specialization is Inferno)
        {
	        tags |= SkillTags.AreaOfEffect;
        }
        else if (Tree.Specialization is ShadowflamePyre)
        {
	        tags |= SkillTags.Chaos;
        }
        else if (Tree.Specialization is FrostfireMeteor)
        {
	        tags |= SkillTags.Cold;
        }
        
        return tags;
    }
    
    private static FireballType GetFireballType(Fireball fireball)
    {
	    SkillSpecial special = fireball.Tree.Specialization;

	    return special switch
	    {
		    Inferno => FireballType.Inferno,
		    ShadowflamePyre => FireballType.Shadowflame,
		    FrostfireMeteor => FireballType.Frostfire,
		    _ => FireballType.Normal
	    };
    }

    public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 8 * 60;
		ManaCost = 20 + 10 * Level;
		Duration = 0;
		WeaponType = ItemType.None;
	}

	public override void UseSkill(Player player)
	{
		base.UseSkill(player);

		int damage = GetTotalDamage((Level - 1) * 20 + 30);
		var source = new EntitySource_UseSkill(player, this);
		float knockback = 2f;
		int type = ModContent.ProjectileType<FireballProj>();
		Vector2 velocity = player.DirectionTo(Main.MouseWorld).RotatedByRandom(0.05f) * 8 * Main.rand.NextFloat(0.9f, 1.1f);
		
		int proj = Projectile.NewProjectile(source, player.Center - new Vector2(0, 12), velocity, type, damage, knockback, player.whoAmI, Level, (float)GetFireballType(this));
		Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().Container[ElementType.Fire].DamageModifier.AddModifiers(0, 1);
		SoundEngine.PlaySound(SoundID.Item20 with { PitchRange = (-0.8f, 0.2f) }, player.Center);
	}

	private class FireballProj : SkillProjectile<Fireball>
	{
		public override string Texture => $"{PoTMod.ModName}/Assets/Skills/" + GetType().Name;

		private Player Owner => Main.player[Projectile.owner];

		private ref float Level => ref Projectile.ai[0];

		private FireballType FireballType => (FireballType)Projectile.ai[1];

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 5;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.timeLeft = 240;
			Projectile.penetrate = 1;
			Projectile.aiStyle = -1;
			Projectile.scale = 0f;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.frame = (int)(Projectile.frameCounter++ / 5f) % 5;
			Projectile.Opacity = (Projectile.velocity.Length() - 2) / 8f * 0.25f + 0.75f;
			Projectile.scale = Math.Min(1, Projectile.scale + 0.025f);
			Projectile.width = (int)(36 * Projectile.scale);
			Projectile.height = (int)(36 * Projectile.scale); 

			if (Projectile.scale == 1)
			{
				Projectile.velocity.Y += 0.01f;
			}

			SpawnDust(1, 0.4f);
		}

		private void SpawnDust(int count, float strength = 1f)
		{
			for (int i = 0; i < count; ++i)
			{
				float scale = Main.rand.NextFloat(0.8f, 1.4f);
				Vector2 vel = Projectile.velocity * strength;
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, vel.X, vel.Y, Scale: scale);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			float chance = FireballType == FireballType.Inferno ? 1 : 0.05f + Level * 0.1f;

			if (Main.rand.NextFloat() < chance)
			{
				if (Owner.HasTreePassive<FireballTree, Pyroclasm>())
				{
					damageDone = (int)(damageDone * 1.25f);
				}

				if (Owner.HasTreePassive<FireballTree, FireballNova>() && target.HasBuff<IgnitedDebuff>())
				{
					int projType = ModContent.ProjectileType<Nova.NovaProjectile>();
					IEntitySource source = Projectile.GetSource_OnHit(target);
					int proj = Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero, projType, damageDone, 6, Projectile.owner, (int)Nova.NovaType.Fire);
					Main.projectile[proj].scale = 0.6f;
					ElementalContainer container = Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().Container;
					ref ElementalDamage damageModifier = ref container[ElementType.Fire].DamageModifier;
					damageModifier = damageModifier.AddModifiers(0, 1);
				}

				IgnitedDebuff.ApplyTo(target, damageDone, Owner.HasTreePassive<FireballTree, FireballNova>() ? 6 * 60 : 4 * 60);
				SpawnDust(12);
			}

			if (Owner.HasTreePassive<FireballTree, SmolderingFury>() && target.life <= 0)
			{
			}
		}

		public override void OnKill(int timeLeft)
		{
			if (!Main.dedServ)
			{
				SpawnDust(20);
				SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Variants = [1, 2], PitchRange = (0.2f, 0.6f) }, Projectile.Center);

				for (int i = 0; i < 30; ++i)
				{
					float scale = Main.rand.NextFloat(1.2f, 2f);
					Vector2 vel = Projectile.velocity + Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.3f, 1f);
					Vector2 pos = Projectile.position + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height));
					Dust.NewDustPerfect(pos, DustID.Torch, vel, Scale: scale);
				}

				for (int i = 0; i < 8; ++i)
				{
					Vector2 vel = Projectile.velocity + Main.rand.NextVector2CircularEdge(4, 4) * Main.rand.NextFloat(0.3f, 1f);
					Gore.NewGorePerfect(Projectile.GetSource_Death(), Projectile.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
				}
			}

			int area = Skill.GetTotalAreaOfEffect(160);
			Projectile.Resize(area, area);
			Projectile.Damage();

			if (Owner.HasTreePassive<FireballTree, ScorchedEarth>())
			{
				int type = ModContent.ProjectileType<StickyFlame>();
				int damage = Projectile.damage / (Owner.HasTreePassive<FireballTree, StrongerScorchedEarth>() ? 2 : 4);

				for (int i = 0; i < 8; ++i)
				{
					Vector2 vel = Main.rand.NextVector2Circular(4, 4) + Projectile.velocity * 0.25f;
					float timeExtension = Owner.HasTreePassive<FireballTree, LongerScorchedEarth>() ? StickyFlame.MaxTimeLeft * 0.25f : 0;
					int proj = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel, type, damage, 0, Main.myPlayer, 0, timeExtension);
					Main.projectile[proj].frameCounter = Main.rand.Next(800);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			int frameHeight = tex.Height / Main.projFrames[Type];
			var src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			Color col = Color.White * Projectile.Opacity;
			Vector2 position = Projectile.Center - Main.screenPosition;
			Vector2 origin = src.Size() / new Vector2(1.5f, 2);
			
			Main.EntitySpriteDraw(tex, position, src, col * Projectile.scale, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
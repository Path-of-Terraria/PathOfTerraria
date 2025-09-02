using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.NPCs;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Summon;

public class FlameSage : Skill
{
	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (15 - Level * 2) * 60;
		ManaCost = 20 - Level * 5;
		Duration = 0;
		WeaponType = ItemType.None;
	}

	public override void UseSkill(Player player)
	{
		base.UseSkill(player);

		player.FindSentryRestingSpot(0, out int x, out int y, out _);
		SentryNPC.Spawn<FlameSentry>(player, new(x, y));
	}

	private class FlameSentry : SentryNPC
	{
		public const int CooldownMax = 30;

		public ref float Cooldown => ref NPC.ai[0];
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2LightningAuraT1;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.Size = new(20);
			NPC.noGravity = true;
		}

		public override void AI()
		{
			if ((Cooldown = Math.Max(Cooldown - 1, 0)) == 0)
			{
				Terraria.Utilities.NPCUtils.TargetSearchResults results = FindTarget();
				NPC target = results.NearestNPC;

				if (results.FoundNPC && Collision.CanHit(NPC, target))
				{
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(target.Center) * 10, ProjectileID.DD2FlameBurstTowerT1Shot, NPC.damage, 3, Owner.whoAmI);
					Cooldown = CooldownMax;
				}
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter = (NPC.frameCounter + 0.2f) % Main.npcFrameCount[Type];
			NPC.frame.Y = frameHeight * (int)NPC.frameCounter;
		}
	}
}
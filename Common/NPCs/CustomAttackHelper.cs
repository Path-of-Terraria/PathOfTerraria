using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

internal class CustomAttackHelper
{
	public static void CheckShootAttack(NPC npc, ref int attackTime, ref int attackingTime, ref int attackDir, ref float attackRotation)
	{
		NPC first = null;
		int checkDist = NPCID.Sets.DangerDetectRange[npc.type] * NPCID.Sets.DangerDetectRange[npc.type];

		foreach (NPC otherNPC in Main.ActiveNPCs)
		{
			if (otherNPC.CanBeChasedBy() && otherNPC.whoAmI != npc.whoAmI && otherNPC.DistanceSQ(npc.Center) < checkDist &&
				(first is null || first.DistanceSQ(npc.Center) > otherNPC.DistanceSQ(npc.Center)) && Collision.CanHit(npc, otherNPC))
			{
				first = otherNPC;
				break;
			}
		}

		if (--attackTime < 0 && first is not null)
		{
			int atkCool = 10;
			int randCool = 0;
			NPCLoader.TownNPCAttackCooldown(npc, ref atkCool, ref randCool);

			float velMul = 1f;
			float throwaway = 0;
			float offset = 0;
			NPCLoader.TownNPCAttackProjSpeed(npc, ref velMul, ref throwaway, ref offset);

			int projId = 0;
			int thrwInt = 0;
			NPCLoader.TownNPCAttackProj(npc, ref projId, ref thrwInt);

			int damage = 0;
			float knockback = 0;
			NPCLoader.TownNPCAttackStrength(npc, ref damage, ref knockback);

			Vector2 velocity = npc.DirectionTo(first.Center).RotatedByRandom(offset * 0.2f) * velMul;

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity, projId, damage, knockback, Main.myPlayer);
				Main.projectile[proj].friendly = true;
				Main.projectile[proj].hostile = false;
				Main.projectile[proj].npcProj = true;
				Main.projectile[proj].netUpdate = true;
			}

			attackingTime = NPCID.Sets.AttackTime[npc.type];
			attackRotation = velocity.ToRotation();
			attackTime = atkCool * 60 + Main.rand.Next(randCool * 60);
			attackTime /= 8;
			attackDir = Math.Sign(velocity.X);

			SoundEngine.PlaySound(SoundID.Item5, npc.Center);
		}

		if (--attackingTime > 0)
		{
			npc.direction = npc.spriteDirection = attackDir;
		}
	}

	public static void DrawShootAttack(NPC npc, Vector2 screenPos, Color drawColor, float attackRotation, int itemType)
	{
		Texture2D item = null;
		Rectangle frame = default;
		float scale = 1f;
		int horizontalOff = 0;

		Vector2 vector11 = Main.DrawPlayerItemPos(1f, itemType);
		Main.GetItemDrawFrame(itemType, out Texture2D itemTexture, out Rectangle value2);
		int xOffset = (int)vector11.X - 4;
		var drawOrigin = new Vector2(-xOffset, value2.Height / 2);

		if (npc.spriteDirection == -1)
		{
			drawOrigin = new Vector2(value2.Width + xOffset, value2.Height / 2);
		}

		NPCLoader.DrawTownAttackGun(npc, ref item, ref frame, ref scale, ref horizontalOff);
		SpriteEffects effects = npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		float rot = attackRotation;

		if (npc.spriteDirection == -1)
		{
			rot -= MathHelper.Pi;
		}

		Main.EntitySpriteDraw(item, npc.Center - screenPos - new Vector2(horizontalOff, npc.gfxOffY), frame, drawColor, rot, drawOrigin, scale, effects);
	}
}

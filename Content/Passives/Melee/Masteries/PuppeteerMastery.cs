using PathOfTerraria.Common;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Utilities;
using ReLogic.Content;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

#nullable enable

internal class PuppeteerMastery : Passive
{
	internal class PuppeteerPlayer : ModPlayer
	{
		[ThreadStatic]
		private static bool StopRecursion;

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<PuppeteerMastery>(out float value) && proj.aiStyle == ProjAIStyleID.Yoyo)
			{
				if (!StopRecursion)
				{
					using var _ = ValueOverride.Create(ref StopRecursion, true);

					foreach (NPC npc in Main.ActiveNPCs)
					{
						if (npc.TryGetGlobalNPC(out PuppeteerNPC pup) && pup.YoyoIdentity == proj.identity && npc.whoAmI != target.whoAmI)
						{
							npc.StrikeNPC(hit with { Damage = (int)(hit.Damage * value / 100f) });
						}
					}
				}

				target.GetGlobalNPC<PuppeteerNPC>().YoyoIdentity = proj.identity;
			}
		}
	}

	internal class PuppeteerNPC : GlobalNPC
	{
		private static readonly Asset<Texture2D> String = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Projectiles/PassiveProjectiles/PuppeteerString");

		public override bool InstancePerEntity => true;

		private Projectile? Yoyo => Main.projectile.FirstOrDefault(x => x.identity == YoyoIdentity);

		internal int YoyoIdentity = -1;

		public override void PostAI(NPC npc)
		{
			if (YoyoIdentity == -1 || Yoyo is not Projectile { active: true } yoyo)
			{
				YoyoIdentity = -1;
				return;
			}

			Vector2 offset = (yoyo.Center - npc.Center) * 0.03f * npc.knockBackResist;

			if (npc.collideX)
			{
				offset.X = 0;
			}

			if (npc.collideY)
			{
				offset.Y = 0;
			}

			npc.position += offset;
		}

		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (YoyoIdentity == -1 || Yoyo is not Projectile yoyo)
			{
				return true;
			}

			Color color = Lighting.GetColor(npc.Center.ToTileCoordinates(), TryApplyingPlayerStringColor(null, yoyo.GetOwner().stringColor, Color.White));
			float scale = MathF.Min(1, yoyo.Distance(npc.Center) / 120f);
			Main.spriteBatch.Draw(String.Value, npc.Center - screenPos, null, color, npc.AngleTo(Yoyo.Center), Vector2.Zero, new Vector2(scale, 1f), SpriteEffects.None, 0);

			return true;
		}

		[UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "TryApplyingPlayerStringColor")]
		private static extern Color TryApplyingPlayerStringColor(Main? main, int playerStringColor, Color stringColor);
	}
}

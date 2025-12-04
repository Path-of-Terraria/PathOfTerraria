using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Utilities;
using ReLogic.Content;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Passives;

internal class MarksmanMastery : Passive
{
	internal class MarkedNPC : GlobalNPC
	{
		private static readonly Asset<Texture2D> Icon = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/MarkedIcon");

		private static bool _markedDamage = false;

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (_markedDamage)
			{
				return;
			}

			if (projectile.TryGetOwner(out Player plr) && projectile.friendly && plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<MarksmanMastery>(out float value))
			{
				damageDone = (int)(damageDone * value / 100f);

				foreach (NPC other in Main.ActiveNPCs)
				{
					if (other.whoAmI != npc.whoAmI && other.CanBeChasedBy() && other.DistanceSQ(npc.Center) < PoTMod.NearbyDistanceSq)
					{
						using var _ = ValueOverride.Create(ref _markedDamage, true);

						other.StrikeNPC(new NPC.HitInfo()
						{
							Damage = damageDone,
							HitDirection = 0,
						});
					}
				}
			}
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (Main.LocalPlayer.GetModPlayer<MarkedPlayer>().TargetNpc == npc.whoAmI)
			{
				float opacity = MathF.Sin(Main.GameUpdateCount * 0.12f) * 0.25f + 0.5f;
				spriteBatch.Draw(Icon.Value, npc.Center - screenPos, null, Color.White * opacity, 0f, Icon.Size() / 2f, 1f, SpriteEffects.None, 0);
			}
		}
	}

	internal class MarkedPlayer : ModPlayer 
	{
		internal int TargetNpc = -1;

		public override void PostUpdateMiscEffects()
		{
			if (!Player.GetModPlayer<PassiveTreePlayer>().HasNode<MarksmanMastery>())
			{
				TargetNpc = -1;
				return;
			}

			if (TargetNpc == -1)
			{
				PriorityQueue<int, float> npcs = new();

				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.DistanceSQ(Player.Center) < 600 * 600 && npc.CanBeChasedBy())
					{
						npcs.Enqueue(npc.whoAmI, Main.rand.NextFloat());
					}
				}

				if (npcs.Count > 0)
				{
					TargetNpc = npcs.Dequeue();
				}
			}
			else
			{
				NPC npc = Main.npc[TargetNpc];

				if (!npc.CanBeChasedBy() || npc.DistanceSQ(Player.Center) > 900 * 900)
				{
					TargetNpc = -1;
					return;
				}
			}
		}
	}
}
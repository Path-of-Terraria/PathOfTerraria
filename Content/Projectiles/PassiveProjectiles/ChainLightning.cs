using PathOfTerraria.Common.Systems.ElementalDamage;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class ChainLightning : ModProjectile
{
	private int OriginNPC => (int)Projectile.ai[0];
	
	private int Target
	{
		get => (int)Projectile.ai[1];
		set => Projectile.ai[1] = value;
	}

	private bool Init
	{
		get => Projectile.ai[2] == 1;
		set => Projectile.ai[2] = value ? 1 : 0;
	}

	private ref float Timer => ref Projectile.localAI[0];

	private List<Vector2> lightningPoints = [];

	public override string Texture => "Terraria/Images/NPC_0";

	public override void SetDefaults()
	{
		Projectile.Size = Vector2.Zero;
		Projectile.timeLeft = 12;
		Projectile.aiStyle = -1;
		Projectile.DamageType = DamageClass.Magic;
	}

	public override void AI()
	{
		if (!Init)
		{
			ref ElementalDamage mod = ref Projectile.GetGlobalProjectile<ElementalProjectile>().Container[ElementType.Lightning].DamageModifier;
			mod = mod.AddModifiers(0, 1);

			Target = -1;
			Init = true;

			if (Main.myPlayer == Projectile.owner)
			{
				PriorityQueue<int, float> queue = new();

				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.CanBeChasedBy() && npc.DistanceSQ(Projectile.Center) < 400 * 400 && npc.whoAmI != OriginNPC)
					{
						queue.Enqueue(npc.whoAmI, Main.rand.Next());
					}
				}

				if (queue.Count > 0)
				{
					Target = queue.Dequeue();
				}

				Projectile.netUpdate = true;
			}

			if (Target == -1)
			{
				Projectile.Kill();
				return;
			}
		}

		if (Projectile.timeLeft == 2 && Target != -1)
		{
			NPC npc = Main.npc[Target];

			npc.StrikeNPC(new NPC.HitInfo()
			{
				Crit = false,
				DamageType = DamageClass.Magic,
				HitDirection = 0,
				Damage = Projectile.damage,
				Knockback = 0,
			});
		}

		Timer++;

		if (Timer % 3 == 0)
		{
			lightningPoints = BuildLightning(Projectile.Center, Main.npc[Target].Center);

			foreach (Vector2 pos in lightningPoints)
			{
				if (Main.rand.NextBool(60))
				{
					Dust.NewDustPerfect(pos, DustID.SparksMech, Vector2.Zero);
				}
			}
		}
	}

	private static List<Vector2> BuildLightning(Vector2 origin, Vector2 target)
	{
		int dist = (int)(target.Distance(origin) / 32f);
		List<Vector2> edges = [];
		Vector2 dir = origin.DirectionTo(target).RotatedBy(MathHelper.PiOver2);

		for (int i = 0; i < dist; ++i)
		{
			float factor = i / (dist - 1f);

			if (i == 0 || i == dist - 1)
			{
				edges.Add(Vector2.Lerp(origin, target, factor));
				continue;
			}

			Vector2 pos = Vector2.Lerp(origin, target, factor) + dir * Main.rand.NextFloat(-30, 30);
			edges.Add(pos);
		}

		return edges;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.MagicPixel.Value;
		var src = new Rectangle(0, 0, 1, 1);

		for (int i = 0; i < lightningPoints.Count - 1; ++i)
		{
			float width = 4f;

			if (i == 0 || i == lightningPoints.Count - 1)
			{
				width = 1f;
			}
			else if (i == 1 || i == lightningPoints.Count - 2)
			{
				width = 2f;
			}

			Vector2 cur = lightningPoints[i];
			Vector2 next = lightningPoints[i + 1];

			var scale = new Vector2(cur.Distance(next) + 4, width);
			Main.spriteBatch.Draw(tex, cur - Main.screenPosition, src, Color.Yellow, cur.AngleTo(next), Vector2.Zero, scale, SpriteEffects.None, 0);
		}

		return false;
	}
}

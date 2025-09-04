using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class IgnitedDebuff : ModBuff
{
	public static void ApplyTo(NPC npc, int hitDamage, int time = 4 * 60)
	{
		// TODO: Add time duration modifier(s)
		IgnitedNPC ignited = npc.GetGlobalNPC<IgnitedNPC>();
		ignited.Stacks.Add(new IgnitedNPC.IgnitedStack(time + 1, hitDamage));
		ignited.Stacks = [.. ignited.Stacks.OrderByDescending(x => x.BaseDamage)];

		if (ignited.Stacks[0].BaseDamage == hitDamage)
		{
			npc.AddBuff(ModContent.BuffType<IgnitedDebuff>(), time);
		}
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (!npc.TryGetGlobalNPC(out IgnitedNPC ignited))
		{
			return;
		}

		if (ignited.Stacks.Count == 0)
		{
			npc.DelBuff(buffIndex);
			buffIndex--;
		}
		else
		{
			npc.buffTime[buffIndex] = Math.Max(npc.buffTime[buffIndex], 2);

			if (Main.rand.NextBool(Math.Max(15 - (int)MathF.Sqrt(ignited.Stacks[0].BaseDamage / 30) + 1, 1)))
			{
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, Scale: Main.rand.NextFloat(1.5f, 2.5f));
			}
		}
	}
}

internal class IgnitedNPC : GlobalNPC
{
	public class IgnitedStack(int time, int baseDamage)
	{
		public int Time = time;
		public int BaseDamage = baseDamage;
	}

	public override bool InstancePerEntity => true;

	public List<IgnitedStack> Stacks = [];
	public float ElapsedDoT = 0;

	public override bool PreAI(NPC npc)
	{
		if (npc.HasBuff<IgnitedDebuff>() && Stacks.Count > 0)
		{
			int baseDamage = Stacks[0].BaseDamage;
			int halfDamage = baseDamage / 2;
			ElapsedDoT += baseDamage / 60f;

			if (ElapsedDoT > halfDamage)
			{
				if (!npc.dontTakeDamage && !npc.immortal)
				{
					if (npc.realLife == -1)
					{
						npc.life -= halfDamage;
					}
					else
					{
						Main.npc[npc.realLife].life -= halfDamage;
					}
				}

				ElapsedDoT -= halfDamage;

				// Vanilla is dumb, this is the easiest way to properly kill an NPC while showing gore & doing death effects,
				// that is, WITHOUT calling StrikeNPC for every hit, which causes a hit sound and forces a specific combat text
				if (npc.life <= 0)
				{
					npc.life = 1;
					NPC.HitInfo info = default;
					info.HideCombatText = true;
					info.Damage = 1;
					npc.StrikeNPC(info);
				}

				//AdvancedPopupRequest request = default;
				//request.Text = halfDamage.ToString();
				//request.DurationInFrames = 30;
				//request.Color = Color.Lerp(Color.OrangeRed, Color.Orange, Main.rand.NextFloat());
				//request.Velocity = new Vector2(0, -12);
				//PopupText.NewText(request, npc.Top);

				CombatText.NewText(npc.Hitbox, Color.Lerp(Color.OrangeRed, Color.Orange, Main.rand.NextFloat()), halfDamage, false, true);
			}

			Stacks[0].Time--;

			if (Stacks[0].Time <= 0)
			{
				npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<IgnitedDebuff>()));
				Stacks.Clear();
			}
		}

		return true;
	}
}
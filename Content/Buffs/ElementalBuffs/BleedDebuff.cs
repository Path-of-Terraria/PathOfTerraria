using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Common.Systems.MobSystem;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class BleedDebuff : ModBuff
{
	public static void Apply(Player player, NPC npc, int time, int damage)
	{
		List<BleedStack> stats = npc.GetGlobalNPC<BleedDebuffNPC>().Stacks;
		BleedPlayer bleedPlayer = player.GetModPlayer<BleedPlayer>();
		
		if (bleedPlayer.MaxBleedStacks <= stats.Count)
		{
			return;
		}

		int realDamage = (int)(damage * bleedPlayer.BleedEffectiveness);

		if (realDamage <= 0)
		{
			return;
		}

		npc.GetGlobalNPC<BleedDebuffNPC>().Stacks.Add(new(realDamage));
		npc.AddBuff(ModContent.BuffType<BleedDebuff>(), time);
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.GetGlobalNPC<PoisonNPC>().Stacks.Count == 0)
		{
			npc.DelBuff(buffIndex);
			buffIndex--;
		}
	}
}

internal class BleedStack(int damage)
{
	public int TimeLeft = 5 * 60;
	public int Damage = damage;
}

internal class BleedDebuffNPC : GlobalNPC
{
	public static Asset<Texture2D> Icon = null;

	public override bool InstancePerEntity => true;

	internal readonly List<BleedStack> Stacks = [];
	internal float ElapsedDoT = 0;

	public override void Load()
	{
		Icon = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/BleedIcon");
	}

	public override bool PreAI(NPC npc)
	{
		for (int i = 0; i < Stacks.Count; i++)
		{
			Stacks[i].TimeLeft--;

			float damage = npc.velocity.LengthSquared() > 0.1f ? Stacks[i].Damage * 3 : Stacks[i].Damage;
			ElapsedDoT += damage / (5 * 60f);
		}

		Stacks.RemoveAll(x => x.TimeLeft <= 0);

		if (ElapsedDoT > 30 || ElapsedDoT > npc.life)
		{
			DoTFunctionality.ApplyDoT(npc, Math.Min(30, npc.life), ref ElapsedDoT, Color.Pink, Color.Red);
		}
		else if (Stacks.Count == 0)
		{
			if (ElapsedDoT > 1)
			{
				DoTFunctionality.ApplyDoT(npc, (int)ElapsedDoT, ref ElapsedDoT, Color.Pink, Color.Red);
			}

			ElapsedDoT = 0;
		}

		return true;
	}

	public override Color? GetAlpha(NPC npc, Color drawColor)
	{
		return Stacks.Count > 0 ? Color.Lerp(drawColor, Lighting.GetColor(npc.Center.ToTileCoordinates(), Color.IndianRed), 0.75f + MathF.Sin(npc.whoAmI) * 0.25f) : null;
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (npc.GetGlobalNPC<FreezeNPC>().Frozen || Stacks.Count <= 0)
		{
			return;
		}

		Vector2 position = npc.Top - screenPos - new Vector2(0, 20 - npc.gfxOffY);

		if (npc.GetGlobalNPC<ArpgNPC>().Affixes.Count > 0)
		{
			position.Y -= 20;
		}

		if (npc.HasBuff<PoisonedDebuff>())
		{
			position.Y -= 20;
		}

		spriteBatch.Draw(Icon.Value, position, null, drawColor, 0f, Icon.Size() / 2f, 1f, SpriteEffects.None, 0);
		string stacks = "x" + Stacks.Count;
		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, stacks, position + new Vector2(8, -2), drawColor, 0f, Vector2.Zero, new(0.8f));
	}
}

public class BleedPlayer : ModPlayer
{
	public const int DefaultMaxBleedStacks = 5;
	public const float DefaultBleedEffectiveness = 0.2f;

	public int MaxBleedStacks = DefaultMaxBleedStacks;
	public float BleedEffectiveness = DefaultBleedEffectiveness;

	public override void ResetEffects()
	{
		MaxBleedStacks = DefaultMaxBleedStacks;
		BleedEffectiveness = DefaultBleedEffectiveness;
	}
}
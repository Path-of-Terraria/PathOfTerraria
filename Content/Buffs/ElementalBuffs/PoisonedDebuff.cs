using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Common.Systems.MobSystem;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI.Chat;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class PoisonedDebuff : ModBuff
{
	public override void Load()
	{
		On_NPC.AddBuff += ModifyPoisonAddition;
	}

	private void ModifyPoisonAddition(On_NPC.orig_AddBuff orig, NPC self, int type, int time, bool quiet)
	{
		if (type == BuffID.Poisoned)
		{
			Apply(self, time);
			return;
		}

		orig(self, type, time, quiet);
	}

	public static void Apply(NPC npc, int time)
	{
		npc.GetGlobalNPC<PoisonNPC>().Stacks.Add(time);
		npc.AddBuff(ModContent.BuffType<PoisonedDebuff>(), time);
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

internal class PoisonNPC : GlobalNPC
{
	public static Asset<Texture2D> PoisonIcon = null;

	public override bool InstancePerEntity => true;

	internal readonly List<int> Stacks = [];
	internal float ElapsedDoT = 0;

	public override void Load()
	{
		PoisonIcon = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/PoisonIcon");
	}

	public override bool PreAI(NPC npc)
	{
		for (int i = 0; i < Stacks.Count; i++)
		{
			Stacks[i]--;
			ElapsedDoT += 4 / 60f;
		}

		Stacks.RemoveAll(x => x <= 0);

		if (ElapsedDoT > 30 || ElapsedDoT > npc.life)
		{
			DoTFunctionality.ApplyDoT(npc, Math.Min(30, npc.life), ref ElapsedDoT, Color.Brown, Color.Red);
		}
		else if (Stacks.Count == 0)
		{
			if (ElapsedDoT > 1)
			{
				DoTFunctionality.ApplyDoT(npc, (int)ElapsedDoT, ref ElapsedDoT, Color.Brown, Color.Red);
			}

			ElapsedDoT = 0;
		}

		return true;
	}

	public override Color? GetAlpha(NPC npc, Color drawColor)
	{
		return Stacks.Count > 0 ? Color.Lerp(drawColor, Lighting.GetColor(npc.Center.ToTileCoordinates(), Color.DarkSeaGreen), 0.75f + MathF.Sin(npc.whoAmI) * 0.25f) : null;
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

		spriteBatch.Draw(PoisonIcon.Value, position, null, drawColor, 0f, PoisonIcon.Size() / 2f, 1f, SpriteEffects.None, 0);
		string stacks = "x" + Stacks.Count;
		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, stacks, position + new Vector2(8, -2), drawColor, 0f, Vector2.Zero, new(0.8f));
	}
}
using Terraria.ID;
using PathOfTerraria.Common.Systems;
using ReLogic.Content;

namespace PathOfTerraria.Content.Buffs;

public sealed class RootedDebuff : ModBuff
{
	public override void Load()
	{
		On_NPC.AddBuff += StopRootedIfImmune;
	}

	private void StopRootedIfImmune(On_NPC.orig_AddBuff orig, NPC self, int type, int time, bool quiet)
	{
		if (type == ModContent.BuffType<RootedDebuff>() && self.GetGlobalNPC<RootedNPC>().RootedImmuneTime > 0)
		{
			return;
		}

		orig(self, type, time, quiet);
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;

		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.GetGlobalNPC<SlowDownNPC>().SpeedModifier = 1;
		npc.GetGlobalNPC<RootedNPC>().RootedImmuneTime = 3 * 60;
	}

	private sealed class RootedNPC : GlobalNPC
	{
		private static Asset<Texture2D> RootedTex = null;

		public override bool InstancePerEntity => true;

		public int RootedImmuneTime = 0;

		private float _alpha = 0;

		public override void Load()
		{
			RootedTex = Mod.Assets.Request<Texture2D>("Assets/Buffs/RootedDebuff_Roots");
		}

		public override bool PreAI(NPC npc)
		{
			if (npc.boss || npc.knockBackResist == 0)
			{
				npc.buffImmune[ModContent.BuffType<RootedDebuff>()] = true;
			}

			RootedImmuneTime--;

			if (RootedImmuneTime == -1)
			{
				npc.buffImmune[ModContent.BuffType<RootedDebuff>()] = false;
			}

			return true;
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			_alpha = MathHelper.Lerp(_alpha, npc.HasBuff<RootedDebuff>() ? 1 : 0, 0.1f);

			if (_alpha <= 0.01f)
			{
				return;
			}

			Vector2 pos = npc.Center - screenPos + new Vector2(0, npc.gfxOffY);
			spriteBatch.Draw(RootedTex.Value, pos, null, drawColor * _alpha, npc.rotation, RootedTex.Size() / 2f, GetPulse(), SpriteEffects.None, 0);
		}

		private static Vector2 GetPulse()
		{
			float x = 1f + MathF.Sin(Main.GameUpdateCount * 0.04f) * 0.2f;
			float y = 1f + MathF.Sin(Main.GameUpdateCount * 0.04f + MathHelper.PiOver2) * 0.2f;

			return new(x, y);
		}

		public override void OnKill(NPC npc)
		{
			if (!npc.HasBuff<RootedDebuff>())
			{
				return;
			}

			for (int i = 0; i < 12; ++i)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.WoodFurniture);
			}
		}
	}
}
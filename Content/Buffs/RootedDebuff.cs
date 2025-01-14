using Terraria.ID;
using PathOfTerraria.Common.Systems;
using ReLogic.Content;

namespace PathOfTerraria.Content.Buffs;

public sealed class RootedDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;

		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
		BuffID.Sets.GrantImmunityWith[Type].Add(BuffID.Confused);
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.GetGlobalNPC<SlowDownNPC>().SlowDown = 1;
	}

	private sealed class RootedNPC : GlobalNPC
	{
		private static Asset<Texture2D> RootedTex = null;

		public override bool InstancePerEntity => true;

		private float _alpha = 0;

		public override void Load()
		{
			RootedTex = Mod.Assets.Request<Texture2D>("Assets/Buffs/RootedDebuff_Roots");
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			_alpha = MathHelper.Lerp(_alpha, npc.HasBuff<RootedDebuff>() ? 1 : 0, 0.1f);

			if (_alpha <= 0.01f)
			{
				return;
			}

			Vector2 pos = npc.Center - screenPos + new Vector2(0, npc.gfxOffY);
			spriteBatch.Draw(RootedTex.Value, pos, null, drawColor, npc.rotation, RootedTex.Size() / 2f, GetPulse(), SpriteEffects.None, 0);
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
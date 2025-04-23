using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Buffs;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Skills.Melee;

public class Berserk : Skill
{
	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (60 - 5 * Level) * 60;
		Timer = 0;
		ManaCost = 10 + 5 * level;
		Duration = (15 + 5 * Level) * 60;
		WeaponType = ItemType.Sword;
	}

	public override void UseSkill(Player player, SkillBuff buff)
	{
		player.CheckMana((int)buff.ManaCost.ApplyTo(ManaCost), true);
		player.AddBuff(ModContent.BuffType<RageBuff>(), Duration);
		Timer = Cooldown;
	}

	internal class BerserkAuraLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new BeforeParent(PlayerDrawLayers.HandOnAcc);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			if (drawInfo.headOnlyRender || drawInfo.shadow > 0 || drawInfo.drawPlayer.GetModPlayer<BerserkVisualsPlayer>().BerserkOpacity < 0.01f || Main.gameMenu)
			{
				return;
			}

			Texture2D tex = Mod.Assets.Request<Texture2D>("Assets/Misc/VFX/BerserkAura").Value;
			float opacity = drawInfo.drawPlayer.GetModPlayer<BerserkVisualsPlayer>().BerserkOpacity;
			int timer = drawInfo.drawPlayer.GetModPlayer<BerserkVisualsPlayer>().BerserkTimer;
			Color color = Color.White * opacity;
			float scale = 1f;
			Vector2 position = drawInfo.Center - Main.screenPosition + new Vector2(0, drawInfo.drawPlayer.height / 2f + 4);
			Rectangle frame = tex.Frame(1, 4, 0, (int)(timer / 4f % 4));
			drawInfo.DrawDataCache.Add(new DrawData(tex, position, frame, color, 0f, frame.Size() / new Vector2(2, 1), scale, SpriteEffects.None, 0));
		}
	}

	internal class BerserkVisualsPlayer : ModPlayer
	{
		internal float BerserkOpacity = 0;
		internal int BerserkTimer = 0;

		public override void ResetEffects()
		{
			BerserkOpacity = MathHelper.Lerp(BerserkOpacity, Player.HasBuff<RageBuff>() ? 1 : 0, 0.05f);
			BerserkTimer++;
		}
	}
}
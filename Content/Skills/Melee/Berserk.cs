using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Utilities;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Skills.Melee;

public class Berserk : Skill
{
	private static SoundStyle BerserkStartSound => new($"{PoTMod.ModName}/Assets/Sounds/Skills/BerserkStart")
	{
		Volume = 0.4f,
		PitchVariance = 0.2f,
	};
	private static SoundStyle BerserkHitSound => new($"{PoTMod.ModName}/Assets/Sounds/Skills/BerserkHit")
	{
		Volume = 0.90f,
		Pitch = -0.5f,
		PitchVariance = 0.25f,
		MaxInstances = 3,
		SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
	};
	private static SoundStyle BerserkEndSound => new($"{PoTMod.ModName}/Assets/Sounds/Skills/BerserkEnd")
	{
		Volume = 0.35f,
		PitchVariance = 0.2f,
	};
	private static SoundStyle BerserkLoopSound => new($"{PoTMod.ModName}/Assets/Sounds/Skills/BerserkLoop")
	{
		Volume = 0.4f,
		IsLooped = true,
	};

	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (35 - 5 * Level) * 60;
		ManaCost = 10 + 5 * level;
		Duration = 10 * Level * 60 / 2;
		WeaponType = ItemType.Melee;
	}

	public override void UseSkill(Player player)
	{
		base.UseSkill(player);
		player.AddBuff(ModContent.BuffType<RageBuff>(), Duration);
	}

	public override bool CanUseSkill(Player player, ref SkillFailure failReason, bool justChecking)
	{
		if (!player.HeldItem.CountsAsClass(DamageClass.Melee))
		{
			failReason = new SkillFailure(SkillFailReason.NeedsMelee);
			return false;
		}

		return base.CanUseSkill(player, ref failReason, justChecking);
	}

	internal class BerserkAuraLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new BeforeParent(PlayerDrawLayers.HandOnAcc);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			if (drawInfo.headOnlyRender || drawInfo.drawPlayer.dead || drawInfo.shadow > 0 || Main.gameMenu
			|| drawInfo.drawPlayer.GetModPlayer<BerserkVisualsPlayer>().BerserkOpacity < 0.01f)
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

	[Autoload(Side = ModSide.Client)]
	internal class BerserkVisualsPlayer : ModPlayer
	{
		public float BerserkOpacity = 0f;
		public float BerserkVolume = 0f;
		public int BerserkTimer = 0;

		private bool hadBuff;
		private SlotId soundSlot;

		public override void PostUpdate()
		{
			bool hasBuff = !Player.dead && Player.HasBuff<RageBuff>();
			float maxVolume = Player.whoAmI == Main.myPlayer ? 1f : 0.25f; // Quiter for remote players.

			BerserkOpacity = MathHelper.Lerp(BerserkOpacity, hasBuff ? 1f : 0f, 0.05f);
			BerserkVolume = MathHelper.Lerp(BerserkVolume, hasBuff ? maxVolume : 0f, 0.10f);
			BerserkTimer++;

			if (hasBuff != hadBuff)
			{
				SoundEngine.PlaySound((hasBuff ? BerserkStartSound : BerserkEndSound), Player.Center);
				hadBuff = hasBuff;
			}

			SoundUtils.UpdateLoopingSound(ref soundSlot, BerserkLoopSound, BerserkVolume, Player.Center);
		}

		public override void PlayerDisconnect()
		{
			SoundUtils.StopAndInvalidateSoundSlot(ref soundSlot);
		}

		public override void OnHitAnything(float x, float y, Entity victim)
		{
			if (!Player.dead && Player.HasBuff<RageBuff>())
			{
				SoundEngine.PlaySound(BerserkHitSound, victim.Center);
			}
		}
	}
}
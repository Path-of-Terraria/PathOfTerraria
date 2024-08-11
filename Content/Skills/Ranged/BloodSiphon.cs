using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Buffs;
using ReLogic.Content;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Ranged;

public class BloodSiphon : Skill
{
	public override int MaxLevel => 3;
	public override List<SkillPassive> Passives => [];

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = 15 * 60;
		Timer = 0;
		ManaCost = 20;
		Duration = 0;
		WeaponType = ItemType.Ranged;
	}

	public override void UseSkill(Player player)
	{
		// Level to the strength of all BloodSiphonAffix
		LevelTo((byte)player.GetModPlayer<AffixPlayer>().StrengthOf<BloodSiphonAffix>());

		int remainingStacksToPop = 20 + Level * 5;

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.DistanceSQ(player.Center) < MathF.Pow(16 * 30, 2))
			{
				SiphonNPC siphonNPC = npc.GetGlobalNPC<SiphonNPC>();
				int stacks = siphonNPC.StacksForPlayer(player.whoAmI);

				if (stacks > 0 && remainingStacksToPop > 0)
				{
					siphonNPC.PopStacks(npc, Level, ref remainingStacksToPop);
				}

				if (stacks <= 0 && npc.CanBeChasedBy())
				{
					npc.AddBuff(ModContent.BuffType<BloodclotDebuff>(), 3 * 60);
				}
			}
		}

		Timer = Cooldown;
	}

	public override bool CanEquipSkill(Player player)
	{
		// TODO: If this needs to be equippable without the affix, figure out that system
		return player.GetModPlayer<AffixPlayer>().StrengthOf<BloodSiphonAffix>() > 0;
	}
}

public class SiphonNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public int BuffDrainTime = 0;

	private static Asset<Texture2D> clotTex = null;

	private readonly Dictionary<int, int> _stacksPerPlayer = [];

	public override void SetStaticDefaults()
	{
		clotTex = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/Clot");
	}

	public override void PostAI(NPC npc)
	{
		if (!npc.HasBuff<BloodclotDebuff>())
		{
			BuffDrainTime = 0;
		}
	}

	public void ApplyStack(int fromPlayer)
	{
		const int MaxStacks = 10;

		if (_stacksPerPlayer.TryGetValue(fromPlayer, out int value))
		{
			_stacksPerPlayer[fromPlayer] = ++value;
		}
		else
		{
			_stacksPerPlayer.Add(fromPlayer, 1);
		}

		if (_stacksPerPlayer[fromPlayer] > MaxStacks)
		{
			_stacksPerPlayer[fromPlayer] = MaxStacks;
		}
	}

	public int StacksForPlayer(int player)
	{
		if (!_stacksPerPlayer.TryGetValue(player, out int value))
		{
			return 0;
		}

		return value;
	}

	public void PopStacks(NPC npc, int skillLevel, ref int remainingStacksToPop)
	{
		foreach (int player in _stacksPerPlayer.Keys)
		{
			Player plr = Main.player[player];
			int value = Math.Min(remainingStacksToPop, _stacksPerPlayer[player]);
			plr.Heal(skillLevel * value);

			remainingStacksToPop -= value;

			for (int i = 0; i < value; ++i)
			{
				float rotation = i / (float)value * MathHelper.TwoPi + Main.GameUpdateCount * 0.02f;
				Vector2 pos = npc.Center + new Vector2(16, 0).RotatedBy(rotation) - new Vector2(0, 50);
				float spriteRotation = npc.velocity.X * 0.06f;

				for (int j = 0; j < 2; ++j)
				{
					Vector2 velocity = Main.rand.NextVector2Circular(4, 4);
					Dust.NewDust(pos, 1, 1, DustID.Blood, velocity.X, velocity.Y);
				}
			}

			for (int i = 0; i < 8; ++i)
			{
				Dust.NewDust(Vector2.Lerp(plr.Center, npc.Center, i / 7f), 1, 1, DustID.Blood, 0, 0, Scale: Main.rand.NextFloat(1f, 2f));
			}

			_stacksPerPlayer[player] -= value;

			if (_stacksPerPlayer[player] <= 0)
			{
				_stacksPerPlayer.Remove(player);
			}

			if (remainingStacksToPop <= 0)
			{
				return;
			}
		}
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (!_stacksPerPlayer.TryGetValue(Main.myPlayer, out int value) || value <= 0)
		{
			return;
		}

		for (int i = 0; i < value; ++i)
		{
			float rotation = i / (float)value * MathHelper.TwoPi + Main.GameUpdateCount * 0.02f;
			Vector2 pos = npc.Center - screenPos + new Vector2(16, 0).RotatedBy(rotation) - new Vector2(0, 50);
			float spriteRotation = npc.velocity.X * 0.06f;
			spriteBatch.Draw(clotTex.Value, pos, null, Color.White * 0.4f, spriteRotation, clotTex.Value.Size() / 2f, 1, SpriteEffects.None, 0);
		}
	}
}
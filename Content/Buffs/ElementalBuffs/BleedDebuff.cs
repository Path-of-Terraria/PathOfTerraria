using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using static PathOfTerraria.Content.Buffs.ElementalBuffs.PoisonNPC;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class BleedDebuff : ModBuff
{
	public const int DefaultTime = 5 * 60;

	public static void Apply(Player player, NPC npc, int damage, int time = DefaultTime)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			BleedStackHandler.Send((short)npc.whoAmI, (ushort)time, (ushort)damage);
			return;
		}

		ReadOnlySpan<BleedStack> stats = npc.GetGlobalNPC<BleedDebuffNPC>().Stacks;
		BleedPlayer bleedPlayer = player.GetModPlayer<BleedPlayer>();
		
		if (bleedPlayer.MaxBleedStacks <= stats.Length)
		{
			return;
		}

		int realDamage = (int)(damage * bleedPlayer.BleedEffectiveness);
		time = (int)(time * bleedPlayer.BleedTime.Value);
		npc.GetGlobalNPC<BleedDebuffNPC>().LastTickCount = bleedPlayer.TickCountModifier.Value;

		if (realDamage <= 0)
		{
			return;
		}

		npc.GetGlobalNPC<BleedDebuffNPC>().AddStack(new(realDamage));
		npc.AddBuff(ModContent.BuffType<BleedDebuff>(), time);
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.GetGlobalNPC<BleedDebuffNPC>().Stacks.Length == 0)
		{
			npc.DelBuff(buffIndex);
			buffIndex--;
		}
	}
}

internal record struct BleedStack(int Damage, int TimeLeft = 5 * 60);

internal class BleedDebuffNPC : GlobalNPC
{
	public static Asset<Texture2D> Icon = null;

	public override bool InstancePerEntity => true;

	internal ReadOnlySpan<BleedStack> Stacks => CollectionsMarshal.AsSpan(_stacks);

	internal float ElapsedDoT = 0;
	internal float LastTickCount = 1f;

	private readonly List<BleedStack> _stacks = [];

	private int _timer = 0;

	public void AddStack(BleedStack stack)
	{
		_stacks.Add(stack);
	}

	/// <summary>
	/// Sets a stack at the given index to the given stack.
	/// </summary>
	public void SetStack(int index, BleedStack stack)
	{
		_stacks[index] = stack;
	}

	public override void Load()
	{
		Icon = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/BleedIcon");
	}

	public override bool PreAI(NPC npc)
	{
		for (int i = 0; i < _stacks.Count; i++)
		{
			BleedStack stack = _stacks[i];
			stack.TimeLeft--;
			_stacks[i] = stack;

			float damage = npc.velocity.LengthSquared() > 0.1f ? _stacks[i].Damage * 3 : _stacks[i].Damage;
			ElapsedDoT += damage / (float)BleedDebuff.DefaultTime;
		}

		_stacks.RemoveAll(x => x.TimeLeft <= 0);

		if (_stacks.Count > 0)
		{
			_timer++;
		}

		if (_timer > 60 * LastTickCount)
		{
			DoTFunctionality.ApplyDoT(npc, (int)ElapsedDoT, ref ElapsedDoT, Color.Pink, Color.Red);

			_timer = 0;
		}
		else if (_stacks.Count == 0)
		{
			if (ElapsedDoT > 1)
			{
				DoTFunctionality.ApplyDoT(npc, (int)ElapsedDoT, ref ElapsedDoT, Color.Pink, Color.Red);
			}

			ElapsedDoT = 0;
		}

		return true;
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		binaryWriter.Write((short)_stacks.Count);

		foreach (BleedStack stack in _stacks)
		{
			binaryWriter.Write((short)stack.TimeLeft);
			binaryWriter.Write(stack.Damage);
		}
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		_stacks.Clear();
		short count = binaryReader.ReadInt16();

		for (int i = 0; i < count; ++i)
		{
			short time = binaryReader.ReadInt16();
			_stacks.Add(new BleedStack(binaryReader.ReadInt32()) { TimeLeft = time });
		}
	}

	public override Color? GetAlpha(NPC npc, Color drawColor)
	{
		return _stacks.Count > 0 ? Color.Lerp(drawColor, Lighting.GetColor(npc.Center.ToTileCoordinates(), Color.IndianRed), 0.75f + MathF.Sin(npc.whoAmI) * 0.25f) : null;
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (npc.GetGlobalNPC<FreezeNPC>().Frozen || _stacks.Count <= 0)
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
		string stacks = "x" + _stacks.Count;
		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, stacks, position + new Vector2(8, -2), drawColor, 0f, Vector2.Zero, new(0.8f));
	}
}

public class BleedPlayer : ModPlayer
{
	public const int DefaultMaxBleedStacks = 5;
	public const float DefaultBleedEffectiveness = 0.2f;

	public int MaxBleedStacks = DefaultMaxBleedStacks;
	public float BleedEffectiveness = DefaultBleedEffectiveness;
	public AddableFloat BleedTime = new();
	public MultipliableFloat TickCountModifier = new();

	public override void ResetEffects()
	{
		MaxBleedStacks = DefaultMaxBleedStacks;
		BleedEffectiveness = DefaultBleedEffectiveness;
		BleedTime = new();
		TickCountModifier = new();
	}
}
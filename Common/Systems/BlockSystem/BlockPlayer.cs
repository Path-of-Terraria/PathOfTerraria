using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Passives;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.BlockSystem;

internal class BlockPlayer : ModPlayer
{
	public const float DefaultMaxBlockChance = 0.4f;
	public const float DefaultBlockCooldown = 2 * 60;

	public float BlockChance { get; private set; } = 0;
	public float MaxBlockChance { get; private set; } = DefaultMaxBlockChance;
	public float BlockCooldown = DefaultBlockCooldown;

	private int _timeSinceLastBlock = 0;
	private bool _hasDodged = false;

	public override void ResetEffects()
	{
		BlockChance = 0;
		MaxBlockChance = DefaultMaxBlockChance;
		BlockCooldown = DefaultBlockCooldown;

		_timeSinceLastBlock++;
	}

	public void AddBlockChance(float add)
	{
		BlockChance += add;

		if (BlockChance > MaxBlockChance)
		{
			BlockChance = MaxBlockChance;
		}
	}

	public override bool FreeDodge(Player.HurtInfo info)
	{
		bool dodged = Main.rand.NextFloat() < BlockChance && _timeSinceLastBlock > BlockCooldown;
		_hasDodged = dodged;

		if (dodged)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(GlancingBlowsPassive)) > 0)
			{
				_timeSinceLastBlock = 0;

				SoundEngine.PlaySound(SoundID.Tink, Player.Center);
				return false;
			}
			else
			{
				Player.AddImmuneTime(ImmunityCooldownID.General, 30);
				Player.immune = true;
				Player.immuneNoBlink = false;

				SoundEngine.PlaySound(SoundID.Tink, Player.Center);

				_timeSinceLastBlock = 0;
			}
		}

		return dodged;
	}

	public override void ModifyHurt(ref Player.HurtModifiers modifiers)
	{
		if (_hasDodged && Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(GlancingBlowsPassive)) > 0)
		{
			modifiers.FinalDamage *= 0.5f;
		}
	}
}

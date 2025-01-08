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

	public float ActualBlockChance => BlockChance * _accruedMultiplier;

	public float BlockChance { get; private set; } = 0;
	public float MaxBlockChance { get; private set; } = DefaultMaxBlockChance;
	public float BlockCooldown = DefaultBlockCooldown;

	private float _accruedMultiplier = 0;
	private int _timeSinceLastBlock = 0;
	private bool _hasDodged = false;

	public override void ResetEffects()
	{
		BlockChance = 0;
		MaxBlockChance = DefaultMaxBlockChance;
		BlockCooldown = DefaultBlockCooldown;

		_accruedMultiplier = 1;
		_timeSinceLastBlock++;
	}

	/// <summary>
	/// Adds to the block chance. This should be done by items that set base chance, such as Shields that have a base block chance.<br/>
	/// If you need to add a multiplier, use <see cref="MultiplyBlockChance(float)"/> instead.
	/// </summary>
	/// <param name="add">How much to add by.</param>
	public void AddBlockChance(float add)
	{
		BlockChance += add;

		if (BlockChance > MaxBlockChance)
		{
			BlockChance = MaxBlockChance;
		}
	}

	/// <summary>
	/// Multiplies the block chance. This should be done by things that modify an existing chance, such as affixes or passives.<br/>
	/// If you need to add a flat value, use <see cref="AddBlockChance(float)"/> instead.
	/// </summary>
	/// <param name="mult">How much to multiply by.</param>
	public void MultiplyBlockChance(float mult)
	{
		_accruedMultiplier *= mult;
	}

	public override bool FreeDodge(Player.HurtInfo info)
	{
		bool dodged = Main.rand.NextFloat() < ActualBlockChance && _timeSinceLastBlock > BlockCooldown;
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

using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// Defines the ability for NPCs to dodge attacks. This is very similar to <see cref="Systems.BlockSystem.BlockPlayer"/>.<br/>
/// TODO: Needs final implementation. At the moment, dodges don't work properly.
/// </summary>
internal class BlockNPC : GlobalNPC
{
	public const float DefaultMaxBlockChance = 0.4f;
	public const float DefaultBlockCooldown = 2 * 60;
	public const int DefaultBlockTime = 80;

	public override bool InstancePerEntity => true;

	public float ActualBlockChance => BlockChance * _accruedMultiplier;

	public float BlockChance { get; private set; } = 0;
	public float MaxBlockChance { get; private set; } = DefaultMaxBlockChance;
	public float BlockCooldown = DefaultBlockCooldown;

	private float _accruedMultiplier = 0;
	private int _timeSinceLastBlock = 0;
	private int _blockTime = 0;

	public override void ResetEffects(NPC npc)
	{
		BlockChance = 0;
		MaxBlockChance = DefaultMaxBlockChance;
		BlockCooldown = DefaultBlockCooldown;

		_accruedMultiplier = 1;
		_timeSinceLastBlock++;
		_blockTime--;
	}

	/// <summary>
	/// Adds max block chance.
	/// </summary>
	/// <param name="add">How much to add by.</param>
	public void AddMaxBlock(float add)
	{
		MaxBlockChance += add;
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

	public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
	{
		TryDodge(npc, player);
		return _blockTime <= 0 ? null : false;
	}

	public override bool CanBeHitByNPC(NPC npc, NPC attacker)
	{
		if (attacker.Hitbox.Intersects(npc.Hitbox))
		{
			TryDodge(npc, null);
		}

		return _blockTime <= 0;
	}

	public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
	{
		if (!projectile.TryGetOwner(out Player player))
		{
			player = null;
		}

		if (projectile.Hitbox.Intersects(npc.Hitbox))
		{
			TryDodge(npc, player);
		}

		return _blockTime <= 0 ? null : false;
	}

	private void TryDodge(NPC npc, Player playerIfAny)
	{
		bool dodged = Main.rand.NextFloat() < ActualBlockChance && _timeSinceLastBlock > BlockCooldown;

		if (dodged)
		{
			_blockTime = DefaultBlockTime;
			_timeSinceLastBlock = 0;

			if (playerIfAny is not null)
			{
				npc.immune[playerIfAny.whoAmI] = DefaultBlockTime;
			}

			SoundEngine.PlaySound(SoundID.Tink, npc.Center);
		}
	}

	public override Color? GetAlpha(NPC npc, Color drawColor)
	{
		return _blockTime > 0 && _blockTime % 8 <= 3 ? drawColor * 0.33f : base.GetAlpha(npc, drawColor);
	}
}

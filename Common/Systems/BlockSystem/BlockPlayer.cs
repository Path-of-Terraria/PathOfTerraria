namespace PathOfTerraria.Common.Systems.BlockSystem;

internal class BlockPlayer : ModPlayer
{
	public const float DefaultMaxBlockChance = 0.4f;

	public float BlockChance { get; private set; } = 0;
	public float MaxBlockChance { get; private set; } = DefaultMaxBlockChance;

	public override void ResetEffects()
	{
		BlockChance = 0;
		MaxBlockChance = DefaultMaxBlockChance;
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
		return Main.rand.NextFloat() < BlockChance;
	}
}

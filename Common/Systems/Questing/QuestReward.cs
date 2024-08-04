namespace PathOfTerraria.Common.Systems.Questing;

public abstract class QuestReward
{
	/// <summary>
	/// Gives the reward to the player.
	/// </summary>
	/// <param name="player">The player that cleared the quest.</param>
	/// <param name="dropPosition">The position to drop if its an item, will be at quest completion npc if there is one.</param>
	public virtual void GiveReward(Player player, Vector2 dropPosition) { }

	/// <summary>
	/// Draws the reward in the quest book, can be overwritten to inclue images.
	/// (or use images by default in the future)
	/// </summary>
	/// <param name="spriteBatch">The currently active SpriteBatch</param>
	/// <param name="topLeft">Where to draw / write from.</param>
	/// <returns>Size of element(s) drawn.</returns>
	public virtual Vector2 DisplayReward(SpriteBatch spriteBatch, Vector2 topLeft)
	{
		string rewardString = RewardString();

		// Draw using the spriteBatch

		return new(0, 0);
	}

	/// <summary>
	/// The string to be written in the quest book; might not get used DisplayReward is overwritten.
	/// </summary>
	public virtual string RewardString() { return "No rewards here."; }
}

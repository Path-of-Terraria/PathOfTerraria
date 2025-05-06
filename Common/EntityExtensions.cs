namespace PathOfTerraria.Common;

public static class EntityExtensions
{
	/// <summary>
	/// Safely normalizes a vector directed at <paramref name="target"/>.<br/>
	/// Default fallback value is <see cref="Vector2.Zero"/>.
	/// </summary>
	/// <param name="entity">Entity to reference.</param>
	/// <param name="target">Target to point to.</param>
	/// <param name="defaultValue">Default value if the normalization wasn't valid (had NaNs). Defaults to <see cref="Vector2.Zero"/>.</param>
	/// <returns>The unit vector pointing towards <paramref name="target"/>, or <paramref name="defaultValue"/>.</returns>
	public static Vector2 SafeDirectionTo(this Entity entity, Vector2 target, Vector2? defaultValue = null)
	{
		return (target - entity.Center).SafeNormalize(defaultValue ?? Vector2.Zero);
	}

	/// <summary>
	/// Safely normalizes a vector directed away from <paramref name="target"/>.<br/>
	/// Default fallback value is <see cref="Vector2.Zero"/>.
	/// </summary>
	/// <param name="entity">Entity to reference.</param>
	/// <param name="target">Target to point away from.</param>
	/// <param name="defaultValue">Default value if the normalization wasn't valid (had NaNs). Defaults to <see cref="Vector2.Zero"/>.</param>
	/// <returns>The unit vector pointing away from <paramref name="target"/>, or <paramref name="defaultValue"/>.</returns>
	public static Vector2 SafeDirectionFrom(this Entity entity, Vector2 target, Vector2? defaultValue = null)
	{
		return (entity.Center - target).SafeNormalize(defaultValue ?? Vector2.Zero);
	}
}

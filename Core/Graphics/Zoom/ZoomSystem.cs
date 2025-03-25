using System.Collections.Generic;
using Terraria.Graphics;

namespace PathOfTerraria.Core.Graphics.Zoom;

[Autoload(Side = ModSide.Client)]
public sealed class ZoomSystem : ModSystem
{
	private static readonly List<IZoomModifier> Modifiers = new();
	
	/// <summary>
	///		Adds a modifier to the list of modifiers.
	/// </summary>
	/// <param name="modifier">The instance of the modifier to add.</param>
	/// <typeparam name="T">The type of the modifier to add.</typeparam>
	public static void AddModifier<T>(T modifier) where T : IZoomModifier
	{
		var index = Modifiers.FindIndex(existing => existing.Identifier == modifier.Identifier);

		if (index != -1)
		{
			Modifiers.Remove(modifier);
		}

		Modifiers.Add(modifier);
	}
	
	public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
	{
		base.ModifyTransformMatrix(ref Transform);

		for (var i = 0; i < Modifiers.Count; i++)
		{
			var modifier = Modifiers[i];

			if (modifier.Finished)
			{
				Modifiers.RemoveAt(i--);
			}
			else
			{
				modifier.Update(ref Transform);
			}
		}
	}
}
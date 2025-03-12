using System.Collections.Generic;
using Terraria.Graphics;

namespace PathOfTerraria.Core.Graphics.Camera;

[Autoload(Side = ModSide.Client)]
public sealed class ZoomSystem : ModSystem
{
	private static readonly List<ZoomModifier> Modifiers = new();
	
	public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
	{
		base.ModifyTransformMatrix(ref Transform);

		for (var i = 0; i < Modifiers.Count; i++)
		{
			var modifier = Modifiers[i];

			modifier.TimeLeft--;
			
			if (modifier.TimeLeft <= 0)
			{
				Modifiers.RemoveAt(i--);
			}
			else
			{
				modifier.Callback(ref Transform, modifier.TimeLeft / (float)modifier.TimeMax);
				
				Modifiers[i] = modifier;
			}
		}
	}
	
	public static void AddModifier(string identifier, int duration, ZoomModifier.ModifierCallback callback)
	{
		var index = Modifiers.FindIndex(modifier => modifier.Identifier == identifier);

		if (index == -1)
		{
			Modifiers.Add(new ZoomModifier(identifier, duration, callback));
			return;
		}
		
		var modifier = Modifiers[index];

		modifier.TimeLeft = Math.Max(modifier.TimeLeft, duration);
		modifier.TimeMax = Math.Max(modifier.TimeMax, duration);
		
		modifier.Callback = callback;

		Modifiers[index] = modifier;
	}
}
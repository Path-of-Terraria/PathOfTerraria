using Terraria.Graphics;

namespace PathOfTerraria.Core.Graphics.Camera;

public struct ZoomModifier(string identifier, int timeLeft, ZoomModifier.ModifierCallback callback)
{
	public delegate void ModifierCallback(ref SpriteViewMatrix matrix, float progress);
	
	public readonly string Identifier { get; } = identifier;
	
	public int TimeMax { get; set; } = timeLeft;
	
	public int TimeLeft { get; set; } = timeLeft;
	
	public ModifierCallback Callback { get; set; } = callback;
}
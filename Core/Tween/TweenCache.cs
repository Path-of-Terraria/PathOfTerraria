using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Core.Tween;
public record struct TweenCache<T>(T Start, T End, bool Pingpong, TweenEaseType EaseType, int Duration) where T : struct;

public static class Tweens 
{
	public static TweenCache<float> NewCache(float Start, float End, bool Pingpong, TweenEaseType EaseType, int Duration) 
	{
		return new TweenCache<float> { Start = Start, End = End, Pingpong = Pingpong, EaseType = EaseType, Duration = Duration };
	}
	public static TweenCache<Vector2> NewCache(Vector2 Start, Vector2 End, bool Pingpong, TweenEaseType EaseType, int Duration) 
	{
		return new TweenCache<Vector2> { Start = Start, End = End, Pingpong = Pingpong, EaseType = EaseType, Duration = Duration };
	}
	public static TweenCache<Color> NewCache(Color Start, Color End, bool Pingpong, TweenEaseType EaseType, int Duration) 
	{
		return new TweenCache<Color> { Start = Start, End = End, Pingpong = Pingpong, EaseType = EaseType, Duration = Duration };
	}
}
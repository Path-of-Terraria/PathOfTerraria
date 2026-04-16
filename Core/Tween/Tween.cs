using PathOfTerraria.Common.Utilities;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Tween;

public enum TweenEaseType : byte
{

	None = 0,
	CubicInOut = 1,
	OutSine = 2,
	InExpo = 3,
	OutExpo = 4,
	OutBack = 5,
}

public enum TweenState : byte
{
	Paused, Running, Stopped, Finished
}

/// <summary>
/// A tweener almost exactly the same as godot's, call <see cref="TweenProperty(TweenCache{T}[])"/> inside anything like OnSpawn hooks and then assign its <see cref="CurrentProgress"/> to anything you want in an update hook
/// </summary>
/// <typeparam name="T"></typeparam>
public class Tween<T> : ITween where T : struct
{
	public List<TweenCache<T>> Cache = [];
	public int CurrentDuration = 0;
	public T CurrentProgress;
	private float currentProgressPercentage = 0;
	public Tween<T> OnFinishTween = null;
	public TweenState State;
	private float endDuration = 0;
	public Action<Tween<T>> OnFinsihed;
	public delegate T lerpFunction(T value1, T value2, float amount);
	public lerpFunction Lerp;
	public bool Active { get; set; }

	public Tween(lerpFunction lerpFunc)
	{
		ModContent.GetInstance<TweenRunner>().Tweens.Add(this);
		Lerp = lerpFunc;
		Active = true;
	}

	public static implicit operator T(Tween<T> tween)
	{
		return tween.CurrentProgress;
	}

	public Tween<T> TweenProperty(params TweenCache<T>[] cache)
	{
		this.Cache = [.. cache];
		CurrentDuration = 0;
		State = TweenState.Running;
		return this;
	}
	public void Pause() 
	{
		State = TweenState.Paused;
	}
	public void Resume() 
	{
		State = TweenState.Running;
	}
	public void Update()
	{
		if (State == TweenState.Paused || State == TweenState.Stopped) 
		{
			return;
		}

		TweenCache<T> tween = Cache[0];
		endDuration = tween.Duration;

		if (CurrentDuration < endDuration) 
		{
			CurrentDuration++;
		}

		switch (tween.EaseType)
		{
			case TweenEaseType.None:
				currentProgressPercentage = CurrentDuration / (float)endDuration;
				break;
			case TweenEaseType.CubicInOut:
				currentProgressPercentage = Easings.CubicInOut(CurrentDuration / (float)endDuration);
				break;
		}
		if (tween.Pingpong) 
		{
			Utils.PingPongFrom01To010(currentProgressPercentage);
		}
		CurrentProgress = Lerp(tween.Start, tween.End, currentProgressPercentage);
		if (CurrentDuration == endDuration)
		{
			this.Cache.RemoveAt(0);
			CurrentDuration = 0;
			if (this.Cache.Count == 0)
			{
				State = TweenState.Stopped;
				Active = false;
				OnFinsihed?.Invoke(this);
			}
		}
	}
}


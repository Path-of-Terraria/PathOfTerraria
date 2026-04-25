using PathOfTerraria.Common.Utilities;
using System.Collections.Generic;
using Terraria.Graphics;

#nullable enable

namespace PathOfTerraria.Core.Tween;

public enum TweenEaseType : byte
{
	None = 0,
	CubicInOut = 1,
}

public enum TweenState : byte
{
	Paused, Running, Stopped, Finished
}

/// <summary>
/// A tweener almost exactly the same as godot's, call <see cref="Start"/> inside anything like OnSpawn hooks and then assign its <see cref="CurrentProgress"/> to anything you want in an update hook
/// </summary>
/// <typeparam name="T"></typeparam>
public class Tween<T> : ITween where T : struct
{
	public static implicit operator T(Tween<T> tween)
	{
		return tween.CurrentProgress;
	}
	public List<TweenCache<T>> Cache = [];
	public int CurrentDuration = 0;
	public T CurrentProgress;
	public TweenState State;
	public Action<Tween<T>> OnFinished = (_) => { };
	public delegate T LerpFunction(T value1, T value2, float amount);
	public LerpFunction LerpFunc { get; private set; }
	private float _currentProgressPercentage = 0;
	private float _endDuration = 0;
	public bool Active { get; set; }

	public Tween(LerpFunction lerpFunc)
	{
		LerpFunc = lerpFunc;
	}
	public Tween<T> Start(params TweenCache<T>[] cache) 
	{
		ModContent.GetInstance<TweenRunner>().Tweens.Add(this);
		Active = true;
		TweenProperty(cache);
		return this;
	}
	private Tween<T> TweenProperty(params TweenCache<T>[] cache)
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
		_endDuration = tween.Duration;

		if (CurrentDuration < _endDuration) 
		{
			CurrentDuration++;
		}

		switch (tween.EaseType)
		{
			case TweenEaseType.None:
				_currentProgressPercentage = CurrentDuration / (float)_endDuration;
				break;
			case TweenEaseType.CubicInOut:
				_currentProgressPercentage = Easings.CubicInOut(CurrentDuration / (float)_endDuration);
				break;
		}
		if (tween.Pingpong) 
		{
			Utils.PingPongFrom01To010(_currentProgressPercentage);
		}
		CurrentProgress = LerpFunc(tween.Start, tween.End, _currentProgressPercentage);
		if (CurrentDuration == _endDuration)
		{
			this.Cache.RemoveAt(0);
			CurrentDuration = 0;
			if (this.Cache.Count == 0)
			{
				State = TweenState.Stopped;
				Active = false;
				OnFinished?.Invoke(this);
			}
		}
	}
}


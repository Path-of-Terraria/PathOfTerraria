using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.RealtimeGen;

/// <summary>
/// Defines a real time generation step.<br/>
/// If this returns true, the step did something and should reset the associated <see cref="RealtimeGenerationAction"/>'s placement timer.
/// </summary>
/// <param name="x"></param>
/// <param name="y"></param>
/// <returns></returns>
public delegate bool RealtimeAction(int x, int y);

internal class RealtimeGenerationAction(IEnumerable<RealtimeAction> actions, float placeSpeedInSeconds, Point16 placePos)
{
	public bool Done => Actions.Count == 0;

	public readonly Queue<RealtimeAction> Actions = new(actions);
	public readonly float PlaceSpeedInSeconds = placeSpeedInSeconds;
	public readonly Point16 PlacePosition = placePos;

	private int _timer = 0;

	public void Update()
	{
		_timer++;

		if (_timer > PlaceSpeedInSeconds * 60)
		{
			RealtimeAction action = Actions.Dequeue();

			while (!action.Invoke(PlacePosition.X, PlacePosition.Y))
			{
				action = Actions.Dequeue();
			}

			_timer = 0;
		}
	}
}

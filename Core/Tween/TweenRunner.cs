using PathOfTerraria.Common.Projectiles;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Tween;

public class TweenRunner : ModSystem
{
    public List<ITween> Tweens = [];
    public List<int> TweensForDeletion = [];
    public override void PreUpdateEntities()
    {
        for (int i = 0; i < Tweens.Count; i++) 
        { 
			if(!Tweens[i].Active)
			{
				TweensForDeletion.Add(i);
				continue;
			}
            Tweens[i].Update(); 
        }

		foreach(int forDeletion in TweensForDeletion) 
		{
			Tweens.RemoveAt(forDeletion);
		}

		TweensForDeletion.Clear();

    }
}

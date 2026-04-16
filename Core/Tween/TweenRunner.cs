using PathOfTerraria.Core.Tween;
using System.Collections.Generic;
using System.Linq;


namespace PathOfTerraria.Core.Tween;

public class TweenRunner : ModSystem
{
    public List<ITween> Tweens = [];
    public override void PreUpdateEntities()
    {
        for (int i = 0; i < Tweens.Count; i++) 
        { 
            Tweens[i].Update(); 
        }

        Tweens.RemoveAll(t => !t.Active);
            
    }
}

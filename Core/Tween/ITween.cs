using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Core.Tween;

public interface ITween
{
    public bool Active { get; set; }
    void Update();
}

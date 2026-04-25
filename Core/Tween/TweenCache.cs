using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Core.Tween;
public record struct TweenCache<T>(T Start, T End, bool Pingpong, TweenEaseType EaseType, int Duration) where T : struct;
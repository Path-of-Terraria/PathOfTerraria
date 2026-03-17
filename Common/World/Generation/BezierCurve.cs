#pragma warning disable IDE0011
#pragma warning disable IDE0300

namespace PathOfTerraria.Common.World.Generation;

/// <summary> Based on https://www.codeproject.com/articles/25237/bezier-curves-made-simple </summary>
internal static class BezierCurve
{
	private static readonly double[] FactorialLookup = new double[33] {
		// fill untill n=32. The rest is too high to represent
		1.0,
		1.0,
		2.0,
		6.0,
		24.0,
		120.0,
		720.0,
		5040.0,
		40320.0,
		362880.0,
		3628800.0,
		39916800.0,
		479001600.0,
		6227020800.0,
		87178291200.0,
		1307674368000.0,
		20922789888000.0,
		355687428096000.0,
		6402373705728000.0,
		121645100408832000.0,
		2432902008176640000.0,
		51090942171709440000.0,
		1124000727777607680000.0,
		25852016738884976640000.0,
		620448401733239439360000.0,
		15511210043330985984000000.0,
		403291461126605635584000000.0,
		10888869450418352160768000000.0,
		304888344611713860501504000000.0,
		8841761993739701954543616000000.0,
		265252859812191058636308480000000.0,
		8222838654177922817725562880000000.0,
		263130836933693530167218012160000000.0,
	};

	private static double Factorial(int n)
	{
		return FactorialLookup[n]; /* returns the value n! as a SUMORealing point number */
	}

	private static double Ni(int n, int i)
	{
		return Factorial(n) / (Factorial(i) * Factorial(n - i));
	}

	private static double Bernstein(int n, int i, double t)
	{
		double basis;
		double ti = Math.Pow(t, i); ;
		double tni = Math.Pow(1 - t, n - i);

		if (t == 0.0 && i == 0) ti = 1.0;
		if (n == i && t == 1.0) tni = 1.0;

		//Bernstein basis
		basis = Ni(n, i) * ti * tni;
		return basis;
	}

	private static void Bezier2D(ReadOnlySpan<double> b, int cpts, Span<double> p)
	{
		int npts = b.Length / 2;

		// Calculate points on curve

		int icount = 0;
		double t = 0;
		double step = 1.0 / (cpts - 1);

		int jcount;

		for (int i1 = 0; i1 != cpts; i1++)
		{
			if (1.0 - t < 5e-6)
				t = 1.0;

			jcount = 0;
			p[icount] = 0.0;
			p[icount + 1] = 0.0;
			for (int i = 0; i != npts; i++)
			{
				double basis = Bernstein(npts - 1, i, t);
				p[icount] += basis * b[jcount];
				p[icount + 1] += basis * b[jcount + 1];
				jcount += 2;
			}

			icount += 2;
			t += step;
		}
	}

	public static Vector2[] GetBezier(ReadOnlySpan<double> orderedPositions)
	{
		const int POINTS_ON_CURVE = 100;

		Span<double> p = stackalloc double[POINTS_ON_CURVE];

		Bezier2D(orderedPositions, POINTS_ON_CURVE / 2, p);

		var res = new Vector2[POINTS_ON_CURVE / 2];

		for (int i = 0; i <= POINTS_ON_CURVE - 2; i += 2)
		{
			res[i / 2] = new Vector2((int)p[i], (int)p[i + 1]);
		}

		return res;
	}
}
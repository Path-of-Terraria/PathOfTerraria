namespace PathOfTerraria.Core.Graphics;

[Autoload(Side = ModSide.Client)]
public sealed class PrimitiveDrawing : ModSystem
{
	private static BasicEffect _simpleVertexEffect;

	public override void Load()
	{
		base.Load();

		Init(Main.graphics.GraphicsDevice);
	}

	private static void Init(GraphicsDevice device){
		Main.QueueMainThreadAction(() => _simpleVertexEffect = new BasicEffect(device){
			VertexColorEnabled = true
		});
	}

	public static void DrawLineStrip(Vector2[] points, Color color){
		ArgumentNullException.ThrowIfNull(points);

		if (points.Length < 2)
		{
			throw new ArgumentException("Too few points provided to draw a line");
		}

		var packet = new PrimitivePacket(PrimitiveType.LineStrip);

		packet.AddDraw(ToPrimitive(points[0], color), ToPrimitive(points[1], color));
		for(int i = 2; i < points.Length; i++)
		{
			packet.AddDraw(ToPrimitive(points[i], color));
		}

		SubmitPacket(packet);
	}

	public static void DrawLineStrip(Vector2[] points, Color start, Color end){
		ArgumentNullException.ThrowIfNull(points);

		if (points.Length < 2)
		{
			throw new ArgumentException("Too few points provided to draw a line");
		}

		float lerpStep = 1f / (points.Length - 1);

		var packet = new PrimitivePacket(PrimitiveType.LineStrip);

		packet.AddDraw(ToPrimitive(points[0], Color.Lerp(start, end, 0)), ToPrimitive(points[1], Color.Lerp(start, end, lerpStep)));
		for(int i = 2; i < points.Length; i++)
		{
			packet.AddDraw(ToPrimitive(points[i], Color.Lerp(start, end, lerpStep * i)));
		}

		SubmitPacket(packet);
	}

	public static void DrawLineStrip(Vector2[] points, Color[] colors){
		ArgumentNullException.ThrowIfNull(points);
		ArgumentNullException.ThrowIfNull(colors);

		if (colors.Length != points.Length)
		{
			throw new ArgumentException("Length of colors array must match length of points array", nameof(colors));
		}

		if(points.Length < 2)
		{
			throw new ArgumentException("Too few points provided to draw a line");
		}

		var packet = new PrimitivePacket(PrimitiveType.LineStrip);

		packet.AddDraw(ToPrimitive(points[0], colors[0]), ToPrimitive(points[1], colors[0]));
		for(int i = 2; i < points.Length; i++)
		{
			packet.AddDraw(ToPrimitive(points[i], colors[i]));
		}

		SubmitPacket(packet);
	}

	public static void DrawLineList(Vector2[] points, Color color){
		ArgumentNullException.ThrowIfNull(points);

		if (points.Length % 2 != 0)
		{
			throw new ArgumentException("Length of points array must be a multiple of 2", nameof(points));
		}

		//Nothing to draw, so don't
		if(points.Length < 2)
		{
			throw new ArgumentException("Too few points provided to draw a line");
		}

		var packet = new PrimitivePacket(PrimitiveType.LineList);
		for(int i = 0; i < points.Length; i += 2)
		{
			packet.AddDraw(ToPrimitive(points[i], color), ToPrimitive(points[i + 1], color));
		}

		SubmitPacket(packet);
	}

	public static void DrawLineList(Vector2[] points, Color[] colors){
		ArgumentNullException.ThrowIfNull(points);
		ArgumentNullException.ThrowIfNull(colors);

		if (points.Length % 2 != 0)
		{
			throw new ArgumentException("Length of points array must be a multiple of 2", nameof(points));
		}

		if(colors.Length != points.Length)
		{
			throw new ArgumentException("Length of colors array must match length of points array", nameof(colors));
		}

		//Nothing to draw, so don't
		if(points.Length < 2)
		{
			return;
		}

		var packet = new PrimitivePacket(PrimitiveType.LineList);
		for(int i = 0; i < points.Length; i += 2)
		{
			packet.AddDraw(ToPrimitive(points[i], colors[i]), ToPrimitive(points[i + 1], colors[i + 1]));
		}

		SubmitPacket(packet);
	}

	public static void DrawHollowRectangle(Vector2 coordTl, Vector2 coordBr, Color colorTl, Color colorTr, Color colorBl, Color colorBr){
		var tr = new Vector2(coordBr.X, coordTl.Y);
		var bl = new Vector2(coordTl.X, coordBr.Y);

		var packet = new PrimitivePacket(PrimitiveType.LineStrip);

		packet.AddDraw(ToPrimitive(coordTl, colorTl), ToPrimitive(tr, colorTr));
		packet.AddDraw(ToPrimitive(tr, colorTr),      ToPrimitive(coordBr, colorBr));
		packet.AddDraw(ToPrimitive(coordBr, colorBr), ToPrimitive(bl, colorBl));
		packet.AddDraw(ToPrimitive(bl, colorBl),      ToPrimitive(coordTl, colorTl));

		SubmitPacket(packet);
	}

	public static void DrawFilledRectangle(Vector2 coordTl, Vector2 coordBr, Color colorTl, Color colorTr, Color colorBl, Color colorBr){
		var tr = new Vector2(coordBr.X, coordTl.Y);
		var bl = new Vector2(coordTl.X, coordBr.Y);

		var packet = new PrimitivePacket(PrimitiveType.TriangleList);

		packet.AddDraw(ToPrimitive(coordTl, colorTl), ToPrimitive(tr, colorTr), ToPrimitive(bl, colorBl));
		packet.AddDraw(ToPrimitive(bl, colorBl), ToPrimitive(tr, colorTr), ToPrimitive(coordBr, colorBr));

		SubmitPacket(packet);
	}

	public static void DrawHollowCircle(Vector2 center, float radius, Color color){
		var packet = new PrimitivePacket(PrimitiveType.LineStrip);

		Vector2 rotate = Vector2.UnitX * radius;
		packet.AddDraw(ToPrimitive(rotate + center, color), ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f) + center, color));
		for(int i = 2; i < 360; i++)
		{
			packet.AddDraw(ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 306f * i), color));
		}

		SubmitPacket(packet);
	}

	public static void DrawFilledCircle(Vector2 center, float radius, Color color){
		var packet = new PrimitivePacket(PrimitiveType.TriangleList);

		Vector2 rotate = Vector2.UnitX * radius;
		packet.AddDraw(ToPrimitive(rotate + center, color),
			ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f) + center, color),
			ToPrimitive(center, color));
		for(int i = 1; i < 360; i++){
			packet.AddDraw(ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360 * i) + center, color),
				ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f * (i + 1)) + center, color),
				ToPrimitive(center, color));
		}

		SubmitPacket(packet);
	}

	public static void DrawFilledCircle(Vector2 center, float radius, Color color, Color edge){
		var packet = new PrimitivePacket(PrimitiveType.TriangleList);

		Vector2 rotate = Vector2.UnitX * radius;
		packet.AddDraw(ToPrimitive(rotate + center, edge),
			ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f) + center, edge),
			ToPrimitive(center, color));
		for(int i = 1; i < 360; i++){
			packet.AddDraw(ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360 * i) + center, edge),
				ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f * (i + 1)) + center, edge),
				ToPrimitive(center, color));
		}

		SubmitPacket(packet);
	}

	public static void DrawTriangleList(Vector2[] points, Color color){
		ArgumentNullException.ThrowIfNull(points);

		if (points.Length % 3 != 0)
		{
			throw new ArgumentException("Length of points array must be a multiple of 3", nameof(points));
		}

		//Nothing to draw, so don't
		if(points.Length < 3)
		{
			throw new ArgumentException("Too few points provided to draw a triangle");
		}

		var packet = new PrimitivePacket(PrimitiveType.TriangleList);
		for(int i = 0; i < points.Length; i += 3)
		{
			packet.AddDraw(ToPrimitive(points[i], color), ToPrimitive(points[i + 1], color), ToPrimitive(points[i + 2], color));
		}

		SubmitPacket(packet);
	}

	public static void DrawTriangleList(Vector2[] points, Color[] colors){
		ArgumentNullException.ThrowIfNull(points);
		ArgumentNullException.ThrowIfNull(colors);

		if (points.Length % 3 != 0)
		{
			throw new ArgumentException("Length of points array must be a multiple of 3", nameof(points));
		}

		if (colors.Length != points.Length)
		{
			throw new ArgumentException("Length of colors array must match length of points array", nameof(colors));
		}

		//Nothing to draw, so don't
		if (points.Length < 3)
		{
			throw new ArgumentException("Too few points provided to draw a triangle");
		}

		var packet = new PrimitivePacket(PrimitiveType.LineList);
		for(int i = 0; i < points.Length; i += 3)
		{
			packet.AddDraw(ToPrimitive(points[i], colors[i]), ToPrimitive(points[i + 1], colors[i + 1]), ToPrimitive(points[i + 2], colors[i + 2]));
		}

		SubmitPacket(packet);
	}

	private static void SubmitPacket(PrimitivePacket packet){
		ArgumentNullException.ThrowIfNull(packet);

		if (packet.Draws is null)
		{
			throw new ArgumentNullException(nameof(packet) + "." + nameof(PrimitivePacket.Draws));
		}

		if(packet.Draws.Count <= 0)
		{
			throw new ArgumentOutOfRangeException("packet.draws.Count", "Packet does not have any primitive data attached to it");
		}

		var buffer = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColor), packet.Draws.Count, BufferUsage.WriteOnly);

		//Calculate the number of primitives that will be drawn
		int count = packet.GetPrimitivesCount();

		//Device must not have a buffer attached for a buffer to be given data
		Main.graphics.GraphicsDevice.SetVertexBuffer(null);
		buffer.SetData(packet.Draws.ToArray());

		//Set the buffer
		Main.graphics.GraphicsDevice.SetVertexBuffer(buffer);
		_simpleVertexEffect.CurrentTechnique.Passes[0].Apply();

		//Draw the vertices
		Main.graphics.GraphicsDevice.DrawPrimitives(packet.Type, 0, count);
	}

	/// <summary>
	/// Converts the riven world <paramref name="worldPos"/> coordinate and <paramref name="color"/> draw color into a <seealso cref="VertexPositionColor"/> that can be used in primitives drawing
	/// </summary>
	/// <param name="worldPos">The absolute world position</param>
	/// <param name="color">The draw color</param>
	private static VertexPositionColor ToPrimitive(Vector2 worldPos, Color color) {
		// Compute screen coordinates by subtracting screen position from world position
		Vector2 screenPos = worldPos - Main.screenPosition;

		// Convert to Vector3
		var pos = new Vector3(screenPos, 0f);

		// Adjust for gravity direction
		if (Main.LocalPlayer.gravDir == -1) {
			pos.Y = -pos.Y;
		}

		return new VertexPositionColor(pos, color);
	}
}
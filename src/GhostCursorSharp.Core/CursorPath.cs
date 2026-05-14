namespace GhostCursorSharp;

/// <summary>
/// Generates human-like cursor paths and timed cursor paths.
/// </summary>
public static class CursorPath
{
    private const double DefaultWidth = 100;
    private const int MinSteps = 25;
    private const int MinSpread = 2;
    private const int MaxSpread = 200;
    private const int LengthSamples = 100;

    /// <summary>
    /// Generates a human-like path between two coordinates.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The ending coordinate.</param>
    /// <param name="options">Optional path generation settings.</param>
    /// <returns>A sequence of cursor points describing the movement.</returns>
    public static IReadOnlyList<Vector> Generate(Vector start, Vector end, PathOptions? options = null)
        => GenerateVectors(start, end, DefaultWidth, options);

    /// <summary>
    /// Generates a human-like path between two coordinates using a specific Bezier spread.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The ending coordinate.</param>
    /// <param name="spreadOverride">The spread to use when generating Bezier anchors.</param>
    /// <returns>A sequence of cursor points describing the movement.</returns>
    public static IReadOnlyList<Vector> Generate(Vector start, Vector end, double spreadOverride)
        => Generate(start, end, new PathOptions { SpreadOverride = spreadOverride });

    /// <summary>
    /// Generates a human-like path from a coordinate to an element box target.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The target element box whose origin is used as the destination.</param>
    /// <param name="options">Optional path generation settings.</param>
    /// <returns>A sequence of cursor points describing the movement.</returns>
    public static IReadOnlyList<Vector> Generate(Vector start, ElementBox end, PathOptions? options = null)
    {
        var target = new Vector(end.X, end.Y);
        var width = end.Width <= double.Epsilon ? DefaultWidth : end.Width;

        return GenerateVectors(start, target, width, options);
    }

    /// <summary>
    /// Generates a human-like path from a coordinate to an element box target using a specific Bezier spread.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The target element box whose origin is used as the destination.</param>
    /// <param name="spreadOverride">The spread to use when generating Bezier anchors.</param>
    /// <returns>A sequence of cursor points describing the movement.</returns>
    public static IReadOnlyList<Vector> Generate(Vector start, ElementBox end, double spreadOverride)
        => Generate(start, end, new PathOptions { SpreadOverride = spreadOverride });

    /// <summary>
    /// Generates a human-like timed path between two coordinates.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The ending coordinate.</param>
    /// <param name="options">Optional path generation settings.</param>
    /// <returns>A sequence of cursor points with Unix timestamps in milliseconds.</returns>
    public static IReadOnlyList<TimedVector> GenerateTimed(Vector start, Vector end, PathOptions? options = null)
        => GenerateTimestamps(Generate(start, end, options), options);

    /// <summary>
    /// Generates a human-like timed path between two coordinates using a specific Bezier spread.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The ending coordinate.</param>
    /// <param name="spreadOverride">The spread to use when generating Bezier anchors.</param>
    /// <returns>A sequence of cursor points with Unix timestamps in milliseconds.</returns>
    public static IReadOnlyList<TimedVector> GenerateTimed(Vector start, Vector end, double spreadOverride)
        => GenerateTimed(start, end, new PathOptions { SpreadOverride = spreadOverride });

    /// <summary>
    /// Generates a human-like timed path from a coordinate to an element box target.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The target element box whose origin is used as the destination.</param>
    /// <param name="options">Optional path generation settings.</param>
    /// <returns>A sequence of cursor points with Unix timestamps in milliseconds.</returns>
    public static IReadOnlyList<TimedVector> GenerateTimed(Vector start, ElementBox end, PathOptions? options = null)
        => GenerateTimestamps(Generate(start, end, options), options);

    /// <summary>
    /// Generates a human-like timed path from a coordinate to an element box target using a specific Bezier spread.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The target element box whose origin is used as the destination.</param>
    /// <param name="spreadOverride">The spread to use when generating Bezier anchors.</param>
    /// <returns>A sequence of cursor points with Unix timestamps in milliseconds.</returns>
    public static IReadOnlyList<TimedVector> GenerateTimed(Vector start, ElementBox end, double spreadOverride)
        => GenerateTimed(start, end, new PathOptions { SpreadOverride = spreadOverride });

    private static IReadOnlyList<Vector> GenerateVectors(Vector start, Vector end, double width, PathOptions? options)
    {
        var curve = CreateCurve(start, end, options?.SpreadOverride);
        var length = curve.Length() * 0.8;

        var speed = options?.MoveSpeed is > 0
            ? 25 / options.MoveSpeed.Value
            : Random.Shared.NextDouble();

        var baseTime = speed * MinSteps;
        var steps = (int)Math.Ceiling((Math.Log2(Fitts(length, width) + 1) + baseTime) * 3);
        var lookupTable = curve.GetLookupTable(steps);

        return ClampPositive(lookupTable);
    }

    private static CubicBezierCurve CreateCurve(Vector start, Vector end, double? spreadOverride)
    {
        var length = Magnitude(Direction(start, end));
        var spread = spreadOverride ?? Clamp(length, MinSpread, MaxSpread);
        var (anchor1, anchor2) = GenerateBezierAnchors(start, end, spread);

        return new CubicBezierCurve(start, anchor1, anchor2, end);
    }

    private static IReadOnlyList<Vector> ClampPositive(IReadOnlyList<Vector> vectors)
        => vectors.Select(v => new Vector(Math.Max(0, v.X), Math.Max(0, v.Y))).ToArray();

    private static IReadOnlyList<TimedVector> GenerateTimestamps(IReadOnlyList<Vector> vectors, PathOptions? options)
    {
        var speed = options?.MoveSpeed ?? (Random.Shared.NextDouble() * 0.5 + 0.5);
        var timedVectors = new TimedVector[vectors.Count];
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        for (var i = 0; i < vectors.Count; i++)
        {
            if (i == 0)
            {
                timedVectors[i] = new TimedVector(vectors[i].X, vectors[i].Y, now);
                continue;
            }

            var p0 = vectors[i - 1];
            var p1 = vectors[i];
            var p2 = i + 1 < vectors.Count ? vectors[i + 1] : Extrapolate(p0, p1);
            var p3 = i + 2 < vectors.Count ? vectors[i + 2] : Extrapolate(p1, p2);
            var time = TimeToMove(p0, p1, p2, p3, vectors.Count, speed);

            timedVectors[i] = new TimedVector(
                vectors[i].X,
                vectors[i].Y,
                timedVectors[i - 1].Timestamp + time);
        }

        return timedVectors;
    }

    private static long TimeToMove(Vector p0, Vector p1, Vector p2, Vector p3, int samples, double speed)
    {
        var total = 0d;
        var dt = 1d / samples;

        for (var t = 0d; t < 1; t += dt)
        {
            var v1 = BezierCurveSpeed(t * dt, p0, p1, p2, p3);
            var v2 = BezierCurveSpeed(t, p0, p1, p2, p3);
            total += (v1 + v2) * dt / 2;
        }

        return (long)Math.Round(total / speed);
    }

    private static double BezierCurveSpeed(double t, Vector p0, Vector p1, Vector p2, Vector p3)
    {
        var b1 = 3 * Math.Pow(1 - t, 2) * (p1.X - p0.X)
            + 6 * (1 - t) * t * (p2.X - p1.X)
            + 3 * Math.Pow(t, 2) * (p3.X - p2.X);
        var b2 = 3 * Math.Pow(1 - t, 2) * (p1.Y - p0.Y)
            + 6 * (1 - t) * t * (p2.Y - p1.Y)
            + 3 * Math.Pow(t, 2) * (p3.Y - p2.Y);

        return Math.Sqrt((b1 * b1) + (b2 * b2));
    }

    private static (Vector First, Vector Second) GenerateBezierAnchors(Vector a, Vector b, double spread)
    {
        var side = Random.Shared.Next(0, 2) == 1 ? 1 : -1;

        Vector Calc()
        {
            var (randomMidpoint, normalVector) = RandomNormalLine(a, b, spread);
            var choice = Multiply(normalVector, side);
            return RandomVectorOnLine(randomMidpoint, Add(randomMidpoint, choice));
        }

        var anchors = new[] { Calc(), Calc() }.OrderBy(v => v.X).ToArray();
        return (anchors[0], anchors[1]);
    }

    private static (Vector Midpoint, Vector Normal) RandomNormalLine(Vector a, Vector b, double range)
    {
        var randomMidpoint = RandomVectorOnLine(a, b);
        var normal = SetMagnitude(Perpendicular(Direction(a, randomMidpoint)), range);
        return (randomMidpoint, normal);
    }

    private static Vector RandomVectorOnLine(Vector a, Vector b)
    {
        var direction = Direction(a, b);
        return Add(a, Multiply(direction, Random.Shared.NextDouble()));
    }

    private static Vector SetMagnitude(Vector a, double amount)
        => Multiply(Unit(a), amount);

    private static Vector Unit(Vector a)
    {
        var magnitude = Magnitude(a);
        return magnitude <= double.Epsilon ? Vector.Origin : Divide(a, magnitude);
    }

    private static double Magnitude(Vector a)
        => Math.Sqrt((a.X * a.X) + (a.Y * a.Y));

    private static Vector Perpendicular(Vector vector)
    {
        var x = vector.Y;
        var y = -vector.X;
        return new Vector(x, y);
    }

    private static Vector Direction(Vector from, Vector to)
        => Subtract(to, from);

    private static Vector Extrapolate(Vector previous, Vector current)
        => Add(current, Subtract(current, previous));

    private static Vector Add(Vector a, Vector b)
        => new(a.X + b.X, a.Y + b.Y);

    private static Vector Subtract(Vector a, Vector b)
        => new(a.X - b.X, a.Y - b.Y);

    private static Vector Multiply(Vector a, double scalar)
        => new(a.X * scalar, a.Y * scalar);

    private static Vector Divide(Vector a, double scalar)
        => new(a.X / scalar, a.Y / scalar);

    private static double Clamp(double target, double min, double max)
        => Math.Min(max, Math.Max(min, target));

    private static double Fitts(double distance, double width)
        => 2 * Math.Log2((distance / width) + 1);

    private sealed class CubicBezierCurve(Vector start, Vector control1, Vector control2, Vector end)
    {
        public double Length()
        {
            var length = 0d;
            var previous = start;

            for (var i = 1; i <= LengthSamples; i++)
            {
                var point = PointAt(i / (double)LengthSamples);
                length += Magnitude(Subtract(point, previous));
                previous = point;
            }

            return length;
        }

        public Vector[] GetLookupTable(int steps)
        {
            var resolvedSteps = Math.Max(1, steps);
            var points = new Vector[resolvedSteps + 1];

            for (var i = 0; i <= resolvedSteps; i++)
            {
                points[i] = PointAt(i / (double)resolvedSteps);
            }

            return points;
        }

        private Vector PointAt(double t)
        {
            var oneMinusT = 1 - t;
            var x =
                (Math.Pow(oneMinusT, 3) * start.X) +
                (3 * Math.Pow(oneMinusT, 2) * t * control1.X) +
                (3 * oneMinusT * Math.Pow(t, 2) * control2.X) +
                (Math.Pow(t, 3) * end.X);
            var y =
                (Math.Pow(oneMinusT, 3) * start.Y) +
                (3 * Math.Pow(oneMinusT, 2) * t * control1.Y) +
                (3 * oneMinusT * Math.Pow(t, 2) * control2.Y) +
                (Math.Pow(t, 3) * end.Y);

            return new Vector(x, y);
        }
    }
}

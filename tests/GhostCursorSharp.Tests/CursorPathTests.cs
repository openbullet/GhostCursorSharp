namespace GhostCursorSharp.Tests;

public class CursorPathTests
{
    [Fact]
    public void GetPointInBox_WithExplicitDestination_UsesTopLeftRelativeCoordinates()
    {
        var box = new ElementBox(320, 240, 120, 40);

        var point = CursorTargeting.GetPointInBox(box, new BoxOptions
        {
            Destination = new Vector(10, 15)
        });

        Assert.Equal(330, point.X);
        Assert.Equal(255, point.Y);
    }

    [Fact]
    public void Generate_ClampsCoordinatesToPositiveSpace()
    {
        var path = CursorPath.Generate(
            new Vector(-25, -10),
            new Vector(150, 80),
            new PathOptions { MoveSpeed = 10, SpreadOverride = 0 });

        Assert.NotEmpty(path);
        Assert.All(path, point =>
        {
            Assert.True(point.X >= 0);
            Assert.True(point.Y >= 0);
        });

        Assert.Equal(Vector.Origin, path[0]);
        Assert.True(Math.Abs(path[^1].X - 150) < 0.000001);
        Assert.True(Math.Abs(path[^1].Y - 80) < 0.000001);
    }

    [Fact]
    public void Generate_WithBoundingBox_EndsAtTheBoxOrigin()
    {
        var box = new ElementBox(320, 240, 120, 40);

        var path = CursorPath.Generate(
            new Vector(0, 0),
            box,
            new PathOptions { MoveSpeed = 10, SpreadOverride = 0 });

        Assert.NotEmpty(path);
        Assert.True(Math.Abs(path[^1].X - box.X) < 0.000001);
        Assert.True(Math.Abs(path[^1].Y - box.Y) < 0.000001);
    }

    [Fact]
    public void GenerateTimed_ReturnsMonotonicTimestamps()
    {
        var path = CursorPath.GenerateTimed(
            new Vector(10, 15),
            new Vector(500, 325),
            new PathOptions { MoveSpeed = 8, SpreadOverride = 0 });

        Assert.True(path.Count > 1);

        for (var i = 1; i < path.Count; i++)
        {
            Assert.True(path[i].Timestamp >= path[i - 1].Timestamp);
        }
    }
}

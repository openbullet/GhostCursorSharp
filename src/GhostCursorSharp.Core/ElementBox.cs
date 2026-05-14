namespace GhostCursorSharp;

/// <summary>
/// Represents a rectangular element box in page coordinates.
/// </summary>
/// <param name="X">The horizontal page coordinate of the top-left corner.</param>
/// <param name="Y">The vertical page coordinate of the top-left corner.</param>
/// <param name="Width">The width of the box.</param>
/// <param name="Height">The height of the box.</param>
public readonly record struct ElementBox(double X, double Y, double Width, double Height);

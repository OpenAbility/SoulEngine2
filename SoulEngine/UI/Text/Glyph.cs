using OpenTK.Mathematics;

namespace SoulEngine.UI.Text;

public struct Glyph(char character, Sprite sprite, Vector2i offset, int advance)
{
    public readonly char Character = character;
    public readonly Sprite Sprite = sprite;
    public readonly Vector2i Offset = offset;
    public readonly int Advance = advance;
}
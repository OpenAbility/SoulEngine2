using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.UI.Text;

[Resource("e.fnt", typeof(Loader))]
[ExpectedExtensions(".fnt")]
public partial class Font : Resource
{
    public string Name { get; private set; } = null!;
    public int FontSize { get; private set; }
    
    public int BaseLine { get; private set; }
    public int LineHeight { get; private set; }

    private Texture[] pages = null!;

    private readonly Dictionary<char, Glyph> glyphs = new Dictionary<char, Glyph>();

    public Glyph? this[char glyph]
    {
        get
        {
            if (glyphs.TryGetValue(glyph, out var value))
                return value;
            return glyph == char.MaxValue ? null : this[char.MaxValue];
        }
    }
}

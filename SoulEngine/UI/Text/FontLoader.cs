using System.Text;
using System.Xml;
using OpenAbility.Logging;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Data;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.UI.Text;

public partial class Font
{
    public class Loader : IResourceLoader<Font>
    {
        private static readonly Logger Logger = Logger.Get("FontLoader");
        
        public Font LoadResource(ResourceManager resourceManager, string id, ContentContext content)
        {
            Stream? stream = content.Load(id);
            if (stream == null)
            {
                string fallback = EngineVarContext.Global.GetString("e_font_fallback", "ui/font/DEFAULT.fnt");
                Logger.Error("Font '{}' not found! Using fallback '{}'", id, fallback);
                id = fallback;
                stream = content.Load(id);
            }

            if (stream == null)
                throw new Exception("Font not found and fallback font could not be loaded!");
            
            XmlDocument document = new XmlDocument();
            document.Load(stream);

            XmlElement rootElement = document.DocumentElement!;

            if (rootElement.Name != "font")
                throw new Exception("Not a font file!");
            
            Font font = new Font();

            XmlElement infoElement = (XmlElement)document.SelectSingleNode("font/info")!;
            XmlElement commonElement = (XmlElement)document.SelectSingleNode("font/common")!;

            font.Name = infoElement.GetAttribute("face");
            font.FontSize = int.Parse(infoElement.GetAttribute("size"));
            font.BaseLine = int.Parse(commonElement.GetAttribute("base"));
            font.LineHeight = int.Parse(commonElement.GetAttribute("lineHeight"));
            
            XmlElement pagesElement = (XmlElement)document.SelectSingleNode("font/pages")!;

            List<Texture> pages = new List<Texture>();
            
            foreach (XmlElement page in pagesElement)
            {
                string pagePath = Path.Join(Path.GetDirectoryName(id), page.GetAttribute("file"));
                Texture texture =
                    resourceManager.Load<Texture>(pagePath);
                
                pages.Add(texture);
                
            }

            font.pages = pages.ToArray();
            
            XmlElement charsElement = (XmlElement)document.SelectSingleNode("font/chars")!;
            
            foreach (XmlElement charElement in charsElement)
            {
                char c = (char)int.Parse(charElement.GetAttribute("id"));

                int pageID = int.Parse(charElement.GetAttribute("page"));
                
                int x = int.Parse(charElement.GetAttribute("x"));
                int y = int.Parse(charElement.GetAttribute("y"));
                
                int width = int.Parse(charElement.GetAttribute("width"));
                int height = int.Parse(charElement.GetAttribute("height"));
                
                int xoffset = int.Parse(charElement.GetAttribute("xoffset"));
                int yoffset = int.Parse(charElement.GetAttribute("yoffset"));
                
                int xadvance = int.Parse(charElement.GetAttribute("xadvance"));

                Sprite sprite = new Sprite(font.pages[pageID], new Vector2i(x, font.pages[pageID].Height - y - height), new Vector2i(width, height));

                Glyph glyph = new Glyph(c, sprite, new Vector2i(xoffset, yoffset), xadvance);

                font.glyphs[c] = glyph;
            }
            
            return font;
        }
    }
}
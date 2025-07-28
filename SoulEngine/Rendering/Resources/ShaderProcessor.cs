using System.Text;
using System.Text.RegularExpressions;
using OpenAbility.Logging;
using SoulEngine.Content;

namespace SoulEngine.Rendering;

public static class ShaderProcessor
{
    private static readonly Logger Logger = Logger.Get("ShaderProcessor");
    
    private static readonly Regex IncludeRegex =
        new Regex(
            "^\\s*#\\s*include\\s+(?:(?:\"([a-zA-Z0-9._/]+)\")|(?:<([a-zA-Z0-9._/]+)>))\\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);
    
    public static string ProcessShader(ContentContext content, string source, string path, Dictionary<string, string>? defines, bool supportsLineDirectives)
    {
        StringBuilder result = new StringBuilder();

        result.AppendLine("#version 450 core").AppendLine();

        if (defines != null)
        {
            result.AppendLine("// DEFINES");
            foreach (var define in defines)
            {
                result.AppendLine($"#define {define.Key} {define.Value}");
            }
        }

        if (supportsLineDirectives)
        {
            result.AppendLine("#extension GL_ARB_shading_language_include : require");
        }

        IncludeFile(content, source, path, supportsLineDirectives, result);

        return result.ToString();
    }

    private static void IncludeFile(ContentContext content, string source, string path, bool supportsLineDirectives,
        StringBuilder result)
    {
        if (supportsLineDirectives)
        {
            result.AppendLine("#line 1 \"" + path + "\"");
        }

        int lineIndex = 1;
        foreach (var line in source.EnumerateLines())
        {
            if (IncludeRegex.IsMatch(line))
            {
                string lineString = new string(line);

                Match match = IncludeRegex.Match(lineString);
                string include = match.Groups[1].Value;
                
                Logger.Debug("Including file '{}'", include);

                string includePath = Path.Combine(Path.GetDirectoryName(path) ?? "", include);

                if (!content.Exists(includePath))
                    throw new Exception("Invalid include: '" + includePath + "'");
                
                IncludeFile(content, content.LoadString(includePath), includePath, supportsLineDirectives, result);
                
                result.AppendLine("#line " + (lineIndex + 1) + " \"" + path + "\"");
            }
            else
            {
                result.Append(line).Append('\n');
            }

            lineIndex++;
        }
        
    }
}
namespace SoulEngine.Rendering;

public static class ShaderProcessor
{
    public static string ProcessShader(string source, string path, Dictionary<string, string>? defines, bool supportsLineDirectives)
    {
        string prefix = "#version 450 core\n\n";

        if (defines != null)
        {
            prefix += "// DEFINES" + string.Join("\n", defines.Select(kvp => "#define " + kvp.Key + " " + kvp.Value)) +
                      "\n\n";
        }
        
        if (supportsLineDirectives)
        {
            prefix += "#extension GL_ARB_shading_language_include : require\n#line 1 \"" + path + "\"\n";
        }
        else
        {
            prefix += "#line 1 0\n";
        }
        source = prefix + source;
            
        //source = ProcessShaderSource(source, id);

        return source;
    }
}
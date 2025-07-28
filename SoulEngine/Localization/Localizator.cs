using Newtonsoft.Json;
using SoulEngine.Core;

namespace SoulEngine.Localization;

public class Localizator : EngineObject
{

    private string currentLocale = null!;
    private readonly Game Game;

    private readonly Dictionary<string, string> strings = new Dictionary<string, string>();

    public Localizator(Game game)
    {
        Game = game;
        LoadLocale("en_GB");
    }

    public void LoadLocale(string locale)
    {
        if(locale == currentLocale)
            return;
        currentLocale = locale;
        
        strings.Clear();

        string[] keysets = Game.Content.LoadAllStrings("locale/" + currentLocale + ".json");
        Array.Reverse(keysets);
        
        foreach (var keyset in keysets)
        {
            Dictionary<string, string> loaded = JsonConvert.DeserializeObject<Dictionary<string, string>>(keyset)!;

            foreach (var key in loaded)
            {
                strings[key.Key] = key.Value;
            }
        }
    }

    public string Localize(string key, params object[] values)
    {
        return !strings.TryGetValue(key, out string? localized) ? key : string.Format(localized, values);
    }


}
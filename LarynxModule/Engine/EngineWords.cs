using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LarynxModule.Engine
{
    public static class EngineWords // TODO: Be compatible with other languages
    {
        public static Dictionary<string, string> PtPunctuation { get; } = new Dictionary<string, string>
        {
            { "interrogação", "?"},
            { "dois pontos", ":"},
            { "ponto e vírgula", ";"},
            { "exclamação", "!"},
            { "hífen", "-"},
            { "abre parêntese", "("},
            { "fecha parêntese", ")"},
            { "arroba", "@" },
            { "ponto", "."},
            { "vírgula", ","},
            //{ "nova linha", "\n"},
        };

        public static string ReplacePunctuation(string Transcript)
        {
            string ret = Transcript;
            foreach (var k in PtPunctuation.Keys)
                ret = Regex.Replace(ret, $"\\s*{k}", PtPunctuation[k], RegexOptions.IgnoreCase);

            return ret;
        }
    }
}

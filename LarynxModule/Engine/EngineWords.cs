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
            { " ponto", "."},
            { "ponto", "."},
            { " vírgula", ","},
            { "vírgula", ","},
            { "interrogação", "?"},
            { "dois pontos", ":"},
            { "ponto e vírgula", ";"},
            { "exclamação", "!"},
            { "hífen", "-"},
            { "nova linha", "\n"},
            { "abre parêntese", "("},
            { "fecha parêntese", ")"},
        };

        public static string ReplacePunctuation(string Transcript)
        {
            string ret = Transcript;
            foreach (var k in PtPunctuation.Keys)
                ret = Regex.Replace(ret, k, PtPunctuation[k], RegexOptions.IgnoreCase);

            return ret;
        }
    }
}

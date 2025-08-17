using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SogdianMerchant.Cli
{
    public class Stats
    {
        public List<int> GuardsWhenFirst { get; } = new List<int>();
        public List<int> GuardsWhenSecond { get; } = new List<int>();
        public Dictionary<string, int> GuidesWhenFirst { get; } = new Dictionary<string, int> { ["None"] = 0, ["Novice"] = 0, ["Veteran"] = 0 };
        public Dictionary<string, int> GuidesWhenSecond { get; } = new Dictionary<string, int> { ["None"] = 0, ["Novice"] = 0, ["Veteran"] = 0 };
        public Dictionary<string, int> MarketsWhenFirst { get; } = new Dictionary<string, int> { ["Do Nothing"] = 0, ["Baghdad Market"] = 0, ["Kashgar Market"] = 0, ["Karachi Market"] = 0 };
        public Dictionary<string, int> MarketsWhenSecond { get; } = new Dictionary<string, int> { ["Do Nothing"] = 0, ["Baghdad Market"] = 0, ["Kashgar Market"] = 0, ["Karachi Market"] = 0 };
        public List<double> BaghdadProfits { get; } = new List<double>();
        public List<double> KashgarProfits { get; } = new List<double>();
        public List<double> KarachiProfits { get; } = new List<double>();
    }
}

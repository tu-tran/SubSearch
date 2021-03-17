using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SubSearch.WPF
{
    public class CommandLineArgs
    {

        [Option('k', "keepJobFile", Required = false, Default = false)]
        public bool KeepJobFile { get; set; }

        [Option('j', "jobFile", Required = false)]
        public string JobPath { get; set; }

        [Option('t', "target", Required = false, Default = new string[0])]
        public IEnumerable<string> Targets { get; set; }

        [Option('q', "quiet", Required = false, Default = false)]
        public bool Quiet { get; set; }
    }
}

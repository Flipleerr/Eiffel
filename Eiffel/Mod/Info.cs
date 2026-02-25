using System;
using System.Collections.Generic;

namespace Eiffel.Mod
{
    public class Info
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public Version Version { get; set; } // use semantic versioning!!!
        public Version MinimumEiffelVErsion { get; set; }
        public List<Dependency> Dependencies { get; set; }
        public string Assembly { get; set; }
    }

    public class Dependency
    {
        public string ID { get; set; }
        public Version MinimumVersion { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered
{
    public class Scp096Helper
    {
        public static Scp096Helper singleton = null;

        public Scp096Helper()
        {
            if(singleton == null)
                singleton = this;
        }

        public Dictionary<ReferenceHub, List<ReferenceHub>> targets = new Dictionary<ReferenceHub, List<ReferenceHub>>();
    }
}

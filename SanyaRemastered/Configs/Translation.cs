using Exiled.API.Interfaces;
using SanyaRemastered.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Configs
{
    public class Translation : ITranslation
    {
        [Description("#Traduction du SanyaRemastered")]
        public bool IsEnabled { get; set; } = true;
        [Description("# Traduction des message hint")]
        public HintList HintList { get; set; } = new();
        [Description("# Traduction des Cassie")]
        public CustomSubtitles CustomSubtitles { get; set; } = new();
    }
}

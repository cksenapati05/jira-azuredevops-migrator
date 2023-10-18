using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraExport
{
    public class PullRequest
    {
        public Uri url { get; set; }

        public DateTime lastUpdate { get; set; }
    }
}
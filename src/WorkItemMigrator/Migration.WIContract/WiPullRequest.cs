using System;

namespace Migration.WIContract
{
    public class WiPullRequest
    {
        public Uri url { get; set; }

        public DateTime lastUpdate { get; set; }
    }
}
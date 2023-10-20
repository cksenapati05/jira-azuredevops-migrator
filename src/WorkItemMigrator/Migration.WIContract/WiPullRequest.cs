using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migration.WIContract
{
    public class WiPullRequest
    {
        public string PullRequestId { get; set; }
        public string RepositoryId { get; set; }

        public override string ToString()
        {
            return $"{RepositoryId}/{PullRequestId}";
        }
    }
}

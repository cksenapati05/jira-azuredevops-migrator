﻿using Microsoft.TeamFoundation.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Migration.WIContract;
using System;

namespace WorkItemImport.WitClient
{
    public static class JsonPatchDocUtils
    {
        public class PatchOperationValue
        {
            public string Rel { get; set; }
            public string Url { get; set; }
            public Attributes Attributes { get; set; }
        }

        public class Attributes
        {
            public string Name { get; set; }
        }

        public static JsonPatchOperation CreateJsonFieldPatchOp(Operation op, string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }

            return new JsonPatchOperation()
            {
                Operation = op,
                Path = "/fields/" + key,
                Value = value
            };
        }

        public static JsonPatchOperation CreateJsonArtifactLinkPatchOp(Operation op, string project, string repository, string commitId)
        {
            if (string.IsNullOrEmpty(commitId))
            {
                throw new ArgumentException(nameof(commitId));
            }

            if (string.IsNullOrEmpty(project))
            {
                throw new ArgumentException(nameof(project));
            }

            if (string.IsNullOrEmpty(repository))
            {
                throw new ArgumentException(nameof(repository));
            }

            return new JsonPatchOperation()
            {
                Operation = op,
                Path = "/relations/-",
                Value = new PatchOperationValue
                {
                    Rel = "ArtifactLink",
                    Url = $"vstfs:///GitHub/Commit/{repository}%2F{commitId}",
                    Attributes = new Attributes
                    {
                        Name = "GitHub Commit"
                    }
                }
            };
        }

        public static JsonPatchOperation CreateGitHubPullRequestLinkPatchOp(Operation op, WiPullRequest pullRequest)
        {
            if (pullRequest is null)
            {
                throw new ArgumentException(nameof(pullRequest));
            }

            if (string.IsNullOrEmpty(pullRequest.PullRequestId))
            {
                throw new ArgumentException(nameof(pullRequest.PullRequestId));
            }

            if (string.IsNullOrEmpty(pullRequest.RepositoryId))
            {
                throw new ArgumentException(nameof(pullRequest.RepositoryId));
            }

            return new JsonPatchOperation()
            {
                Operation = op,
                Path = "/relations/-",
                Value = new PatchOperationValue
                {
                    Rel = "ArtifactLink",
                    Url = $"vstfs:///GitHub/PullRequest/{pullRequest.RepositoryId}%2F{pullRequest.PullRequestId}",
                    Attributes = new Attributes
                    {
                        Name = "GitHub Pull Request"
                    }
                }
            };
        }
    }
}

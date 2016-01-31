using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Anne.Model.Git.Helper
{
    internal static class RepositoryHelper
    {
        internal static void UpdateGraph(ObservableCollection<Commit> commits, Dictionary<string, Commit> shaToCommit)
        {
            Commit priorCommit = null;

            commits.ForEach(commit =>
            {
                var depth = 0;

                commit.CommitGraphNode.Clear();

                if (priorCommit != null)
                {
                    var priorCommitParents = priorCommit.ParentShas
                        .Select(x => shaToCommit[x])
                        .ToArray();

                    var index = priorCommitParents.TakeWhile(p => p != commit).Count();

                    depth = index;
                }

                commit.CommitGraphNode.AddNodeCell(depth);

                var parentCount = 0;
                commit.ParentShas.ForEach(parentSha =>
                {
                    commit.CommitGraphNode.AddLineCell(depth, parentCount);
                    ++ parentCount;
                });

                priorCommit = commit;
            });
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Anne.Foundation.Extentions;

namespace Anne.Model.Git.Helper
{
    internal static class RepositoryHelper
    {
        internal static void UpdateGraph(ObservableCollection<Commit> commits, Dictionary<string, Commit> shaToCommit)
        {
            var commitScans = new Dictionary<string /*Sha*/, CommitScan>();
            commits
                .Do(commit => commit.CommitGraphNode.Clear())
                .ForEach(commit =>
                {
#if false
                    if (commit.Sha == "fd1e90e788827bdf6b9b944126f038498051fcee")
                    {
                        Debug.WriteLine(commit.Sha);
                    }
#endif

                    // 深さを決定する
                    //      すでに決定したらそれを使う
                    //      決定していなかったら、現在一番深いもの＋１を使う
                    {
                        var maxDepth = commitScans.Any() ? commitScans.Values.Max(x => x.Depth) + 1 : 0;
                        var depth = commitScans.TryGetValue(commit.Sha)?.Depth ?? maxDepth;

                        commit.CommitGraphNode.AddNodeCell(depth);
                    }

                    // 親の深さを作る
                    {
                        var firstDepth = commit.CommitGraphNode.NodeIndex;

                        commit.ParentShas.ForEach((parentSha, isFirst) =>
                        {
                            var parent = commitScans.TryGetValue(parentSha);
                            if (parent != null)
                            {
                                // 既知の親

                                // 浅くできそうであれば浅くする
                                var depth = isFirst ? firstDepth : commitScans.MaxDepth() + 1;
                                if (parent.Depth > depth)
                                    parent.Depth = depth;
                            }
                            else
                            {
                                // 未知の親

                                // 登録する
                                parent =
                                    new CommitScan
                                    {
                                        Depth = isFirst
                                            ? firstDepth
                                            : commitScans.MaxDepth() + 1
                                    };

                                commitScans.Add(parentSha, parent);
                            }

                            parent.Children.Add(commit.Sha);
                        });
                    }

                    // var current = commitScans.TryGetValue(commit.Sha);
                    // current?.Children.ForEach(c => commitScans.Remove(c));

                    commitScans.Remove(commit.Sha);
                });
        }

        private static int MaxDepth(this Dictionary<string /*Sha*/, CommitScan> source)
        {
            return source.Any() ? source.Values.Max(x => x.Depth) : -1;
        }

        private class CommitScan
        {
            public int Depth { get; set; }

            public List<string> Children { get; } = new List<string>();
        }
    }
}
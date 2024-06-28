using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ch.kainoo.platform
{

    public sealed class TreeNode<T>
    {
        public TreeNode(T value)
        {
            Value = value;
            ChildNodes = new List<TreeNode<T>>();
        }
        public TreeNode(T value, List<TreeNode<T>> childNodes)
        {
            Value = value;
            ChildNodes = childNodes;
        }

        public T Value { get; private set; }
        public List<TreeNode<T>> ChildNodes { get; private set; }

        /// <summary>
        /// Flatten the tree into a list using different strategies (cf. <see href="https://en.wikipedia.org/wiki/Tree_traversal"/>)
        /// <list type="bullet">
        /// <item>Top-down: we output an element to the list as soon as we see it and then we traverse its children (pre-order traversal)</item>
        /// <item>Bottom-up: we first traverse the children of an element and then output the element (post-order traversal)</item>
        /// </list>
        /// You also have the option to remove duplicates in case you are using this for a dependency tree.
        /// </summary>
        /// <param name="listMode">Which order do you want to order the elements: top-down, left-to-right, or bottom-up, left-to-right</param>
        /// <param name="removeDuplicates">Use this if you want to remove duplicates from the list. Be careful to override GetHashCode on your type as this implementation is dependent on HashSet&lt;<typeparamref name="T"/>&gt;.</param>
        /// <returns>An ordered list of the flattened tree</returns>
        public List<T> ToList(TreeNodeListMode listMode = TreeNodeListMode.TopDown, bool removeDuplicates = false)
        {
            if (removeDuplicates)
            {
                return ToEnumerableNoDuplicates(listMode)
                    .Select(n => n.Value)
                    .ToList();
            }
            else
            {
                return ToEnumerable(listMode)
                    .Select(n => n.Value)
                    .ToList();
            }
        }

        public IEnumerable<TreeNode<T>> ToEnumerable(TreeNodeListMode listMode = TreeNodeListMode.TopDown)
        {
            if (listMode == TreeNodeListMode.TopDown)
            {
                yield return this;
            }

            foreach (var childNode in ChildNodes)
            {
                var nodes = childNode.ToEnumerable();
                foreach (var n in nodes) yield return n;
            }

            if (listMode == TreeNodeListMode.BottomUp)
            {
                yield return this;
            }
        }

        public IEnumerable<TreeNode<T>> ToEnumerableNoDuplicates(TreeNodeListMode listMode = TreeNodeListMode.TopDown)
        {
            HashSet<T> seenValues = new HashSet<T>();

            var nodes = ToEnumerable(listMode);
            foreach (var n in nodes)
            {
                if (seenValues.Contains(n.Value))
                {
                    continue;
                }

                seenValues.Add(n.Value);
                yield return n;
            }

            yield break;
        }

        public void AddChild(T value)
        {
            AddChild(new TreeNode<T>(value));
        }
        public void AddChild(TreeNode<T> value)
        {
            ChildNodes.Add(value);
        }

    }

    public enum TreeNodeListMode
    {
        TopDown,
        BottomUp,
    }

}
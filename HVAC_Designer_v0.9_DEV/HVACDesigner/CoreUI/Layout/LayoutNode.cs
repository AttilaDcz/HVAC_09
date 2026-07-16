using System;
using System.Collections.Generic;

namespace HVACDesigner.CoreUI.Layout
{
    /// <summary>
    /// A Layout rendszer alap logikai csomópontja.
    /// Minden Host elem ebből a hierarchiából épül fel a memóriában.
    /// </summary>
    public class LayoutNode
    {
        public string Name { get; set; }
        public LayoutNode? Parent { get; private set; }
        public IReadOnlyList<LayoutNode> Children => _children;
        private readonly List<LayoutNode> _children = new List<LayoutNode>();

        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;

        public LayoutNode(string name)
        {
            Name = name;
        }

        public virtual void Add(LayoutNode child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            child.Parent = this;
            _children.Add(child);
        }

        public virtual void Remove(LayoutNode child)
        {
            if (_children.Remove(child))
            {
                child.Parent = null;
            }
        }

        public virtual void Clear()
        {
            foreach (var child in _children)
                child.Parent = null;

            _children.Clear();
        }

        public LayoutNode? Find(string name)
        {
            if (Name == name)
                return this;

            foreach (var child in _children)
            {
                var found = child.Find(name);
                if (found != null)
                    return found;
            }

            return null;
        }

        public IEnumerable<LayoutNode> Traverse()
        {
            yield return this;

            foreach (var child in _children)
            {
                foreach (var item in child.Traverse())
                    yield return item;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

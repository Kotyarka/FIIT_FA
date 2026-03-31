using System.Reflection;
using System.Runtime.CompilerServices;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new(key, value);
    }
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        fixInsertion(newNode);
    }

    private void fixInsertion(RbNode<TKey, TValue> t)
    {
        if (t.Parent == null)
        {
            t.Color = RbColor.Black;
            return;
        }
        while (t.Parent.Color == RbColor.Red)
        {
            RbNode<TKey, TValue>  parent = t.Parent;
            RbNode<TKey, TValue>  grandpa = t.Parent.Parent;
            if (grandpa == null)
            {
                return;
            }// Need some style changes
            if (parent == grandpa.Left)
            {
                RbNode<TKey, TValue>  uncle = grandpa.Right;
                if (uncle != null && uncle.Color == RbColor.Red)
                {
                    parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    grandpa.Color = RbColor.Red;
                    t = grandpa;
                }
                else
                {
                    if (t == parent.Right)
                    {
                        RotateLeft(parent);
                        t = parent;
                        parent = t.Parent;
                    }
                    RotateRight(grandpa);
                    (parent.Color, grandpa.Color) = (grandpa.Color, parent.Color);
                    return;
                }
            }
            else
            {
                RbNode<TKey, TValue>  uncle = grandpa.Left;
                if (uncle != null && uncle.Color == RbColor.Red)
                {                    
                    parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    grandpa.Color = RbColor.Red;
                    t = grandpa;
                }
                else
                {
                    if (t == parent.Left)
                    {
                        RotateRight(parent);
                        t = parent;
                        parent = t.Parent;
                    }
                    RotateLeft(grandpa);
                    (parent.Color, grandpa.Color) = (grandpa.Color, parent.Color);
                    return;
                }
            }
        }
        if (Root is RbNode<TKey, TValue> root)
        {
            root.Color = RbColor.Black;
        }
    }

    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        throw new NotImplementedException();
    }
}
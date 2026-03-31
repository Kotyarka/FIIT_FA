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
        while (t != null && t.Parent != null && t.Parent.Color == RbColor.Red)
        {
            RbNode<TKey, TValue>  parent = t.Parent;
            RbNode<TKey, TValue>  grandpa = t.Parent.Parent;
            if (grandpa == null)
            {
                return;
            }
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
        RbNode<TKey, TValue> node = child ?? parent;
        RbNode<TKey, TValue> nodeParent = parent;

        while (node != Root && GetColor(node) == RbColor.Black)
        {
            if (node == parent?.Left)
            {
                RbNode<TKey, TValue> sibling = parent.Right;
                
                if (GetColor(sibling) == RbColor.Red)
                {
                    sibling.Color = RbColor.Black;
                    parent.Color = RbColor.Red;
                    RotateLeft(parent);
                    sibling = parent.Right;
                }
                
                if (GetColor(sibling?.Left) == RbColor.Black && 
                    GetColor(sibling?.Right) == RbColor.Black)
                {
                    if (sibling != null)
                        sibling.Color = RbColor.Red;
                    node = parent;
                    parent = node?.Parent;
                }
                else
                {
                    if (GetColor(sibling?.Right) == RbColor.Black)
                    {
                        if (sibling?.Left != null)
                            sibling.Left.Color = RbColor.Black;
                        if (sibling != null)
                            sibling.Color = RbColor.Red;
                        RotateRight(sibling);
                        sibling = parent.Right;
                    }
                    
                    if (sibling != null)
                    {
                        sibling.Color = GetColor(parent);
                        parent.Color = RbColor.Black;
                        if (sibling.Right != null)
                            sibling.Right.Color = RbColor.Black;
                        RotateLeft(parent);
                    }
                    node = Root as RbNode<TKey, TValue>;
                    break;
                }
            }
            else
            {
                RbNode<TKey, TValue> sibling = parent?.Left;
                
                if (GetColor(sibling) == RbColor.Red)
                {
                    if (sibling != null)
                        sibling.Color = RbColor.Black;
                    if (parent != null)
                        parent.Color = RbColor.Red;
                    RotateRight(parent);
                    sibling = parent?.Left;
                }
                
                if (GetColor(sibling?.Left) == RbColor.Black && 
                    GetColor(sibling?.Right) == RbColor.Black)
                {
                    if (sibling != null)
                        sibling.Color = RbColor.Red;
                    node = parent;
                    parent = node?.Parent;
                }
                else
                {
                    if (GetColor(sibling?.Left) == RbColor.Black)
                    {
                        if (sibling?.Right != null)
                            sibling.Right.Color = RbColor.Black;
                        if (sibling != null)
                            sibling.Color = RbColor.Red;
                        RotateLeft(sibling);
                        sibling = parent?.Left;
                    }
                    
                    if (sibling != null)
                    {
                        sibling.Color = GetColor(parent);
                        if (parent != null)
                            parent.Color = RbColor.Black;
                        if (sibling.Left != null)
                            sibling.Left.Color = RbColor.Black;
                        RotateRight(parent);
                    }
                    node = Root as RbNode<TKey, TValue>;
                    break;
                }
            }
        }
        
        if (node != null)
            node.Color = RbColor.Black;
        else if (Root is RbNode<TKey, TValue> root)
            root.Color = RbColor.Black;
    }

    private static RbColor GetColor(RbNode<TKey, TValue>? node)
    {
        return node?.Color ?? RbColor.Black;
    }

}

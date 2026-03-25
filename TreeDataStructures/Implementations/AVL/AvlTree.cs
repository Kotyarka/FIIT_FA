using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        addParentHeight(newNode);
    }

    protected void addParentHeight(AvlNode<TKey, TValue> newNode)
    {
        AvlNode<TKey, TValue> node = newNode;
        while (node.Parent != null)
        {
            node.Parent.Height = MaxHeight(node.Parent.Left, node.Parent.Right) + 1;
            node = node.Parent;
        }
    }

    protected int MaxHeight(AvlNode<TKey, TValue> first, AvlNode<TKey, TValue> second)
    {
        if (first.Height > second.Height)
        {
            return first.Height;
        }
        return second.Height;
    }
    
}
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using TreeDataStructures.Core;
using TreeDataStructures.Implementations.Treap;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        AvlNode<TKey, TValue> node = newNode.Parent;
        setHeight(node);
        int balance = getBalance(node);
        if (balance > 1)
        {
            if (getBalance(node.Right) < 0)
            {
                RotateBigLeft(node);
            } else
            {
                RotateLeft(node);
            }
        } else if (balance < 1)
        {
            if (getBalance(node.Left) > 0)
            {
                RotateBigRight(node);
            } else
            {
                RotateRight(node);
            }
        }
    }

    protected int getBalance(AvlNode<TKey, TValue> node)
    {
        if (node == null)
        {
            return 0;
        }
        return node.Left.Height - node.Right.Height;
    }
    protected void setHeight(AvlNode<TKey, TValue> node)
    {
        if (node != null)
        {
            node.Height = MaxHeight(node.Left, node.Right) + 1;
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
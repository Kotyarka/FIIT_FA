using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    where TKey : IComparable<TKey>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (child != Root)
        {
            Splay(parent);
        }
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            Splay(node);
            return true;
        }
        value = default;
        return false;
    }

protected void Splay(BstNode<TKey, TValue>? node) 
{
    if (node == null) return;

    while (node.Parent != null)
    {
        var parent = node.Parent;
        var grandParent = parent.Parent; // added this two to fix warning
        
        if (node == parent.Left)
        {
            if (grandParent == null)
            {
                RotateRight(parent);
            }
            else if (parent == grandParent.Left)
            {
                RotateRight(grandParent);
                RotateRight(parent);
            }
            else
            {
                RotateRight(parent);
                RotateLeft(parent);
            }
        }
        else
        {
            if (grandParent == null)
            {
                RotateLeft(parent);
            }
            else if (parent == grandParent.Right)
            {
                RotateLeft(grandParent);
                RotateLeft(parent);
            }
            else
            {
                RotateLeft(parent);
                RotateRight(parent);
            }
        }
    }
}
}

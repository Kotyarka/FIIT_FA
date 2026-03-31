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

    public override bool ContainsKey(TKey key) { // почему это есть в тестах, а прототипа не было((
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node != null)
        {
            Splay(node);
            return true;
        }
        return false;
    }
    
    protected void Splay(BstNode<TKey, TValue>? node) 
    {
        if (node == null) return;

        while (node.Parent != null)
        {
            
            if (node == node.Parent.Left)
            {
                if (node.Parent.Parent == null)
                {
                    RotateRight(node.Parent);
                }
                else if (node.Parent == node.Parent.Parent.Left) // tbd implement isleftchild functions
                {
                    RotateRight(node.Parent.Parent);
                    RotateRight(node.Parent);
                }
                else
                {
                    RotateRight(node.Parent);
                    RotateLeft(node.Parent);
                }
            }
            else
            {
                if (node.Parent.Parent == null)
                {
                    RotateLeft(node.Parent);
                }
                else if (node.Parent == node.Parent.Parent.Right)
                {
                    RotateLeft(node.Parent.Parent);
                    RotateLeft(node.Parent);
                }
                else
                {
                    RotateLeft(node.Parent);
                    RotateRight(node.Parent);
                }
            }
        }
        
    }
}

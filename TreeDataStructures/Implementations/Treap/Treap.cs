using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
        {
            return (null, null);
        }
        
        if (Comparer.Compare(key, root.Key) < 0)
        {
            (TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right) = Split(root.Left, key);
            root.Left = right;
            return (left, root);
        }
        else
        {
            (TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right) = Split(root.Right, key);
            root.Right = left;
            return (root, right);
        }
    }
    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null)
        {
            return right;
        }
        if (right == null)
        {
            return left;
        }
        else if (left.Priority > right.Priority)
        {
            left.Right = Merge(left.Right, right);
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);
        }
        return right;
    }
    

    public override void Add(TKey key, TValue value)
    {
        TreapNode<TKey, TValue>? newNode = CreateNode(key, value);
        (TreapNode<TKey, TValue>? splittedTree1, TreapNode<TKey, TValue>? splittedTree2) = Split(Root, key);
        splittedTree1 =  Merge(splittedTree1, newNode);
        Root = Merge(splittedTree1, splittedTree2);
        Count++;
    }

    public override bool Remove(TKey key)
    {
        (TreapNode<TKey, TValue>? splittedTree1, TreapNode<TKey, TValue>? splittedTree2) = Split(Root, key);
        if (splittedTree2 == null)
        {
            return false;
        }
        TreapNode<TKey, TValue> node = splittedTree2;
        while (node.Left != null)
        {
            node = node.Left;
        }
       RemoveNode(node);
       Root = Merge(splittedTree1, splittedTree2);
       Count--; 
       return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
        Console.WriteLine($"Added one node with value {newNode.Value} and key {newNode.Key}");
    }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
        Console.WriteLine($"Removed one node");
    }
    
}
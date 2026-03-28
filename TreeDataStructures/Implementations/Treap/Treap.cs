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
        throw new NotImplementedException("Implement Add using Split and Merge");
    }

    public override bool Remove(TKey key)
    {
        throw new NotImplementedException("Implement Remove using Split and Merge");
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
        throw new NotImplementedException();
    }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
        throw new NotImplementedException();
    }
    
}
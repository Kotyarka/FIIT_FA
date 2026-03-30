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
        
        if (right != null)
        {
            right.Parent = root;
        }
        if (left != null)
        {
            left.Parent = null;
        }
        root.Parent = null;
        return (left, root);
    }
    else
    {
        (TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right) = Split(root.Right, key);
        root.Right = left;
        
        if (left != null)
        {
            left.Parent = root;
        }
        if (right != null)
        {
            right.Parent = null;
        }
        root.Parent = null;
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
        if (right != null)
        {
            right.Parent = null;
        }
        return right;
    }
    if (right == null)
    {
        if (left != null)
        {
            left.Parent = null;
        }
        return left;
    }
    
    if (left.Priority > right.Priority)
    {
        TreapNode<TKey, TValue>? merged = Merge(left.Right, right);
        left.Right = merged;
        if (merged != null)
        {
            merged.Parent = left;
        }
        left.Parent = null;
        return left;
    }
    else
    {
        TreapNode<TKey, TValue>? merged = Merge(left, right.Left);
        right.Left = merged;
        if (merged != null)
        {
            merged.Parent = right;
        }
        right.Parent = null;
        return right;
    }
}
    

    public override void Add(TKey key, TValue value)
    {
        TreapNode<TKey, TValue>? ifKeyExist = FindNode(key);
        if (ifKeyExist != null)
        {
            ifKeyExist.Value = value;
            return;
        }
        TreapNode<TKey, TValue>? newNode = CreateNode(key, value);
        (TreapNode<TKey, TValue>? splittedTree1, TreapNode<TKey, TValue>? splittedTree2) = Split(Root, key);
        splittedTree1 =  Merge(splittedTree1, newNode);
        Root = Merge(splittedTree1, splittedTree2);
        Count++;
    }

public override bool Remove(TKey key)
{
    TreapNode<TKey, TValue>? nodeToRemove = FindNode(key);
    if (nodeToRemove == null)
    {
        return false;
    }

    TreapNode<TKey, TValue>? mergedChildren = Merge(nodeToRemove.Left, nodeToRemove.Right);
    
    if (mergedChildren != null)
    {
        mergedChildren.Parent = nodeToRemove.Parent;
    }
    
    if (nodeToRemove.Parent == null)
    {
        Root = mergedChildren;
    }
    else
    {
        if (nodeToRemove.Parent.Left == nodeToRemove)
        {
            nodeToRemove.Parent.Left = mergedChildren;
        }
        else
        {
            nodeToRemove.Parent.Right = mergedChildren;
        }
    }
    
    Count--;
    OnNodeRemoved(nodeToRemove.Parent, nodeToRemove);
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
using System.ComponentModel;
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
    
private AvlNode<TKey, TValue>? doBalance(AvlNode<TKey, TValue> node)
    {
        setHeight(node);
        int balance = getBalance(node);
        if (balance > 1)
        {
            if (getBalance(node.Left) < 0)
            {
                node.Left = AVLLeftRotate(node.Left!);
            }
            return AVLRightRotate(node);
        } // безусловно, было бы здорово использовать сразу двойные повороты, но мне лень :( 
        if (balance < -1)
        { 
            if (getBalance(node.Right) > 0)
            {
                node.Right = AVLRightRotate(node.Right!);
            }
            return AVLLeftRotate(node);      
        }
        return node;
    }
protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
{
    AvlNode<TKey, TValue>? node = newNode.Parent;
    while (node != null)
    {
        var parent = node.Parent;
        var isLeft = parent?.Left == node; 
        AvlNode<TKey, TValue> balancedNode = doBalance(node);
        
        if (parent == null)
            Root = balancedNode;
        else if (isLeft)
            parent.Left = balancedNode;
        else
            parent.Right = balancedNode;
        
        node = parent;
    }
}

protected int getBalance(AvlNode<TKey, TValue>? node)
{
    if (node == null) return 0;
    
    int leftHeight = node.Left?.Height ?? 0;  
    int rightHeight = node.Right?.Height ?? 0;
    return leftHeight - rightHeight;
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
    int firstHeight = first?.Height ?? 0;
    int secondHeight = second?.Height ?? 0;
    return Math.Max(firstHeight, secondHeight);
}
    
protected override void OnNodeRemoved(AvlNode<TKey, TValue>? father, AvlNode<TKey, TValue>? child)
{
    AvlNode<TKey, TValue>? node = father ?? child;
    while (node != null)
    {
        var parent = node.Parent;
        var isLeft = parent?.Left == node;
        AvlNode<TKey, TValue> balancedNode = doBalance(node);
        
        if (parent == null)
            Root = balancedNode;
        else if (isLeft)
            parent.Left = balancedNode;
        else
            parent.Right = balancedNode;
        
        node = parent;
    }
}

protected AvlNode<TKey, TValue> AVLRightRotate(AvlNode<TKey, TValue> node)
    {
        RotateRight(node);
        setHeight(node);
        setHeight(node.Parent!);
        return node.Parent;
    }

protected AvlNode<TKey, TValue> AVLLeftRotate(AvlNode<TKey, TValue> node)
    {
        RotateLeft(node);
        setHeight(node);
        setHeight(node.Parent!);
        return node.Parent;
    }
}
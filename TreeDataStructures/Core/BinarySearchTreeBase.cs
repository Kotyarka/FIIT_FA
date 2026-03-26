using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null)
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }

    public bool IsReadOnly => false;

    public ICollection<TKey> Keys {
        get
        {
            var keys = new List<TKey>();
            foreach (var node in InOrder()){
                keys.Add(node.Key);
            }
            return keys;
        }
    }
    public ICollection<TValue> Values {
        get
        {
            var values = new List<TValue>();
            foreach (var node in InOrder()){
                values.Add(node.Value);
            }
            return values;
        }
    }


    public virtual void Add(TKey key, TValue value)
    {
        TNode? newNode = CreateNode(key, value);

        if (Root == null)
        {
            Root = newNode;
            Count++;
            OnNodeAdded(newNode);
            return;
        }

        TNode? current = Root;
        TNode? parent = null;
        int isBigger = 0;

        while (current != null)
        {
            parent = current;
            isBigger = Comparer.Compare(key, current.Key);

            if (isBigger == 0)
            {
                current.Value = value;
                return;
            }

            current = isBigger < 0 ? current.Left : current.Right;
        }

        newNode.Parent = parent;

        if (isBigger < 0)
            parent?.Left = newNode;
        else
            parent?.Right = newNode;

        Count++;
        OnNodeAdded(newNode);
        return;
    }


    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }

    protected virtual void RemoveNode(TNode? node)
    {
        ArgumentNullException.ThrowIfNull(node); 
        if (node.Left == null && node.Right == null)
        {
            Transplant(node, null);
        } else if (node.Left == null)
        {
           Transplant(node, node.Right);
        } else if (node.Right == null)
        {
            Transplant(node, node.Left);
        } else
        {
            TNode temp = node.Left;
            while (temp.Right != null)
            {
                temp = temp.Right;
            }

            if (temp.Parent != node)
            {
                Transplant(temp, temp.Left);
                temp.Left = node.Left;
                temp.Left.Parent = temp;
            }
            Transplant(node, temp);
            temp.Right = node.Right;
            temp.Right.Parent = temp;
        }
    }


public virtual bool ContainsKey(TKey key) => FindNode(key) != null;

public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
{
    TNode? node = FindNode(key);
    if (node != null)
    {
        value = node.Value;
        return true;
    }
    value = default;
    return false;
}

public TValue this[TKey key]
{
    get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
    set => Add(key, value);
}


#region Hooks

/// <summary>
/// Вызывается после успешной вставки
/// </summary>
/// <param name="newNode">Узел, который встал на место</param>
protected virtual void OnNodeAdded(TNode newNode) { }

/// <summary>
/// Вызывается после удаления. 
/// </summary>
/// <param name="parent">Узел, чей ребенок изменился</param>
/// <param name="child">Узел, который встал на место удаленного</param>
protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }

#endregion


#region Helpers
protected abstract TNode CreateNode(TKey key, TValue value);

private TNode FindMin(TNode node) /// its mine own helper!!!
{
    if (node == null)
        throw new ArgumentNullException(nameof(node));

    while (node.Left != null)
    {
        node = node.Left;
    }
    return node;
}

protected TNode? FindNode(TKey key)
{
    TNode? current = Root;
    while (current != null)
    {
        int cmp = Comparer.Compare(key, current.Key);
        if (cmp == 0) { return current; }
        current = cmp < 0 ? current.Left : current.Right;
    }
    return null;
}

protected void RotateLeft(TNode x)
{
    if (x.Right == null)
        throw new InvalidOperationException("Cannot rotate left: right child is null");

    TNode y = x.Right;

    x.Right = y.Left;
    if (y.Left != null)
        y.Left.Parent = x;

    y.Parent = x.Parent;
    if (x.Parent == null)
        Root = y;
    else if (x == x.Parent.Left)
        x.Parent.Left = y;
    else
        x.Parent.Right = y;

    y.Left = x;
    x.Parent = y;
}

protected void RotateRight(TNode y)
{
    if (y.Left == null)
        throw new InvalidOperationException("Cannot rotate right: left child is null");

    TNode x = y.Left;

    y.Left = x.Right;
    if (x.Right != null)
        x.Right.Parent = y;

    x.Parent = y.Parent;
    if (y.Parent == null)
        Root = x;
    else if (y == y.Parent.Left)
        y.Parent.Left = x;
    else
        y.Parent.Right = x;

    x.Right = y;
    y.Parent = x;
}

protected void RotateBigLeft(TNode x)
{

    if (x == null || x.Right == null)
        throw new InvalidOperationException("Cannot rotate big left: right child is null");
    RotateRight(x.Right);
    RotateLeft(x);
}

protected void RotateBigRight(TNode y)
{

    if (y == null || y.Left == null)
        throw new InvalidOperationException("Cannot rotate big right: left child is null");
    RotateLeft(y.Left);
    RotateRight(y);
}

protected void RotateDoubleLeft(TNode x)
{
    if (x == null || x.Right == null)
        throw new InvalidOperationException("Cannot rotate big left: right child is null");
    RotateLeft(x.Right);
    RotateLeft(x);
}

protected void RotateDoubleRight(TNode y)
{
    if (y == null || y.Left == null)
        throw new InvalidOperationException("Cannot rotate big right: left child is null");
    RotateRight(y.Left);
    RotateRight(y);
}

    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }

        if (v != null)
        {
            v.Parent = u.Parent;
        }
    }
    #endregion

public IEnumerable<TreeEntry<TKey, TValue>> InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
public IEnumerable<TreeEntry<TKey, TValue>> PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
public IEnumerable<TreeEntry<TKey, TValue>> PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder);
public IEnumerable<TreeEntry<TKey, TValue>> InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse);
public IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
public IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse);

/// <summary>
/// Внутренний класс-итератор. 
/// Реализует паттерн Iterator вручную, без yield return (ban).
/// </summary>
private struct TreeIterator :
    IEnumerable<TreeEntry<TKey, TValue>>,
    IEnumerator<TreeEntry<TKey, TValue>>
{
    private readonly TNode? _root;
    private readonly TraversalStrategy _strategy;
    private Stack<TNode>? _stack;
    private Stack<TNode>? _outputStack;
    private TNode? _current;

    public TreeIterator(TNode? root, TraversalStrategy strategy)
    {
        _root = root;
        _strategy = strategy;
        _stack = null;
        _outputStack = null;
        _current = null;
    }

    public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
    IEnumerator IEnumerable.GetEnumerator() => this;

    public TreeEntry<TKey, TValue> Current
    {
        get
        {
            if (_current == null)
                throw new InvalidOperationException("Enumeration not started or ended");
            int depth = 0;
            TNode cur = _current;
            while (cur.Parent != null)
            {
                depth++;
                cur = cur.Parent;
            }

            return new TreeEntry<TKey, TValue>(_current.Key, _current.Value, depth);
        }
    }
    object IEnumerator.Current => Current;


    public bool MoveNext()
    {
        if (_root == null)
            return false;

        if (_strategy == TraversalStrategy.InOrder)
        {
            if (_stack == null)
            {
                    _stack = new Stack<TNode>();
                    _current = _root;

                while (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Left;
                }

                if (_stack.Count > 0)
                {
                    _current = _stack.Pop();
                    return true;
                }

                _current = null;
                return false;
            }

            if (_current != null)
            {
                _current = _current.Right;
                while (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Left;
                }
            }

            if (_stack.Count > 0)
            {
                _current = _stack.Pop();
                return true;
            }

            _current = null;
            return false;
        }
        else if (_strategy == TraversalStrategy.PreOrder)
        {
            if (_stack == null)
            {
                _stack = _stack = new Stack<TNode>();

                    if (_root != null)
                    _stack.Push(_root);

                if (_stack.Count > 0)
                {
                    _current = _stack.Pop();

                    if (_current.Right != null)
                        _stack.Push(_current.Right);
                    if (_current.Left != null)
                        _stack.Push(_current.Left);

                    return true;
                }

                _current = null;
                return false;
            }

            if (_stack.Count > 0)
            {
                _current = _stack.Pop();

                if (_current.Right != null)
                    _stack.Push(_current.Right);
                if (_current.Left != null)
                    _stack.Push(_current.Left);

                return true;
            }

            _current = null;
            return false;
        }
        else if (_strategy == TraversalStrategy.PostOrder)
        {
            if (_stack == null)
            {
                    _stack = new Stack<TNode>();
                    _outputStack = new Stack<TNode>();

                if (_root != null)
                    _stack.Push(_root);

                while (_stack.Count > 0)
                {
                    var node = _stack.Pop();
                    _outputStack.Push(node);

                    if (node.Left != null)
                        _stack.Push(node.Left);
                    if (node.Right != null)
                        _stack.Push(node.Right);
                }

                if (_outputStack.Count > 0)
                {
                    _current = _outputStack.Pop();
                    return true;
                }

                _current = null;
                return false;
            }

            if (_outputStack?.Count > 0)
            {
                _current = _outputStack.Pop();
                return true;
            }

            _current = null;
            return false;
        }
        else if (_strategy == TraversalStrategy.InOrderReverse)

        /* во первых я насчет реверсов не уверен, имелось ли просто поменять местами лево и право, или вообще
         * все наоборот сделать, во вторых - код немного не соответствует DRY тому шо разница лишь в левом и правом ребенке
         * но вроде работает
         * 
         * 
         * надеюсь
         */
        {
            if (_stack == null)
            {
                 _stack = new Stack<TNode>();
                _current = _root;

                while (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Right;
                }

                if (_stack.Count > 0)
                {
                    _current = _stack.Pop();
                    return true;
                }

                _current = null;
                return false;
            }

            if (_current != null)
            {
                _current = _current.Left;

                while (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Right;
                }
            }

            if (_stack.Count > 0)
            {
                _current = _stack.Pop();
                return true;
            }

            _current = null;
            return false;
        }
        else if (_strategy == TraversalStrategy.PostOrderReverse)
        {
            if (_stack == null)
            {
                _stack = new Stack<TNode>();

                    if (_root != null)
                    _stack.Push(_root);
            }

            if (_stack.Count > 0)
            {
                _current = _stack.Pop();

                if (_current.Left != null)
                    _stack.Push(_current.Left);

                if (_current.Right != null)
                    _stack.Push(_current.Right);

                return true;
            }

            _current = null;
            return false;
        }
        else if (_strategy == TraversalStrategy.PreOrderReverse)
        {
            if (_stack == null)
            {
                _stack = new Stack<TNode>();
                    _outputStack = new Stack<TNode>();

                    if (_root != null)
                    _stack.Push(_root);

                while (_stack.Count > 0)
                {
                    var node = _stack.Pop();
                    _outputStack.Push(node);

                    if (node.Right != null)
                        _stack.Push(node.Right);
                    if (node.Left != null)
                        _stack.Push(node.Left);
                }

                if (_outputStack.Count > 0)
                {
                    _current = _outputStack.Pop();
                    return true;
                }

                _current = null;
                return false;
            }

            if (_outputStack?.Count > 0)
            {
                _current = _outputStack.Pop();
                return true;
            }

            _current = null;
            return false;
        }

        throw new NotImplementedException($"Strategy {_strategy} not implemented");
    }

    public void Reset()
    {
        _stack = null;
        _current = null;
    }

    public void Dispose()
    {
        _stack = null;
        _current = null;
    }
}


private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }

public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
{
    return new TreeKeyValueEnumerator(Root);
}

private struct TreeKeyValueEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
{
    private TreeIterator _iterator;
    private KeyValuePair<TKey, TValue> _current;

    public TreeKeyValueEnumerator(TNode? root)
    {
        _iterator = new TreeIterator(root, TraversalStrategy.InOrder);
        _current = default;
    }

    public KeyValuePair<TKey, TValue> Current => _current;
    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        if (_iterator.MoveNext())
        {
            var entry = _iterator.Current;
            _current = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            return true;
        }
        
        _current = default;
        return false;
    }

    public void Reset()
    {
        _iterator.Reset();
        _current = default;
    }

    public void Dispose()
    {
        _iterator.Dispose();
    }
}

IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
public void Clear() { Root = null; Count = 0; }
public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("Destination array is not long enough");

        foreach (var pair in this)
        {
            array[arrayIndex++] = pair;
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}
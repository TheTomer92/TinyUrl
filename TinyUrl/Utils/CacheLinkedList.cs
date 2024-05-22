namespace TinyUrl.Utils
{
    public class Node<T>
    {
        public required T Value { get; set; }
        public Node<T>? Prev { get; set; }
        public Node<T>? Next { get; set; }
    }

    public class CacheLinkedList<T>
    {
        private Node<T>? _head;
        private Node<T>? _tail;

        public void AddFirst(Node<T> node)
        {
            if (_head == null)
            {
                _head = _tail = node;
            }
            else
            {
                node.Next = _head;
                _head.Prev = node;
                _head = node;
            }
        }

        public void MoveToFront(Node<T> node)
        {
            if (node == _head) return;

            if (node.Prev != null) node.Prev.Next = node.Next;
            if (node.Next != null) node.Next.Prev = node.Prev;

            if (node == _tail) _tail = node.Prev;

            node.Prev = null;
            node.Next = _head;
            if (_head != null) _head.Prev = node;
            _head = node;
        }

        public Node<T>? RemoveLast()
        {
            if (_tail == null) return null;

            var lastNode = _tail;
            if (_tail.Prev != null) _tail.Prev.Next = null;
            else _head = null;

            _tail = _tail.Prev;
            return lastNode;
        }
    }
}
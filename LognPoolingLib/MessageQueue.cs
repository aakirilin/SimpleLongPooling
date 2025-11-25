using System.Collections;

namespace LognPoolingLib
{

    public class MessageQueue : ICollection<Message>
    {
        public string Name { get; private set; }

        private Queue<Message> messages;

        public DateTime CreateDT { get; private set; }         
        public MessageQueue(string name)
        {
            this.Name = name;
            this.messages = new Queue<Message>();
            this.CreateDT = DateTime.Now;
        }

        public int Count => messages.Count;

        public bool IsReadOnly => false;

        public void Enqueue(Message item)
        {
            messages.Enqueue(item);
        }

        public void Clear()
        {
            messages.Clear();
        }

        public bool Contains(Message item)
        {
            return messages.Contains(item);
        }

        public void CopyTo(Message[] array, int arrayIndex)
        {
            messages.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Message> GetEnumerator()
        {
            return messages.GetEnumerator();
        }

        public Message Dequeue()
        {
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }
            return default;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Message item)
        {
            Enqueue(item);
        }

        public bool Remove(Message item)
        {
            throw new NotImplementedException();
        }
    }
}

namespace LognPoolingLib
{
    public class MessageQueuePool
    {
        private static object locker = new object();
        private Dictionary<string, MessageQueue> messageQueues;
        private readonly MessageQueuePoolOptions options;

        public MessageQueuePool(MessageQueuePoolOptions options)
        {
            this.messageQueues = new Dictionary<string, MessageQueue>();
            this.options = options;
        }

        public MessageQueue this[string name]
        {
            get
            {
                lock (locker)
                {
                    Collect();
                    if (!messageQueues.ContainsKey(name))
                    {
                        messageQueues.Add(name, new MessageQueue(name));
                    }    
                        
                    return messageQueues[name];
                }
            }
        }

        private void Collect()
        {
            foreach (var messageQueue in messageQueues.Values)
            {
                if ((messageQueue.CreateDT + options.LiveTime) <= DateTime.Now)
                {
                    messageQueues.Remove(messageQueue.Name);
                }
            }
        }

        public event Action<Message> AddMessageEvent;
        public void Enqueue(Message message)
        {
            lock (locker)
            {
                this[message.User].Enqueue(message);
                AddMessageEvent?.Invoke(message);
            }
        }
        public Message Dequeue(string name)
        {
            lock (locker)
            {
                var result = this[name].Dequeue();

                if (this[name].Count == 0)
                {
                    messageQueues.Remove(name);
                }

                return result;
            }
        }
        public int Count(string name)
        {
            lock (locker)
            {
                return this[name].Count;
            }
        }
    }
}

namespace LognPoolingLib
{
    public class EventManager
    {
        public event Action<string> MessageDelivered;
        public void CallEvent(string message) => MessageDelivered?.Invoke(message);
    }
}

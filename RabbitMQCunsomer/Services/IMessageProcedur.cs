namespace Publisher.Abstract
{
    public interface IMessageProcedur
    {
        void SendMessage<T>(T message);
    }
}

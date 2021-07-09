namespace Vostok.Clusterclient.Core.Model
{
    public interface IContentConsumer
    {
        void Consume(byte[] src, int offset, int count);
    }
}
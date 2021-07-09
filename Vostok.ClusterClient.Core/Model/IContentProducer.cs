using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Clusterclient.Core.Model
{
    public interface IContentProducer
    {
        // note (patrofimov, 09.07.2021): как будто не нужна сигнатура с целым Stream. Если делать стрим, то по интерфейсу пользователю не будет ясно, как себя стрим поведет, можно ли читать, писать, искать и т. п.
        Task Produce(IContentConsumer contentConsumer, CancellationToken cancellationToken);
    }
}
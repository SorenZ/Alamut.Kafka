using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Alamut.Kafka.Contracts
{
    public interface IJObjectSubscriber
    {
        Task Handle(JObject message, CancellationToken token);
    }
}
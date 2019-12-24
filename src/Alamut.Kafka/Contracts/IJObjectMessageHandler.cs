using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Alamut.Kafka.Contracts
{
    public interface IJObjectMessageHandler
    {
        Task Handle(JObject message, CancellationToken token);
    }
}
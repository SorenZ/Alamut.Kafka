namespace Alamut.Kafka.Models
{
    public class KafkaConfig
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public string[] Topics { get; set; }
    }
}
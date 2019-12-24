namespace Alamut.Kafka.Models
{
    /// <summary>
    /// basic configuration for Kafka client
    /// </summary>
    public class KafkaConfig
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public string[] Topics { get; set; }
    }
}
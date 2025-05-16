using System.ComponentModel.DataAnnotations;

namespace todo_service;

public class Configuration
{
    [Required(AllowEmptyStrings = false)]
    public required string KafkaBootstrapServer { get; set; }
}
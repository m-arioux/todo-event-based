using System.ComponentModel.DataAnnotations;

namespace todo_service;

public record Todo
{
    [Required(AllowEmptyStrings = false)]
    public required string Description { get; set; }

    public Guid? Id { get; set; }
}

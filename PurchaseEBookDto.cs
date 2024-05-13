using System.ComponentModel.DataAnnotations;

public class PurchaseEBookDto
{
    public required int Id { get; set; }

    public required int Quantity { get; set; }

    public required int Pay { get; set; }

}
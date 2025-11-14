public class ItemTicket
{
    public string Descricao { get; set; } = "";

    private decimal quantidade;
    public decimal Quantidade
    {
        get => quantidade;
        set => quantidade = Math.Round(value, 3); // sempre 3 casas decimais
    }

    public decimal ValorUnitario { get; set; }

    public decimal PrecoUnitario { get; set; }

    public decimal ValorTotal { get; set; }

    public decimal Total => Quantidade * ValorUnitario;
}

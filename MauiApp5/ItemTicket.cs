public class ItemTicket
{
    public string Descricao { get; set; } = "";
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotal { get; set; }

    public decimal Total => Quantidade * ValorUnitario;
}

public class ComprovanteModel
{
    public string Tipo { get; set; }
    public int Numero { get; set; }
    public string Operador { get; set; }
    public string Data { get; set; }
    public string Hora { get; set; }
    public decimal ValorBruto { get; set; }
    public decimal Desconto { get; set; }
    public decimal ValorFinal { get; set; }
    public string FormaPagamento { get; set; }
    public string CodigoBarras { get; set; }
    public List<ItemTicket> Itens { get; set; }

}

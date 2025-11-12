#if ANDROID
using MauiApp5.Platforms.Android;
#endif
namespace MauiApp5
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
#if ANDROID
            // Criando o modelo da comanda
           var pedido = new ComprovanteModel
            {
                Tipo = "PEDIDO",
                Numero = 1234,
                Operador = "Luciano",
                Data = DateTime.Now.ToString("dd/MM/yyyy"),
                Hora = DateTime.Now.ToString("HH:mm"),
                ValorBruto = 172.00m,
                Desconto = 2.00m,
                ValorFinal = 170.00m,
                FormaPagamento = "DINHEIRO",
                Itens = new List<ItemTicket>
                {
                    new ItemTicket { Descricao = "X-BURGUER", Quantidade = 1, ValorUnitario = 12.00m },
                    new ItemTicket { Descricao = "REFRIGERANTE LATA", Quantidade = 2, ValorUnitario = 5.00m },
                    new ItemTicket { Descricao = "BATATA FRITA", Quantidade = 1, ValorUnitario = 150.00m }
                }
            };

            // Instancia o serviço e imprime
            var impressora = new ImpressoraService();
            impressora.ImprimirComprovante("JP80H_D483", pedido);
#else
            DisplayAlert("Aviso", "Impressão disponível apenas no Android", "OK");
#endif
        }
    }
}

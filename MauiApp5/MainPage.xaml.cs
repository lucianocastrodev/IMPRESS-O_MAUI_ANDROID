using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using MauiApp5.Services; // Certifique-se de que o namespace da interface está correto


namespace MauiApp5
{
    public partial class MainPage : ContentPage
    {
        // Variável de campo para armazenar a dependência injetada
        private readonly IImpressoraServiceCupom _impressoraService;

        // A CORREÇÃO: O serviço de impressão DEVE ser injetado no construtor
        public MainPage(IImpressoraServiceCupom impressoraService)
        {
            InitializeComponent();
            // Armazena a instância injetada
            _impressoraService = impressoraService;
        }

 
        private async void PrintTestBtn_Clicked(object sender, EventArgs e)
        {
            PrintTestBtn.IsEnabled = false;
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

            // Usando o serviço injetado
            _impressoraService.ImprimirComprovante("JP80H_D483", pedido);


            await Task.Delay(2000);
            PrintTestBtn.IsEnabled = true;
        }
    }
}

using Android.Bluetooth;
using Java.Util;
using System.Text;

namespace MauiApp5.Platforms.Android
{
    public class ImpressoraService
    {
        public void ImprimirComprovante(string nomeImpressora, ComprovanteModel pedido)
        {
            var adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null || !adapter.IsEnabled)
            {
                Console.WriteLine("Bluetooth não disponível.");
                return;
            }

            var device = adapter.BondedDevices.FirstOrDefault(r => r.Name == nomeImpressora);
            if (device == null)
            {
                Console.WriteLine("Impressora não encontrada.");
                return;
            }

            var uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
            using var sock = device.CreateRfcommSocketToServiceRecord(uuid);
            sock.Connect();

            var output = sock.OutputStream;

            // Reset
            byte[] init = new byte[] { 0x1B, 0x40 };
            output.Write(init, 0, init.Length);

            StringBuilder sb = new();

            // --- Funções auxiliares ---
            string Center(string text, int width = 48)
            {
                if (string.IsNullOrWhiteSpace(text)) return "";
                text = text.Trim();
                if (text.Length >= width) return text;
                int spaces = (width - text.Length) / 2;
                return new string(' ', spaces) + text;
            }

            string Line(int width = 47) => new string('-', width);

            string AlignRight(string left, string right, int width = 47)
            {
                int rightLength = right.Length;
                int leftLength = left.Length;
                int spaces = width - leftLength - rightLength;
                return left + new string(' ', spaces) + right;
            }

            // --- Cabeçalho ---
            sb.AppendLine(Center("LANCHONETE SABOR BOM"));
            sb.AppendLine(Center("CNPJ: 12.345.678/0001-90"));
            sb.AppendLine(Center("RUA DAS FLORES, 123 - CENTRO"));
            sb.AppendLine(Center("BOTUCATU - SP"));
            sb.AppendLine(Line());
            sb.AppendLine(Center($"{pedido.Tipo} Nº {pedido.Numero}"));
            sb.AppendLine($"OPERADOR: {pedido.Operador}");
            sb.AppendLine(AlignRight($"DATA: {pedido.Data}", $"HORA: {pedido.Hora}"));
            sb.AppendLine(Line());

            // --- Cabeçalho de Itens (Linha Única) ---
            string header = "ITEM".PadRight(20) + "QTD".PadRight(7) + "VLR. UNIT.".PadLeft(10) + "VLR. TOTAL".PadLeft(10);
            sb.AppendLine(header);
            sb.AppendLine(Line());

            // --- Itens ---
            foreach (var item in pedido.Itens)
            {
                string desc = item.Descricao.Length > 47 ? item.Descricao.Substring(0, 47) : item.Descricao;
                sb.AppendLine(desc);

                string item_qtd = item.Quantidade.ToString().PadRight(7);
                string item_vUnit = item.ValorUnitario.ToString("N2").PadLeft(10);
                string item_total = item.Total.ToString("N2").PadLeft(10);

                sb.AppendLine($"{new string(' ', 20)}{item_qtd}{item_vUnit}{item_total}");
                sb.AppendLine(Line());
            }

            // --- Totais ---
            sb.AppendLine(AlignRight("SUBTOTAL:", pedido.ValorBruto.ToString("N2")));
            sb.AppendLine(AlignRight("DESCONTO:", pedido.Desconto.ToString("N2")));

            // Negrito para o Total Final
            sb.Append(Encoding.UTF8.GetString(new byte[] { 0x1B, 0x45, 0x01 }));
            sb.AppendLine(AlignRight("TOTAL FINAL:", pedido.ValorFinal.ToString("N2")));
            sb.Append(Encoding.UTF8.GetString(new byte[] { 0x1B, 0x45, 0x00 }));

            sb.AppendLine(Line());
            sb.AppendLine($"FORMA PGTO: {pedido.FormaPagamento}");
            sb.AppendLine(Line());

            // --- Rodapé ---
            sb.AppendLine(Center("*** NÃO É DOCUMENTO FISCAL ***"));
            sb.AppendLine(Center("OBRIGADO PELA PREFERÊNCIA!"));
            sb.AppendLine(Center("www.lanchonetesaborbom.com.br"));
            sb.AppendLine(Center("(14) 99999-9999"));

            // --- Envio ---
            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            output.Write(data, 0, data.Length);

            // Alimenta e corta
            byte[] feed = new byte[] { 0x1B, 0x64, 0x05 };
            output.Write(feed, 0, feed.Length);

            byte[] cut = new byte[] { 0x1D, 0x56, 0x00 };
            output.Write(cut, 0, cut.Length);

            output.Flush();
            sock.Close();
        }
    }
}
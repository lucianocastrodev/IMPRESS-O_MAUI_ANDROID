using Android.Bluetooth;
using Java.Util;
using System.Text;
using MauiApp5.Services;

namespace MauiApp5.Platforms.Android
{
    public class ImpressoraServiceCupom: IImpressoraServiceCupom
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
            sb.AppendLine(AlignRight($"DATA: {pedido.Data}", $"HORA: {pedido.Hora}"));
            sb.AppendLine(Line());

            // --- Cabeçalho de Itens ---
            string header = "ITEM".PadRight(14) + "QTD".PadLeft(9) + "VLR.UNIT".PadLeft(12) + "VLR.TOTAL".PadLeft(12);
            sb.AppendLine(header);
            sb.AppendLine(Line());

            // --- Itens ---
            foreach (var item in pedido.Itens)
            {
                string desc = item.Descricao.Length > 47 ? item.Descricao.Substring(0, 47) : item.Descricao;
                sb.AppendLine(desc);

                // Quantidade com 3 casas + unidade
                string item_qtd = (item.Quantidade.ToString("N3") + " kg").PadLeft(9);
                string item_vUnit = ("R$ " + item.ValorUnitario.ToString("N2")).PadLeft(12);
                string item_total = ("R$ " + item.Total.ToString("N2")).PadLeft(12);

                sb.AppendLine($"{new string(' ', 14)}{item_qtd}{item_vUnit}{item_total}");
                sb.AppendLine(Line());
            }


            // Pula 2 linhas
            sb.AppendLine();
            sb.AppendLine();

            // --- Envio do texto ---
            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            output.Write(data, 0, data.Length);

            // --- Imprime código de barras/ Qrcode antes do corte ---
            PrintBarcode(output, "C"+pedido.Numero.ToString());

            // Alimenta e corta
            byte[] feed = new byte[] { 0x1B, 0x64, 0x04 };
            output.Write(feed, 0, feed.Length);

            byte[] cut = new byte[] { 0x1D, 0x56, 0x00 };
            output.Write(cut, 0, cut.Length);

            output.Flush();
            sock.Close();
        }

        private void PrintQRCode(System.IO.Stream output, string qrData)
        {
            // Centraliza QR Code
            byte[] center = new byte[] { 0x1B, 0x61, 0x01 };
            output.Write(center, 0, center.Length);

            // 1. Definir tamanho do módulo
            byte[] sizeCommand = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x06 };
            output.Write(sizeCommand, 0, sizeCommand.Length);

            // 2. Definir nível de correção de erro
            byte[] errorCommand = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x30 };
            output.Write(errorCommand, 0, errorCommand.Length);

            // 3. Armazenar dados do QR code
            byte[] qrBytes = Encoding.UTF8.GetBytes(qrData);
            int len = qrBytes.Length + 3;
            byte pL = (byte)(len % 256);
            byte pH = (byte)(len / 256);

            byte[] storeCommand = new byte[] { 0x1D, 0x28, 0x6B, pL, pH, 0x31, 0x50, 0x30 };
            output.Write(storeCommand, 0, storeCommand.Length);
            output.Write(qrBytes, 0, qrBytes.Length);

            // 4. Imprimir QR code
            byte[] printCommand = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 };
            output.Write(printCommand, 0, printCommand.Length);

            // Volta para alinhamento à esquerda
            byte[] left = new byte[] { 0x1B, 0x61, 0x00 };
            output.Write(left, 0, left.Length);
        }

        private void PrintBarcode(System.IO.Stream output, string data)
        {
            // Centraliza o código de barras
            byte[] center = new byte[] { 0x1B, 0x61, 0x01 };
            output.Write(center, 0, center.Length);

            // 1. Definir altura do código de barras (n=80)
            byte[] height = new byte[] { 0x1D, 0x68, 100 };
            output.Write(height, 0, height.Length);

            // 2. Definir largura das barras (n=2)
            byte[] width = new byte[] { 0x1D, 0x77, 2 };
            output.Write(width, 0, width.Length);

            // 3. Definir posição do texto (abaixo do código de barras)
            byte[] textPosition = new byte[] { 0x1D, 0x48, 0x02 };
            output.Write(textPosition, 0, textPosition.Length);

            // 4. Imprimir código de barras (Code 128)
            // Comando: GS k m n d1...dn
            // m=73 (Code 128), n=tamanho dos dados
            byte[] barcodeType = new byte[] { 0x1D, 0x6B, 0x49 }; // 0x49 = 73 (Code 128)
            output.Write(barcodeType, 0, barcodeType.Length);

            byte[] barcodeData = Encoding.UTF8.GetBytes(data);
            byte[] length = new byte[] { (byte)barcodeData.Length };
            output.Write(length, 0, length.Length);
            output.Write(barcodeData, 0, barcodeData.Length);

            // Volta para alinhamento à esquerda
            byte[] left = new byte[] { 0x1B, 0x61, 0x00 };
            output.Write(left, 0, left.Length);
        }





    }
}

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using System.IO;
using Dropbox.Api;
using System.IO.Compression;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Trafego
{
	class Program
	{

		/*GOOGLE SHEETS*/
		public static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
		public static readonly string ApplicationName = "Trafego";

		public readonly string SpreadsheetId = "1KPY4pYHvqjLIzbpnNU17qQbOVI6-IgZ_5vb_x042yB8";
		public static readonly string sheet = "Semaforos";
		public static SheetsService service;


		/*DROPBOX*/
		public static string token = "_5O_7p2LSwsAAAAAAAAAARRfRjcZR0LNLjNGzTteNjbQB3l1-GU_WHNsOKwi2xzf";

		[STAThread]

		static void Main(string[] args)
		{
			GoogleCredential credential;
			using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
			{
				credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
			}
			service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		public dynamic ReadEntries()
		{
			//Recebendo Tabela
			var distancia = $"{sheet}!A:C";

			var request = service.Spreadsheets.Values.Get(SpreadsheetId, distancia);

			var response = request.Execute();
			var valores = response.Values;
			return valores;
		}

		public static async Task DownloadImagem()
		{
			//Fazendo download das imagens
			using (var dbx = new DropboxClient(token))
			{
				using (var response = await dbx.Files.DownloadAsync("/sem1.jpg"))
				{
					var s = response.GetContentAsByteArrayAsync();
					s.Wait();
					var d = s.Result;
					File.WriteAllBytes(@"Imagens\sem1.jpg", d);
				}

				using (var response = await dbx.Files.DownloadAsync("/sem2.jpg"))
				{
					var s = response.GetContentAsByteArrayAsync();
					s.Wait();
					var d = s.Result;
					File.WriteAllBytes(@"Imagens\sem2.jpg", d);
				}
			}

			//Redimensionando o tamanho das imagens e salvando na pasta Imagens
			for (int i = 1; i < 3; i++)
			{
				Image Imagem = Image.FromFile(@"Imagens\sem" + i + ".jpg");
				var novaImagem = RedimensionarImagem(Imagem, 500, 300);
				novaImagem.Save(@"Imagens\sem" + i + ".jpeg", ImageFormat.Jpeg);
			}

		}

		//Função para Redimensionar
		public static Bitmap RedimensionarImagem(Image image, int width, int height)
		{
			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);

					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}
	}
}


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
			//Fazendo download dos zip's
			using (var dbx = new DropboxClient(token))
			{
				using (var response = await dbx.Files.DownloadAsync("/Detected1.zip"))
				{
					var s = response.GetContentAsByteArrayAsync();
					s.Wait();
					var d = s.Result;
					File.WriteAllBytes("Detected1.zip", d);
				}

				using (var response = await dbx.Files.DownloadAsync("/Detected2.zip"))
				{
					var s = response.GetContentAsByteArrayAsync();
					s.Wait();
					var d = s.Result;
					File.WriteAllBytes("Detected2.zip", d);
				}
			}
			//Extraindo ZIP na pasta Imagens
			ZipFile.ExtractToDirectory(@"Detected1.zip","Imagens");
			ZipFile.ExtractToDirectory(@"Detected2.zip", "Imagens2");

			//Redimensionando o tamanho das imagens e salvando na pasta Imagens
			for (int i = 1; i < 3; i++)
			{
				Image Imagem = Image.FromFile(@"Imagens2\" + "img-" + i + ".jpg");
				var novaImagem = redImagem(Imagem, 500, 300);
				novaImagem.Save(@"Imagens2\img-" + i + ".jpeg", ImageFormat.Jpeg);
			}

			for (int i = 1; i < 6; i++)
			{
				Image Imagem = Image.FromFile(@"Imagens\" + "img-" + i + ".jpg");
				var novaImagem = redImagem(Imagem, 500 ,300);
				novaImagem.Save(@"Imagens\img-" + i + ".jpeg", ImageFormat.Jpeg);
			}


		}

		//Função para Redimensionar
		public static Bitmap redImagem(Image image, int width, int height)
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


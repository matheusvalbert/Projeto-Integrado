using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Trafego
{
	public partial class Form1 : Form
	{
		internal Program teste = new Program();
		internal List<Dados> dados = new List<Dados>();
		public int aux = 0;
		public float[] tempoAberto, tempoFechado, tempoTotal;

		System.Windows.Forms.Timer temporizador = new System.Windows.Forms.Timer();

		public Form1()
		{
			InitializeComponent();

			//Intervalo de tempo - 5 seg
			temporizador.Interval = 5000;
			temporizador.Tick += new EventHandler(AtualizaDados);
			temporizador.Tick += new EventHandler(AtualizaImagem);
			temporizador.Start();

		}

		void AtualizaDados(object sender, EventArgs e)
		{
			var valores = teste.ReadEntries();
			int aux = 1, auxConjunto1 = 0, auxConjunto2 = 0;

			//Resetando dados
			dados.Clear();
			guna2DataGridView1.Rows.Clear();

			//Recebendo os valores e criando a tabela
			if (valores != null && valores.Count > 0)
			{
				foreach (var elemento in valores)
				{
					try
					{
						dados.Add(new Dados(aux, Int32.Parse(elemento[0]), Int32.Parse(elemento[1]), Int32.Parse(elemento[2])));
						guna2DataGridView1.Rows.Add(new object[] { aux, elemento[1], elemento[0], elemento[2] });
						aux++;
					}
					catch
					{
						Console.WriteLine("Erro na captura de dados");
					}
				}
			}

			//Atualizando o Painel da tela inicial
			foreach (var n in dados)
			{
				//Atualizando conjunto 1
				if (n.Semaforo == 1 && auxConjunto1 == 0)
				{
					labelValorAberto1.Text = Convert.ToString(n.TempoAberto);
					labelValorQuantidade1.Text = Convert.ToString(n.TotalVeiculos);
					auxConjunto1 = 1;
				}

				//Atualizando conjunto 2
				if (n.Semaforo == 2 && auxConjunto2 == 0)
				{
					labelValorAberto2.Text = Convert.ToString(n.TempoAberto);
					labelValorQuantidade2.Text = Convert.ToString(n.TotalVeiculos);
					auxConjunto2 = 1;
				}
			}

			//Sem dados conjunto 1
			if (auxConjunto1 == 0)
			{
				labelValorAberto1.Text = "0";
				labelValorQuantidade1.Text = "0";

			}
			//Sem dados conjunto 2
			if (auxConjunto2 == 0)
			{
				labelValorAberto2.Text = "0";
				labelValorQuantidade2.Text = "0";
			}
			
			GeraGrafico();
		}

		void AtualizaImagem(object sender, EventArgs e)
		{
			var task = Task.Run((Func<Task>)Program.DownloadImagem);

			try
			{
				//Criando copia
				Image im = GetCopyImage(@"Imagens\sem1.jpeg");
				Image im2 = GetCopyImage(@"Imagens\sem2.jpeg");

				//Carregando imagem
				guna2PictureBox1.Image = im;
				guna2PictureBox2.Image = im2;

			}
			catch
			{
				//Carregando gif
				guna2PictureBox1.ImageLocation = "loading.gif";
				guna2PictureBox2.ImageLocation = "loading.gif";
			}


			try
			{
				//Deletando arquivos e pastas
				Directory.Delete("Imagens", true);
				Directory.CreateDirectory("Imagens");

			}
			catch
			{
				Console.Write("Erro ao apagar arquivos");
			}

		}

		private void guna2ToggleSwitch1_CheckedChanged_1(object sender, EventArgs e)
		{
			if (aux == 0)
			{
				chart1.Visible = false;
				chart2.Visible = false;
				guna2DataGridView1.Visible = true;
				aux = 1;
			}
			else
			{
				chart1.Visible = true;
				chart2.Visible = true;
				guna2DataGridView1.Visible = false;
				aux = 0;
			}
		}

		public void GeraGrafico()
		{
			int aux1 = 0;
			int aux2 = 0;

			//Apagando grafico
			foreach (var series in chart1.Series)
			{
				series.Points.Clear();
			}
			foreach (var series in chart2.Series)
			{
				series.Points.Clear();
			}

			//Escrevendo dados
			try
			{
				foreach(var n in dados)
				{
					if (aux1 < 10 && n.Semaforo == 1)
					{
						this.chart1.Series["Tempo Aberto"].Points.AddXY("", n.TempoAberto);
						this.chart1.Series["Quant. veículos"].Points.AddXY("", n.TotalVeiculos);
						aux1++;
					}

					if (aux2 < 10 && n.Semaforo == 2)
					{
						this.chart2.Series["Tempo Aberto"].Points.AddXY("", n.TempoAberto);
						this.chart2.Series["Quant. veículos"].Points.AddXY("", n.TotalVeiculos);
						aux2++;
					}
				}
			}
			catch
			{
				Console.Write("Ocorreu um erro nos gráficos");
			}
		}

		private static Image GetCopyImage(string path)
		{
			using (Image im = Image.FromFile(path))
			{
				Bitmap bm = new Bitmap(im);
				return bm;
			}
		}
	}
}

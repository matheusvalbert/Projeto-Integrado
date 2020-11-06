using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.IO;

namespace Trafego
{
	public partial class Form1 : Form
	{
		internal Program teste = new Program();
		internal List<Dados> dados = new List<Dados>();
		public float[] tempoAberto, tempoFechado, tempoTotal;

		System.Windows.Forms.Timer temporizador = new System.Windows.Forms.Timer();

		public Form1()
		{
			InitializeComponent();

			//Intervalo de tempo - 5 seg
			temporizador.Interval = 5000;
			temporizador.Tick += new EventHandler(atualizaDados);
			temporizador.Tick += new EventHandler(atualizaImagem);
			temporizador.Start();

		}

		void atualizaDados(object sender, EventArgs e)
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
				if (n.Conjunto == 1 && auxConjunto1 == 0)
				{
					labelValorAberto1.Text = Convert.ToString(n.TempoAberto);
					labelValorQuantidade1.Text = Convert.ToString(n.TotalVeiculos);

					labelValorAberto2.Text = Convert.ToString(n.TempoAberto);
					labelValorQuantidade2.Text = Convert.ToString(n.TotalVeiculos);

					labelValorAberto3.Text = Convert.ToString(n.TempoAberto);
					labelValorQuantidade3.Text = Convert.ToString(n.TotalVeiculos);
					auxConjunto1 = 1;
				}

				//Atualizando conjunto 2
				if (n.Conjunto == 2 && auxConjunto2 == 0)
				{
					labelValorAberto4.Text = Convert.ToString(n.TempoAberto);
					labelValorQuantidade4.Text = Convert.ToString(n.TotalVeiculos);

					labelValorAberto5.Text = Convert.ToString(n.TempoAberto);
					labelValorQuantidade5.Text = Convert.ToString(n.TotalVeiculos);
					auxConjunto2 = 1;
				}
			}

			//Sem dados conjunto 1
			if (auxConjunto1 == 0)
			{
				labelValorAberto1.Text = "0";
				labelValorQuantidade1.Text = "0";

				labelValorAberto2.Text = "0";
				labelValorQuantidade2.Text = "0";

				labelValorAberto3.Text = "0";
				labelValorQuantidade3.Text = "0";
			}
			//Sem dados conjunto 2
			if (auxConjunto2 == 0)
			{
				labelValorAberto4.Text = "0";
				labelValorQuantidade4.Text = "0";

				labelValorAberto5.Text = "0";
				labelValorQuantidade5.Text = "0";
			}
			
			geraGrafico();
		}

		void atualizaImagem(object sender, EventArgs e)
		{
			var task = Task.Run((Func<Task>)Program.DownloadImagem);

			try
			{
				//Criando copia
				Image im = GetCopyImage(@"Imagens\img-1.jpeg");
				Image im2 = GetCopyImage(@"Imagens\img-2.jpeg");
				Image im3 = GetCopyImage(@"Imagens\img-3.jpeg");
				Image im4 = GetCopyImage(@"Imagens2\img-1.jpeg");
				Image im5 = GetCopyImage(@"Imagens2\img-2.jpeg");

				//Carregando imagem
				guna2PictureBox1.Image = im;
				guna2PictureBox2.Image = im2;
				guna2PictureBox3.Image = im3;
				guna2PictureBox4.Image = im4;
				guna2PictureBox5.Image = im5;
			}
			catch
			{
				//Carregando gif
				guna2PictureBox1.ImageLocation = "loading.gif";
				guna2PictureBox2.ImageLocation = "loading.gif";
				guna2PictureBox3.ImageLocation = "loading.gif";
				guna2PictureBox4.ImageLocation = "loading.gif";
				guna2PictureBox5.ImageLocation = "loading.gif";
			}


			try
			{
				//Deletando arquivos e pastas
				File.Delete("Detected1.zip");
				File.Delete("Detected2.zip");
				Directory.Delete("Imagens", true);
				Directory.Delete("Imagens2", true);

			}
			catch
			{
				Console.Write("Erro ao apagar arquivos");
			}

		}

		public void geraGrafico()
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
					if (aux1 < 10 && n.Conjunto == 1)
					{
						this.chart1.Series["Tempo Aberto"].Points.AddXY("", n.TempoAberto);
						this.chart1.Series["Máx. veículos"].Points.AddXY("", n.TotalVeiculos);
						aux1++;
					}

					if (aux2 < 10 && n.Conjunto == 2)
					{
						this.chart2.Series["Tempo Aberto"].Points.AddXY("", n.TempoAberto);
						this.chart2.Series["Máx. veículos"].Points.AddXY("", n.TotalVeiculos);
						aux2++;
					}
				}
			}
			catch
			{
				Console.Write("Ocorreu um erro nos gráficos");
			}
		}

		private void ButtonEstatistica_Click(object sender, EventArgs e)
		{
			trocaTela(0);
		}

		private void ButtonCâmeras_Click(object sender, EventArgs e)
		{
			trocaTela(1);
		}

		public void trocaTela(int tela)
		{
			Boolean aux = true;
			if (tela == 0)
			{
				aux = false;
			}

			ButtonCâmeras.Enabled = !aux;
			ButtonEstatistica.Enabled = aux;

			//Semaforo
			label1.Visible = aux;
			label2.Visible = aux;
			label3.Visible = aux;
			label4.Visible = aux;
			label5.Visible = aux;

			//Imagens
			guna2PictureBox1.Visible = aux;
			guna2PictureBox2.Visible = aux;
			guna2PictureBox3.Visible = aux;
			guna2PictureBox4.Visible = aux;
			guna2PictureBox5.Visible = aux;
			
			//Panels
			guna2ShadowPanel1.Visible = aux;
			guna2ShadowPanel2.Visible = aux;
			guna2ShadowPanel3.Visible = aux;
			guna2ShadowPanel4.Visible = aux;
			guna2ShadowPanel5.Visible = aux;
			guna2ShadowPanel6.Visible = aux;
			guna2ShadowPanel7.Visible = aux;
			guna2ShadowPanel8.Visible = aux;
			guna2ShadowPanel9.Visible = aux;
			guna2ShadowPanel10.Visible = aux;

			//Gráficos
			chart1.Visible = !aux;
			chart2.Visible = !aux;

			//Tabela
			guna2DataGridView1.Visible = !aux;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trafego
{
	class Dados
	{
		public int Indice;
		public int TempoAberto;
		public int Semaforo;
		public int TotalVeiculos;

		public Dados(int indice, int tempoaberto, int sem, int veicTotal)
		{
			this.Indice = indice;
			this.TempoAberto = tempoaberto;
			this.Semaforo = sem;
			this.TotalVeiculos = veicTotal;
		}
	}

}

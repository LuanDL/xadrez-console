﻿using System;
using tabuleiro;
using xadrez;

namespace xadrez_console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                PartidaDeXadrez partida = new PartidaDeXadrez();
             
                Tela.ImprimirTabuleiro(partida.tab);


            }
            catch (TabuleiroException e)
            {
                Console.WriteLine(e.Message);
            }
                Console.ReadLine();
        }
    }
}

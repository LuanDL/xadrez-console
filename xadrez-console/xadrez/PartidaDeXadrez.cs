﻿using System.Collections.Generic;
using tabuleiro;


namespace xadrez
{
    class PartidaDeXadrez
    {

        public Tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set; }

        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }

        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Branca;
            terminada = false;
            xeque = false;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            ColocarPecas();
        }

        public Peca ExecutaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.IncrementarQteMovimentos();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if (pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }
            // #jogadaespecial roque pequeno
            if (p is Rei && destino.coluna == origem.coluna + 2) 
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(origemT);
                T.IncrementarQteMovimentos();
                tab.colocarPeca(T, destinoT);
            }
            // #jogadaespecial roque grande
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna - 1);
                Peca T = tab.retirarPeca(origemT);
                T.IncrementarQteMovimentos();
                tab.colocarPeca(T, destinoT);
            }

            return pecaCapturada;
        }

        public void DesfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.retirarPeca(destino);
            p.DecrementarQteMovimentos();
            if (pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            tab.colocarPeca(p, origem);

            // #jogadaespecial roque pequeno
            if (p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(destinoT);
                T.IncrementarQteMovimentos();
                tab.colocarPeca(T, origemT);
            }
            // #jogadaespecial roque grande
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna - 1);
                Peca T = tab.retirarPeca(destinoT);
                T.IncrementarQteMovimentos();
                tab.colocarPeca(T, origemT);
            }

        }

        public void RealizaJogada(Posicao origem, Posicao destino)
        {
            Peca PecaCapturada = ExecutaMovimento(origem, destino);

            if (EstaEmXeque(jogadorAtual))
            {
                DesfazMovimento(origem, destino, PecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque!");
            }

            if (EstaEmXeque(Adversaria(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }
            if (testeXequeMate(Adversaria(jogadorAtual)))
            {
                terminada = true;
            }
            else
            {
                turno++;
                MudaJogador();
            }
        }

        public void ValidarPosicaoDeOrigem(Posicao pos)
        {
            if (tab.peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição de origem escolhida!");
            }
            if (jogadorAtual != tab.peca(pos).cor)
            {
                throw new TabuleiroException("A peça de origem escolhida não é sua!");
            }
            if (!tab.peca(pos).ExisteMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
            }
        }

        public void ValidarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!tab.peca(origem).MovimentoPossivel(destino))
            {
                throw new TabuleiroException("Posição de destino inválida!");
            }
        }

        private void MudaJogador()
        {
            if (jogadorAtual == Cor.Branca)
            {
                jogadorAtual = Cor.Preta;
            }
            else
            {
                jogadorAtual = Cor.Branca;
            }
        }

        public HashSet<Peca> PecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in capturadas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> PecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(PecasCapturadas(cor));
            return aux;
        }

        private Cor Adversaria(Cor cor)
        {
            if (cor == Cor.Branca)
            {
                return Cor.Preta;
            }
            else
            {
                return Cor.Branca;
            }
        }

        private Peca Rei(Cor cor)
        {
            foreach (Peca x in PecasEmJogo(cor))
            {
                if (x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        public bool EstaEmXeque(Cor cor)
        {
            Peca R = Rei(cor);
            if (R == null)
            {
                throw new TabuleiroException("Não tem rei da cor" + cor + "no tabuleiro!");
            }

            foreach (Peca x in PecasEmJogo(Adversaria(cor)))
            {
                bool[,] mat = x.MovimentosPossiveis();
                if (mat[R.posicao.linha, R.posicao.coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool testeXequeMate(Cor cor)
        {
            if (!EstaEmXeque(cor))
            {
                return false;
            }
            foreach (Peca x in PecasEmJogo(cor))
            {
                bool[,] mat = x.MovimentosPossiveis();
                for (int i = 0; i < tab.linhas; i++)
                {
                    for (int j = 0; j < tab.colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturda = ExecutaMovimento(origem, destino);
                            bool testeXeque = EstaEmXeque(cor);
                            DesfazMovimento(origem, destino, pecaCapturda);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }


        public void ColocarNovaPeca(char coluna, int linha, Peca peca)
        {
            tab.colocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            pecas.Add(peca);
        }

        private void ColocarPecas()
        {
            ColocarNovaPeca('a', 1, new Torre(tab, Cor.Branca));
            ColocarNovaPeca('b', 1, new Cavalo(tab, Cor.Branca));
            ColocarNovaPeca('c', 1, new Bispo(tab, Cor.Branca));
            ColocarNovaPeca('d', 1, new Dama(tab, Cor.Branca));
            ColocarNovaPeca('e', 1, new Rei(tab, Cor.Branca, this));
            ColocarNovaPeca('f', 1, new Bispo(tab, Cor.Branca));
            ColocarNovaPeca('g', 1, new Cavalo(tab, Cor.Branca));
            ColocarNovaPeca('h', 1, new Torre(tab, Cor.Branca));
            ColocarNovaPeca('a', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('b', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('c', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('d', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('e', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('f', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('g', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('h', 2, new Peao(tab, Cor.Branca));



            ColocarNovaPeca('a', 8, new Torre(tab, Cor.Preta));
            ColocarNovaPeca('b', 8, new Cavalo(tab, Cor.Preta));
            ColocarNovaPeca('c', 8, new Bispo(tab, Cor.Preta));
            ColocarNovaPeca('d', 8, new Dama(tab, Cor.Preta));
            ColocarNovaPeca('e', 8, new Rei(tab, Cor.Preta, this));
            ColocarNovaPeca('f', 8, new Bispo(tab, Cor.Preta));
            ColocarNovaPeca('g', 8, new Cavalo(tab, Cor.Preta));
            ColocarNovaPeca('h', 8, new Torre(tab, Cor.Preta));
            ColocarNovaPeca('a', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('b', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('c', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('d', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('e', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('f', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('g', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('h', 7, new Peao(tab, Cor.Preta));
        }
    }
}
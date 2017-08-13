using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Speech.Recognition; //biblioteca usada para o reconhecimento
using System.Threading;
using System.Media; //biblioteca usada para reprodução de audios
//TESTE DO PODER DO GIT HUB
namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        SoundPlayer explosao = new SoundPlayer(@"c:\imagem\explosao.wav"); //explosão final de game over
        SoundPlayer desvio = new SoundPlayer(@"c:\imagem\desvio.wav"); //desvio de nave
        SoundPlayer esquerda = new SoundPlayer(@"c:\imagem\esquerda.wav"); //tiro vindo da esquerda
        SoundPlayer direita = new SoundPlayer(@"c:\imagem\direita.wav"); //tiro vindo da direita
        SoundPlayer impesq = new SoundPlayer(@"c:\imagem\impdir.wav"); //impacto na esquerda
        SoundPlayer impdir = new SoundPlayer(@"c:\imagem\imesq.wav");// impacto na direita
        int vida; //do jogador
        int tempo; //intervalo de reação
        int n;//variável que armazena numero aleatório para ataque
        int t;//variável de segurança, usada na verificação da sirene
        int l;// variável de segurança de dado, sempre recebe valor de 'n'
        private static SpeechRecognitionEngine engine;
        public Form1()
        {
            InitializeComponent();
        }
        private void LoadSpeech()
        {
                engine = new SpeechRecognitionEngine(); //caso haja erro nesta linha o problema é com os pacotes, verificar também a versão do sistema: 32 ou 64 bits
                engine.SetInputToDefaultAudioDevice(); //caso haja erro nesta linha significa que o programa não conseguiu localizar um microfone para ser utilizado no reconhecimento

                string[] words = { "esquerda", "direita" };//gramática, novas palavras a serem reconhecidas devem ser introduzidas aqui

                engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words))));//carrega gramática
                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);
                engine.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.URL = @"c:\imagem\fundo.wav"; //som de fundo
            axWindowsMediaPlayer1.settings.playCount = 1000; //quantidade de repetição do audio
            LoadSpeech(); //inicia o reconhecimento
        }

        private void rec(object s, SpeechRecognizedEventArgs e)
        {
            if (vida < 5)
            {
                tempo = 0;//seta a variável de tempo
                timer1.Stop();//para o timer
                switch (e.Result.Text) //variável que possui o resultado obtido no reconhecimento
                {
                    case "esquerda":
                        if (l == 0)
                        {
                            impesq.Play();
                            vida++;
                            sirene();
                            verifica();
                        }
                        else
                        {
                            desvio.Play();
                            ataque();
                        }
                        break;
                    case "direita":
                        if (l == 1)
                        {
                            impdir.Play();
                            vida++;
                            sirene();
                            verifica();
                        }
                        else
                        {
                            desvio.Play();
                            ataque();
                        }
                        break;
                       
                }
            }
            
        }
        private void ataque()//ataque da nave inimiga
        {
               
                Random randNum = new Random();
                n = randNum.Next(2); //gera numero aleatório
                tempo = 0;//seta a variável de tempo
                //n = 0; //linha para teste (tira a aleatoriedade, sempre será esquerda)

                Thread.Sleep(1500);//intervalo para carregar audio
                if (n == 0)
                {
                    esquerda.Play();
                    timer1.Start(); //inicia o timer
                    l = n;
                }

                if (n == 1)
                {
                    direita.Play();
                    timer1.Start(); //inicia o timer
                    l = n;
                }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ataque();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tempo++;//adiciona 1 a cada 1 segundo a variavel tempo
            if (tempo == 4)//tempo de reação (do disparo até o impacto)
            {
                if (l == 0)
                {
                    impesq.Play();
                    vida++;
                }
                else
                {
                    impdir.Play();
                    vida++;
                }
                sirene();
                verifica();
            }
        }
        private void sirene()//ativa sons de sirene depois de três impactos
        {
            if (vida == 3 && t != 1)
            {
                t = 1;//impede reprodução repitida
                axWindowsMediaPlayer2.URL = @"c:\imagem\sirene.mp4";
                axWindowsMediaPlayer2.settings.playCount = 2000;
            }
        }
        private void verifica()//verifica se jogador perdeu, caso contrário chama ataque() novamente
        {
            if (vida < 5)//para alterar quantidade de vidas do jogador modifique esta linha
            {
                ataque();
            }
            else //game over
            {
                vida = 6;
                axWindowsMediaPlayer2.Ctlcontrols.stop();
                axWindowsMediaPlayer1.Ctlcontrols.stop();
                explosao.Play();
                axWindowsMediaPlayer2.settings.playCount = 0;
                axWindowsMediaPlayer2.URL = @"c:\imagem\fimdejogo.mp3";
                axWindowsMediaPlayer2.settings.volume = 2000;
            }
        }
    }
}

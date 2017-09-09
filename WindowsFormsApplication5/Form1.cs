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
namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        SoundPlayer troca = new SoundPlayer(@"c:\imagem\audios\troca.wav");
        SoundPlayer beep_direita = new SoundPlayer(@"c:\imagem\audios\beep_direita.wav");
        SoundPlayer explosao = new SoundPlayer(@"c:\imagem\audios\explosao.wav"); //explosão final de game over
        SoundPlayer desvio = new SoundPlayer(@"c:\imagem\audios\desvio.wav"); //desvio de nave
        SoundPlayer esquerda = new SoundPlayer(@"c:\imagem\audios\esquerda.wav"); //tiro vindo da esquerda
        SoundPlayer direita = new SoundPlayer(@"c:\imagem\audios\direita.wav"); //tiro vindo da direita
        SoundPlayer impesq = new SoundPlayer(@"c:\imagem\audios\impdir.wav"); //impacto na esquerda
        SoundPlayer impdir = new SoundPlayer(@"c:\imagem\audios\imesq.wav");// impacto na direita
        int vida; //do jogador
        int tempo; //intervalo de reação
        int n;//variável que armazena numero aleatório para ataque
        int t;//variável de segurança, usada na verificação da sirene
        int l;// variável de segurança de dado, sempre recebe valor de 'n'
        int minigameid;//armazena id correspondente ao minigame, utilizada no reconhecimento

        //NOTA: Sempre que relacionar um novo número a um minigame anotar aqui:
        // 111 morto
        //-2 inicio
        // -1 configuração
        // 0 falas aleatórias
        // 1 desvio de tiros

        private static SpeechRecognitionEngine engine;
        public Form1()
        {
            InitializeComponent();
        }
        private void LoadSpeech()
        {
                engine = new SpeechRecognitionEngine(); //caso haja erro nesta linha o problema é com os pacotes, verificar também a versão do sistema: 32 ou 64 bits
                engine.SetInputToDefaultAudioDevice(); //caso haja erro nesta linha significa que o programa não conseguiu localizar um microfone para ser utilizado no reconhecimento

                string[] words = {"iniciar", "esquerda", "direita" };//gramática, novas palavras a serem reconhecidas devem ser introduzidas aqui

                engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words))));//carrega gramática
                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);
                engine.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.URL = @"c:\imagem\audios\fundo.wav"; //som de fundo
            axWindowsMediaPlayer1.settings.playCount = 1000; //quantidade de repetição do audio
            axWindowsMediaPlayer1.settings.volume = 30;
            minigameid = -2;
            LoadSpeech(); //inicia o reconhecimento
        }

        private void rec(object s, SpeechRecognizedEventArgs e)
        {
            switch (minigameid)//limitação de palavras da gramática baseado no id de cada caso
            {
                case -2:
                    switch (e.Result.Text)
                    {
                        case "iniciar":
                            inicio();
                            minigameid = 111;
                            break;
                    }
                break;
                case -1:
                    switch (e.Result.Text)
                    {
                        case "esquerda":
                            troca.Play();
                        break;
                    }
                break;
                case 1:
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
                    break;
            }
            
        }
        private void inicio()
        {
            axWindowsMediaPlayer2.URL = @"c:\imagem\audios\Carregamento.wav";
            axWindowsMediaPlayer2.settings.volume = 2000;
            pictureBox2.Image = Image.FromFile(@"c:\imagem\gif\igo.gif");
        }
        private void config()
        {
            minigameid = -1; //id
            beep_direita.Play();
        }
        private void ataque()//ataque da nave inimiga
        {
                minigameid = 1;
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
                pictureBox2.Image = Image.FromFile(@"c:\imagem\gif\sirene.gif");
                axWindowsMediaPlayer2.URL = @"c:\imagem\audios\sirene.mp4";
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
                axWindowsMediaPlayer2.URL = @"c:\imagem\audios\fimdejogo.mp3";
                pictureBox2.Image = Image.FromFile(@"c:\imagem\gif\gameover.jpg");
                axWindowsMediaPlayer2.settings.volume = 2000;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Speech.Recognition;
using System.Threading;
using System.Media;

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        SoundPlayer explosao = new SoundPlayer(@"c:\imagem\explosao.wav");
        SoundPlayer desvio = new SoundPlayer(@"c:\imagem\desvio.wav");
        SoundPlayer esquerda = new SoundPlayer(@"c:\imagem\esquerda.wav");
        SoundPlayer direita = new SoundPlayer(@"c:\imagem\direita.wav");
        SoundPlayer impesq = new SoundPlayer(@"c:\imagem\impdir.wav");
        SoundPlayer impdir = new SoundPlayer(@"c:\imagem\imesq.wav");
        int vida;
        int tempo;
        int n;
        int t;
        int l;
        private static SpeechRecognitionEngine engine;
        public Form1()
        {
            InitializeComponent();
        }
        private void LoadSpeech()
        {
            try
            {
                engine = new SpeechRecognitionEngine();
                engine.SetInputToDefaultAudioDevice();

                string[] words = { "esquerda", "direita" };

                engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words))));
                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);
                engine.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch(Exception ex)
            {
                MessageBox.Show("ocorreu um erro: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.URL = @"c:\imagem\fundo.wav";
            axWindowsMediaPlayer1.settings.playCount = 1000;
            LoadSpeech();
        }

        private void rec(object s, SpeechRecognizedEventArgs e)
        {
            if (vida < 5)
            {
                tempo = 0;//seta a variável de tempo
                timer1.Stop();
                switch (e.Result.Text)
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
        private void ataque()
        {
               
                Random randNum = new Random();
                n = randNum.Next(2); //gera numero aleatório
                tempo = 0;//seta a variável de tempo
                //n = 0; //linha para teste (tira a aleatoriedade, sempre será esquerda)

                Thread.Sleep(1500);
                if (n == 0)
                {
                    esquerda.Play();
                    timer1.Start();
                    l = n;
                }

                if (n == 1)
                {
                    direita.Play();
                    timer1.Start();
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
        private void sirene()
        {
            if (vida == 3 && t != 1)//ativa alerta restam 2 vidas
            {
                t = 1;
                axWindowsMediaPlayer2.URL = @"c:\imagem\sirene.mp4";
                axWindowsMediaPlayer2.settings.playCount = 2000;
            }
        }
        private void verifica()
        {
            if (vida < 5)//quantidade de vidas
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

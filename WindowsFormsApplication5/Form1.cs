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
        SoundPlayer beep_direita = new SoundPlayer(@"arquivos\audios\beep_direita.wav"); //usado na configuração de audio
        SoundPlayer explosao = new SoundPlayer(@"arquivos\audios\explosao.wav"); //explosão final de game over
        SoundPlayer desvio = new SoundPlayer(@"arquivos\audios\desvio.wav"); //desvio de nave
        SoundPlayer esquerda = new SoundPlayer(@"arquivos\audios\esquerda.wav"); //tiro vindo da esquerda
        SoundPlayer direita = new SoundPlayer(@"arquivos\audios\direita.wav"); //tiro vindo da direita
        SoundPlayer impesq = new SoundPlayer(@"arquivos\audios\impdir.wav"); //impacto na esquerda
        SoundPlayer impdir = new SoundPlayer(@"arquivos\audios\imesq.wav");// impacto na direita
        int vida; //do jogador
        int tempo; //intervalo de reação
        int n;//variável que armazena numero aleatório para ataque
        int t;//variável de segurança, usada na verificação da sirene
        int l;// variável de segurança de dado, sempre recebe valor de 'n'
        int minigameid;//armazena id correspondente ao minigame, utilizada no reconhecimento
        int tempo_padrao; //tempo padrao
        string fala;
        int ganha;
        int ttt;
        //NOTA: Sempre que relacionar um novo número a um minigame anotar aqui:
        // 111 morto
        //-2 inicio
        // -1 configuração
        // 0 falas aleatórias
        // 1 desvio de tiros
        // 7 ATIRAR
        // 222 recomeça ganha
        private static SpeechRecognitionEngine engine;
        public Form1()
        {
            InitializeComponent();
        }
        private void LoadSpeech()
        {
                engine = new SpeechRecognitionEngine(); //caso haja erro nesta linha o problema é com os pacotes, verificar também a versão do sistema: 32 ou 64 bits
                engine.SetInputToDefaultAudioDevice(); //caso haja erro nesta linha significa que o programa não conseguiu localizar um microfone para ser utilizado no reconhecimento

                string[] words = {"iniciar", "fechar", "esquerda", "direita", "atirar" };//gramática, novas palavras a serem reconhecidas devem ser introduzidas aqui

                engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words))));//carrega gramática
                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);
                engine.RecognizeAsync(RecognizeMode.Multiple);
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSpeech(); //inicia o reconhecimento
            começo();
        }



        private void começo()
        {
            ttt = 0;
            ganha = 0;
            fala = "";
            tempo_padrao = 0;
            minigameid = 111;
            tempo = 0;
            l = 3;
            t = 0;
            vida = 0;
            axWindowsMediaPlayer1.URL = @"arquivos\audios\fundo.wav"; //som de fundo
            axWindowsMediaPlayer1.settings.playCount = 1000; //quantidade de repetição do audio
            axWindowsMediaPlayer1.settings.volume = 30; //volume do audio
            axWindowsMediaPlayer2.URL = @"arquivos\audios\iniciarfechar.wav";
            axWindowsMediaPlayer2.settings.volume = 200;
            pictureBox2.Image = Image.FromFile(@"arquivos\gif\eva.jpg");
            minigameid = -2;
        }



        private void rec(object s, SpeechRecognizedEventArgs e)
        {
            fala = (e.Result.Text);
            switch (minigameid)//limitação de palavras da gramática baseado no id de cada caso
            {
                case 7:
                    switch (fala)
                    {
                        case "atirar":
                            timer_padrao.Stop();
                            tempo_padrao = 0;
                            fim();

                            break;
                    }
                            break;
                case -2:
                    switch (fala)
                    {
                        case "iniciar":
                            inicio();
                            minigameid = 111;
                            break;
                        case "fechar":
                            Close();
                            break;

                    }
                    break;
                case -1:
                    switch (fala)
                    {
                        case "iniciar":
                            inicioataque();
                            fala = "";
                            break;
                    }
                break;
                case 1:
                    if (vida < 5)
                    {
                        tempo = 0;//seta a variável de tempo
                        timer1.Stop();//para o timer
                        switch (fala) //variável que possui o resultado obtido no reconhecimento
                        {
                            case "esquerda":
                                if (l == 0)
                                {
                                    fala = "";
                                    impesq.Play();
                                    vida++;
                                    sirene();
                                    verifica();
                                }
                                else
                                {
                                    fala = "";
                                    ganha++;
                                    desvio.Play();
                                    verifica();
                                }
                                break;
                            case "direita":
                                if (l == 1)
                                {
                                    fala = "";
                                    impdir.Play();
                                    vida++;
                                    sirene();
                                    verifica();
                                }
                                else
                                {
                                    fala = "";
                                    ganha++;
                                    desvio.Play();
                                    verifica();
                                }
                                break;

                        }
                    }
                    break;
            }
            
        }




        private void inicio()
        {
            axWindowsMediaPlayer2.URL = @"arquivos\audios\igo_saudaçao_calibraçao.wav";
            axWindowsMediaPlayer2.settings.volume = 2000;
            pictureBox2.Image = Image.FromFile(@"arquivos\gif\eva2.gif");
            timer_padrao.Start();
        }




        private void config()
        {
            minigameid = -1; //id
        }



        private void inicioataque()
        {
            ttt = 1;
            axWindowsMediaPlayer2.URL = @"arquivos\audios\preataque.wav";
            timer_padrao.Start();
        }




        private void ataque()//ataque da nave inimiga
        {

                minigameid = 1;
                //LoadSpeech(); //inicia o reconhecimento
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
                pictureBox2.Image = Image.FromFile(@"arquivos\gif\sirene.gif");
                axWindowsMediaPlayer2.URL = @"arquivos\audios\sirene.mp4";
                axWindowsMediaPlayer2.settings.playCount = 2000;
            }
        }



        private void verifica()//verifica se jogador perdeu, caso contrário chama ataque() novamente
        {
            if (vida < 5)//para alterar quantidade de vidas do jogador modifique esta linha
            {
                if (ganha >= 5)
                {
                    minigameid = 7;
                    timer1.Stop();
                    tempo_padrao = 0;
                    axWindowsMediaPlayer2.settings.playCount = 0;
                    axWindowsMediaPlayer2.URL = @"arquivos\audios\atirar.wav";
                    axWindowsMediaPlayer2.settings.volume = 3000;
                    timer_padrao.Start();
                }
                else
                {
                    ataque();
                }
            }
            else //game over
            {
                minigameid = 111;
                vida = 6;
                axWindowsMediaPlayer2.Ctlcontrols.stop();
                axWindowsMediaPlayer1.Ctlcontrols.stop();
                explosao.Play();
                axWindowsMediaPlayer2.settings.playCount = 0;
                axWindowsMediaPlayer2.URL = @"arquivos\audios\fimdejogo.mp3";
                pictureBox2.Image = Image.FromFile(@"arquivos\gif\gameover.jpg");
                axWindowsMediaPlayer2.settings.volume = 2000;
                timer_padrao.Start();
                timer1.Stop();
            }
        }




        private void timer_padrao_Tick(object sender, EventArgs e)
        {
            tempo_padrao++;
            if (ttt == 1)
            {
                if (tempo_padrao == 14)
                {
                    timer_padrao.Stop();
                    tempo_padrao = 0;
                    ttt = 0;
                    ataque();
                }
            }
            if (vida == 6)
            {
                if (tempo_padrao == 6)
                {
                    timer_padrao.Stop();
                    tempo_padrao = 0;
                    começo();
                }
            }
            else
            {
                if (minigameid == 222)
                {
                    if (tempo_padrao == 14)
                    {
                        tempo_padrao = 0;
                        timer_padrao.Stop();
                        começo();
                    }
                }
                if (minigameid == 111)
                {
                    if (tempo_padrao == 67)
                    {
                        timer_padrao.Stop();
                        tempo_padrao = 0;
                        config();
                    }
                }
                if (minigameid == 7)
                {
                    if (tempo_padrao == 6)
                    {
                        tempo_padrao = 0;
                        timer_padrao.Stop();
                        ganha = 0;
                        ataque();
                    }
                }
            }
        }
        private void fim()
        {
            minigameid = 222;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            axWindowsMediaPlayer2.settings.playCount = 0;
            axWindowsMediaPlayer2.URL = @"arquivos\audios\fim.wav";
            timer_padrao.Start();
        }
    }
}

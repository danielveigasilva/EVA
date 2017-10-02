/* REGRAS DE PROGRAMAÇÃO
 * -----------Regras de declaração de variável-------------------------------------------

	NOMEVAR_SUFIXO

Nomes Sufixos:

_som -> variável que armazena efeito sonoro
_tmr -> variável contador do timer
_id  -> variável que armazena um valor id utilizado para localização de ponto do jogo
_rdm -> variável que armazena valor randômico
_fala-> variável que armazena resultado da fala
_sct -> variável que armazena valor de outra variável já atribuida
_bin -> variável que armazena valor binário (0 ou 1)
_cont-> variável que realiza contagem

--------------Regras de declaração de objeto---------------------------------------------

	NOMEOBJ_SUFIXO

Nomes Sufixos:

_timer -> Timer
_mp    -> Windows Midia Player
_img   -> PictureBox

-----------------------------------------------------------------------------------------
*/

//Bibliotecas
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
//Bibliotecas

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        SoundPlayer explosao_som = new SoundPlayer(@"arquivos\audios\explosao.wav"); //explosão final de game over
        SoundPlayer desvio_som = new SoundPlayer(@"arquivos\audios\desvio.wav"); //desvio de nave
        SoundPlayer esquerda_som = new SoundPlayer(@"arquivos\audios\esquerda.wav"); //tiro vindo da esquerda
        SoundPlayer direita_som = new SoundPlayer(@"arquivos\audios\direita.wav"); //tiro vindo da direita
        SoundPlayer impactoesq_som = new SoundPlayer(@"arquivos\audios\impdir.wav"); //impacto na esquerda
        SoundPlayer impactodir_som = new SoundPlayer(@"arquivos\audios\imesq.wav");// impacto na direita
        int vida_cont; //do jogador
        int tempo_tmr; //intervalo de reação
        int resul_tiro_rdm;//variável que armazena numero aleatório para ataque
        int verifica_sirene_bin;//variável de segurança, usada na verificação da sirene
        int result_tiro_sct;// variável de segurança de dado, sempre recebe valor de 'n'
        int subgame_id;//armazena id correspondente ao minigame, utilizada no reconhecimento
        int tempo_padrao_tmr; //tempo padrao
        string fala;
        int ganha_cont;
        int inicio_id;

        /*INDICE ID MINIGAME

        111 -> morto
        -2  -> inicio
        -1  -> configuração
        0   -> falas aleatórias
        1   -> desvio de tiros
        7   -> ATIRAR
        222 -> recomeça ganha

         */

        private static SpeechRecognitionEngine engine; //Reconhecimento de Fala
        public Form1()
        {
            InitializeComponent();
        }

//Reconhecimento de Fala (Carregamento)
        private void LoadSpeech()
        {
                engine = new SpeechRecognitionEngine(); //caso haja erro nesta linha o problema é com os pacotes, verificar também a versão do sistema: 32 ou 64 bits
                engine.SetInputToDefaultAudioDevice(); //caso haja erro nesta linha significa que o programa não conseguiu localizar um microfone para ser utilizado no reconhecimento

                string[] words = {"iniciar", "fechar", "esquerda", "direita", "atirar" };//gramática, novas palavras a serem reconhecidas devem ser introduzidas aqui

                engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words))));//carrega gramática
                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);
                engine.RecognizeAsync(RecognizeMode.Multiple);
        }



//----------------------------------------------Timers--------------------------------------------------
 //Timer Padrão
        private void timer_padrao_Tick(object sender, EventArgs e)
        {
            tempo_padrao_tmr++;
            if (inicio_id == 1)
            {
                if (tempo_padrao_tmr == 14)
                {
                    timer_padrao.Stop();
                    tempo_padrao_tmr = 0;
                    inicio_id = 0;
                    ataque();
                }
            }
            if (vida_cont == 6)
            {
                if (tempo_padrao_tmr == 6)
                {
                    timer_padrao.Stop();
                    tempo_padrao_tmr = 0;
                    começo();
                }
            }
            else
            {
                if (subgame_id == 222)
                {
                    if (tempo_padrao_tmr == 14)
                    {
                        tempo_padrao_tmr = 0;
                        timer_padrao.Stop();
                        começo();
                    }
                }
                if (subgame_id == 111)
                {
                    if (tempo_padrao_tmr == 34)
                    {
                        timer_padrao.Stop();
                        tempo_padrao_tmr = 0;
                        config();
                    }
                }
                if (subgame_id == 7)
                {
                    if (tempo_padrao_tmr == 6)
                    {
                        tempo_padrao_tmr = 0;
                        timer_padrao.Stop();
                        ganha_cont = 0;
                        ataque();
                    }
                }
            }
        }

//Timer Tiros
        private void timer1_Tick(object sender, EventArgs e)
        {
            tempo_tmr++;//adiciona 1 a cada 1 segundo a variavel tempo
            if (tempo_tmr == 4)//tempo de reação (do disparo até o impacto)
            {
                if (result_tiro_sct == 0)
                {
                    impactoesq_som.Play();
                    vida_cont++;
                }
                else
                {
                    impactodir_som.Play();
                    vida_cont++;
                }
                sirene();
                verifica();
            }
        }

//------------------------------------------------------------------------------------------------------


//Load Form
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSpeech(); //inicia o reconhecimento
            começo();
        }


//Configurações iniciais
        private void começo()
        {
            inicio_id = 0;
            ganha_cont = 0;
            fala = "";
            tempo_padrao_tmr = 0;
            subgame_id = 111;
            tempo_tmr = 0;
            result_tiro_sct = 3;
            verifica_sirene_bin = 0;
            vida_cont = 0;
            axWindowsMediaPlayer1.URL = @"arquivos\audios\fundo.wav"; //som de fundo
            axWindowsMediaPlayer1.settings.playCount = 1000; //quantidade de repetição do audio
            axWindowsMediaPlayer1.settings.volume = 10; //volume do audio
            axWindowsMediaPlayer2.URL = @"arquivos\audios\iniciarfechar.wav";
            axWindowsMediaPlayer2.settings.volume = 200;
            pictureBox2.Image = Image.FromFile(@"arquivos\gif\eva.jpg");
            subgame_id = -2;
        }


//Ações de Reconhecimento de Fala
        private void rec(object s, SpeechRecognizedEventArgs e)
        {
            fala = (e.Result.Text);
            switch (subgame_id)//limitação de palavras da gramática baseado no id de cada caso
            {
                case 7:
                    switch (fala)
                    {
                        case "atirar":
                            timer_padrao.Stop();
                            tempo_padrao_tmr = 0;
                            fim();

                            break;
                    }
                            break;
                case -2:
                    switch (fala)
                    {
                        case "iniciar":
                            inicio();
                            subgame_id = 111;
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
                    if (vida_cont < 5)
                    {
                        tempo_tmr = 0;//seta a variável de tempo
                        timer1.Stop();//para o timer
                        switch (fala) //variável que possui o resultado obtido no reconhecimento
                        {
                            case "esquerda":
                                if (result_tiro_sct == 0)
                                {
                                    fala = "";
                                    impactoesq_som.Play();
                                    vida_cont++;
                                    sirene();
                                    verifica();
                                }
                                else
                                {
                                    fala = "";
                                    ganha_cont++;
                                    desvio_som.Play();
                                    verifica();
                                }
                                break;
                            case "direita":
                                if (result_tiro_sct == 1)
                                {
                                    fala = "";
                                    impactodir_som.Play();
                                    vida_cont++;
                                    sirene();
                                    verifica();
                                }
                                else
                                {
                                    fala = "";
                                    ganha_cont++;
                                    desvio_som.Play();
                                    verifica();
                                }
                                break;

                        }
                    }
                    break;
            }
            
        }


//Inicio Jogo (Apresentação)
        private void inicio()
        {
            axWindowsMediaPlayer2.URL = @"arquivos\audios\Carregamento_ola.wav";
            axWindowsMediaPlayer2.settings.volume = 2000;
            pictureBox2.Image = Image.FromFile(@"arquivos\gif\eva2.gif");
            timer_padrao.Start();
        }


//Configurações ID
        private void config()
        {
            subgame_id = -1; //id
        }


//Inicio do Ataque
        private void inicioataque()
        {
            inicio_id = 1;
            axWindowsMediaPlayer2.URL = @"arquivos\audios\preataque.wav";
            timer_padrao.Start();
        }


//Ataque
        private void ataque()//ataque da nave inimiga
        {

                subgame_id = 1;
                //LoadSpeech(); //inicia o reconhecimento
                Random randNum = new Random();
                resul_tiro_rdm = randNum.Next(2); //gera numero aleatório
                tempo_tmr = 0;//seta a variável de tempo
                //n = 0; //linha para teste (tira a aleatoriedade, sempre será esquerda)

                Thread.Sleep(1500);//intervalo para carregar audio
                if (resul_tiro_rdm == 0)
                {
                    esquerda_som.Play();
                    timer1.Start(); //inicia o timer
                    result_tiro_sct = resul_tiro_rdm;
                }

                if (resul_tiro_rdm == 1)
                {
                    direita_som.Play();
                    timer1.Start(); //inicia o timer
                    result_tiro_sct = resul_tiro_rdm;
                }
        }


//Verifica Sirene
        private void sirene()//ativa sons de sirene depois de três impactos
        {
            if (vida_cont == 3 && verifica_sirene_bin != 1)
            {
                verifica_sirene_bin = 1;//impede reprodução repitida
                pictureBox2.Image = Image.FromFile(@"arquivos\gif\sirene.gif");
                axWindowsMediaPlayer2.URL = @"arquivos\audios\sirene.mp4";
                axWindowsMediaPlayer2.settings.playCount = 2000;
            }
        }


//Verifica Game Over 
        private void verifica()//verifica se jogador perdeu, caso contrário chama ataque() novamente
        {
            if (vida_cont < 5)//para alterar quantidade de vidas do jogador modifique esta linha
            {
                if (ganha_cont >= 5)
                {
                    subgame_id = 7;
                    timer1.Stop();
                    tempo_padrao_tmr = 0;
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
                subgame_id = 111;
                vida_cont = 6;
                axWindowsMediaPlayer2.Ctlcontrols.stop();
                axWindowsMediaPlayer1.Ctlcontrols.stop();
                explosao_som.Play();
                axWindowsMediaPlayer2.settings.playCount = 0;
                axWindowsMediaPlayer2.URL = @"arquivos\audios\fimdejogo.mp3";
                pictureBox2.Image = Image.FromFile(@"arquivos\gif\gameover.jpg");
                axWindowsMediaPlayer2.settings.volume = 2000;
                timer_padrao.Start();
                timer1.Stop();
            }
        }


//Fim de Jogo  
        private void fim()
        {
            subgame_id = 222;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            axWindowsMediaPlayer2.settings.playCount = 0;
            axWindowsMediaPlayer2.URL = @"arquivos\audios\fim.wav";
            timer_padrao.Start();
        }
    }
}

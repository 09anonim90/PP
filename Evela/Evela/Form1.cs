using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Evela
{
    public partial class Form1 : Form
    {
        string fpath;//путь к файлу текста
        string fname;//файл текста
        XmlReader xmlReader;//XmlReader обеспечивает чтение данных с xml-файла
        string qw;// вопрос
        string[] answ = new string[3];//Вариант ответа
        string pic;//путь к файлу иллюстрации
        int timeLimit = 30000;
        int time = 0;
        int right;//правильный ответ (номер)
        int otv;//выбранный ответ(номер)
        int n;//количесвто правельных ответов
        int nv;//общее количество вопросов
        int mode;//состояние программы
        //0 - начало работы
        //1 - тестирование
        //2 - завершение работы

        public Form1(string[] args)
        {
            InitializeComponent();
            //Отключение кнопок выбора
            radioButton1.Visible = false;
            radioButton2.Visible = false;
            radioButton4.Visible = false;

            if (args.Length > 0)
            {
                if (args[0].IndexOf(";") == -1)
                {
                    fpath = Application.StartupPath + "\\";//Указание пути
                    fname = args[0];//Указание имени
                }
                else
                {
                    fpath = args[0].Substring(0, args[0].LastIndexOf("\\") + 1);
                    fname = args[0].Substring(args[0].LastIndexOf("\\") + 1);
                }
                try
                {
                    xmlReader = new XmlTextReader(fpath + fname);//Подключение xmlReader
                    xmlReader.Read();
                    mode = 0;//Переключение режима работы в "Начало работы"
                    n = 0;//Обнуление правильных ответов
                    showHead();//Запуск процедуры
                    showDescription();//Запуск процедуры
                }
                catch//Обработка исключений
                {
                    label1.Text = "Ошибка доступа к файлу " + fpath + fname;
                    MessageBox.Show("Ошибка доступа к файлу.\n" + fpath + fname + "\n", "Экзаменатор", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    mode = 2;//Переключение режима работы в "Завершение работы"
                }
            }
            else
            {
                //Сообщение ои отсутствии файла
                label1.Text = "Файл теста необходимо указать в команде запуска программы.\n Например: 'exam economics.xml' или 'exam c:\\spb.xml'.";
                mode = 2;//Переключение режима работы в "Завершение работы"
            }
        }

        private void showHead()
        {
            do xmlReader.Read();
            while (xmlReader.Name != "head");//Чтение файла до тега "head"
            xmlReader.Read();
            this.Text = xmlReader.Value;
            xmlReader.Read();
        }
        private void showDescription()
        {
            do xmlReader.Read();
            while (xmlReader.Name != "description");//Read file until tag 'description'
            xmlReader.Read();
            label1.Text = xmlReader.Value;//Add text value
            xmlReader.Read();
            do xmlReader.Read();
            while (xmlReader.Name != "qw");//Read file until tag 'qw'
            xmlReader.Read();
        }
        private Boolean getQw()
        {
            xmlReader.Read();
            if (xmlReader.Name == "q")
            {
                qw = xmlReader.GetAttribute("text");//Add attribute 'text'
                pic = xmlReader.GetAttribute("src");//Add attribute 'src'
                if (!pic.Equals(string.Empty)) pic = fpath = pic;//Picture load
                xmlReader.Read();
                int i = 0;
                while (xmlReader.Name != "q")//Work with tag 'q'
                {
                    xmlReader.Read();
                    if (xmlReader.Name == "a")//Work with tag 'a'
                    {
                        if (xmlReader.GetAttribute("right") == "yes") right = i;//Checking correct answer
                        xmlReader.Read();
                        if (i < 3) answ[i] = xmlReader.Value;
                        xmlReader.Read();
                        i++;
                    }
                }
                xmlReader.Read();
                return true;
            }
            else return false;
        }
        private void showQw()
        {
            label1.Text = qw;
            if (pic.Length != 0)
            {
                try
                {
                    pictureBox1.Image = new Bitmap(pic);
                    pictureBox1.Visible = true;
                    radioButton1.Top = pictureBox1.Bottom + 16;//Picture show
                }
                catch
                {
                    if (pictureBox1.Visible)
                    {
                        pictureBox1.Visible = false;
                        label1.Text += "\n\n\nОшибка доступа к файлу " + pic + "/";
                        radioButton1.Top = label1.Bottom + 8;
                    }
                }
            }
        
            else 
            {
                if (pictureBox1.Visible) pictureBox1.Visible = false;//Picture hide
                radioButton1.Top = label1.Bottom;
            }
            radioButton1.Text = answ[0];
            radioButton2.Top = radioButton1.Top + 24;
            radioButton2.Text = answ[1];
            radioButton4.Top = radioButton2.Top + 24;
            radioButton4.Text = answ[2];

            radioButton3.Checked = true;
            button1.Enabled = false;
        }
        private void showLevel()
        {
            do xmlReader.Read();
            while (xmlReader.Name != "levels");//Read file until tag 'levels'
            xmlReader.Read();
            while (xmlReader.Name != "levels")
            {
                xmlReader.Read();
                if (xmlReader.Name == "levels")
                    if (n >= Convert.ToInt32(xmlReader.GetAttribute("score")))
                        break;
            }
            //Test end
            label1.Text = "Тестирование завершено.\n" + "Всего вопросов: " + nv.ToString() + ". " + "Правельных ответов: " + n.ToString() + ".\n" + xmlReader.GetAttribute("text");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (mode)
            {
                case 0:
                    //Turn on radiobuttons
                    radioButton1.Visible = true;
                    radioButton2.Visible = true;
                    radioButton4.Visible = true;
                    getQw();//Getting question
                    showQw();//Show question
                    mode = 1;//Switch mod to '1'
                    button1.Enabled = false;//Disable button 1
                    radioButton3.Checked = true;//Enable radiobutton Checking
                    button1.Text = "Ok";//Set button text to 'Ok'
                    timer1.Enabled = true;//Turning on timer
                    break;
                case 1:
                    nv++;//Inc questions value
                    if (otv == right) n++;//inc right questions value
                    if (getQw()) showQw();
                    else
                    {
                        //Turn off radiobuttons
                        radioButton1.Visible = false;
                        radioButton2.Visible = false;
                        radioButton4.Visible = false;
                        pictureBox1.Visible = false;
                        showLevel();
                        mode = 2;//switch mod to '2'
                    }
                    break;
                case 2: Close(); break;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if ((RadioButton) sender == radioButton1) otv = 0;
            if ((RadioButton) sender == radioButton2) otv = 0;
            if ((RadioButton) sender == radioButton3) otv = 0;
            button1.Enabled = true;//turning on button1
        }

        private void timer1_Tick(object sender, EventArgs e)
        { 
            time++;
            if (time == timeLimit) mode = 2;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Media;
using System.Diagnostics;
using TwitchCSharp.Clients;
using TwitchCSharp.Models;
  

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        #region  variables
        private static string username = Properties.Settings.Default.username;//"hentaidude"; //
        private static string password = Properties.Settings.Default.oauth; //"oauth:qi8e4p8454w9xvyx3q17eokdb5bxhc"; //
        private string autopermitbool;
        private static string TwtichClientId = Properties.Settings.Default.clientid; //"lrvs7xwodl1enad0j4nqsljpwy95z1m"; //
        IrcClient irc = new IrcClient("irc.chat.twitch.tv", 6667, username, password);
        NetworkStream serverstream = default(NetworkStream);
        string readdata = "";
        Thread chatthread;
        TwitchReadOnlyClient APIClient = new TwitchReadOnlyClient(TwtichClientId);
        TwitchROChat ChatClient = new TwitchROChat(TwtichClientId);
        List<string> bannedwords = new List<string> { "nigger", "faggot", "fag", "dickhead", "asshole", "cunt", "banme" };
        List<string> bannedsite = new List<string> { ".com", ".ru", ".co", ".net", ".org", ".xyc" };
        bool commandspamfilter = false;
        public bool bannedsitebool = true;
        List<CommandSpamUser> CommandSpamUser = new List<CommandSpamUser>();

        Inifile PointsIni = new Inifile(@"C:\Users\christoff\Documents\visual studio 2015\Projects\WindowsFormsApplication2\WindowsFormsApplication2\points.ini");
        #endregion

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Spamfilter.Start();
            label3.Text = Properties.Settings.Default.timeout;
            irc.joinroom(Properties.Settings.Default.channel);
            chatthread = new Thread(getmsg);
            chatthread.Start();
            ViewerBoxTimer.Start();
            ViewerBoxTimer_Tick(null, null);
            label5.Text = Properties.Settings.Default.channel;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            irc.leaveroom();
            serverstream.Dispose();
            Environment.Exit(0);

        }
        #region MSG, Filters and commands (det meste af botten er her xd
        private void getmsg()
            {
            serverstream = irc.tcpClient.GetStream();
            int buffsize = 0;
            byte[] instream = new byte[10025];
            buffsize = irc.tcpClient.ReceiveBufferSize;
            while(true)
            {
                try
                {
                    readdata = irc.readmsg();
                    msg();
                }
            
                catch(Exception e)
                {

                }

            }
        }
        private void msg()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(msg));
            else
            {
                string[] separator = new string[] { "#"+ Properties.Settings.Default.channel + " :" };
                string[] singleset = new string[] { ":","!" };
                if (readdata.Contains("PRIVMSG"))
                {
                    string username = readdata.Split(singleset, StringSplitOptions.None)[1];
                    string message = readdata.Split(separator, StringSplitOptions.None)[1];
                    if (bannedsitefilter(username, message)) return;
                    if (message[0] == '!') commands(username, message);
                    if (bannedwordfilter(username, message)) return;


                    ChatBox.Text = ChatBox.Text + username +": " + message + Environment.NewLine;

                    int num = ChatBox.Lines.Count();
                    if(ChatBox.Lines.Count()>21)
                    {
                        var foos = new List<string>(ChatBox.Lines);
                        foos.RemoveAt(0);
                        ChatBox.Lines = foos.ToArray();
                    }
            }
                //ChatBox.Text = ChatBox.Text + readdata.ToString() + Environment.NewLine;
            }
        }
        private void commands(string username, string message)
        {
            string command = message.Split(new[] { ' ', '!' }, StringSplitOptions.None)[1]; 
            
            switch(command.ToLower())
            {
                case "hstatus":
                    if (username == "Foustas1" || username == "xec dee" || username == "hentaidude" || username == "foustas1")
                        irc.sendmsg("random shit : Autopermit> " + autopermitbool + " CommandFilter: " + commandspamfilter + ", Alive and well... " + label2.Text + ", Connected to " + Properties.Settings.Default.channel);
                        break;
                case "hdude":
                   irc.sendmsg("He's a bot Kappa");
                    break;
                case "hbot":
                    irc.sendmsg("Bot is a work in progress, and its only use (currently) is autopermit, if no mod is present or active. Note only foustas can actiave autopermit, use !hcommands for more info");
                    break;
                case "hautopermit":
                    irc.sendmsg("'Auto-permit' is something the host can activate if theres no mods active in chat. Basic usages: !permitme, to get the bot to permit you to post a link 'NOTE: this bot is still a work in progress and might break into tiny pieces '");
                    break;
                case "autopermittrue":
                    if(username == "Foustas1" || username == "xec dee" || username == "hentaidude" || username == "foustas1" )
                    irc.sendmsg("Auto permit is now on");
                    autopermitbool = "true";
                    bannedsitebool = false;
                    break;
                case "autopermitfalse":
                    if(username == "Foustas1" || username == "HentaiDude" || username == "hentaidude" || username == "foustas1")
                    irc.sendmsg("Auto permit is now off");
                    autopermitbool = "false";
                    bannedsitebool = true;
                        break;
                case "permit":
                    if (username == "Foustas1" || username == "HentaiDude" || username == "hentaidude" || username == "foustas1")
                        bannedsitebool = false;
                    timer2.Start(); 
                    irc.sendmsg(username + " test permit. you have 90 seconds to post ya link");
                    irc.sendmsg("!permit " + username);
                    
                    break;
                case "permitme":
                    if (autopermitbool == "true")
                        irc.sendmsg("!permit " + username);
                    bannedsitebool = false;
                    break;
                case "hcommands":
                    irc.sendmsg("Viewer commands: !hdude, !hbot, !permitme, !hautopermit");
                    irc.sendmsg("/me Host only: !autopermittrue, !autopermitfalse, !badusers, !permit");
                        break;
                case "hreward":
                    if(username == "Foustas1" || username == "HentaiDude" || username == "mgdelux")
                    {
                        string recipent = message.Split(new string[] { " " }, StringSplitOptions.None)[1];
                        if (recipent== username) break;
                        if (recipent[0] == '@')
                        {
                            recipent = recipent.Split(new[] { '@' }, StringSplitOptions.None)[1];
                        }
                        string pointtransfers = message.Split(new string[] { " " }, StringSplitOptions.None)[2];
                        double pointtotrans = 0;
                        bool validnumber = double.TryParse(pointtransfers.Split(new[] { ' ' }, StringSplitOptions.None)[0], out pointtotrans);
                        if (!validnumber) break;
                        if(pointtotrans>0)
                        {
                            AddPoints(recipent, pointtotrans);
                            irc.sendmsg("Points works xd");
                        }
                    }
                    break;
                default:
                case "default":
                    break;
                case "hsteal":
                    if (username == "Foustas1" || username == "HentaiDude" || username == "mgdelux")
                    {
                        string recipent = message.Split(new string[] { " " }, StringSplitOptions.None)[1];
                        if (recipent == username) break;
                        if (recipent[0] == '@')
                        {
                            recipent = recipent.Split(new[] { '@' }, StringSplitOptions.None)[1];
                        }
                        string pointtransfers = message.Split(new string[] { " " }, StringSplitOptions.None)[2];
                        double pointtotrans = 0;
                        bool validnumber = double.TryParse(pointtransfers.Split(new[] { ' ' }, StringSplitOptions.None)[0], out pointtotrans);
                        if (!validnumber) break;
                        if (pointtotrans > 0)
                        {
                            AddPoints(recipent, -pointtotrans);
                            irc.sendmsg("Rip ur points mate");
                        }
                    }
                    break;
                case "hpoints":
                    if(!commandspamfilter)
                    {
                        foreach(CommandSpamUser singeluser in CommandSpamUser)
                        {
                            if (username == singeluser.username) return;
                        }

                        commandspamfilter = true;
                        CommandSpamUser User = new CommandSpamUser();
                        User.username = username;
                        User.TimeOfMessage = DateTime.Now;
                        CommandSpamUser.Add(User);

                        string yourpoints = PointsIni.inireadvalue(Properties.Settings.Default.channel+"." + username, "Points");
                    if(yourpoints == "")
                    {
                        irc.sendmsg("you dont have any points :(.. or do you?");
                        yourpoints = "20";
                        AddPoints(username, double.Parse(yourpoints));
                    }
                    else
                    {
                        double thepoints = double.Parse(yourpoints);
                        if(thepoints<20)
                        {
                            AddPoints(username, 20 - thepoints);
                            yourpoints = "20";
                        }
                    }
                    irc.sendmsg(username + " has " + yourpoints + " Points!");
                    }
                    break;
                case "badusers":
                     if (username == "Foustas1" || username == "HentaiDude" || username == "mgdelux")
                    {
                        irc.sendmsg("cumming soon xD");
                    }
                    break;
            }
        }
        private bool bannedsitefilter(string username, string message)
        {
            foreach (string word in bannedsite)
            {
                if (bannedsitebool == true && message.Contains(word)) 
                {
                    string command = "/timeout " + username + " " + Properties.Settings.Default.timeout;
                    irc.sendmsg(">" + username + " Do not post links..");
                    irc.sendmsg("/Timeout " + username + " " + Properties.Settings.Default.timeout);

                    return true;
                }

            }
            return false;
        }
        private bool bannedwordfilter(string username, string message)
        {
            foreach(string word in bannedwords)
            {
                if(message.Contains(word))
                {
                    string command = "/timeout " + username + " " +Properties.Settings.Default.timeout;
                    irc.sendmsg(command);
                    irc.sendmsg(">"+username + " You've been a bad boy! (timeout)");
                    return true;
                }
               
            }
            return false;
        }
        #endregion
        #region Points
        private void AddPoints(string username, double points)
        {
            double FinalNumber = 0;
            try
            {
                string[] seperator = new string[] { @"\r\n" };
                username = username.Trim().ToLower();
                string pointsofuser = PointsIni.inireadvalue(Properties.Settings.Default.channel+"." + username, "Points");
                double numberofpoints = double.Parse(pointsofuser);
                FinalNumber = Convert.ToDouble(numberofpoints + points);
                if (FinalNumber > 0) PointsIni.iniwritevalue(Properties.Settings.Default.channel + "." + username, "Points", FinalNumber.ToString());
                if (FinalNumber <=0) PointsIni.iniwritevalue(Properties.Settings.Default.channel + "." + username, "Points", "0");
            }
            catch(Exception e)
            {
                
            if(points>0) PointsIni.iniwritevalue(Properties.Settings.Default.channel + "." + username, "Points", points.ToString());
            }
        }
        #endregion
        #region MISC
        private void button1_Click(object sender, EventArgs e)
        {
            irc.sendmsg("/me " + textBox1.Text);
            textBox1.Clear();
            
        }

        private void settngsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ViewerBoxTimer_Tick(object sender, EventArgs e)
        {
            viewerlistupdate();
        }

        private void viewerlistupdate()
        {
            ViewerBox.Items.Clear();
            Chatters AllChatters = ChatClient.GetChatters(Properties.Settings.Default.channel);
            int numofchat = ChatClient.GetChatterCount(Properties.Settings.Default.channel);
            label1.Text = "Chatters: " + numofchat;
            ViewerBox.Text += "Updateing viewerlist";
            foreach (string admin in AllChatters.Admins)
            {
                ViewerBox.Items.Add(admin + Environment.NewLine);
            }
            foreach (string staff in AllChatters.Staff)
            {
                ViewerBox.Items.Add(staff + Environment.NewLine);
            }
            foreach (string gemods in AllChatters.GlobalMods)
            {
                ViewerBox.Items.Add(gemods + Environment.NewLine);
            }
            foreach (string mods in AllChatters.Moderators)
            {
                ViewerBox.Items.Add(mods + Environment.NewLine);
            }
            foreach (string viewer in AllChatters.Viewers)
            {
                ViewerBox.Items.Add(viewer + Environment.NewLine);
            }

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            viewerlistupdate();
        }

        private void Loyalpointtimer_Tick(object sender, EventArgs e)
        {
            foreach (string username in ViewerBox.Items)
            {
                double pointtest = Convert.ToDouble(Properties.Settings.Default.points);
                AddPoints(username, Properties.Settings.Default.points);
                label2.Text = "Points has been Givin @" + DateTime.Now.ToShortTimeString();
                label3.Text = Properties.Settings.Default.timeout;

            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void botLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void creToolStripMenuItem_Click(object sender, EventArgs e)
        {
            login login2 = new login();
            login2.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
           
        }

        private void channelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            channel channel2 = new channel();
            channel2.Show();
           
        }

        private void label3_Click(object sender, EventArgs e)
        {
         
        }

        private void label4_Click(object sender, EventArgs e)
        {
          
        }

        private void label5_Click(object sender, EventArgs e)
        {
           
        }

        private void pointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            points points = new points();
            points.Show();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            
           
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void bannedWordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void label3_Click_1(object sender, EventArgs e)
        {
            label3.Text = Properties.Settings.Default.timeout;
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("HentaiBot as it is currently 'known' is a bot thats work in progress." + Environment.NewLine + " Made in c#. currently only works for twitch.tv/foustas1, but credentials is still needed to have the bot working.. " + Environment.NewLine + "Programmed by 'HentaiDude'... Thanks to TallestOne for DLL and some code :)" );
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("No help to find here yet :/");
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            launcher launcher = new launcher();
            launcher.Show();
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Spamfilter.Start();
            label3.Text = Properties.Settings.Default.timeout;
            irc.joinroom(Properties.Settings.Default.channel);
            chatthread = new Thread(getmsg);
            chatthread.Start();
            ViewerBoxTimer.Start();
            ViewerBoxTimer_Tick(null, null);
            label5.Text = Properties.Settings.Default.channel;
            ChatBox.Clear();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void ChatBox_TextChanged(object sender, EventArgs e)
        {
            ChatBox.SelectionStart = ChatBox.Text.Length;
            ChatBox.ScrollToCaret();
        }

        private void Spamfilter_Tick(object sender, EventArgs e)
        {
            commandspamfilter = false;
            List<CommandSpamUser> temp = CommandSpamUser;
            foreach(CommandSpamUser user in temp)
            {
                TimeSpan duration = DateTime.Now - user.TimeOfMessage;
                if(duration >TimeSpan.FromSeconds(30))
                {
                    CommandSpamUser.Remove(user);
                    return;
                }
            }
        }

        private void ViewerBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            bannedsitebool = true;
            irc.sendmsg("You can no longer post links ");
            timer2.Stop();
        }
    }
    }
#endregion 
#region Classes
class IrcClient
    {
        private string username;
        private string channel;

        public TcpClient tcpClient;
        private StreamReader inputstream;
        private StreamWriter outputstream;

        public IrcClient(string ip, int port, string username, string passowrd)
        {
            tcpClient = new TcpClient(ip, port);
            inputstream = new StreamReader(tcpClient.GetStream());
            outputstream = new StreamWriter(tcpClient.GetStream());

            outputstream.WriteLine("PASS " + passowrd);
            outputstream.WriteLine("NICK " + username);
            outputstream.WriteLine("USER " + username + " 8 * :" + username);
            outputstream.WriteLine("CAP REQ :twitch.tv/membership");
            outputstream.WriteLine("CAP REQ :twitch.tv/commands");
            outputstream.Flush();
        }
        public void joinroom(string channel)

           {
            this.channel = channel;
            outputstream.WriteLine("JOIN #" + channel);
            outputstream.Flush();

        }

        public void leaveroom()
        {
            outputstream.Close();
            inputstream.Close();
        }

        public void sendircmsg(string msg)
        {
            outputstream.WriteLine(msg);
            outputstream.Flush();

        }

        public void sendmsg(string msg)
        {
            sendircmsg(":" + username + "!" + username + "@" + username + "tmi.twtich.tv PRIVMSG #" + channel + " :" + msg);
        }

        public void Pingrespond()
        {
            sendircmsg("PONG tmi.twitch.tv\r\n");
        }

        public string readmsg()
        {
            string message = "";
            message = inputstream.ReadLine();
            return message;
        }

    }
    class Inifile
    {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filepath);

        public Inifile(string inipath)
        {
            path = inipath;
        }

        public void iniwritevalue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, this.path);
        }

        public string inireadvalue(string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", temp, 225, this.path);
            return temp.ToString();
        }
    }
class CommandSpamUser
{
    public string username;
    public DateTime TimeOfMessage;
}
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;

namespace NHL_Game_Chooser
{
    public partial class mainWindow : Form
    {
        Logic logic;
        List<Button[]> mainGrid;
        List<Label> labels;
        Teams teams;
        Process process;

        public mainWindow()
        {
            //fixDLL();
            logic = new Logic();
            makeDictionary();

            InitializeComponent();

            this.MaximizeBox = false;
        }

        private void fixDLL()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {

                String resourceName = "Newtonsoft.Json." +

                   new AssemblyName(args.Name).Name + ".dll";

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {

                    Byte[] assemblyData = new Byte[stream.Length];

                    stream.Read(assemblyData, 0, assemblyData.Length);

                    return Assembly.Load(assemblyData);

                }

            }; 
        }

        private void makeDictionary()
        {
            teams = new Teams();

            teams.Add("ANA", "Anaheim Ducks", getImage("ANA"));
            teams.Add("ARI", "Arizona Coyototes", getImage("ARI"));
            teams.Add("BOS", "Boston Bruins", getImage("BOS"));
            teams.Add("BUF", "Buffalo Sabres", getImage("BUF"));
            teams.Add("CAR", "Carolina Hurricanes", getImage("CAR"));
            teams.Add("CBJ", "Columbus Blue Jackets", getImage("CBJ"));
            teams.Add("CGY", "Calgary Flames", getImage("CGY"));
            teams.Add("CHI", "Chicago Blackhawks", getImage("CHI"));
            teams.Add("COL", "Colorado Avalanche", getImage("COL"));
            teams.Add("DAL", "Dallas Stars", getImage("DAL"));
            teams.Add("DET", "Detroit Red Wings", getImage("DET"));
            teams.Add("EDM", "Edmonton Oilers", getImage("EDM"));
            teams.Add("FLA", "Florida Panthers", getImage("FLA"));
            teams.Add("LAK", "Los Angeles Kings", getImage("LAK"));
            teams.Add("MIN", "Minnesota Wild", getImage("MIN"));
            teams.Add("MTL", "Montréal Canadiens", getImage("MTL"));
            teams.Add("NJD", "New Jersey Devils", getImage("NJD"));
            teams.Add("NSH", "Nashville Predators", getImage("NSH"));
            teams.Add("NYI", "New York Islanders", getImage("NYI"));
            teams.Add("NYR", "New York Rangers", getImage("NYR"));
            teams.Add("OTT", "Ottawa Senators", getImage("OTT"));
            teams.Add("PHI", "Philadelphia Flyers", getImage("PHI"));
            teams.Add("PIT", "Pittsburgh Penguins", getImage("PIT"));
            teams.Add("SJS", "San Jose Sharks", getImage("SJS"));
            teams.Add("STL", "St Louis Blues", getImage("STL"));
            teams.Add("TBL", "Tampa Bay Lightning", getImage("TBL"));
            teams.Add("TOR", "Toronto Maple Leafs", getImage("TOR"));
            teams.Add("VAN", "Vancouver Canucks", getImage("VAN"));
            teams.Add("WPG", "Winnipeg Jets", getImage("WPG"));
            teams.Add("WSH", "Washington Captials", getImage("WSH"));
        }

        public static Image getImage(String team) {
            Assembly a = Assembly.GetExecutingAssembly();
            String file = "NHL_Game_Chooser.logo." + team + ".png";
            Stream image = a.GetManifestResourceStream(file);

            if (image != null)
            {
                return Image.FromStream(image);
            }
            else
            {
                return null;
            }
        }

        private void setTodayButton_Click(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            logic.getDate(dateTimePicker1.Value.ToString("yyyy-MM-dd"));
            updateDisplay();
            //testDisplay();
        }

        private void updateDisplay()
        {
            mainGrid = new List<Button[]>(5);
            labels = new List<Label>(5);

            int windowSize = 124 + (57 * logic.numOfGames);

            this.Controls.Clear();
            this.Controls.Add(dateTimePicker1);
            this.Controls.Add(setTodayButton);
            this.Controls.Add(menuStrip);

            mainGrid.Clear();
            labels.Clear();

            for (int i = 0; i < logic.numOfGames; i++)
            {
                Game game = logic.games[i];
                Team home = teams[game.hta];
                Team away = teams[game.ata];
                int currentPosition = 80 + (59 * i-1);

                mainGrid.Add(new Button[2]);

                for (int j = 0; j < 2; ++j)
                {
                    mainGrid[i][j] = new Button();
                    mainGrid[i][j].Size = new Size(163, 32);
                    mainGrid[i][j].Click += new EventHandler(this.gameButtonClick);
                }

                mainGrid[i][0].Text = away.longName;
                mainGrid[i][0].Image = away.icon;
                mainGrid[i][0].Location = new Point(12,currentPosition);
                mainGrid[i][0].Name = "a" + i.ToString("00");
                mainGrid[i][0].ImageAlign = ContentAlignment.MiddleLeft;
                mainGrid[i][0].TextAlign = ContentAlignment.MiddleRight;

                mainGrid[i][1].Text = home.longName;
                mainGrid[i][1].Image = home.icon;
                mainGrid[i][1].Location = new Point(215,currentPosition);
                mainGrid[i][1].Name = "h" + i.ToString("00");
                mainGrid[i][1].ImageAlign = ContentAlignment.MiddleRight;
                mainGrid[i][1].TextAlign = ContentAlignment.MiddleLeft;

                labels.Add(new Label());
                labels[i].Text = "@";
                labels[i].Location = new Point(186,currentPosition+9);

                this.Controls.Add(mainGrid[i][0]);
                this.Controls.Add(mainGrid[i][1]);
                this.Controls.Add(labels[i]);
            }

            this.Size = new Size(406, windowSize);
        }

        private void gameButtonClick(object sender, EventArgs e)
        {
            String data = ((Button)sender).Name;
            bool home;

            //home/away
            if (data[0] == 'h')
            {
                home = true;
            }
            else if (data[0] == 'a')
            {
                home = false;
            }
            else
            {
                throw new Exception("Button name is neither home or away");
            }

            int gameID = logic.games[Convert.ToInt32(data.Substring(1))].id;

            if (logic.copyJar(gameID, home))
            {
                this.Size = new Size(this.Size.Width, this.Size.Height + 60);
                this.Controls.Add(outputBox);
                outputBox.Location = new Point(10, this.Size.Height - 100);
                outputBox.ScrollBars = ScrollBars.Vertical;

                process = logic.proc;
                Thread FNeulionUpdateThread = new Thread(new ThreadStart(updateFNeulionData));
                FNeulionUpdateThread.Start();

                //updateFNeulionData();
            }
        }

        private void updateFNeulionData()
        {
            while (!process.StandardOutput.EndOfStream)
            {
                string fuckNeulionOutput = process.StandardOutput.ReadLine();
                outputBox.Text += fuckNeulionOutput + "\r\n";
                outputBox.Update();
            }
        }

        private void selectFuckNeulionjarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fuckNeulion = new OpenFileDialog();
            fuckNeulion.Filter = "FuckNeulion.jar|*.jar";
            //fuckNeulion.Filter = "jar files (*.jar)|*.jar";

            if (fuckNeulion.ShowDialog() == DialogResult.OK)
            {
                logic.jarFile = fuckNeulion.FileName;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://xrxs.net/nhl/");
        }
    }

    class Logic
    {
        public String jarFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FuckNeulion.jar";
        public Game[] games;
        public int numOfGames;
        public Process proc;
        

        public bool copyJar(int gameID, bool home)
        {
            //String java = @"C:\Program Files\Java\jdk1.7.0_40\bin\java.exe";
            String toCopy = "/C java -jar " + '"' + jarFile + '"' + " " + gameID.ToString() + " ";

            toCopy += home ? "home" : "away"; //if home, append "home" or "away" to toCopy

            if (!File.Exists(jarFile))
            {
                MessageBox.Show("FuckNeulion.jar was not found. Ensure it is properly set in the file menu.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show(jarFile);
                return false;
            }
            else
            {
                runJar(toCopy);
                return true;
            }
        }

        private void runJar(String command)
        {
            //Configure process
            proc = new Process {
                StartInfo = new ProcessStartInfo {
                    //WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            proc.Start();
        }

        public Game[] parseData(String toParse)
        {
            Game[] games;

            String[] gameSplit = toParse.Split('{', '}'); //split data based on braces
            int numGames = gameSplit.Length - 4; //get number of games (-4 due to padding data, /2 to remove data between games)

            numGames -= (numGames / 2);
            numOfGames = numGames;

            if (numGames > 0)
            {
                games = new Game[numGames]; //create game array
            }
            else
            {
                numGames = 0; //error occured, fail
                return null;
            }

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Game));

            for (int i = 0; i < numGames; i++) //for each game
            {
                int n = 2 + (2 * i); //navigate around padding data

                String jsonData = "{" + gameSplit[n] + "}";
                games[i] = (Game)ser.ReadObject(GenerateStreamFromString(jsonData));
            }

            return games;
        }

        private MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        public Game[] getDate(String dateString)
        {
            string url = "http://live.nhle.com/GameData/GCScoreboard/" + dateString + ".jsonp";
            String toParse = WebInterface.getPage(url);
            Game[] gameList = parseData(toParse);
            games = gameList;
            return gameList;
        }
    }

    class WebInterface
    {
        //Control method for this class - chooses which method to use
        public static String getPage(String url)
        {
            return webClient(url);
        }

        //WebClient variation
        private static String webClient(String url)
        {
            WebClient client = new WebClient();
            string value = client.DownloadString(url);

            return value;
        }

        //WebRequest variation
        private static String webRequest(String url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            string html = String.Empty;
            using (StreamReader sr = new StreamReader(data))
            {
                html = sr.ReadToEnd();
            }
            return html;
        }

        public static void testTimes()
        {
            String url = "http://live.nhle.com/GameData/GCScoreboard/2015-05-01.jsonp";

            DateTime start1 = DateTime.Now;
            webClient(url);
            DateTime end1 = DateTime.Now;

            DateTime start2 = DateTime.Now;
            webRequest(url);
            DateTime end2 = DateTime.Now;

            TimeSpan t1 = end1 - start1;
            TimeSpan t2 = end2 - start2;

            Console.WriteLine("Web Client: " + t1.Milliseconds + "ms");
            Console.WriteLine("Web Request: " + t2.Milliseconds + "ms");
        }
    }

    [DataContract]
    class Game
    {
        [DataMember]
        public String hta;
        [DataMember]
        public String ata;
        [DataMember]
        public int id;

        public override string ToString()
        {
            return "hta:" + hta + ", ata:" + ata + ", id: " + id;
        }
    }
}

public struct Team {
    public string longName;
    public Image icon;
}

public class Teams : Dictionary<string, Team>
{
    public void Add(string shortN, string longN, Image ico)
    {
        Team team;
        team.longName = longN;
        team.icon = ico;
        this.Add(shortN, team);
    }
}

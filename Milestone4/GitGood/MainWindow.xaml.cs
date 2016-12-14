using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitGood
{
    class Git
    {
        static string pathToGit;
        public Git() {
            if (string.IsNullOrEmpty(pathToGit))
                pathToGit = "\"" + System.IO.Path.GetFullPath(@"PortableGit\bin\git.exe") + "\"";
        }

        public string command(string dir, string arguments)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = dir;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = pathToGit;
            Console.WriteLine("Path to git: " + pathToGit);
            Console.WriteLine("Dir: " + dir);
            Console.WriteLine("Args: " + arguments);
            startInfo.Verb = "runas";
            //startInfo.Arguments = @"git@github.com:scoochflex/CS489-Ass1.git" + " " + @"C:\Users\Don\Pictures\Overwolf";
            startInfo.Arguments = arguments;
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine("Output:\n\"" + output + "\"");
            return output;
        }
    }

    public class Branch{
        public string name;
        public string sha1;

        public Branch(string name, string sha1) {
            this.name = name;
            this.sha1 = sha1;
        }
    }
    public class Repo
    {
        public string remote_url;
        public string path;
        public Branch[] branches;
        public string name;
        public string error;
        Git git = new Git();

        public Repo(string remote_url, string path)
        {
            this.remote_url = remote_url;
            this.path = path;

            if (!this.remote_url.StartsWith("https://"))
            {
                this.error = "Can only clone from an https:// clone address...";
            }
            else {
                this.remote_url = this.remote_url.Insert(8,"FakeUser36:FakeUser366@");
                Console.WriteLine("Url with user and pass: " + this.remote_url);

                String result = this.git.command(path, " clone " + this.remote_url);

                this.name =  this.remote_url.Substring(this.remote_url.LastIndexOf('/')+1);
                this.name = this.name.Remove(this.name.Length-4);
                Console.WriteLine("Name determined to be: " + this.name);

                this.path += @"\" + this.name;

                if (Directory.Exists(this.path))
                {
                    this.determineBranches();
                }
                else {
                    this.error = "Could not clone repo...";
                }
            }
        }

        //Add repo from just path...
        public Repo(string path) {
            this.path = path;

            String result = this.git.command(path, " remote -v");

            if (string.IsNullOrEmpty(result))
            {
                this.error = "Not a valid git repo... Please clone a valid repo and try again...s";
            }
            else {
                //origin https://github.com/scoochflex/HC3-Assignment2 (fetch)
                string[] lines = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                string toParse = "";
                if (lines.Length > 0)
                {
                    toParse = lines[0];
                }
                else {
                    toParse = result;
                }

                string[] parts = (Regex.Split(toParse, @"\s+").Where(s => s != string.Empty)).ToArray<string>();

                if (parts.Length==3) {
                    this.remote_url = parts[1];
                    Console.WriteLine("Remote URL determined to be: " + this.remote_url);
                }

                if (!this.remote_url.StartsWith("https://"))
                {
                    this.error = "Can only clone from an https:// clone address...";
                }else
                {
                    this.name = this.remote_url.Substring(this.remote_url.LastIndexOf('/') + 1);
                    this.name = this.name.Remove(this.name.Length - 4);
                    Console.WriteLine("Name determined to be: " + this.name);

                    determineBranches();
                }
            }
            //Git.command("clone " + this.path);
            //Load url and name and current branch from path....
            //If good, populate name, else don't populate name, 
            //check for name value after using constructor to determine success
        }

        public void determineBranches()
        {
            Console.WriteLine("Determine branches...");
            String result = this.git.command(this.path, " branch -v");

            if (string.IsNullOrEmpty(result))
            {
                this.error = "Could not populate branch list";
            }
            else
            {
                //* master e72875a Initial commit
                string[] lines = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                int numBranches = lines.Length;
                this.branches = new Branch[numBranches];
                for (int i =0; i< numBranches; i++) {
                    Console.WriteLine("Branch line: " + lines[i]);
                    string[] parts = (Regex.Split(lines[i], @"\s+").Where(s => s != string.Empty)).ToArray<string>();
                    if (parts.Length>=3) {
                        Console.WriteLine("Parts of line: " + parts[0] + ", " + parts[1] + ", " + parts[2]);
                        this.branches[i] = new Branch(parts[1], parts[2]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Repo> repos = new List<Repo>();
        int selectedRepo = 0;

        public MainWindow()
        {
            InitializeComponent();
            RepoUrl.Text = "";//"git@github.com:scoochflex/HC3-Assignment2.git";
            RepoPath.Text = @"C:\Users\Don\Pictures\test1\HC3-Assignment2";
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuEmulLoadLast(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        //All files selection changed
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBlock_Initialized(object sender, EventArgs e)
        {
            ((TextBlock)sender).Text = "My Text \n Your Text";
        }

        private void repo_add_Click(object sender, RoutedEventArgs e)
        {
            Popup_RepoAdd.Visibility = System.Windows.Visibility.Visible;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // YesButton Clicked! Let's hide our InputBox and handle the input text.
            if (repos.Count == 0) {
                repo_list.Items.Remove(repo_none);
            }

            string repoUrl = RepoUrl.Text;
            string repoPath = RepoPath.Text;

            if (string.IsNullOrEmpty(repoUrl) && string.IsNullOrEmpty(repoPath))
            {
                RepoAddError.Content = "Please fill out the path to add an already instantiated repo,\n or fill out both feilds to clone the repo in the given path!";
            } else if(string.IsNullOrEmpty(repoPath))
            {
                //Path is empty, get mad
                RepoAddError.Content = "Please fill out the path!";
            }
            else if(string.IsNullOrEmpty(repoUrl))
            {
                //Path is filled, if url is empty try to inti from given path..
                Repo newRepo = new Repo(repoPath);
                if (string.IsNullOrEmpty(newRepo.error))
                {
                    MenuItem newMenuItem1 = new MenuItem();
                    repos.Add(newRepo);

                    //Index of repo in repo list
                    newMenuItem1.Name = "repo" + (repos.Count - 1).ToString();
                    newMenuItem1.Click += new RoutedEventHandler(repo_select);
                    newMenuItem1.Header = newRepo.name;

                    repo_list.Items.Add(newMenuItem1);

                    // Clear InputBox.
                    RepoUrl.Text = String.Empty;
                    RepoPath.Text = String.Empty;
                    statusText.Text = "Successfully added repo: " + repoUrl;
                }
                else
                {
                    RepoAddError.Content = newRepo.error;
                    statusText.Text = "Could not add repo, error: " + newRepo.error;
                }
                Popup_RepoAdd.Visibility = System.Windows.Visibility.Collapsed;
            }
            //Both feilds are filled out... Clone repo from url into path
            else {
                //Path is filled, if url is empty try to inti from given path..
                Repo newRepo = new Repo(repoUrl, repoPath);
                if (string.IsNullOrEmpty(newRepo.error))
                {
                    MenuItem newMenuItem1 = new MenuItem();                    
                    repos.Add(newRepo);

                    //Index of repo in repo list
                    newMenuItem1.Name = "repo" + (repos.Count - 1).ToString();
                    newMenuItem1.Click += new RoutedEventHandler(repo_select);
                    newMenuItem1.Header = newRepo.name;

                    repo_list.Items.Add(newMenuItem1);

                    // Clear InputBox.
                    RepoUrl.Text = String.Empty;
                    RepoPath.Text = String.Empty;
                    statusText.Text = "Successfully added repo: " + repoUrl;
                }
                else
                {
                    RepoAddError.Content = newRepo.error;
                    statusText.Text = "Could not add repo, error: " + newRepo.error;
                }
                Popup_RepoAdd.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        
        private void repo_select(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Got to repo select!!!!!!!! ");
            int num;
            bool res = int.TryParse(((MenuItem)sender).Name.Remove(0, 4),out num);
            if (res)
            {
                selectedRepo = num;
                CurrentRepoName.Content = repos[selectedRepo].name;
            }
            else {
                Console.WriteLine("Error parseing menu item num: " + ((MenuItem)sender).Name.Remove(0, 4));
            }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            // NoButton Clicked! Let's hide our InputBox.
            Popup_RepoAdd.Visibility = System.Windows.Visibility.Collapsed;

            // Clear InputBox.
            RepoUrl.Text = String.Empty;
            RepoPath.Text = String.Empty;
        }

        private void BranchSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

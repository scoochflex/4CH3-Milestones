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

        public string command(string dir, string arguments,bool returnExitCode = false)
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
            
            if (returnExitCode) {
                output = process.ExitCode.ToString();
            } 
            return output;
        }
    }

    public class Branch
    {
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
        public string remote_url_with_user;
        public string path;
        public List<Branch> branches;
        public int currentBranch = 0;
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
                this.remote_url_with_user = this.remote_url.Insert(8, "FakeUser36:FakeUser366@");
                Console.WriteLine("Url with user and pass: " + this.remote_url_with_user);

                String result = this.git.command(path, " clone " + this.remote_url_with_user);

                this.name = this.remote_url.Substring(this.remote_url.LastIndexOf('/') + 1);
                this.name = this.name.Remove(this.name.Length - 4);
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
        public Repo(string path)
        {
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

                if (parts.Length == 3) {
                    this.remote_url = parts[1];
                    Console.WriteLine("Remote URL determined to be: " + this.remote_url);
                }

                if (!this.remote_url.StartsWith("https://"))
                {
                    this.error = "Can only clone from an https:// clone address...";
                } else
                {
                    this.name = this.remote_url.Substring(this.remote_url.LastIndexOf('/') + 1);
                    this.name = this.name.Remove(this.name.Length - 4);
                    this.remote_url_with_user = this.remote_url.Insert(8, "FakeUser36:FakeUser366@");
                    Console.WriteLine("Name determined to be: " + this.name);

                    determineBranches();
                }
            }
            //Git.command("clone " + this.path);
            //Load url and name and current branch from path....
            //If good, populate name, else don't populate name, 
            //check for name value after using constructor to determine success
        }

        public void changeBranch(int branchNum)
        {
            Console.WriteLine(this.branches[branchNum].name);

            String result = this.git.command(this.path, " checkout " + this.branches[branchNum].name);

            if (string.IsNullOrEmpty(result))
            {
                this.error = "Not a valid git repo... Please clone a valid repo and try again...s";
            }
            else
            {
                Console.WriteLine("Successfully switched branches!");

            }
            this.currentBranch = branchNum;
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
                this.branches = new List<Branch>();
                for (int i = 0; i < numBranches; i++) {
                    Console.WriteLine("Branch line: " + lines[i]);
                    string[] parts = (Regex.Split(lines[i], @"\s+").Where(s => s != string.Empty)).ToArray<string>();
                    if (parts.Length >= 3) {
                        Console.WriteLine("Parts of line: " + parts[0] + ", " + parts[1] + ", " + parts[2]);
                        if (parts[0] == "*") {
                            this.branches.Add(new Branch(parts[1], parts[2]));
                            this.currentBranch = i;
                        } else {
                            this.branches.Add(new Branch(parts[0], parts[1]));
                        }
                    }
                }
            }
        }

        public List<String> getAllFiles() {
            //git ls-files
            List<String> files = new List<String>();
            String result = this.git.command(this.path, " ls-files");
            if (string.IsNullOrEmpty(result))
            {
                this.error = "Could not populate branch list";
            }
            else
            {
                string[] lines = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                foreach (String filePath in lines) {
                    files.Add(filePath);
                }
            }
            return files;
        }

        public List<String> getChangedFiles(Boolean untracked = true)
        {
            //git ls-files
            List<String> files = new List<String>();
            String result = this.git.command(this.path, " ls-files --modified");
            if (string.IsNullOrEmpty(result))
            {
                this.error = "Could not populate branch list";
            }
            else
            {
                string[] lines = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                foreach (String filePath in lines)
                {
                    files.Add(filePath);
                }
            }
            //Now get all untracked files
            if (untracked)
            {
                result = this.git.command(this.path, " ls-files --others --exclude-standard");
                if (string.IsNullOrEmpty(result))
                {
                    this.error = "Could not populate branch list";
                }
                else
                {
                    string[] lines = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    if (lines.Count() > 0)
                    {
                        files.Add("");
                        files.Add("Untracked Files:");
                        files.Add("");
                    }
                    foreach (String filePath in lines)
                    {
                        files.Add(filePath);
                    }
                }
            }
            return files;
        }

        public List<String> getStagedFiles()
        {
            //git ls-files
            List<String> files = new List<String>();
            String result = this.git.command(this.path, " diff --name-only --cached");
            if (string.IsNullOrEmpty(result))
            {
                this.error = "Could not populate branch list";
            }
            else
            {
                string[] lines = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                foreach (String filePath in lines)
                {
                    files.Add(filePath);
                }
            }
            return files;
        }

        public String getHeadVersion(String fullPath) {
            String headVer = "";
            headVer = git.command(this.path, " show HEAD:" + fullPath);
            if (headVer=="") {
                headVer = "No file found in HEAD";
            }
            return headVer;
        }

        public void stageFile(string selectedFilePath)
        {
            String result = this.git.command(this.path, " add " + selectedFilePath);
            if (string.IsNullOrEmpty(result))
            {
                this.error = "Could not populate branch list";
            }
        }

        public void unstageFile(string selectedFilePath)
        {
            String result = this.git.command(this.path, " reset HEAD " + selectedFilePath);
            if (string.IsNullOrEmpty(result))
            {
                this.error = "Could not populate branch list";
            }
        }

        public void discardChanges(string selectedFilePath)
        {
            String result = this.git.command(this.path, " checkout -- " + selectedFilePath);
            if (string.IsNullOrEmpty(result))
            {
                this.error = "";
            }
        }

        public void commit(string text)
        {
            String result = this.git.command(this.path, " commit -m " + "\""+text+"\"");
            if (string.IsNullOrEmpty(result))
            {
                this.error = "Could not commit!";
            }
        }

        public bool checkCommit() {
            bool res = true;
            String result = this.git.command(this.path, " log origin/" + this.branches[this.currentBranch].name + "..HEAD");
            if (string.IsNullOrEmpty(result))
            {
                this.error = "No commits!";
                res = false;
            }
            return res;
        }

        public void push()
        {
            String result = this.git.command(this.path, " push " + this.remote_url_with_user, true);
            if (string.IsNullOrEmpty(result))
            {
                this.error = "Could not push!";
            }
            else if (result != "0")
            {
                this.error = "Something went wrong! Exit code: " + result;
            }
            else {
                this.error = "";
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Repo> repos = new List<Repo>();
        int selectedRepo = -1;

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

        public void populateFileList() {
            //Ensure we have a selected repo and a selected branch...
            //git ls-tree --full-tree -r HEAD  -------------Lists all files git knows about
            //git status --porcelain ------------Lists alll modified or deleted files
            if (selectedRepo!=-1 && this.repos[selectedRepo].branches.Count>0) {
                List<String> allFilesList=  this.repos[selectedRepo].getAllFiles();
                AllFiles.Items.Clear();
                foreach (String path in allFilesList) {
                    if (path != "" && path != null)
                    {
                        ListBoxItem newFile = new ListBoxItem();
                        newFile.Content = path;
                        AllFiles.Items.Add(newFile);
                    }
                }                
            }
        }

        public void populateChangedFileList()
        {
            if(selectedRepo!=-1 && this.repos [selectedRepo].branches.Count>0) {
                List<String> allFilesList = this.repos[selectedRepo].getChangedFiles();
                ChangedFiles.Items.Clear();
                foreach (String path in allFilesList)
                {
                    if (path != "" && path != null)
                    {
                        ListBoxItem newFile = new ListBoxItem();
                        newFile.Content = path;
                        ChangedFiles.Items.Add(newFile);
                    }
                }
            }
        }

        public void populateStagedFileList()
        {
            if (selectedRepo != -1 && this.repos[selectedRepo].branches.Count > 0)
            {
                List<String> allFilesList = this.repos[selectedRepo].getStagedFiles();
                StagedFiles.Items.Clear();
                foreach (String path in allFilesList)
                {
                    if (path!="" && path!=null) {
                        ListBoxItem newFile = new ListBoxItem();
                        newFile.Content = path;
                        StagedFiles.Items.Add(newFile);
                    }
                }
            }
        }

        private void fileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String selectedFilePath = "";

            if (((sender as ListBox).SelectedItem as ListBoxItem)!=null) {
                selectedFilePath = ((sender as ListBox).SelectedItem as ListBoxItem).Content.ToString();
            }
            

            if (selectedFilePath!="" || selectedFilePath!= "Untracked Files:") {
                String fullPath = repos[selectedRepo].path + "\\" + selectedFilePath;
                Console.WriteLine("Full path determined to be: " + fullPath);
                if (File.Exists(fullPath)) {
                    LocalFileView.Content = File.ReadAllText(fullPath);
                } else {
                    LocalFileView.Content = "File at\n" + fullPath + "\nhas been deleted";
                }

                HeadFileView.Content = repos[selectedRepo].getHeadVersion(selectedFilePath);
            }
        }

        //Clicked in changes 
        private void StageSelected_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilePath = (ChangedFiles.SelectedItem as ListBoxItem).Content.ToString();
            if (selectedFilePath != "" || selectedFilePath != "Untracked Files:") {
                repos[selectedRepo].stageFile(selectedFilePath);
                populateStagedFileList();
                populateChangedFileList();
            }
        }


        private void StageAllChanges_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilePath = "";

            foreach (ListBoxItem item in ChangedFiles.Items) {
                selectedFilePath = item.Content.ToString();
                if (selectedFilePath != "" || selectedFilePath != "Untracked Files:")
                {
                    repos[selectedRepo].stageFile(selectedFilePath);
                }
            }
            populateStagedFileList();
            populateChangedFileList();
        }


        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilePath = (ChangedFiles.SelectedItem as ListBoxItem).Content.ToString();
            if (selectedFilePath != "" || selectedFilePath != "Untracked Files:")
            {
                repos[selectedRepo].discardChanges(selectedFilePath);
                populateStagedFileList();
                populateChangedFileList();
            }
        }


        private void Unstage_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilePath = (StagedFiles.SelectedItem as ListBoxItem).Content.ToString();
            if (selectedFilePath != "" || selectedFilePath != "Untracked Files:")
            {
                repos[selectedRepo].unstageFile(selectedFilePath);
                populateStagedFileList();
                populateChangedFileList();
            }
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
                    ComboBoxItem newMenuItem1 = new ComboBoxItem();
                    repos.Add(newRepo);

                    //Index of repo in repo list
                    newMenuItem1.Name = "repo" + (repos.Count - 1).ToString();
                    newMenuItem1.Content = newRepo.name;

                    repo_list.Items.Add(newMenuItem1);

                    // Clear InputBox.
                    RepoUrl.Text = String.Empty;
                    RepoPath.Text = String.Empty;
                    statusText.Text = "Successfully added repo: " + repoUrl;
                    populateBranchList();
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
                    ComboBoxItem newMenuItem1 = new ComboBoxItem();                    
                    repos.Add(newRepo);

                    //Index of repo in repo list
                    newMenuItem1.Name = "repo" + (repos.Count - 1).ToString();
                    newMenuItem1.Content = newRepo.name;

                    repo_list.Items.Add(newMenuItem1);

                    // Clear InputBox.
                    RepoUrl.Text = String.Empty;
                    RepoPath.Text = String.Empty;
                    statusText.Text = "Successfully added repo: " + repoUrl;
                    populateBranchList();
                }
                else
                {
                    RepoAddError.Content = newRepo.error;
                    statusText.Text = "Could not add repo, error: " + newRepo.error;
                }
                Popup_RepoAdd.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        
        private void repo_select(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            
            Console.WriteLine("Got to repo select!!!!!!!! ");
            
            selectedRepo = comboBox.SelectedIndex;
            String repoName = "";
            if ((comboBox.SelectedItem as ComboBoxItem)!=null) {
                //CurrentRepoName.Content = repos[selectedRepo].name;
                repoName = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
                if (repoName != "None") {
                    populateBranchList();
                    populateFileList();
                    populateChangedFileList();
                    populateStagedFileList();
                }
            }
        }

        public void populateBranchList() {
            if (selectedRepo!=-1) { 
                BranchList.Items.Clear();
                ListBoxItem newItem = new ListBoxItem();
                for (int i = 0; i < repos[selectedRepo].branches.Count; i++)
                {
                    newItem = new ListBoxItem();
                    newItem.Name = "BranchItem" + i.ToString();
                    newItem.Content = repos[selectedRepo].branches[i].name;
                    if (this.repos[selectedRepo].currentBranch == i)
                    {
                        newItem.Background = new LinearGradientBrush(Colors.LightBlue, Colors.SlateBlue, 90);
                        //newItem.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    BranchList.Items.Add(newItem);
                }
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
            if (selectedRepo!=-1 && this.repos[selectedRepo].branches.Count>0) { 
                ListBoxItem selected = (sender as ListBox).SelectedItem as ListBoxItem;
                int num = 0;
                bool res = int.TryParse(selected.Name.Remove(0,10), out num);
                if (res)
                {
                    int current = (repos[selectedRepo].currentBranch);
                    ((ListBoxItem)BranchList.Items[current]).Background = new LinearGradientBrush();
                    //Printing
                   /* foreach (ListBoxItem item in BranchList.Items) {
                        int tmp;
                        int.TryParse(item.Name.Remove(0, 10), out tmp);
                        Console.WriteLine("Item: " + tmp + ", name: " + item.Content);
                    }
                    for ( int i=0;i < repos[selectedRepo].branches.Count;i++) {
                        Console.WriteLine("Branch: " + i + ", name: " + repos[selectedRepo].branches[i].name);
                    }
                    */
                    Console.WriteLine("Current Branch hghlighted before: "+repos[selectedRepo].branches[current].name);

                    repos[selectedRepo].changeBranch(num);
                    Console.WriteLine("Current Branch hghlighted after: " + repos[selectedRepo].branches[num].name);
                    ListBoxItem after = (BranchList.Items[num]) as ListBoxItem;
                    after.Background = Brushes.Red;
                    populateFileList();
                    populateChangedFileList();
                    populateStagedFileList();
                }
                else
                {
                    Console.WriteLine("Error parsing menu item num: " + ((MenuItem)sender).Name.Remove(0, 4));
                }
            }
        }

        private void BranchYesButton_Click(object sender, RoutedEventArgs e)
        {
            PopupBranch.Visibility = Visibility.Collapsed;
            BranchError.Content = "";
        }

        private void BranchNoButton_Click(object sender, RoutedEventArgs e)
        {
            PopupBranch.Visibility = Visibility.Collapsed;
            BranchError.Content = "";
        }

        private void GenericYesButton_Click(object sender, RoutedEventArgs e)
        {
            PopupGeneric.Visibility = Visibility.Collapsed;
        }

        private void CommitViewYesButton_Click(object sender, RoutedEventArgs e)
        {
            PopupCommitView.Visibility = Visibility.Collapsed;
        }

        private void CommitViewNoButton_Click(object sender, RoutedEventArgs e)
        {
            PopupCommitView.Visibility = Visibility.Collapsed;
        }

        private void MergeYesButton_Click(object sender, RoutedEventArgs e)
        {
            PopupMerge.Visibility = Visibility.Collapsed;
        }

        private void MergeNoButton_Click(object sender, RoutedEventArgs e)
        {
            PopupMerge.Visibility = Visibility.Collapsed;
        }

        private void CommitYesButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommitMessage.Text != "")
            {
                if (selectedRepo != -1)
                {
                    List<String> changedFiles = repos[selectedRepo].getChangedFiles(false);
                    if (changedFiles.Count > 0)
                    {
                        CommitError.Content = "You still have unstaged changes!";
                    }
                    else
                    {
                        repos[selectedRepo].commit(CommitMessage.Text);
                        PopupCommit.Visibility = Visibility.Collapsed;
                        statusText.Text = "Commit successful!";
                    }
                }
            }
            else {
                CommitError.Content ="You need to enter a commit message before you can commit";
            }
        }

        private void CommitNoButton_Click(object sender, RoutedEventArgs e)
        {
            PopupCommit.Visibility = Visibility.Collapsed;
        }
        
        private void WorkflowCommit_Click(object sender, RoutedEventArgs e)
        {
            PopupCommit.Visibility = Visibility.Visible;
        }

        private void WorkflowPush_Click(object sender, RoutedEventArgs e)
        {
            PopupPush.Visibility = Visibility.Visible;
        }

        private void WorkflowCompare_Click(object sender, RoutedEventArgs e)
        {
            //PopupCommitView.Visibility = Visibility.Visible;
        }

        private void WorkflowPull_Click(object sender, RoutedEventArgs e)
        {
            PopupPull.Visibility = Visibility.Visible;
        }

        private void WorkflowBranch_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedRepo < 0)
            {
                BranchError.Content = "Warning: No repo selected";
            }
            else
            {
                List<string> data = new List<string>();
                foreach (Branch branch in this.repos[this.selectedRepo].branches)
                {
                    data.Add(branch.name);
                    Console.WriteLine(branch.name);
                }
                BranchesDropdown.ItemsSource = data;
            }
            PopupBranch.Visibility = Visibility.Visible;
        }

        private void WorkflowMerge_Click(object sender, RoutedEventArgs e)
        {
            PopupMerge.Visibility = Visibility.Visible;
            mergePopupBranchName.Content = repos[selectedRepo].branches[repos[selectedRepo].currentBranch].name;
        }

        private void PullNoButton_Click(object sender, RoutedEventArgs e)
        {
            PopupPull.Visibility = Visibility.Collapsed;
        }

        private void PullYesButton_Click(object sender, RoutedEventArgs e)
        {
            PopupPull.Visibility = Visibility.Collapsed;
        }

        private void PushNoButton_Click(object sender, RoutedEventArgs e)
        {
            PopupPush.Visibility = Visibility.Collapsed;
        }

        private void PushYesButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedRepo < 0)
            {
                PushError.Content = "No repo selected, cannot push!";
            }else
            {
                List<String> changedFiles = repos[selectedRepo].getChangedFiles(false);
                if (changedFiles.Count > 0)
                {
                    PushError.Content = "You still have unstaged changes!";
                }
                else
                {
                    //Check to see if there is a commit
                    if (this.repos[selectedRepo].checkCommit())
                    {
                        repos[selectedRepo].push();
                        if (repos[selectedRepo].error == "")
                        {
                            PopupCommit.Visibility = Visibility.Collapsed;
                            statusText.Text = "Push successful!";
                        }
                        else
                        {
                            statusText.Text = repos[selectedRepo].error;
                        }
                    }
                    else
                    {
                        statusText.Text = "Could not push! No unpushed commits detected";
                    }
                }

                PopupPush.Visibility = Visibility.Collapsed;
            }            
        }
    }
}

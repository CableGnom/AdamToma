using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F_D_F
{

    public partial class Form1 : Form
    {
        #region const
        const int MIN_THREAD = 1;
        #endregion
        #region global variables
        string[] fileArray;
        Queue<string> queue_4_search;

        int thread_No;
        Thread[] Search_threadsArray;//Searching/writing threads
        string path_save;//path 4 saving out file
        string name_save;//out file name
      
        bool stopButtonActionStatus = false;//Stop/Continue button changing

        //for thread sinchronisation
        object key_saving = new object(); 
        object key_comparing = new object();
        #endregion

        public void Get_File_Names(string given_folder)
        {
            //fill fileArray with all file names in selected dir
            try
            {
                fileArray = Directory.GetFiles(given_folder, "*.*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                Console.WriteLine(ex.ToString());
            }
        }

        public void Get_Queue_4_search()
            {
            queue_4_search = new Queue<string>();
            for (int i = 0; i < fileArray.Length - 1; i++)
                {
                for (int j = i + 1; j < fileArray.Length; j++)
                    {
                        //Filing queue in order witch files will be compared
                        queue_4_search.Enqueue(fileArray[i]);
                        queue_4_search.Enqueue(fileArray[j]);
                    }
                }
            }

        public void Run_search_threads(string action)
        {
            switch (action)
            {
                case ("Start"):
                    Get_Queue_4_search();
                    if (queue_4_search.Count>1)
                    {
                        for (int i = 0; i < thread_No; i++)
                        {
                            Search_threadsArray[i] = new Thread(new ThreadStart(Work_Giveaway));
                            Search_threadsArray[i].Start();
                        }
                    }
                    else
                    {
                        MessageBox.Show("STATUS: " + "File count< 1 no worc can be done here. Select another dir", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    }
                    break;

                case ("Stop"):
                    for (int i = 0; i < thread_No; i++)
                    {
                        if (Search_threadsArray[i].ThreadState == System.Threading.ThreadState.Running)
                        {
                            #pragma warning disable CS0618 // Type or member is obsolete
                            Search_threadsArray[i].Suspend();
                            #pragma warning restore CS0618 // Type or member is obsolete
                        }
                    }
                    break;

                case ("Continue"):
                    for (int i = 0; i < thread_No; i++)
                    {
                        if (Search_threadsArray[i].ThreadState == System.Threading.ThreadState.Suspended)
                        {
                            #pragma warning disable CS0618 // Type or member is obsolete
                            Search_threadsArray[i].Resume();
                            #pragma warning restore CS0618 // Type or member is obsolete
                        }
                    }
                    break;

                case ("Restart"):

                    Get_Queue_4_search();
                    for (int i = 0; i < thread_No; i++)
                    {
                        Search_threadsArray[i] = new Thread(new ThreadStart(Work_Giveaway));
                        Search_threadsArray[i].Start();
                    }
                    break;
            }
        }

        public void Work_Giveaway()
        {
            //Gives tasks to threads
            do
            {
                Monitor.Enter(key_comparing);
                if (queue_4_search.Count != 0)
                {
                    Search_4_Duplicates(queue_4_search.Dequeue(), queue_4_search.Dequeue()); //<-----Monitor.Exit(key_comparing);
                }
            } while (queue_4_search.Count > 0);
            Work_Done();
        }

        public void Work_Done()
        {
            int thread_id = Thread.CurrentThread.ManagedThreadId;
            MessageBox.Show("STATUS: " + "Work is done", "thread id:" + thread_id, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            Thread.CurrentThread.Abort();
        }

        public void Search_4_Duplicates(string F1_dir, string F2_dir)
        {
            //compares firs ch of files if they equal then compare files line by line
            Monitor.Exit(key_comparing);//Opens the lock 4 another thread

            StreamReader file1;
            StreamReader file2;
            try
            {
                file1 = new StreamReader(F1_dir);
                file2 = new StreamReader(F2_dir);

                char Ch_file1 = (char)file1.Read();
                char Ch_file2 = (char)file2.Read();

                if (Ch_file1 == Ch_file2)
                {
                    string Line_file1;
                    string Line_file2;
                    bool Dublicate_Files = true;

                    while (Dublicate_Files == true)
                    {
                        Line_file1 = file1.ReadLine();
                        Line_file2 = file2.ReadLine();

                        if (Line_file1 != Line_file2)
                        {
                            Dublicate_Files = false;
                        }

                        if (Line_file1 == null && Line_file2 == null)
                        {
                            //Saving_Results(F1_dir, F2_dir);
                            Saving_Results(Path.GetFileName(F1_dir),Path.GetFileName(F2_dir));
                            Dublicate_Files = false;
                        }
                    }
                }
                file1.Close();
                file2.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                Console.WriteLine(ex.ToString());
            }

        }
        
        public void Saving_Results(string F1_Name, string F2_Name)
        {
        //Save to file names of the files with dublicated content
            lock (key_saving)
            {
                string save_p = path_save + @"\" + name_save;

                if (!File.Exists(save_p))
                {
                    try
                    {
                        File.Create(save_p).Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                        Console.WriteLine(ex.ToString());
                    }
                }

                string appendText =F1_Name + " = " + F2_Name + Environment.NewLine;
                try
                {
                    File.AppendAllText(save_p, appendText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox t = (TextBox)sender;
            try
            {
                //check if input is integer
                int i = Convert.ToInt32(t.Text);
                //if input is lesser than min value then change it to min value
                if (i < MIN_THREAD)
                {
                    t.Text = MIN_THREAD.ToString();
                }
               
                thread_No =i;
                Search_threadsArray = new Thread[thread_No];
            }
            catch (Exception ex)
            {
             
                if (t.Text != "")
                {
                    MessageBox.Show("Exception: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    textBox1.Text = "";
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            TextBox t = (TextBox)sender;
            string out_file_name = Convert.ToString(t.Text);
            name_save = out_file_name + ".txt";
        }
 
        private void button1_Click(object sender, EventArgs e)
        {
                Run_search_threads("Start");
        } 

        private void button2_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (stopButtonActionStatus)//if button text is "Stop"
            {
                b.Text = "Stop";
                Run_search_threads("Continue");
            }
            else
            {
                b.Text = "Continue";
                Run_search_threads("Stop");
            }
            stopButtonActionStatus = !stopButtonActionStatus;
        }
            
        private void button3_Click(object sender, EventArgs e)
        {
            Run_search_threads("Continue");
        }

        private void button4_Click(object sender, EventArgs e)
        {

            Run_search_threads("Restart");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            FBD.Description = "Select folder for File Dublicates Search";
            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                label3.Text = FBD.SelectedPath;
                //new Thread(() => Get_File_Names(FBD.SelectedPath)).Start();
                Get_File_Names(FBD.SelectedPath);//<== Path to folder where to search
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            FBD.Description = "Select folder to saver results";
            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                label4.Text = FBD.SelectedPath;
                path_save = FBD.SelectedPath;//<== Path to where to save
            }
        }
    }
}
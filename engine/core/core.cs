// libs
using ui;
using utils;
using connection;
using System;
using System.IO;
using System.Runtime.InteropServices;


namespace core
{
    // main class for app
    class Core
    {
        // variables
        private UI display = new UI();

        // pages
        pageData descriptionSend = new pageData("Description to sending", "ESC > exit, Up Down > Navigation, Enter > input data, Space > continue", new string[] { "IP Address", "Port", "TCP/UDP", "Disk/Root path" });
        pageData descriptionReceive = new pageData("Description to receiving", "ESC > exit, Up Down > Navigation, Enter > input data, Space > continue", new string[] { "Port","TCP/UDP", "Root folder" ,"File name" });
        pageData connectionError = new pageData("Connection error", "Press ESC to exit...", new string[] { "Login or password not correct!" });
        pageData incorrectInput = new pageData("Input error", "Press ESC to continue...", new string[] { "Check inputed data and try again" });
        pageData successMessage = new pageData("Success", "Press ESC to continue...", new string[] { "Operation was successful" });


        // constructor
        public Core()
        {
            switch (display.Menu(new pageData("Select action", "ESC > exit, Up Down > Navigation, Enter > input data, Space > continue", new string[] { "Send", "Receive" })))
            {
                // send file
                case 0:
                    Sender();
                break;

                // receive file
                case 1:
                    Receive();
                break;
            }
        }

        // Application main loop
        public void Sender()
        {
            // variables
            string directory;
            string file;
            ConnectionSend con;


            // 
            // start application
            // 

            try
            {
                // get data about connection and file
                object[] connectionProperties = display.Input(descriptionSend);

                // select directory
                directory = SelectDirectory(connectionProperties[3].ToString());

                // select file
                file = SelectFile(directory);

                // open connection and send file
                con = new ConnectionSend(connectionProperties[0].ToString(), int.Parse(connectionProperties[1].ToString()), connectionProperties[2].ToString(), file);

            }

            catch
            {
                display.Message(incorrectInput);
            }
        }

        // method for receive file
        public void Receive()
        {
            // variables
            string[] directory;
            string file = "";
            ConnectionReceive con;


            // 
            // start application
            // 

            try
            {
                // get data about connection and file
                object[] connectionProperties = display.Input(descriptionReceive);

                // check for windows or linux

                // windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // folder processing
                    directory = SelectDirectory(connectionProperties[2].ToString()).Split("\\");

                    // creating string path to file
                    foreach (string folder in directory)
                    {
                        file += $"{folder}\\";
                    }
                }

                // linux and other
                else
                {
                    // folder processing
                    directory = SelectDirectory(connectionProperties[2].ToString()).Split("/");

                    // creating string path to file
                    foreach (string folder in directory)
                    {
                        file += $"{folder}/";
                    }
                }
                // add filename to file path
                file += connectionProperties[3].ToString();

                // open connection and receive file
                // con = new ConnectionReceive(connectionProperties[0].ToString(), int.Parse(connectionProperties[1].ToString()), connectionProperties[2].ToString(), file);
                con = new ConnectionReceive(int.Parse(connectionProperties[0].ToString()),connectionProperties[1].ToString(),file);

            }

            catch
            {
                display.Message(incorrectInput);
            }
        }
        // function for select directory
        private string SelectDirectory(string currentPath)
        {
            // variable for stor loop
            bool notSelected = true;

            // start main loop
            while (notSelected)
            {
                // try exception
                try
                {
                    // variable for directory list
                    string[] list = GetDirectoryList(currentPath);

                    // if folder is empty stop loop
                    if (list.Length == 0)
                    {
                        break;
                    }

                    // show folders
                    else
                    {
                        currentPath = list[display.Menu(new pageData("Select folder", "ESC > to select current folder, Up Down > Navigation, Left Right > Pages, Space > select", list))];
                    }
                }
                catch
                {
                    break;
                }
            }

            // return value
            return currentPath;
        }

        // function for select file
        private string SelectFile(string currentPath)
        {
            // variable for stor loop
            bool notSelected = true;

            // start main loop
            while (notSelected)
            {
                // try exception
                try
                {
                    // variable for directory list
                    string[] list = GetFileList(currentPath);

                    // if folder is empty stop loop
                    if (list.Length == 0)
                    {
                        break;
                    }

                    // show folders
                    else
                    {
                        currentPath = list[display.Menu(new pageData("Select file to sending", "Up Down > Navigation, Left Right > Pages, Space > select", list))];
                    }
                }
                catch
                {
                    break;
                }
            }

            // return value
            return currentPath;
        }

        // method for select file
        private string[] GetFileList(string path)
        {
            // variabls
            return Directory.GetFiles(path);
        }

        // method for select file
        private string[] GetDirectoryList(string path)
        {
            // variabls
            return Directory.GetDirectories(path);
        }
    }
}
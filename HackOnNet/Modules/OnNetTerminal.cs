using Hacknet;
using HackOnNet.GUI;
using HackOnNet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules
{
    class OnNetTerminal : OnCoreModule
    {
        public static float PROMPT_OFFSET = 0f;

        private System.Collections.Generic.List<string> history;

        private System.Collections.Generic.List<string> runCommands = new System.Collections.Generic.List<string>();

        private int commandHistoryOffset;

        public string currentLine;

        public string lastRunCommand;

        public string prompt;

        public string prefixcommand;

        public bool usingTabExecution = false;

        public bool preventingExecution = false;

        public bool executionPreventionIsInteruptable = false;

        private Color outlineColor = new Color(68, 68, 68);

        private Color backColor = new Color(8, 8, 8);

        private Color historyTextColor = new Color(220, 220, 220);

        private Color currentTextColor = Color.White;

        public OnNetTerminal(Rectangle location, UserScreen screen) : base(location, screen)
		{
            this.history = new System.Collections.Generic.List<string>(512);
            this.prompt = "";
            this.currentLine = "";
            Hacknet.Gui.TextBox.cursorPosition = 0;
            Hacknet.Gui.TextBox.textDrawOffsetPosition = 0;
        }

        public override void LoadContent()
        {
            this.history = new System.Collections.Generic.List<string>(512);
            this.runCommands = new System.Collections.Generic.List<string>(512);
            this.commandHistoryOffset = 0;
            this.currentLine = "";
            this.lastRunCommand = "";
            this.prompt = "{{blue}}"+userScreen.username+ "{{white}}$ ";
            Hacknet.GuiData.getFilteredKeys();
        }

        public override void Update(float t)
        {
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            float tinyFontCharHeight = GuiData.ActiveFontConfig.tinyFontCharHeight;
            this.spriteBatch.Draw(Utils.white, this.bounds, this.userScreen.displayModuleExtraLayerBackingColor);
            int num = (int)((float)(this.bounds.Height - 12) / (tinyFontCharHeight + 1f));
            num -= 3;
            num = System.Math.Min(num, this.history.Count);
            Vector2 input = new Vector2((float)(this.bounds.X + 4), (float)(this.bounds.Y + this.bounds.Height) - tinyFontCharHeight * 5f);
            if (num > 0)
            {
                for (int i = this.history.Count; i > this.history.Count - num; i--)
                {
                    try
                    {
                        this.spriteBatch.DrawFormatString(this.history[i - 1], Utils.ClipVec2ForTextRendering(input));
                        input.Y -= tinyFontCharHeight + 1f;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            this.doGui();
        }

        public void executeLine()
        {
            string text = this.currentLine;
            if (Hacknet.Gui.TextBox.MaskingText)
            {
                text = "";
                for (int i = 0; i < this.currentLine.Length; i++)
                {
                    text += "*";
                }
            }
            this.history.Add(this.prompt + text);
            this.lastRunCommand = this.currentLine;
            this.runCommands.Add(this.currentLine);
            /*if (!this.preventingExecution)
            {
                this.commandHistoryOffset = 0;
                this.os.execute(this.currentLine);
                if (this.currentLine.Length > 0)
                {
                    StatsManager.IncrementStat("commands_run", 1);
                }
            }*/
            this.userScreen.Execute(this.currentLine);
            this.currentLine = "";
            Hacknet.Gui.TextBox.cursorPosition = 0;
            Hacknet.Gui.TextBox.textDrawOffsetPosition = 0;
            this.executionPreventionIsInteruptable = false;
        }

        public string GetRecentTerminalHistoryString()
        {
            string text = "";
            int num = this.history.Count - 1;
            while (num > this.history.Count - 30 && this.history.Count > num)
            {
                text = text + this.history[num] + "\r\n";
                num--;
            }
            return text;
        }

        public System.Collections.Generic.List<string> GetRecentTerminalHistoryList()
        {
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            int num = 0;
            while (num < 30 && num < this.history.Count)
            {
                list.Add(this.history[num]);
                num++;
            }
            return list;
        }

        public void NonThreadedInstantExecuteLine()
        {
            /*string text = this.currentLine;
            if (Hacknet.Gui.TextBox.MaskingText)
            {
                text = "";
                for (int i = 0; i < this.currentLine.Length; i++)
                {
                    text += "*";
                }
            }
            this.history.Add(this.prompt + text);
            this.lastRunCommand = this.currentLine;
            this.runCommands.Add(this.currentLine);
            if (!this.preventingExecution)
            {
                this.commandHistoryOffset = 0;
                ProgramRunner.ExecuteProgram(this.os, this.currentLine.Split(new char[]
                {
                    ' '
                }));
            }
            this.currentLine = "";
            Hacknet.Gui.TextBox.cursorPosition = 0;
            Hacknet.Gui.TextBox.textDrawOffsetPosition = 0;
            this.executionPreventionIsInteruptable = false;*/
        }

        public void doGui()
        {
            SpriteFont tinyfont = GuiData.tinyfont;
            float tinyFontCharHeight = GuiData.ActiveFontConfig.tinyFontCharHeight;
            int num = -4;
            int num2 = (int)((float)(this.bounds.Y + this.bounds.Height - 16) - tinyFontCharHeight - (float)num);
            int i = (int)tinyfont.MeasureString(this.prompt.RemoveFormatting()).X;
            if (this.bounds.Width > 0)
            {
                while (i >= (int)((double)this.bounds.Width * 0.7))
                {
                    this.prompt = this.prompt.Substring(1);
                    i = (int)tinyfont.MeasureString(this.prompt.RemoveFormatting()).X;
                }
            }
            this.spriteBatch.DrawFormatString(this.prompt, new Vector2((float)(this.bounds.X + 3), (float)num2));
            if (Hacknet.Localization.LocaleActivator.ActiveLocaleIsCJK())
            {
                num -= 4;
            }
            num2 += num;
            if (/*this.userScreen.inputEnabled*/true)
            {
                if (!this.inputLocked)
                {
                    Hacknet.Gui.TextBox.LINE_HEIGHT = (int)(tinyFontCharHeight + 15f);
                    this.currentLine = Hacknet.Gui.TextBox.doTerminalTextField(7001, this.bounds.X + 3 + (int)Terminal.PROMPT_OFFSET + (int)tinyfont.MeasureString(this.prompt.RemoveFormatting()).X, num2, this.bounds.Width - i - 4, this.bounds.Height, 1, this.currentLine, tinyfont);
                    if (Hacknet.Gui.TextBox.BoxWasActivated)
                    {
                        //prefixcommand is used to make sure that commands which require a second level of input into the terminal not run the second level arguments as a seperate command.
                        //prefixcommand should be stuff like "login" or "help".
                        //prefixcommand is set in OnNetDisplayModule.cs.
                        if (prefixcommand == null)
                        {
                            this.executeLine();
                        }
                        else
                        {
                            this.currentLine = prefixcommand + ' ' + this.currentLine;
                            this.executeLine();
                            //whenever the login buttin was pressed, it would auto execute line and reset prefix command so it would treat the second level as an arugment anyways, so
                            //to combat this i checked if the previous executed command was a nothing command.
                            if(this.lastRunCommand != "" && prefixcommand != null)
                            {
                                prefixcommand = null;
                            }

                        }

                    }
                    if (Hacknet.Gui.TextBox.UpWasPresed)
                    {
                        if (this.runCommands.Count > 0)
                        {
                            this.commandHistoryOffset++;
                            if (this.commandHistoryOffset > this.runCommands.Count)
                            {
                                this.commandHistoryOffset = this.runCommands.Count;
                            }
                            this.currentLine = this.runCommands[this.runCommands.Count - this.commandHistoryOffset];
                            Hacknet.Gui.TextBox.cursorPosition = this.currentLine.Length;
                        }
                    }
                    if (Hacknet.Gui.TextBox.DownWasPresed)
                    {
                        if (this.commandHistoryOffset > 0)
                        {
                            this.commandHistoryOffset--;
                            if (this.commandHistoryOffset < 0)
                            {
                                this.commandHistoryOffset = 0;
                            }
                            if (this.commandHistoryOffset <= 0)
                            {
                                this.currentLine = "";
                            }
                            else
                            {
                                this.currentLine = this.runCommands[this.runCommands.Count - this.commandHistoryOffset];
                            }
                            Hacknet.Gui.TextBox.cursorPosition = this.currentLine.Length;
                        }
                    }
                    if (Hacknet.Gui.TextBox.TabWasPresed)
                    {
                        if (this.usingTabExecution)
                        {
                            this.executeLine();
                        }
                        else
                        {
                            this.doTabComplete();
                        }
                    }
                }
            }
        }

        public void doTabComplete()
        {
            /*System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            if (this.currentLine.Length != 0)
            {
                int num = this.currentLine.IndexOf(' ');
                if (num >= 1)
                {
                    string text = this.currentLine.Substring(num + 1);
                    string text2 = this.currentLine.Substring(0, num);
                    if (text2.Equals("upload") || text2.Equals("up"))
                    {
                        int num2 = text.LastIndexOf('/');
                        if (num2 < 0)
                        {
                            num2 = 0;
                        }
                        string text3 = text.Substring(0, num2) + "/";
                        if (text3.StartsWith("/"))
                        {
                            text3 = text3.Substring(1);
                        }
                        string text4 = text.Substring(num2 + ((num2 == 0) ? 0 : 1));
                        Folder folder = Programs.getFolderAtPathAsFarAsPossible(text, this.os, this.os.thisComputer.files.root);
                        bool flag = false;
                        if (folder == this.os.thisComputer.files.root && text3.Length > 1)
                        {
                            flag = true;
                        }
                        if (folder == null)
                        {
                            folder = this.os.thisComputer.files.root;
                        }
                        if (!flag)
                        {
                            for (int i = 0; i < folder.folders.Count; i++)
                            {
                                if (folder.folders[i].name.ToLower().StartsWith(text4.ToLower(), System.StringComparison.InvariantCultureIgnoreCase))
                                {
                                    list.Add(string.Concat(new string[]
                                    {
                                        text2,
                                        " ",
                                        text3,
                                        folder.folders[i].name,
                                        "/"
                                    }));
                                }
                            }
                            for (int i = 0; i < folder.files.Count; i++)
                            {
                                if (folder.files[i].name.ToLower().StartsWith(text4.ToLower()))
                                {
                                    list.Add(text2 + " " + text3 + folder.files[i].name);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (text == null || ((text.Equals("") || text.Length < 1) && !text2.Equals("exe")))
                        {
                            return;
                        }
                        Folder folder = Programs.getCurrentFolder(this.os);
                        for (int i = 0; i < folder.folders.Count; i++)
                        {
                            if (folder.folders[i].name.StartsWith(text, System.StringComparison.InvariantCultureIgnoreCase))
                            {
                                list.Add(text2 + " " + folder.folders[i].name + "/");
                            }
                        }
                        for (int i = 0; i < folder.files.Count; i++)
                        {
                            if (folder.files[i].name.StartsWith(text, System.StringComparison.InvariantCultureIgnoreCase))
                            {
                                list.Add(text2 + " " + folder.files[i].name);
                            }
                        }
                        if (list.Count == 0)
                        {
                            for (int i = 0; i < folder.files.Count; i++)
                            {
                                if (folder.files[i].name.StartsWith(text, System.StringComparison.InvariantCultureIgnoreCase))
                                {
                                    list.Add(text2 + " " + folder.files[i].name);
                                }
                            }
                        }
                    }
                }
                else
                {
                    System.Collections.Generic.List<string> list2 = new System.Collections.Generic.List<string>();
                    list2.AddRange(ProgramList.programs);
                    list2.AddRange(ProgramList.getExeList(this.os));
                    for (int i = 0; i < list2.Count; i++)
                    {
                        if (list2[i].ToLower().StartsWith(this.currentLine.ToLower()))
                        {
                            list.Add(list2[i]);
                        }
                    }
                }
                if (list.Count == 1)
                {
                    this.currentLine = list[0];
                    Hacknet.Gui.TextBox.moveCursorToEnd(this.currentLine);
                }
                else if (list.Count > 1)
                {
                    this.os.write(this.prompt + this.currentLine);
                    string text5 = list[0];
                    for (int i = 0; i < list.Count; i++)
                    {
                        this.os.write(list[i]);
                        for (int j = 0; j < text5.Length; j++)
                        {
                            if (list[i].Length <= j || string.Concat(text5[j]).ToLowerInvariant()[0] != string.Concat(list[i][j]).ToLowerInvariant()[0])
                            {
                                text5 = text5.Substring(0, j);
                                break;
                            }
                        }
                        this.currentLine = text5;
                        Hacknet.Gui.TextBox.moveCursorToEnd(this.currentLine);
                    }
                }
            }*/
        }

        public void writeLine(string text)
        {
            text = Utils.SuperSmartTwimForWidth(text, this.bounds.Width - 6, GuiData.tinyfont);
            string[] array = text.Split(new char[]
            {
                '\n'
            });
            for (int i = 0; i < array.Length; i++)
            {
                this.history.Add(array[i]);
            }
        }

        public void write(string text)
        {
            if (this.history.Count <= 0 || GuiData.tinyfont.MeasureString(this.history[this.history.Count - 1] + text).X > (float)(this.bounds.Width - 6))
            {
                this.writeLine(text);
            }
            else
            {
                System.Collections.Generic.List<string> list;
                int index;
                (list = this.history)[index = this.history.Count - 1] = list[index] + text;
            }
        }

        public void clearCurrentLine()
        {
            this.currentLine = "";
            Hacknet.Gui.TextBox.cursorPosition = 0;
            Hacknet.Gui.TextBox.textDrawOffsetPosition = 0;
        }

        public void reset()
        {
            this.history.Clear();
            this.clearCurrentLine();
        }

        public int commandsRun()
        {
            return this.runCommands.Count;
        }

        public string getLastRunCommand()
        {
            return this.lastRunCommand;
        }
    }
}

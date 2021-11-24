using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using WebREPLNETClient;
using System.Text.RegularExpressions;

namespace WebREPLNETDemo
{
    public partial class DemoForm : Form
    {
        private int CursorPos;
        private WebREPLConnection client;

        public DemoForm()
        {
            InitializeComponent();

            client = new WebREPLConnection();
            client.DataReceived += Client_DataReceived;
            client.FileRecieved += Client_FileRecieved;
            client.UpdateStatus += Client_UpdateStatus;
        }

        private void Client_FileRecieved(object sender, WebREPLConnection.FileReceivedEventArgs e)
        {
            // Invoke in UI Thread
            this.Invoke((MethodInvoker)(() =>
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.FileName = e.Filename;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllBytes(dialog.FileName, e.Data);
                }
            }));
        }

        private void Client_UpdateStatus(object sender, WebREPLConnection.UpdateFileStatus e)
        {
            // Invoke in UI Thread
            this.Invoke(new Action(() => FileStatusLabel.Text = e.Status));
        }

        private void Client_DataReceived(object sender, WebREPLConnection.DataReceivedEventArgs e)
        {
            // Invoke in UI Thread
            this.Invoke((MethodInvoker)(() => AddToConsole(e.Data)));
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (client.Connected == false)
            {
                client.Connect(UrlTextBox.Text);
            }
            else
            {
                client.Close();
            }

            ConsoleTextbox.Focus();
        }        

        private void GetFromDeviceButton_Click(object sender, EventArgs e)
        {
            bool result = client.get_file(FileToGetTextbox.Text);
        }

        private void SendFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string remoteFilename = System.IO.Path.GetFileName(dialog.FileName);
                byte[] data = System.IO.File.ReadAllBytes(dialog.FileName);
                client.put_file(remoteFilename, data);
            }
        }

        #region Console stuff

        // Append to Textbox
        private void AppendTextbox(string text)
        {
            ConsoleTextbox.AppendText(text);
            CursorPos = ConsoleTextbox.SelectionStart;
        }

        // Update in Textbox
        private void UpdateTextbox(string text)
        {
            if (CursorPos == ConsoleTextbox.TextLength)
            {
                AppendTextbox(text);
            }
            else
            {
                ConsoleTextbox.Text = ConsoleTextbox.Text.Remove(CursorPos, text.Length);
                ConsoleTextbox.Text = ConsoleTextbox.Text.Insert(CursorPos, text);
                CursorPos = CursorPos + text.Length;
            }
        }
        private void MoveCursorTextboxBack()
        {
            if (ConsoleTextbox.SelectionStart > 0)
            {
                ConsoleTextbox.SelectionStart--;
                CursorPos--;
            }
        }

        private void RemoveToEndOfLineTextbox(int num = 1)
        {
            int cpos = ConsoleTextbox.SelectionStart;

            while (cpos < ConsoleTextbox.Text.Length)
            {
                char ch = ConsoleTextbox.Text[cpos];
                if (ch == 10 || ch == 13) break;
                ConsoleTextbox.Text = ConsoleTextbox.Text.Remove(cpos, 1);
            }
        }

        private void AddToConsole(string data)
        {
            ConsoleTextbox.SelectionStart = CursorPos;
            while (data.Length > 0)
            {
                Token token = GetToken(ref data);

                if (token == null)
                {
                    continue;
                }
                if (token.Type == Token.TokenType.EscapeToken)
                {
                    switch (token.Char)
                    {
                        case "\b":
                            MoveCursorTextboxBack();
                            break;

                        case "\r":
                            // Ignore
                            break;

                        case "\n": // End of line
                        case "\r\n": // End of line

                            AppendTextbox("\r\n");
                            break;

                        case "K":
                            // Clear from cursor to end of line
                            RemoveToEndOfLineTextbox();
                            break;
                        case "D":
                            // Remove x number of chars
                            for (int i = 0; i < token.Count; i++) MoveCursorTextboxBack();
                            break;

                        case "J":
                            //ClearTextbox();

                            break;

                        default:
                            break;
                    }
                }
                else if (token.Type == Token.TokenType.Char)
                {
                    UpdateTextbox(token.Char);
                }
            }
        }

        private void ConsoleTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (client != null)
            {
                byte[] pressed;

                switch (e.KeyChar)
                {
                    case '\r':
                        pressed = new byte[] { 13, 10 };
                        break;
                    default:
                        pressed = new byte[] { (byte)e.KeyChar };
                        break;
                }

                client.Send(pressed, WebSocketMessageType.Text);
                e.Handled = true;
            }
        }

        // Hacked together ESC-handler....
        private Token GetToken(ref string sdata)
        {
            const string ESC = "\x1b";
            Token result;

            if (sdata.Length > 2)
            {
                if (sdata.Substring(0, 1) == ESC) // ESC
                {
                    string strE = "^\\x1b\\[([0-9]{1,3})(A|B|C|D|E|F|G)";
                    Match match = Regex.Match(sdata, strE, RegexOptions.None);
                    if (match.Success)
                    {
                        result = new Token(Token.TokenType.EscapeToken, match.Groups[2].Value, Convert.ToInt32(match.Groups[1].Value));
                        sdata = Regex.Replace(sdata, strE, "");
                        return result;
                    }
                    strE = "^\\x1b\\[(J|K)";  // Clear row
                    match = Regex.Match(sdata, strE, RegexOptions.None);
                    if (match.Success)
                    {
                        result = new Token(Token.TokenType.EscapeToken, match.Groups[1].Value);
                        sdata = Regex.Replace(sdata, strE, "");
                        return result;
                    }

                    // ESC[xm
                    strE = "^\\x1b\\[(\\d)m"; // Colors
                    match = Regex.Match(sdata, strE, RegexOptions.None);
                    if (match.Success)
                    {
                        result = new Token(Token.TokenType.EscapeToken, match.Groups[1].Value, 0, 0);
                        sdata = Regex.Replace(sdata, strE, "");
                        return result;
                    }

                    // ESC[x;ym
                    strE = "^\\x1b\\[(\\d);(\\d{1,3})m"; // Colors
                    match = Regex.Match(sdata, strE, RegexOptions.None);
                    if (match.Success)
                    {
                        result = new Token(Token.TokenType.EscapeToken, match.Groups[1].Value, Convert.ToInt32(match.Groups[2].Value), 0);
                        sdata = Regex.Replace(sdata, strE, "");
                        return result;
                    }

                    // ESC[x;y;zm
                    strE = "^\\x1b\\[(\\d);(\\d{1,3});(\\d{1,3})m"; // Colors
                    match = Regex.Match(sdata, strE, RegexOptions.None);
                    if (match.Success)
                    {
                        result = new Token(Token.TokenType.EscapeToken, match.Groups[1].Value, Convert.ToInt32(match.Groups[2].Value), Convert.ToInt32(match.Groups[3].Value));
                        sdata = Regex.Replace(sdata, strE, "");
                        return result;
                    }

                    // Error
                    sdata = sdata.Substring(1);
                    return null;
                }
                else  // Not starting with ESC
                {
                    Token.TokenType tokentype = Token.TokenType.Char;
                    if (sdata.StartsWith("\b"))
                    {
                        tokentype = Token.TokenType.EscapeToken;
                    }
                    else if (sdata.StartsWith("\r\n"))
                    {
                        tokentype = Token.TokenType.EscapeToken;
                        result = new Token(tokentype, sdata.Substring(0, 2));
                        sdata = sdata.Substring(2);
                        return result;
                    }
                    else if (sdata.StartsWith("\r") || sdata.StartsWith("\n"))
                    {
                        tokentype = Token.TokenType.EscapeToken;
                        result = new Token(tokentype, sdata.Substring(0, 1));
                        sdata = sdata.Substring(1);
                        return result;
                    }

                    result = new Token(tokentype, sdata.Substring(0, 1));
                    sdata = sdata.Substring(1);
                    return result;
                }
            }
            else if (sdata.Length > 0)  // Less than 2 chars in buffer
            {
                Token.TokenType tokentype = Token.TokenType.Char;
                if (sdata.StartsWith("\b"))
                {
                    tokentype = Token.TokenType.EscapeToken;
                }
                else if (sdata.StartsWith("\r") || sdata.StartsWith("\n"))
                {
                    tokentype = Token.TokenType.EscapeToken;
                }

                result = new Token(tokentype, sdata.Substring(0, 1));

                if (sdata.Length > 1)
                {
                    sdata = sdata.Substring(1);
                }
                else
                    sdata = "";
                return result;
            }
            else
            {
                result = new Token(Token.TokenType.Char, "");
                return result;
            }
        }
        #endregion

        #region Token class
        private class Token
        {
            public enum TokenType
            {
                Char,
                EscapeToken
            }

            public TokenType Type;
            public string Char = "";
            public int Count;
            public int Foreground;
            public int Background;
            private bool HasCount;

            public Token(TokenType type, string CharValue)
            {
                Char = CharValue;
                Type = type;
                HasCount = false;
            }

            public Token(TokenType type, string CharValue, int count)
            {
                Char = CharValue;
                Type = type;
                Count = count;
                HasCount = true;
            }

            public Token(TokenType type, string CharValue, int foreground, int background)
            {
                Char = CharValue;
                Type = type;
                Foreground = foreground;
                Background = background;
                HasCount = true;
            }

            public override string ToString()
            {
                switch (Type)
                {
                    case TokenType.Char:
                        return Char;
                    case TokenType.EscapeToken:
                        string result = "ESC[" + Count.ToString();

                        if ((char)Char[0] < 32)
                        {
                            result += "\\" + Convert.ToString((char)Char[0]);
                        }
                        else
                        {
                            result += Char;
                        }

                        return result;
                }

                return base.ToString();
            }
        }

        private void ConsoleTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            string tosend = "";
            bool send = false;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    tosend = "\x1b[A";
                    send = true;
                    e.Handled = true;
                    break;
                case Keys.Down:
                    tosend = "\x1b[B";
                    send = true;
                    e.Handled = true;
                    break;
                case Keys.Right:
                    tosend = "\x1b[C";
                    send = true;
                    e.Handled = true;
                    break;
                case Keys.Left:
                    tosend = "\x1b[D";
                    send = true;
                    e.Handled = true;
                    break;
                case Keys.Home:
                    tosend = "\x1b[G";
                    send = true;
                    e.Handled = true;
                    break;
                case Keys.PageUp:
                    tosend = "\x1b[A";
                    send = true;
                    e.Handled = true;
                    break;
                case Keys.PageDown:
                    tosend = "\x1b[B";
                    send = true;
                    e.Handled = true;
                    break;

            }
            if (send)
            {
                if (client != null)
                {
                    client.Send(tosend);
                }
            }
        }

        private void ConsoleTextbox_Click(object sender, EventArgs e)
        {
            ConsoleTextbox.SelectionStart = CursorPos;
        }

        private void UrlTextBox_TextChanged(object sender, EventArgs e)
        {
            ConnectButton.Enabled = (Uri.IsWellFormedUriString(UrlTextBox.Text, UriKind.RelativeOrAbsolute));
        }
    }
    #endregion
}

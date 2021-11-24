namespace WebREPLNETDemo
{
        partial class DemoForm
        {
            /// <summary>
            /// Required designer variable.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary>
            /// Clean up any resources being used.
            /// </summary>
            /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Windows Form Designer generated code

            /// <summary>
            /// Required method for Designer support - do not modify
            /// the contents of this method with the code editor.
            /// </summary>
            private void InitializeComponent()
            {
            this.ConsoleTextbox = new System.Windows.Forms.TextBox();
            this.UrlTextBox = new System.Windows.Forms.TextBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SendFileButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.FileToGetTextbox = new System.Windows.Forms.TextBox();
            this.GetFromDeviceButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.FileStatusLabel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConsoleTextbox
            // 
            this.ConsoleTextbox.Location = new System.Drawing.Point(12, 35);
            this.ConsoleTextbox.MaxLength = 327670;
            this.ConsoleTextbox.Multiline = true;
            this.ConsoleTextbox.Name = "ConsoleTextbox";
            this.ConsoleTextbox.Size = new System.Drawing.Size(533, 309);
            this.ConsoleTextbox.TabIndex = 0;
            this.ConsoleTextbox.Click += new System.EventHandler(this.ConsoleTextbox_Click);
            this.ConsoleTextbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConsoleTextbox_KeyDown);
            this.ConsoleTextbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ConsoleTextbox_KeyPress);
            // 
            // UrlTextBox
            // 
            this.UrlTextBox.Location = new System.Drawing.Point(12, 9);
            this.UrlTextBox.Name = "UrlTextBox";
            this.UrlTextBox.Size = new System.Drawing.Size(162, 22);
            this.UrlTextBox.TabIndex = 1;
            this.UrlTextBox.Text = "ws://192.168.4.1:8266/";
            this.UrlTextBox.TextChanged += new System.EventHandler(this.UrlTextBox_TextChanged);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(180, 8);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 23);
            this.ConnectButton.TabIndex = 2;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.SendFileButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(551, 35);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(173, 60);
            this.panel1.TabIndex = 3;
            // 
            // SendFileButton
            // 
            this.SendFileButton.Location = new System.Drawing.Point(6, 19);
            this.SendFileButton.Name = "SendFileButton";
            this.SendFileButton.Size = new System.Drawing.Size(75, 23);
            this.SendFileButton.TabIndex = 1;
            this.SendFileButton.Text = "Browse...";
            this.SendFileButton.UseVisualStyleBackColor = true;
            this.SendFileButton.Click += new System.EventHandler(this.SendFileButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Send a file";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.FileToGetTextbox);
            this.panel2.Controls.Add(this.GetFromDeviceButton);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Location = new System.Drawing.Point(551, 101);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(173, 86);
            this.panel2.TabIndex = 4;
            // 
            // FileToGetTextbox
            // 
            this.FileToGetTextbox.Location = new System.Drawing.Point(6, 19);
            this.FileToGetTextbox.Name = "FileToGetTextbox";
            this.FileToGetTextbox.Size = new System.Drawing.Size(152, 22);
            this.FileToGetTextbox.TabIndex = 3;
            // 
            // GetFromDeviceButton
            // 
            this.GetFromDeviceButton.Location = new System.Drawing.Point(6, 48);
            this.GetFromDeviceButton.Name = "GetFromDeviceButton";
            this.GetFromDeviceButton.Size = new System.Drawing.Size(104, 23);
            this.GetFromDeviceButton.TabIndex = 2;
            this.GetFromDeviceButton.Text = "Get from device";
            this.GetFromDeviceButton.UseVisualStyleBackColor = true;
            this.GetFromDeviceButton.Click += new System.EventHandler(this.GetFromDeviceButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Get a file";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.FileStatusLabel);
            this.panel3.Location = new System.Drawing.Point(551, 193);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(173, 36);
            this.panel3.TabIndex = 5;
            // 
            // FileStatusLabel
            // 
            this.FileStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileStatusLabel.Location = new System.Drawing.Point(0, 0);
            this.FileStatusLabel.Name = "FileStatusLabel";
            this.FileStatusLabel.Size = new System.Drawing.Size(173, 36);
            this.FileStatusLabel.TabIndex = 0;
            this.FileStatusLabel.Text = "Nothing yet";
            this.FileStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 386);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.UrlTextBox);
            this.Controls.Add(this.ConsoleTextbox);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DemoForm";
            this.Text = "DemoForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

            }

            #endregion

            private System.Windows.Forms.TextBox ConsoleTextbox;
            private System.Windows.Forms.TextBox UrlTextBox;
            private System.Windows.Forms.Button ConnectButton;
            private System.Windows.Forms.Panel panel1;
            private System.Windows.Forms.Button SendFileButton;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.Panel panel2;
            private System.Windows.Forms.TextBox FileToGetTextbox;
            private System.Windows.Forms.Button GetFromDeviceButton;
            private System.Windows.Forms.Label label2;
            private System.Windows.Forms.Panel panel3;
            private System.Windows.Forms.Label FileStatusLabel;
    }
    }

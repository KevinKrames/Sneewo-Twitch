﻿namespace SneetoApplication
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.consoleTextBox = new System.Windows.Forms.RichTextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.stemBox = new System.Windows.Forms.TextBox();
            this.stemButton = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.richTextMemory = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 337);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(104, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Convert Old Log";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(133, 337);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "Train";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // consoleTextBox
            // 
            this.consoleTextBox.Location = new System.Drawing.Point(157, 12);
            this.consoleTextBox.Name = "consoleTextBox";
            this.consoleTextBox.Size = new System.Drawing.Size(345, 300);
            this.consoleTextBox.TabIndex = 3;
            this.consoleTextBox.Text = "";
            this.consoleTextBox.TextChanged += new System.EventHandler(this.consoleTextBox_TextChanged);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(371, 337);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(131, 23);
            this.button4.TabIndex = 4;
            this.button4.Text = "MAKE A SENTENCE";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // stemBox
            // 
            this.stemBox.Location = new System.Drawing.Point(12, 65);
            this.stemBox.Name = "stemBox";
            this.stemBox.Size = new System.Drawing.Size(139, 20);
            this.stemBox.TabIndex = 5;
            // 
            // stemButton
            // 
            this.stemButton.Location = new System.Drawing.Point(13, 92);
            this.stemButton.Name = "stemButton";
            this.stemButton.Size = new System.Drawing.Size(75, 23);
            this.stemButton.TabIndex = 6;
            this.stemButton.Text = "Get Stem";
            this.stemButton.UseVisualStyleBackColor = true;
            this.stemButton.Click += new System.EventHandler(this.stemButton_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // richTextMemory
            // 
            this.richTextMemory.Location = new System.Drawing.Point(509, 13);
            this.richTextMemory.Name = "richTextMemory";
            this.richTextMemory.Size = new System.Drawing.Size(411, 299);
            this.richTextMemory.TabIndex = 7;
            this.richTextMemory.Text = "";
            this.richTextMemory.TextChanged += new System.EventHandler(this.richTextMemory_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(932, 372);
            this.Controls.Add(this.richTextMemory);
            this.Controls.Add(this.stemButton);
            this.Controls.Add(this.stemBox);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.consoleTextBox);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        public System.Windows.Forms.RichTextBox consoleTextBox;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox stemBox;
        private System.Windows.Forms.Button stemButton;
        private System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.RichTextBox richTextMemory;
    }
}


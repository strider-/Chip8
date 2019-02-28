namespace Emulator.Chip8.Display
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
            this.addressList = new System.Windows.Forms.ListBox();
            this.vRegisters = new System.Windows.Forms.Label();
            this.reset = new System.Windows.Forms.Button();
            this.output = new Emulator.Chip8.Display.BetterPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.output)).BeginInit();
            this.SuspendLayout();
            // 
            // addressList
            // 
            this.addressList.Font = new System.Drawing.Font("Source Code Pro", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addressList.FormattingEnabled = true;
            this.addressList.ItemHeight = 17;
            this.addressList.Location = new System.Drawing.Point(646, 6);
            this.addressList.Name = "addressList";
            this.addressList.Size = new System.Drawing.Size(234, 310);
            this.addressList.TabIndex = 1;
            // 
            // vRegisters
            // 
            this.vRegisters.AutoSize = true;
            this.vRegisters.Font = new System.Drawing.Font("Source Code Pro", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vRegisters.Location = new System.Drawing.Point(12, 323);
            this.vRegisters.Name = "vRegisters";
            this.vRegisters.Size = new System.Drawing.Size(88, 17);
            this.vRegisters.TabIndex = 2;
            this.vRegisters.Text = "vRegisters";
            // 
            // reset
            // 
            this.reset.Location = new System.Drawing.Point(646, 323);
            this.reset.Name = "reset";
            this.reset.Size = new System.Drawing.Size(75, 56);
            this.reset.TabIndex = 3;
            this.reset.Text = "Reset";
            this.reset.UseVisualStyleBackColor = true;
            this.reset.Click += new System.EventHandler(this.ResetGame);
            // 
            // output
            // 
            this.output.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.output.Location = new System.Drawing.Point(0, 0);
            this.output.Name = "output";
            this.output.Size = new System.Drawing.Size(640, 320);
            this.output.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.output.TabIndex = 0;
            this.output.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 391);
            this.Controls.Add(this.reset);
            this.Controls.Add(this.vRegisters);
            this.Controls.Add(this.addressList);
            this.Controls.Add(this.output);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            ((System.ComponentModel.ISupportInitialize)(this.output)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BetterPictureBox output;
        private System.Windows.Forms.ListBox addressList;
        private System.Windows.Forms.Label vRegisters;
        private System.Windows.Forms.Button reset;
    }
}


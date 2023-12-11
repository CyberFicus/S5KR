namespace Simple_RSC
{
    partial class RS_form
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RS_form));
            start_btn = new Button();
            dblabel = new Label();
            txtstart_btn = new Button();
            input_textBox = new TextBox();
            RichTextBox = new RichTextBox();
            label_info = new Label();
            btn_save_txt = new Button();
            SuspendLayout();
            // 
            // start_btn
            // 
            start_btn.Location = new Point(12, 66);
            start_btn.Name = "start_btn";
            start_btn.Size = new Size(142, 34);
            start_btn.TabIndex = 0;
            start_btn.Text = "Рассчитать";
            start_btn.UseVisualStyleBackColor = true;
            start_btn.Click += start_btn_Click;
            // 
            // dblabel
            // 
            dblabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            dblabel.AutoSize = true;
            dblabel.Font = new Font("Segoe UI", 6F, FontStyle.Regular, GraphicsUnit.Point);
            dblabel.Location = new Point(3, 435);
            dblabel.Name = "dblabel";
            dblabel.Size = new Size(17, 12);
            dblabel.TabIndex = 1;
            dblabel.Text = "---";
            // 
            // txtstart_btn
            // 
            txtstart_btn.Location = new Point(12, 106);
            txtstart_btn.Name = "txtstart_btn";
            txtstart_btn.Size = new Size(142, 34);
            txtstart_btn.TabIndex = 2;
            txtstart_btn.Text = "Из файла";
            txtstart_btn.UseVisualStyleBackColor = true;
            txtstart_btn.Click += txtstart_btn_Click;
            // 
            // input_textBox
            // 
            input_textBox.Location = new Point(12, 32);
            input_textBox.Name = "input_textBox";
            input_textBox.Size = new Size(142, 27);
            input_textBox.TabIndex = 3;
            input_textBox.Text = "310; 0; 2; 1";
            // 
            // RichTextBox
            // 
            RichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            RichTextBox.Location = new Point(160, 12);
            RichTextBox.Name = "RichTextBox";
            RichTextBox.ReadOnly = true;
            RichTextBox.Size = new Size(628, 420);
            RichTextBox.TabIndex = 4;
            RichTextBox.Text = resources.GetString("RichTextBox.Text");
            // 
            // label_info
            // 
            label_info.AutoSize = true;
            label_info.Location = new Point(12, 9);
            label_info.Name = "label_info";
            label_info.Size = new Size(96, 20);
            label_info.TabIndex = 6;
            label_info.Text = "gen; b; td; err";
            // 
            // btn_save_txt
            // 
            btn_save_txt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btn_save_txt.Location = new Point(12, 398);
            btn_save_txt.Name = "btn_save_txt";
            btn_save_txt.Size = new Size(142, 34);
            btn_save_txt.TabIndex = 8;
            btn_save_txt.Text = "Сохранить";
            btn_save_txt.UseVisualStyleBackColor = true;
            btn_save_txt.Click += btn_save_txt_Click;
            // 
            // RS_form
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btn_save_txt);
            Controls.Add(label_info);
            Controls.Add(RichTextBox);
            Controls.Add(input_textBox);
            Controls.Add(txtstart_btn);
            Controls.Add(dblabel);
            Controls.Add(start_btn);
            Name = "RS_form";
            Text = "Simple RSC";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button start_btn;
        private Label dblabel;
        private Button txtstart_btn;
        private TextBox input_textBox;
        private RichTextBox RichTextBox;
        private Label label_info;
        private Button btn_save_txt;
    }
}
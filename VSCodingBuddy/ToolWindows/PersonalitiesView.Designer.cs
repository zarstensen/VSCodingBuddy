namespace VSCodingBuddy.ToolWindows
{
    partial class PersonalitiesView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PersonalitiesBox = new System.Windows.Forms.GroupBox();
            this.CompressCompileError = new System.Windows.Forms.Button();
            this.CompressException = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.NameLabel = new System.Windows.Forms.Label();
            this.CompileErrorTextArea = new System.Windows.Forms.TextBox();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.NewButton = new System.Windows.Forms.Button();
            this.PersonalityList = new System.Windows.Forms.ListBox();
            this.CompileErrorPromptLabel = new System.Windows.Forms.Label();
            this.ExceptionTextArea = new System.Windows.Forms.TextBox();
            this.ExceptionPromptLabel = new System.Windows.Forms.Label();
            this.PersonalitiesBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PersonalitiesBox
            // 
            this.PersonalitiesBox.AutoSize = true;
            this.PersonalitiesBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PersonalitiesBox.Controls.Add(this.CompressCompileError);
            this.PersonalitiesBox.Controls.Add(this.CompressException);
            this.PersonalitiesBox.Controls.Add(this.SaveButton);
            this.PersonalitiesBox.Controls.Add(this.NameTextBox);
            this.PersonalitiesBox.Controls.Add(this.NameLabel);
            this.PersonalitiesBox.Controls.Add(this.CompileErrorTextArea);
            this.PersonalitiesBox.Controls.Add(this.DeleteButton);
            this.PersonalitiesBox.Controls.Add(this.NewButton);
            this.PersonalitiesBox.Controls.Add(this.PersonalityList);
            this.PersonalitiesBox.Controls.Add(this.CompileErrorPromptLabel);
            this.PersonalitiesBox.Controls.Add(this.ExceptionTextArea);
            this.PersonalitiesBox.Controls.Add(this.ExceptionPromptLabel);
            this.PersonalitiesBox.Location = new System.Drawing.Point(3, 0);
            this.PersonalitiesBox.Name = "PersonalitiesBox";
            this.PersonalitiesBox.Size = new System.Drawing.Size(536, 321);
            this.PersonalitiesBox.TabIndex = 0;
            this.PersonalitiesBox.TabStop = false;
            this.PersonalitiesBox.Text = "Personalities";
            // 
            // CompressCompileError
            // 
            this.CompressCompileError.Location = new System.Drawing.Point(242, 233);
            this.CompressCompileError.Name = "CompressCompileError";
            this.CompressCompileError.Size = new System.Drawing.Size(112, 23);
            this.CompressCompileError.TabIndex = 12;
            this.CompressCompileError.Text = "Compress";
            this.CompressCompileError.UseVisualStyleBackColor = true;
            this.CompressCompileError.Click += new System.EventHandler(this.CompressCompileError_Click);
            // 
            // CompressException
            // 
            this.CompressException.Location = new System.Drawing.Point(242, 158);
            this.CompressException.Name = "CompressException";
            this.CompressException.Size = new System.Drawing.Size(112, 23);
            this.CompressException.TabIndex = 11;
            this.CompressException.Text = "Compress";
            this.CompressException.UseVisualStyleBackColor = true;
            this.CompressException.Click += new System.EventHandler(this.CompressException_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(124, 91);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(112, 23);
            this.SaveButton.TabIndex = 10;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(6, 136);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(112, 22);
            this.NameTextBox.TabIndex = 9;
            this.NameTextBox.TextChanged += new System.EventHandler(this.NameTextBox_TextChanged);
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(3, 117);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(44, 16);
            this.NameLabel.TabIndex = 8;
            this.NameLabel.Text = "Name";
            // 
            // CompileErrorTextArea
            // 
            this.CompileErrorTextArea.AcceptsReturn = true;
            this.CompileErrorTextArea.Location = new System.Drawing.Point(5, 262);
            this.CompileErrorTextArea.Multiline = true;
            this.CompileErrorTextArea.Name = "CompileErrorTextArea";
            this.CompileErrorTextArea.Size = new System.Drawing.Size(524, 38);
            this.CompileErrorTextArea.TabIndex = 4;
            this.CompileErrorTextArea.TextChanged += new System.EventHandler(this.CompileErrorTextArea_TextChanged);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(242, 91);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(112, 23);
            this.DeleteButton.TabIndex = 7;
            this.DeleteButton.Text = "Delete";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // NewButton
            // 
            this.NewButton.Location = new System.Drawing.Point(6, 91);
            this.NewButton.Name = "NewButton";
            this.NewButton.Size = new System.Drawing.Size(112, 23);
            this.NewButton.TabIndex = 5;
            this.NewButton.Text = "New";
            this.NewButton.UseVisualStyleBackColor = true;
            this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
            // 
            // PersonalityList
            // 
            this.PersonalityList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PersonalityList.FormattingEnabled = true;
            this.PersonalityList.ItemHeight = 16;
            this.PersonalityList.Location = new System.Drawing.Point(8, 21);
            this.PersonalityList.Name = "PersonalityList";
            this.PersonalityList.Size = new System.Drawing.Size(523, 68);
            this.PersonalityList.TabIndex = 0;
            this.PersonalityList.SelectedIndexChanged += new System.EventHandler(this.PersonalityList_SelectedIndexChanged);
            // 
            // CompileErrorPromptLabel
            // 
            this.CompileErrorPromptLabel.AutoSize = true;
            this.CompileErrorPromptLabel.Location = new System.Drawing.Point(3, 236);
            this.CompileErrorPromptLabel.Name = "CompileErrorPromptLabel";
            this.CompileErrorPromptLabel.Size = new System.Drawing.Size(135, 16);
            this.CompileErrorPromptLabel.TabIndex = 3;
            this.CompileErrorPromptLabel.Text = "Compile Error Prompt";
            // 
            // ExceptionTextArea
            // 
            this.ExceptionTextArea.AcceptsReturn = true;
            this.ExceptionTextArea.Location = new System.Drawing.Point(6, 187);
            this.ExceptionTextArea.Multiline = true;
            this.ExceptionTextArea.Name = "ExceptionTextArea";
            this.ExceptionTextArea.Size = new System.Drawing.Size(523, 38);
            this.ExceptionTextArea.TabIndex = 2;
            this.ExceptionTextArea.TextChanged += new System.EventHandler(this.ExceptionTextArea_TextChanged);
            // 
            // ExceptionPromptLabel
            // 
            this.ExceptionPromptLabel.AutoSize = true;
            this.ExceptionPromptLabel.Location = new System.Drawing.Point(3, 161);
            this.ExceptionPromptLabel.Name = "ExceptionPromptLabel";
            this.ExceptionPromptLabel.Size = new System.Drawing.Size(112, 16);
            this.ExceptionPromptLabel.TabIndex = 1;
            this.ExceptionPromptLabel.Text = "Exception Prompt";
            // 
            // PersonalitiesView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PersonalitiesBox);
            this.Name = "PersonalitiesView";
            this.Size = new System.Drawing.Size(540, 388);
            this.PersonalitiesBox.ResumeLayout(false);
            this.PersonalitiesBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox PersonalitiesBox;
        private System.Windows.Forms.Label CompileErrorPromptLabel;
        private System.Windows.Forms.TextBox ExceptionTextArea;
        private System.Windows.Forms.Label ExceptionPromptLabel;
        private System.Windows.Forms.ListBox PersonalityList;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button NewButton;
        private System.Windows.Forms.TextBox CompileErrorTextArea;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button CompressException;
        private System.Windows.Forms.Button CompressCompileError;
    }
}

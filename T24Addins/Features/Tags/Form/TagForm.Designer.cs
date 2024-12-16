namespace T24AddIns.Features.Tags.Form
{
    partial class TagForm
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
            this.TagDoorBtn = new System.Windows.Forms.Button();
            this.AddPropertiesBtn = new System.Windows.Forms.Button();
            this.TagWindowBtn = new System.Windows.Forms.Button();
            this.GenerateScheduleBtn = new System.Windows.Forms.Button();
            this.TagWallBtn = new System.Windows.Forms.Button();
            this.ExitBtn = new System.Windows.Forms.Button();
            this.K24Tool = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TagDoorBtn
            // 
            this.TagDoorBtn.Location = new System.Drawing.Point(38, 93);
            this.TagDoorBtn.Name = "TagDoorBtn";
            this.TagDoorBtn.Size = new System.Drawing.Size(117, 37);
            this.TagDoorBtn.TabIndex = 0;
            this.TagDoorBtn.Text = "Tag Doors";
            this.TagDoorBtn.UseVisualStyleBackColor = true;
            // 
            // AddPropertiesBtn
            // 
            this.AddPropertiesBtn.Location = new System.Drawing.Point(214, 93);
            this.AddPropertiesBtn.Name = "AddPropertiesBtn";
            this.AddPropertiesBtn.Size = new System.Drawing.Size(117, 37);
            this.AddPropertiesBtn.TabIndex = 1;
            this.AddPropertiesBtn.Text = "Add Properties";
            this.AddPropertiesBtn.UseVisualStyleBackColor = true;
            this.AddPropertiesBtn.Click += new System.EventHandler(this.AddPropertiesBtn_Click);
            // 
            // TagWindowBtn
            // 
            this.TagWindowBtn.Location = new System.Drawing.Point(38, 168);
            this.TagWindowBtn.Name = "TagWindowBtn";
            this.TagWindowBtn.Size = new System.Drawing.Size(117, 37);
            this.TagWindowBtn.TabIndex = 2;
            this.TagWindowBtn.Text = "Tag Windows";
            this.TagWindowBtn.UseVisualStyleBackColor = true;
            // 
            // GenerateScheduleBtn
            // 
            this.GenerateScheduleBtn.Location = new System.Drawing.Point(214, 168);
            this.GenerateScheduleBtn.Name = "GenerateScheduleBtn";
            this.GenerateScheduleBtn.Size = new System.Drawing.Size(117, 37);
            this.GenerateScheduleBtn.TabIndex = 3;
            this.GenerateScheduleBtn.Text = "Generate Schedules";
            this.GenerateScheduleBtn.UseVisualStyleBackColor = true;
            // 
            // TagWallBtn
            // 
            this.TagWallBtn.Location = new System.Drawing.Point(38, 237);
            this.TagWallBtn.Name = "TagWallBtn";
            this.TagWallBtn.Size = new System.Drawing.Size(117, 37);
            this.TagWallBtn.TabIndex = 4;
            this.TagWallBtn.Text = "Tag Walls";
            this.TagWallBtn.UseVisualStyleBackColor = true;
            // 
            // ExitBtn
            // 
            this.ExitBtn.BackColor = System.Drawing.Color.Transparent;
            this.ExitBtn.Location = new System.Drawing.Point(214, 237);
            this.ExitBtn.Name = "ExitBtn";
            this.ExitBtn.Size = new System.Drawing.Size(117, 37);
            this.ExitBtn.TabIndex = 5;
            this.ExitBtn.Text = "Exit";
            this.ExitBtn.UseVisualStyleBackColor = false;
            // 
            // K24Tool
            // 
            this.K24Tool.AutoSize = true;
            this.K24Tool.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.K24Tool.Location = new System.Drawing.Point(82, 34);
            this.K24Tool.Name = "K24Tool";
            this.K24Tool.Size = new System.Drawing.Size(211, 26);
            this.K24Tool.TabIndex = 6;
            this.K24Tool.Text = "K2D T24 Revit Tools";
            // 
            // TagForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 331);
            this.Controls.Add(this.K24Tool);
            this.Controls.Add(this.ExitBtn);
            this.Controls.Add(this.TagWallBtn);
            this.Controls.Add(this.GenerateScheduleBtn);
            this.Controls.Add(this.TagWindowBtn);
            this.Controls.Add(this.AddPropertiesBtn);
            this.Controls.Add(this.TagDoorBtn);
            this.Name = "TagForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button TagDoorBtn;
        private System.Windows.Forms.Button AddPropertiesBtn;
        private System.Windows.Forms.Button TagWindowBtn;
        private System.Windows.Forms.Button GenerateScheduleBtn;
        private System.Windows.Forms.Button TagWallBtn;
        private System.Windows.Forms.Button ExitBtn;
        private System.Windows.Forms.Label K24Tool;
    }
}
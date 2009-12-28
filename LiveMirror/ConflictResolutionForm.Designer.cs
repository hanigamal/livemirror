namespace LiveMirror
{
    partial class ConflictResolutionForm
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
            this.pnlConflicts = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // pnlConflicts
            // 
            this.pnlConflicts.AutoScroll = true;
            this.pnlConflicts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlConflicts.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlConflicts.Location = new System.Drawing.Point(0, 0);
            this.pnlConflicts.Name = "pnlConflicts";
            this.pnlConflicts.Size = new System.Drawing.Size(424, 451);
            this.pnlConflicts.TabIndex = 0;
            this.pnlConflicts.WrapContents = false;
            // 
            // ConflictResolutionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 451);
            this.Controls.Add(this.pnlConflicts);
            this.MinimumSize = new System.Drawing.Size(440, 100);
            this.Name = "ConflictResolutionForm";
            this.Text = "ConflictResolution";
            this.Load += new System.EventHandler(this.ConflictResolutionForm_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ConflictResolutionForm_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel pnlConflicts;
    }
}
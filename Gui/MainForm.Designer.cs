namespace GenArt
{
    partial class MainForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.tmrRedraw = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.picPattern = new System.Windows.Forms.PictureBox();
            this.pnlCanvas = new System.Windows.Forms.PictureBox();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.tsslFittnessError = new System.Windows.Forms.ToolStripStatusLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.trackBarScale = new System.Windows.Forms.TrackBar();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelFitness = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelGeneration = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelSelected = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelPoints = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelPolygons = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelAvgPoints = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chbShowProgress = new System.Windows.Forms.CheckBox();
            this.chbLastGen = new System.Windows.Forms.CheckBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPattern)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlCanvas)).BeginInit();
            this.statusStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarScale)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Source image";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(76, 33);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // tmrRedraw
            // 
            this.tmrRedraw.Interval = 1000;
            this.tmrRedraw.Tick += new System.EventHandler(this.tmrRedraw_Tick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Generated image";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 40);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.picPattern);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.pnlCanvas);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(1282, 512);
            this.splitContainer1.SplitterDistance = 660;
            this.splitContainer1.TabIndex = 21;
            // 
            // picPattern
            // 
            this.picPattern.Location = new System.Drawing.Point(12, 16);
            this.picPattern.Name = "picPattern";
            this.picPattern.Size = new System.Drawing.Size(640, 480);
            this.picPattern.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picPattern.TabIndex = 3;
            this.picPattern.TabStop = false;
            // 
            // pnlCanvas
            // 
            this.pnlCanvas.Location = new System.Drawing.Point(6, 16);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Size = new System.Drawing.Size(269, 167);
            this.pnlCanvas.TabIndex = 9;
            this.pnlCanvas.TabStop = false;
            this.pnlCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlCanvas_Paint);
            // 
            // statusStrip2
            // 
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslFittnessError});
            this.statusStrip2.Location = new System.Drawing.Point(0, 646);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(1282, 25);
            this.statusStrip2.TabIndex = 5;
            this.statusStrip2.Text = "statusStrip2";
            // 
            // tsslFittnessError
            // 
            this.tsslFittnessError.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.tsslFittnessError.ForeColor = System.Drawing.Color.Black;
            this.tsslFittnessError.Name = "tsslFittnessError";
            this.tsslFittnessError.Size = new System.Drawing.Size(41, 20);
            this.tsslFittnessError.Text = "error";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(325, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(119, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "Generated image scale:";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(697, 20);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(19, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "10";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(175, 14);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(13, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "1";
            // 
            // trackBarScale
            // 
            this.trackBarScale.Location = new System.Drawing.Point(166, 30);
            this.trackBarScale.Minimum = 1;
            this.trackBarScale.Name = "trackBarScale";
            this.trackBarScale.Size = new System.Drawing.Size(550, 45);
            this.trackBarScale.TabIndex = 21;
            this.trackBarScale.Value = 1;
            this.trackBarScale.Scroll += new System.EventHandler(this.trackBarScale_Scroll);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabelFitness,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabelGeneration,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabelSelected,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabelPoints,
            this.toolStripStatusLabel5,
            this.toolStripStatusLabelPolygons,
            this.toolStripStatusLabel6,
            this.toolStripStatusLabelAvgPoints});
            this.statusStrip1.Location = new System.Drawing.Point(0, 621);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1282, 25);
            this.statusStrip1.Stretch = false;
            this.statusStrip1.TabIndex = 26;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(53, 20);
            this.toolStripStatusLabel1.Text = "Fitness";
            // 
            // toolStripStatusLabelFitness
            // 
            this.toolStripStatusLabelFitness.AutoSize = false;
            this.toolStripStatusLabelFitness.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelFitness.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabelFitness.Name = "toolStripStatusLabelFitness";
            this.toolStripStatusLabelFitness.Size = new System.Drawing.Size(166, 20);
            this.toolStripStatusLabelFitness.Spring = true;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(82, 20);
            this.toolStripStatusLabel2.Text = "Generation";
            // 
            // toolStripStatusLabelGeneration
            // 
            this.toolStripStatusLabelGeneration.AutoSize = false;
            this.toolStripStatusLabelGeneration.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelGeneration.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabelGeneration.Name = "toolStripStatusLabelGeneration";
            this.toolStripStatusLabelGeneration.Size = new System.Drawing.Size(166, 20);
            this.toolStripStatusLabelGeneration.Spring = true;
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(66, 20);
            this.toolStripStatusLabel3.Text = "Selected";
            // 
            // toolStripStatusLabelSelected
            // 
            this.toolStripStatusLabelSelected.AutoSize = false;
            this.toolStripStatusLabelSelected.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabelSelected.Name = "toolStripStatusLabelSelected";
            this.toolStripStatusLabelSelected.Size = new System.Drawing.Size(166, 20);
            this.toolStripStatusLabelSelected.Spring = true;
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(49, 20);
            this.toolStripStatusLabel4.Text = "Points";
            // 
            // toolStripStatusLabelPoints
            // 
            this.toolStripStatusLabelPoints.AutoSize = false;
            this.toolStripStatusLabelPoints.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabelPoints.Name = "toolStripStatusLabelPoints";
            this.toolStripStatusLabelPoints.Size = new System.Drawing.Size(166, 20);
            this.toolStripStatusLabelPoints.Spring = true;
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(69, 20);
            this.toolStripStatusLabel5.Text = "Polygons";
            // 
            // toolStripStatusLabelPolygons
            // 
            this.toolStripStatusLabelPolygons.AutoSize = false;
            this.toolStripStatusLabelPolygons.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabelPolygons.Name = "toolStripStatusLabelPolygons";
            this.toolStripStatusLabelPolygons.Size = new System.Drawing.Size(166, 20);
            this.toolStripStatusLabelPolygons.Spring = true;
            // 
            // toolStripStatusLabel6
            // 
            this.toolStripStatusLabel6.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
            this.toolStripStatusLabel6.Size = new System.Drawing.Size(118, 20);
            this.toolStripStatusLabel6.Text = "Points / polygon";
            // 
            // toolStripStatusLabelAvgPoints
            // 
            this.toolStripStatusLabelAvgPoints.AutoSize = false;
            this.toolStripStatusLabelAvgPoints.Name = "toolStripStatusLabelAvgPoints";
            this.toolStripStatusLabelAvgPoints.Size = new System.Drawing.Size(117, 17);
            this.toolStripStatusLabelAvgPoints.Spring = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chbShowProgress);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.chbLastGen);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.trackBarScale);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.btnStart);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 552);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1282, 69);
            this.panel1.TabIndex = 27;
            // 
            // chbShowProgress
            // 
            this.chbShowProgress.AutoSize = true;
            this.chbShowProgress.Location = new System.Drawing.Point(780, 43);
            this.chbShowProgress.Name = "chbShowProgress";
            this.chbShowProgress.Size = new System.Drawing.Size(96, 17);
            this.chbShowProgress.TabIndex = 26;
            this.chbShowProgress.Text = "Show progress";
            this.chbShowProgress.UseVisualStyleBackColor = true;
            // 
            // chbLastGen
            // 
            this.chbLastGen.AutoSize = true;
            this.chbLastGen.Location = new System.Drawing.Point(780, 20);
            this.chbLastGen.Name = "chbLastGen";
            this.chbLastGen.Size = new System.Drawing.Size(159, 17);
            this.chbLastGen.TabIndex = 25;
            this.chbLastGen.Text = "Show best in last generation";
            this.chbLastGen.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.MinimumSize = new System.Drawing.Size(0, 40);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1282, 40);
            this.toolStrip1.TabIndex = 10;
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(95, 37);
            this.toolStripButton1.Text = "Open Image";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(44, 37);
            this.toolStripButton2.Text = "Save";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1282, 671);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.statusStrip2);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Genetic Vectorizer by Roger Alsing ";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPattern)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlCanvas)).EndInit();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarScale)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        
        private System.Windows.Forms.PictureBox picPattern;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Timer tmrRedraw;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar trackBarScale;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFitness;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelGeneration;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSelected;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelPoints;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelPolygons;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelAvgPoints;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel6;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox chbLastGen;
        private System.Windows.Forms.CheckBox chbShowProgress;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripStatusLabel tsslFittnessError;
        private System.Windows.Forms.PictureBox pnlCanvas;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
    }
}


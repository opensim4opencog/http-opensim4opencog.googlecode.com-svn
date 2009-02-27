// Copyright 2003 Eric Marchesin - <eric.marchesin@laposte.net>
//
// This source file(s) may be redistributed by any means PROVIDING they
// are not sold for profit without the authors expressed written consent,
// and providing that this notice and the authors name and all copyright
// notices remain intact.
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED. USE IT AT YOUR OWN RISK. THE AUTHOR ACCEPTS NO
// LIABILITY FOR ANY DATA DAMAGE/LOSS THAT THIS PRODUCT MAY CAUSE.
//-----------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using cogbot.TheOpenSims.Navigation;


namespace cogbot.TheOpenSims.Navigation.Debug
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
    public partial class GraphFormer : System.Windows.Forms.Form
	{
        public readonly static float DSCALE = 4.5f;
        volatile public static GraphFormer DEBUGGER = null;
		#region Construction / Destruction

		private ContextMenu MenuContextuel;
		APropos DialogueAPropos = new APropos();
		private FlickerFreePanel GraphPanel;

		private System.Windows.Forms.ToolBarButton AEtoileEtape;
		private System.Windows.Forms.ImageList ImagesActions;
		private System.Windows.Forms.ImageList ImagesPasAPas;
		private System.Windows.Forms.ToolBar FichierToolBar;
		private System.Windows.Forms.ImageList ImagesFichier;
		private System.Windows.Forms.ToolBarButton BoutonSauver;
		private System.Windows.Forms.ToolBarButton BoutonCharger;
		private System.Windows.Forms.ToolBarButton Sep1;
		private System.Windows.Forms.ToolBarButton Sep2;
		private System.Windows.Forms.Label LabelAide;
		private System.Windows.Forms.ToolBarButton BoutonNouveau;
		private System.Windows.Forms.ToolBarButton BoutonDessiner;
		private System.Windows.Forms.ToolBarButton BoutonEffacer;
		private System.Windows.Forms.ToolBarButton BoutonDeplacer;
		private System.Windows.Forms.ToolBarButton BoutonChangerEtat;
		private System.Windows.Forms.ToolBarButton BoutonAEtoile;
		private System.Windows.Forms.ToolBarButton BoutonAProposDe;
		private System.Windows.Forms.StatusBar GraphStatusBar;
		private System.Windows.Forms.StatusBarPanel NbNodesPanel;
		private System.Windows.Forms.StatusBarPanel NbArcsPanel;
		private System.Windows.Forms.StatusBarPanel CoordsPanel;
		private System.Windows.Forms.ToolBar EditionToolBar;
		private System.Windows.Forms.ToolBar AEtoileToolBar;
		private System.Windows.Forms.ToolBarButton AEtoileDebut;
		private System.Windows.Forms.ToolBarButton AEtoileFin;
		private System.ComponentModel.IContainer components;

		static int Rayon = 6;
		static int Epaisseur = 1;
		static Pen CrayonNoeuds;
		static Pen CrayonArcs;
		static Pen CrayonNoeudsInactifs;
		static Pen CrayonArcsInactifs;
		static Pen CrayonTemp;
		static Pen CrayonChemin;
        static Pen CrayonArcsPas;
        static Pen CrayonArcsWeak;
        static Pen CrayonArcsStrong;
        SimPathStore PathStore;

        static GraphFormer()
		{
			CrayonNoeuds = new Pen(Color.Black, Epaisseur);
            CrayonNoeudsInactifs = new Pen(Color.Red, Epaisseur);
            CrayonArcsWeak = new Pen(Color.Pink, Epaisseur);
            CrayonArcsStrong = new Pen(Color.Green, Epaisseur);

			CrayonArcs = new Pen(Color.Black, Epaisseur);
            // Arrows
            //CrayonArcs.EndCap = LineCap.Custom;
			//CrayonArcs.CustomEndCap = new AdjustableArrowCap(3, 6, true);

            CrayonArcsInactifs = new Pen(Color.Red, Epaisseur);
            // Arrows
            //CrayonArcsInactifs.EndCap = LineCap.Custom;
			//CrayonArcsInactifs.CustomEndCap = new AdjustableArrowCap(3, 6, true);

			CrayonTemp = new Pen(Color.Gray, Epaisseur);
			CrayonTemp.DashStyle = DashStyle.Dash;

			CrayonChemin = new Pen(Color.DarkTurquoise, 3);
			CrayonChemin.DashStyle = DashStyle.Dot;

            CrayonArcsPas = new Pen(Color.LawnGreen, 3);
            // Arrows
            //CrayonArcsPas.EndCap = LineCap.Custom;
			//CrayonArcsPas.CustomEndCap = new AdjustableArrowCap(4, 8, true);
		}

		public GraphFormer(SimPathStore SPS)
		{
            DEBUGGER = this;
            PathStore = SPS;

			MenuContextuel = new ContextMenu();
			MenuContextuel.MenuItems.Add(new MenuItem("Automatic", new EventHandler(ChoixAutomatique)));
			MenuContextuel.MenuItems.Add(new MenuItem("Step by step", new EventHandler(ChoixPasAPas)));
			InitializeComponent();
			BoutonAEtoile.DropDownMenu = MenuContextuel;

            G = SPS;// SimPathStore.Default;
			NouveauGraphe();
			Mode = Action.Dessiner;
		}

        public void Reactivate()
        {
            this.Show();
            //if (this.WindowState == FormWindowState.Minimized)              
            this.WindowState = FormWindowState.Normal;
            this.Visible = true;
            this.Activate();
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing && components != null) components.Dispose();
			base.Dispose( disposing );
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.GraphPanel = new cogbot.TheOpenSims.Navigation.Debug.FlickerFreePanel();
            this.EditionToolBar = new System.Windows.Forms.ToolBar();
            this.BoutonDessiner = new System.Windows.Forms.ToolBarButton();
            this.BoutonEffacer = new System.Windows.Forms.ToolBarButton();
            this.BoutonDeplacer = new System.Windows.Forms.ToolBarButton();
            this.BoutonChangerEtat = new System.Windows.Forms.ToolBarButton();
            this.BoutonAEtoile = new System.Windows.Forms.ToolBarButton();
            this.ImagesActions = new System.Windows.Forms.ImageList(this.components);
            this.GraphStatusBar = new System.Windows.Forms.StatusBar();
            this.NbNodesPanel = new System.Windows.Forms.StatusBarPanel();
            this.NbArcsPanel = new System.Windows.Forms.StatusBarPanel();
            this.CoordsPanel = new System.Windows.Forms.StatusBarPanel();
            this.FichierToolBar = new System.Windows.Forms.ToolBar();
            this.BoutonNouveau = new System.Windows.Forms.ToolBarButton();
            this.BoutonCharger = new System.Windows.Forms.ToolBarButton();
            this.BoutonSauver = new System.Windows.Forms.ToolBarButton();
            this.BoutonAProposDe = new System.Windows.Forms.ToolBarButton();
            this.Sep1 = new System.Windows.Forms.ToolBarButton();
            this.ImagesFichier = new System.Windows.Forms.ImageList(this.components);
            this.AEtoileToolBar = new System.Windows.Forms.ToolBar();
            this.Sep2 = new System.Windows.Forms.ToolBarButton();
            this.AEtoileDebut = new System.Windows.Forms.ToolBarButton();
            this.AEtoileEtape = new System.Windows.Forms.ToolBarButton();
            this.AEtoileFin = new System.Windows.Forms.ToolBarButton();
            this.ImagesPasAPas = new System.Windows.Forms.ImageList(this.components);
            this.LabelAide = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.NbNodesPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbArcsPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CoordsPanel)).BeginInit();
            this.SuspendLayout();
            // 
            // GraphPanel
            // 
            this.GraphPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.GraphPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.GraphPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GraphPanel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GraphPanel.Location = new System.Drawing.Point(0, 24);
            this.GraphPanel.Name = "GraphPanel";
            this.GraphPanel.Size = new System.Drawing.Size(744, 470);
            this.GraphPanel.TabIndex = 2;
            this.GraphPanel.MouseLeave += new System.EventHandler(this.GraphPanel_MouseLeave);
            this.GraphPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphPanel_Paint);
            this.GraphPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GraphPanel_MouseMove);
            this.GraphPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GraphPanel_MouseDown);
            this.GraphPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GraphPanel_MouseUp);
            // 
            // EditionToolBar
            // 
            this.EditionToolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.EditionToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.BoutonDessiner,
            this.BoutonEffacer,
            this.BoutonDeplacer,
            this.BoutonChangerEtat,
            this.BoutonAEtoile});
            this.EditionToolBar.ButtonSize = new System.Drawing.Size(16, 16);
            this.EditionToolBar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EditionToolBar.Divider = false;
            this.EditionToolBar.Dock = System.Windows.Forms.DockStyle.None;
            this.EditionToolBar.DropDownArrows = true;
            this.EditionToolBar.ImageList = this.ImagesActions;
            this.EditionToolBar.Location = new System.Drawing.Point(104, 0);
            this.EditionToolBar.Name = "EditionToolBar";
            this.EditionToolBar.ShowToolTips = true;
            this.EditionToolBar.Size = new System.Drawing.Size(136, 26);
            this.EditionToolBar.TabIndex = 0;
            this.EditionToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.GraphToolBar_ButtonClick);
            // 
            // BoutonDessiner
            // 
            this.BoutonDessiner.ImageIndex = 0;
            this.BoutonDessiner.Name = "BoutonDessiner";
            this.BoutonDessiner.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.BoutonDessiner.Tag = 0;
            this.BoutonDessiner.ToolTipText = "Draw nodes and arcs";
            // 
            // BoutonEffacer
            // 
            this.BoutonEffacer.ImageIndex = 1;
            this.BoutonEffacer.Name = "BoutonEffacer";
            this.BoutonEffacer.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.BoutonEffacer.Tag = 1;
            this.BoutonEffacer.ToolTipText = "Erase nodes and arcs";
            // 
            // BoutonDeplacer
            // 
            this.BoutonDeplacer.ImageIndex = 2;
            this.BoutonDeplacer.Name = "BoutonDeplacer";
            this.BoutonDeplacer.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.BoutonDeplacer.Tag = 2;
            this.BoutonDeplacer.ToolTipText = "Move nodes";
            // 
            // BoutonChangerEtat
            // 
            this.BoutonChangerEtat.ImageIndex = 3;
            this.BoutonChangerEtat.Name = "BoutonChangerEtat";
            this.BoutonChangerEtat.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.BoutonChangerEtat.Tag = 3;
            this.BoutonChangerEtat.ToolTipText = "Change the state of nodes and arcs";
            // 
            // BoutonAEtoile
            // 
            this.BoutonAEtoile.ImageIndex = 4;
            this.BoutonAEtoile.Name = "BoutonAEtoile";
            this.BoutonAEtoile.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
            this.BoutonAEtoile.Tag = 4;
            this.BoutonAEtoile.ToolTipText = "Place starting and ending flags, then find the best path.";
            // 
            // ImagesActions
            // 
            this.ImagesActions.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ImagesActions.ImageSize = new System.Drawing.Size(16, 16);
            this.ImagesActions.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // GraphStatusBar
            // 
            this.GraphStatusBar.Location = new System.Drawing.Point(0, 494);
            this.GraphStatusBar.Name = "GraphStatusBar";
            this.GraphStatusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.NbNodesPanel,
            this.NbArcsPanel,
            this.CoordsPanel});
            this.GraphStatusBar.ShowPanels = true;
            this.GraphStatusBar.Size = new System.Drawing.Size(744, 24);
            this.GraphStatusBar.TabIndex = 1;
            this.GraphStatusBar.Text = "GraphStatusBar";
            // 
            // NbNodesPanel
            // 
            this.NbNodesPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.NbNodesPanel.Name = "NbNodesPanel";
            this.NbNodesPanel.Text = "NbNodes";
            this.NbNodesPanel.ToolTipText = "Number of nodes";
            this.NbNodesPanel.Width = 61;
            // 
            // NbArcsPanel
            // 
            this.NbArcsPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.NbArcsPanel.Name = "NbArcsPanel";
            this.NbArcsPanel.Text = "NbArcs";
            this.NbArcsPanel.ToolTipText = "Number of arcs";
            this.NbArcsPanel.Width = 51;
            // 
            // CoordsPanel
            // 
            this.CoordsPanel.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            this.CoordsPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.CoordsPanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            this.CoordsPanel.Name = "CoordsPanel";
            this.CoordsPanel.Text = "Coordinates";
            this.CoordsPanel.Width = 616;
            // 
            // FichierToolBar
            // 
            this.FichierToolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.FichierToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.BoutonNouveau,
            this.BoutonCharger,
            this.BoutonSauver,
            this.BoutonAProposDe,
            this.Sep1});
            this.FichierToolBar.ButtonSize = new System.Drawing.Size(16, 16);
            this.FichierToolBar.Divider = false;
            this.FichierToolBar.Dock = System.Windows.Forms.DockStyle.None;
            this.FichierToolBar.DropDownArrows = true;
            this.FichierToolBar.ImageList = this.ImagesFichier;
            this.FichierToolBar.Location = new System.Drawing.Point(0, 0);
            this.FichierToolBar.Name = "FichierToolBar";
            this.FichierToolBar.ShowToolTips = true;
            this.FichierToolBar.Size = new System.Drawing.Size(104, 26);
            this.FichierToolBar.TabIndex = 0;
            this.FichierToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.FichierToolBar_ButtonClick);
            // 
            // BoutonNouveau
            // 
            this.BoutonNouveau.ImageIndex = 0;
            this.BoutonNouveau.Name = "BoutonNouveau";
            this.BoutonNouveau.Tag = 0;
            this.BoutonNouveau.ToolTipText = "Clear the current graph to create a new one";
            // 
            // BoutonCharger
            // 
            this.BoutonCharger.ImageIndex = 1;
            this.BoutonCharger.Name = "BoutonCharger";
            this.BoutonCharger.Tag = 1;
            this.BoutonCharger.ToolTipText = "Load a graph";
            // 
            // BoutonSauver
            // 
            this.BoutonSauver.ImageIndex = 2;
            this.BoutonSauver.Name = "BoutonSauver";
            this.BoutonSauver.Tag = 2;
            this.BoutonSauver.ToolTipText = "Save the current graph";
            // 
            // BoutonAProposDe
            // 
            this.BoutonAProposDe.ImageIndex = 3;
            this.BoutonAProposDe.Name = "BoutonAProposDe";
            this.BoutonAProposDe.Tag = 3;
            this.BoutonAProposDe.ToolTipText = "About GraphFormer...";
            // 
            // Sep1
            // 
            this.Sep1.Name = "Sep1";
            this.Sep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // ImagesFichier
            // 
            this.ImagesFichier.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ImagesFichier.ImageSize = new System.Drawing.Size(16, 16);
            this.ImagesFichier.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // AEtoileToolBar
            // 
            this.AEtoileToolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.AEtoileToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.Sep2,
            this.AEtoileDebut,
            this.AEtoileEtape,
            this.AEtoileFin});
            this.AEtoileToolBar.ButtonSize = new System.Drawing.Size(16, 16);
            this.AEtoileToolBar.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.AEtoileToolBar.Divider = false;
            this.AEtoileToolBar.Dock = System.Windows.Forms.DockStyle.None;
            this.AEtoileToolBar.DropDownArrows = true;
            this.AEtoileToolBar.ImageList = this.ImagesPasAPas;
            this.AEtoileToolBar.Location = new System.Drawing.Point(240, 0);
            this.AEtoileToolBar.Name = "AEtoileToolBar";
            this.AEtoileToolBar.ShowToolTips = true;
            this.AEtoileToolBar.Size = new System.Drawing.Size(80, 26);
            this.AEtoileToolBar.TabIndex = 3;
            this.AEtoileToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.AEtoileToolBar_ButtonClick);
            // 
            // Sep2
            // 
            this.Sep2.Name = "Sep2";
            this.Sep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // AEtoileDebut
            // 
            this.AEtoileDebut.ImageIndex = 0;
            this.AEtoileDebut.Name = "AEtoileDebut";
            this.AEtoileDebut.Tag = 0;
            this.AEtoileDebut.ToolTipText = "Initialize A*";
            // 
            // AEtoileEtape
            // 
            this.AEtoileEtape.ImageIndex = 1;
            this.AEtoileEtape.Name = "AEtoileEtape";
            this.AEtoileEtape.Tag = 1;
            this.AEtoileEtape.ToolTipText = "Perform A*\'s next step";
            // 
            // AEtoileFin
            // 
            this.AEtoileFin.ImageIndex = 2;
            this.AEtoileFin.Name = "AEtoileFin";
            this.AEtoileFin.Tag = 2;
            this.AEtoileFin.ToolTipText = "Perform A* to the end";
            // 
            // ImagesPasAPas
            // 
            this.ImagesPasAPas.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ImagesPasAPas.ImageSize = new System.Drawing.Size(16, 16);
            this.ImagesPasAPas.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // LabelAide
            // 
            this.LabelAide.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelAide.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelAide.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.LabelAide.Location = new System.Drawing.Point(0, 0);
            this.LabelAide.Name = "LabelAide";
            this.LabelAide.Size = new System.Drawing.Size(744, 24);
            this.LabelAide.TabIndex = 4;
            this.LabelAide.Text = "Aide";
            this.LabelAide.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // GraphFormer
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(744, 518);
            this.Controls.Add(this.GraphPanel);
            this.Controls.Add(this.GraphStatusBar);
            this.Controls.Add(this.FichierToolBar);
            this.Controls.Add(this.EditionToolBar);
            this.Controls.Add(this.AEtoileToolBar);
            this.Controls.Add(this.LabelAide);
            this.Name = "GraphFormer";
            this.Text = "GraphFormer";
            ((System.ComponentModel.ISupportInitialize)(this.NbNodesPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbArcsPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CoordsPanel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		#region Etats

		bool BoutonDEnfonce = false;
		bool BoutonGEnfonce = false;

		SimPathStore G;
		SimWaypoint TempN1, TempN2;
		bool AjouterN1, AjouterN2;
		SimMovement AE;
		SimWaypoint NDepart, NArrivee;
		IList<SimWaypoint> Chemin;
		Point TempP;
		int NbHeuristique = 0;
		
		bool _PasAPas;
		bool PasAPas
		{
			get { return _PasAPas; }
			set
			{
				_PasAPas = value;
				if ( AdapterAEtoile() ) GraphPanel.Invalidate();
				MenuContextuel.MenuItems[0].Checked = !_PasAPas;
				MenuContextuel.MenuItems[1].Checked = _PasAPas;
				AEtoileToolBar.Visible = _PasAPas;
			}
		}

		bool CalculPossible { get { return NDepart!=null && NArrivee!=null; } }

		enum Action { Dessiner, Effacer, Deplacer, ChangerEtat, AEtoile }
		Action _Mode;
		Action Mode
		{
			get { return _Mode; }
			set
			{
				_Mode = value;
				foreach ( ToolBarButton B in EditionToolBar.Buttons ) B.Pushed = false; // Actions exclusives
				EditionToolBar.Buttons[(int)_Mode].Pushed = true;
				switch( _Mode )
				{
					case Action.AEtoile:
					{
						LabelAide.Text = "Choose starting and ending nodes with the respective left and right mouse buttons";
						AfficherInfosResultat();
						break;
					}
					case Action.ChangerEtat:
					{
						LabelAide.Text = "Select a node or an arc to activate/deactivate";
						break;
					}
					case Action.Deplacer:
					{
						LabelAide.Text = "Select a node with left button to move it (right button to move the entire graph)";
						break;
					}
					case Action.Dessiner:
					{
						LabelAide.Text = "Clic and drag to draw an arc between 2 nodes (right button for bidirectional arcs)";
						break;
					}
					case Action.Effacer:
					{
						LabelAide.Text = "Chose nodes to delete (clic or select a rectangle)";
						break;
					}
				}
			}
		}
		#endregion

		#region Methodes outils
		static Rectangle Boite(params SimWaypoint[] NoeudsAEnglober)
		{
			Rectangle R = RectangleCentres(NoeudsAEnglober);
			Size S = new Size(Rayon+2*Epaisseur, Rayon+2*Epaisseur);
			R.Inflate(S);
			return R;
		}

		static Rectangle RectangleCentres(params SimWaypoint[] NoeudsAEnglober)
		{
			float[] Min, Max;
			SimWaypoint.BoundingBox(NoeudsAEnglober, out Min, out Max);
			return Rectangle.FromLTRB((int)(Min[0]* DSCALE), (int)(Min[1]* DSCALE), (int)(Max[0]* DSCALE), (int)(Max[1]* DSCALE));
		}

		static bool Collision(SimWaypoint N1, SimWaypoint N2)
		{
			return SimWaypoint.SquareEuclidianDistance(N1, N2)<=Rayon*Rayon;
		}

		bool NoeudSelonPosition(int X, int Y, ref SimWaypoint N)
		{
			SimWaypoint NSJ = NoeudSousJacent(X, Y);
			if ( NSJ!=null )
			{
				N = NSJ;
				return false;
			}
			else
			{
                if (N == null) N = SimWaypoint.Create(X / DSCALE, Y / DSCALE, 0, PathStore);
                else N.ChangeXYZDebug(X/DSCALE, Y/DSCALE, 0);
				return true;
			}
		}

		SimWaypoint NoeudSousJacent(int X, int Y)
		{
			float Distance;
            SimWaypoint ClosestNode = G.ClosestNode((float)(X / DSCALE), (float)(Y / DSCALE), 0, out Distance, false);
			return (Distance* DSCALE)<=2*Rayon ? ClosestNode : null;
		}

		SimRoute ArcSousJacent(int X, int Y)
		{
            float Distance;
			SimRoute ArcPlusProche = G.ClosestArc((float)(X/ DSCALE), (float)(Y/ DSCALE), 0, out Distance, false);
            return (Distance * DSCALE) <= Rayon ? ArcPlusProche : null;
		}
		#endregion

		#region Réponses aux actions
		private void GraphToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			Mode = (Action)e.Button.Tag;
		}

		private void ChoixAutomatique(object sender, EventArgs e) { PasAPas = false; }
		private void ChoixPasAPas(object sender, EventArgs e) { PasAPas = true; }

		private void GraphPanel_MouseLeave(object sender, System.EventArgs e)
		{
			if ( !CalculPossible ) CoordsPanel.Text = String.Empty;
		}

		private void GraphPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ( e.Button==MouseButtons.Left ) BoutonGEnfonce = true;
			else if ( e.Button==MouseButtons.Right ) BoutonDEnfonce = true;
			else return;
			if ( BoutonGEnfonce && BoutonDEnfonce ) return;

			TempN1 = TempN2 = null;
			switch ( Mode )
			{
				case Action.Effacer:
				case Action.Dessiner:
				{
					AjouterN1 = NoeudSelonPosition(e.X, e.Y, ref TempN1);
                    TempN2 = SimWaypoint.Create((float)((TempN1.DX) / DSCALE), (float)((TempN1.DY) / DSCALE), (float)0, PathStore);
					GraphPanel.Invalidate( Boite(TempN1, TempN2) );
					break;
				}
				case Action.Deplacer:
				{
					TempP = new Point(e.X, e.Y);
					TempN1 = NoeudSousJacent(e.X, e.Y);
					break;
				}
				default: break;
			}
		}

		string SB_Point(int X, int Y) { return "{"+X+";"+Y+"}"; }
		string SB_Noeud(int X, int Y) { return "Node "+SB_Point(X, Y); }
		string SB_TempArc(int X, int Y, bool DoubleSens)
		{
			string Fleche = DoubleSens ? " <-> " : " -> ";
			SimWaypoint N = NoeudSousJacent(X, Y);
			string Cible = N!=null ? SB_Noeud((int)N.DX, (int)N.DY) : SB_Point((int)TempN2.DX, (int)TempN2.DY);
			return SB_Point((int)TempN1.DX, (int)TempN1.DY)+Fleche+Cible+" : Length = "+(int)SimWaypoint.EuclidianDistance(TempN1, TempN2);
		}
		string SB_TempRectangle()
		{
			return SB_Point((int)TempN1.DX, (int)TempN1.DY)+" + "+SB_Point((int)Math.Abs(TempN1.DX-TempN2.DX), (int)Math.Abs(TempN1.DY-TempN2.DY));
		}
		string SB_DetecterNoeud(int X, int Y)
		{
			SimWaypoint N = NoeudSousJacent(X, Y);
			return N!=null ? SB_Noeud((int)N.DX, (int)N.DY) : SB_Point(X, Y);
		}
		string SB_DetecterNoeudOuArc(int X, int Y)
		{
			SimWaypoint N = NoeudSousJacent(X, Y);
			if ( N!=null ) return SB_Noeud((int)N.DX, (int)N.DY);
			SimRoute A = ArcSousJacent(X, Y);
			if ( A!=null ) return "Arc "+SB_Point((int)A.StartNode.DX, (int)A.StartNode.DY)+" -> "+SB_Point((int)A.EndNode.DX, (int)A.EndNode.DY);
			return SB_Point(X, Y);
		}
		string SB_NoeudPlusProche(int X, int Y)
		{
			float Distance;
			SimWaypoint N = G.ClosestNode(X/DSCALE, Y/DSCALE, 0, out Distance, true);
			return N!=null ? SB_Noeud((int)N.DX, (int)N.DY) : SB_Point(X, Y);
		}

		void StatusBarMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			int X = e.X;
			int Y = e.Y;
			bool BoutonEnfonce = e.Button==MouseButtons.Left || e.Button==MouseButtons.Right;

			switch( Mode )
			{
				case Action.Dessiner:
				{
					CoordsPanel.Text = BoutonEnfonce ? SB_TempArc(X, Y, e.Button==MouseButtons.Right) : SB_DetecterNoeud(X, Y);
					break;
				}
				case Action.Effacer:
				{
					CoordsPanel.Text = BoutonEnfonce ? SB_TempRectangle() : SB_DetecterNoeudOuArc(X, Y);
					break;
				}
				case Action.Deplacer:
				{
					if ( e.Button==MouseButtons.Right ) CoordsPanel.Text = String.Empty;
					else
					{
						if ( BoutonEnfonce )
							CoordsPanel.Text = TempN1!=null ? SB_Noeud(X, Y) : SB_Point(X, Y);
						else
							CoordsPanel.Text = SB_DetecterNoeud(X, Y);
					}
					break;
				}
				case Action.ChangerEtat:
				{
					CoordsPanel.Text = SB_DetecterNoeudOuArc(X, Y);
					break;
				}
				case Action.AEtoile:
				{
					string S = String.Empty;
					if ( NDepart==null && NArrivee==null ) S = "Select STARTING and ENDING nodes";
					else if ( NDepart==null ) S = "Select STARTING node (with left button)";
					else if ( NArrivee==null ) S = "Select ENDING node (with right button)";
					CoordsPanel.Text = S+". Current : "+SB_NoeudPlusProche(X, Y);
					break;
				}
			}
		}

		private void GraphPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ( BoutonGEnfonce && BoutonDEnfonce ) return;
			switch( Mode )
			{
				case Action.Effacer:
				case Action.Dessiner:
				{
					if ( e.Button==MouseButtons.Left || e.Button==MouseButtons.Right )
					{
						if ( TempN1==null || TempN2==null ) return;
						Rectangle AncienRect = Boite(TempN1, TempN2);
                        TempN2.ChangeXYZDebug(e.X / DSCALE, e.Y / DSCALE, 0);
						Rectangle NouveauRect = Boite(TempN1, TempN2);
						GraphPanel.Invalidate( Rectangle.Union(AncienRect, NouveauRect) );
					}
					break;
				}
				case Action.Deplacer:
				{
					if ( e.Button==MouseButtons.Left )
					{
						if ( TempN1==null ) break;
						Rectangle AncienRect = Boite(TempN1.Molecule);

						IList<SimWaypoint> AncienChemin = null;
						if ( Chemin!=null )
						{
							AncienChemin = new SimWaypoint[Chemin.Count];
							for ( int i=0; i<AncienChemin.Count; i++ ) AncienChemin[i] = (SimWaypoint)Chemin[i].Clone();
						}

                        TempN1.ChangeXYZDebug(e.X / DSCALE, e.Y / DSCALE, 0);
						Rectangle NouveauRect = Boite(TempN1.Molecule);
						if ( AdapterAEtoile() && CheminsDifferents(AncienChemin, Chemin) ) GraphPanel.Invalidate();
						else GraphPanel.Invalidate( Rectangle.Union(AncienRect, NouveauRect) );
					}
					else if ( e.Button==MouseButtons.Right )
					{
						int DX = (int)(e.X-TempP.X);
						int DY = (int)(e.Y-TempP.Y);
						TempP.X = e.X;
						TempP.Y = e.Y;
                        foreach (SimWaypoint N in G.Nodes) N.ChangeXYZDebug((float)(N.DX + DX) / DSCALE, (float)(N.DY + DY) / DSCALE, 0);
						GraphPanel.Invalidate();
					}
					break;
				}
				default: break;
			}
			if ( !CalculPossible ) StatusBarMouseMove(e);
		}

		private void GraphPanel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			bool Retour = BoutonGEnfonce && BoutonDEnfonce;
			if ( e.Button==MouseButtons.Left ) BoutonGEnfonce = false;
			else if ( e.Button==MouseButtons.Right ) BoutonDEnfonce = false;
			else return;
			if ( Retour ) return;

			switch( Mode )
			{
				case Action.Dessiner:
				{
					if ( TempN1==null || TempN2==null ) return;
					bool AjouterArc = false;
					Rectangle AncienRect = Boite(TempN1, TempN2);
					AjouterN2 = NoeudSelonPosition(e.X, e.Y, ref TempN2);
					if ( AjouterN1 ) { TempN1.Isolate(); G.AddNode(TempN1); }
					if ( !Collision(TempN1, TempN2) )
					{
						if ( AjouterN2 ) { TempN2.Isolate(); G.AddNode(TempN2); }
						if ( e.Button == MouseButtons.Left )
							G.AddArc(TempN1, TempN2, 1);
						else if ( e.Button == MouseButtons.Right ) G.Add2Arcs(TempN1, TempN2, 1);
						NbArcsPanel.Text = G.Arcs.Count.ToString();
						AjouterArc = true;
					}
					NbNodesPanel.Text = G.Nodes.Count.ToString();

					if ( AjouterArc && ( !AjouterN1 || !AjouterN2 ) && AdapterAEtoile() ) GraphPanel.Invalidate();
					else
					{
						Rectangle NouveauRect = Boite(TempN1, TempN2);
						GraphPanel.Invalidate( Rectangle.Union(AncienRect, NouveauRect) );
					}
					break;
				}
				case Action.Effacer:
				{
					if ( TempN1==null || TempN2==null ) return;
					bool Selection = false;
					Rectangle Zone = RectangleCentres(TempN1, TempN2);
					Zone.Inflate(1,1);
					Region Invalide = new Region(Zone);
					if ( Zone.Size.Width<2*Rayon && Zone.Size.Height<2*Rayon )
					{
						SimWaypoint N = NoeudSousJacent(e.X, e.Y);
						if ( N!=null )
						{
							TestAEtoile(N, ref Invalide);
							Invalide.Union(Boite(N.Molecule));
							G.RemoveNode(N);
							NbNodesPanel.Text = G.Nodes.Count.ToString();
							NbArcsPanel.Text = G.Arcs.Count.ToString();
							Selection = true;
						}
						else
						{
							SimRoute A = ArcSousJacent(e.X, e.Y);
							if ( A!=null )
							{
								Invalide.Union( Boite(A.StartNode, A.EndNode) );
								G.RemoveArc(A);
								NbArcsPanel.Text = G.Arcs.Count.ToString();
								Selection = true;
							}
						}
					}
					else
					{
						System.Collections.ArrayList ListeNoeuds = new System.Collections.ArrayList();
						foreach ( SimWaypoint N in G.Nodes )
						{
							if ( Zone.Contains(new Point((int)N.DX, (int)N.DY)) )
							{
								TestAEtoile(N, ref Invalide);
								Invalide.Union( Boite(N.Molecule) );
								ListeNoeuds.Add(N);
								Selection = true;
							}
						}
						foreach ( SimWaypoint N in ListeNoeuds ) G.RemoveNode(N);
						NbNodesPanel.Text = G.Nodes.Count.ToString();
						NbArcsPanel.Text = G.Arcs.Count.ToString();
					}

					if ( Selection && AdapterAEtoile() ) GraphPanel.Invalidate();
					else GraphPanel.Invalidate(Invalide);
					break;
				}
				case Action.ChangerEtat:
				{
					SimWaypoint N = NoeudSousJacent(e.X, e.Y);
					Region Invalide = null;
					if ( N!=null )
					{
						N.Passable = !N.Passable;
						Invalide = new Region(Boite(N.Molecule));
					}
					else
					{
						SimRoute A = ArcSousJacent(e.X, e.Y);
						if ( A!=null )
						{
							A.Passable = !A.Passable;
							Invalide = new Region(Boite(A.StartNode, A.EndNode));
						}
					}

					if ( Invalide!=null )
					{
						if ( AdapterAEtoile() ) GraphPanel.Invalidate();
						else GraphPanel.Invalidate(Invalide);
					}
					break;
				}
				case Action.AEtoile:
				{
					float Distance;
					SimWaypoint NoeudPlusProche = G.ClosestNode(e.X/DSCALE, e.Y/DSCALE, 0, out Distance, true);
					if ( NoeudPlusProche==null ) break;
					Rectangle Invalide = Boite(NoeudPlusProche);

					if ( NDepart!=null ) Invalide = Rectangle.Union(Invalide, Boite(NDepart));
					if ( NArrivee!=null ) Invalide = Rectangle.Union(Invalide, Boite(NArrivee));

					if ( e.Button==MouseButtons.Left ) NDepart = NDepart==NoeudPlusProche ? null : NoeudPlusProche;
					else if ( e.Button==MouseButtons.Right ) NArrivee = NArrivee==NoeudPlusProche ? null : NoeudPlusProche;

					if ( AdapterAEtoile() ) GraphPanel.Invalidate();
					else
					{
						if ( Chemin!=null )
						{
							Chemin = null;
							GraphPanel.Invalidate();
						}
						else GraphPanel.Invalidate(Invalide);
					}
					break;
				}
				default: break;
			}
			TempN1 = TempN2 = null;
		}

		// retourne true s'il faut invalider tout le panel
		bool AdapterAEtoile()
		{
			if ( !PasAPas )
			{
				if ( !CalculPossible ) return false;
				AEtoile_Fin();
				return true;
			}
			else
			{
				bool IlResteDesPas  = AE.Closed.Length>0 || AE.Open.Length>1;
				AEtoile_Debut();
				return IlResteDesPas;
			}
		}

		bool CheminsDifferents(IList<SimWaypoint> C1, IList<SimWaypoint> C2)
		{
            if (C1 == null || C2 == null || C1.Count != C2.Count) return true;
            for (int i = 0; i < C1.Count; i++) if (!C1[i].Equals(C2[i])) return true;
			return false;
		}

		void TestAEtoile(SimWaypoint N, ref Region ZoneInvalide)
		{
			if ( N!=null )
			{
				if ( N==NDepart ) NDepart=null;
				if ( N==NArrivee ) NArrivee=null;
				if ( Chemin!=null && Chemin.IndexOf(N)>=0 )
				{
					Chemin=null;
					ZoneInvalide.Union(new Rectangle(new Point(0,0),GraphPanel.Size));				
				}
			}
		}

		private void AEtoileToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if ( !CalculPossible ) MessageBox.Show(
									   @"Before performing A* you must choose the starting and ending nodes
with the respective left and right mouse buttons.", "Impossible action", MessageBoxButtons.OK, MessageBoxIcon.Stop);

			switch( (int)e.Button.Tag )
			{
				case 0:
				{
					AEtoile_Debut();
					break;
				}
				case 1:
				{
					AEtoile_Etape();
					break;
				}
				case 2:
				{
					AEtoile_Fin();
					break;
				}
			}
			GraphPanel.Invalidate();
		}


        public void SetTryPathNow(SimWaypoint start,SimWaypoint end, IList<SimWaypoint> ch) {
            NDepart = start;
            NArrivee = end;
         //   CalculPossible = true;
          //  AEtoile_Debut();
          //  AEtoile_Fin();
            Chemin = ch;// AE.SearchPath(NDepart, NArrivee) ? AE.PathByNodes : null;
            GraphPanel.Invalidate();
        }

		private void AEtoile_Debut()
		{
			if ( !CalculPossible )
			{
				foreach ( ToolBarButton B in AEtoileToolBar.Buttons ) B.Enabled = false;
				return;
			}
			else
			{
				AEtoileDebut.Enabled = false;
				AEtoileFin.Enabled = true;
				AEtoileEtape.Enabled = true;
				Chemin = null;
				AE.Initialize(NDepart, NArrivee);
				AfficherInfosResultat();
			}
		}

		private void AEtoile_Etape()
		{
			if ( !CalculPossible ) return;
			if ( !AE.NextStep() )
			{
				Chemin = AE.PathByNodes;
				AEtoileFin.Enabled = false;
				AEtoileEtape.Enabled = false;
			}
			AfficherInfosResultat();
			AEtoileDebut.Enabled = true;
		}

		private void AEtoile_Fin()
		{
			if ( !CalculPossible ) return;
			Chemin = AE.SearchPath(NDepart, NArrivee) ? AE.PathByNodes : null;
			AfficherInfosResultat();
			AEtoileDebut.Enabled = true;
			AEtoileFin.Enabled = false;
			AEtoileEtape.Enabled = false;
		}

		void AfficherInfosResultat()
		{
			if ( !CalculPossible ) return;

			else if ( Chemin==null )
			{
				if ( PasAPas ) CoordsPanel.Text = String.Format("Open list : {0} (green) ; Closed list : {1} (red)   -   Current step : {2}", AE.Open.Length, AE.Closed.Length, AE.StepCounter);
				else CoordsPanel.Text = "There is no possible path in this configuration.";
			}
			else 
			{
				int NbArcsParcourus;
				float Cout;
				if ( AE.PathFound )
				{
					AE.ResultInformation(out NbArcsParcourus, out Cout);
					CoordsPanel.Text = String.Format("Shortest path found in {0} step(s) : {1} arc(s) \\ cost = {2}", AE.StepCounter, NbArcsParcourus, (int)Cout);
				}
			}

			if ( PasAPas && AE.SearchEnded && !AE.PathFound )
			{
				GraphPanel.Invalidate();
				MessageBox.Show(
					@"In this configuration, there is no possible path. You can :
- either modify the graph
- or change the starting and/or ending nodes.", "No result", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void FichierToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch( (int)e.Button.Tag )
			{
				case 0:
				{
					if ( G.Nodes.Count>0 )
					{
						DialogResult DR = MessageBox.Show("You are about to clear the current graph. Do you confirm ?", "New graph", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if ( DR==DialogResult.No ) break;
					}
					G.Clear();
					NouveauGraphe();
					GraphPanel.Invalidate();
					break;
				}
				case 1:
				{
					if ( G.Nodes.Count>0 )
					{
						DialogResult DR = MessageBox.Show("You are about to replace the current graph with another. Do you confirm ?", "Load a graph", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if ( DR==DialogResult.No ) break;
					}
					if ( Charger() )
					{
						NouveauGraphe();
						GraphPanel.Invalidate();
					}
					break;
				}
				case 2:
				{
					if ( Sauver() ) GraphPanel.Invalidate();
					break;
				}
				case 3:
				{
					DialogueAPropos.DijkstraHeuristiqueBalance = AE.DijkstraHeuristicBalance;
					DialogueAPropos.HeuristiqueChoisie = NbHeuristique;

					DialogResult DR = DialogueAPropos.ShowDialog(this);
					if ( DR==DialogResult.OK ) AppliquerChangement(DialogueAPropos);
					break;
				}
			}
		}

		internal void AppliquerChangement(APropos DialogueAPropos)
		{
			AE.DijkstraHeuristicBalance = DialogueAPropos.DijkstraHeuristiqueBalance;
			if ( NbHeuristique != DialogueAPropos.HeuristiqueChoisie )
			{
				NbHeuristique = DialogueAPropos.HeuristiqueChoisie;
				switch ( NbHeuristique )
				{
					case 0: AE.ChoosenHeuristic = SimMovement.EuclidianHeuristic; break;
					case 1: AE.ChoosenHeuristic = SimMovement.ManhattanHeuristic; break;
					case 2: AE.ChoosenHeuristic = SimMovement.MaxAlongAxisHeuristic; break;
				}
			}
			AdapterAEtoile();
			GraphPanel.Invalidate();
		}

        void NouveauGraphe()
		{
			TempN1 = TempN2 = null;
			AjouterN1 = AjouterN2 = false;
			AE = new SimMovement(G);
			CoordsPanel.Text = String.Empty;;
			NbNodesPanel.Text = G.Nodes.Count.ToString();
			NbArcsPanel.Text = G.Arcs.Count.ToString();
			NDepart = NArrivee = null;
			Chemin = null;
			PasAPas = false;
			AEtoileToolBar.Visible = false;
		}
		#endregion

		#region Archivage
		bool Charger()
		{
			try
			{
			//	Stream StreamRead;
				OpenFileDialog DialogueCharger = new OpenFileDialog();
				if( DialogueCharger.ShowDialog() == DialogResult.OK )
				{
                    if (G.LoadFile(DialogueCharger.FileName))
                    {
                        return true;
                    }
				}
			}
			catch {}
			return false;
		}

		bool Sauver()
		{
			try
			{
				//Stream StreamWrite;
				SaveFileDialog DialogueSauver = new SaveFileDialog();
				if( DialogueSauver.ShowDialog() == DialogResult.OK )
				{
                    return G.SaveFile(DialogueSauver.FileName);
				}
			}
			catch {}
			return false;
		}
		#endregion


        public void RepaintNow()
        {
         //   GraphPanel_Paint(null, null);
            GraphPanel.Invalidate();
        }
        Graphics GrfxI;
        #region Dessin
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        /// 
		private void GraphPanel_Paint(object senderee, System.Windows.Forms.PaintEventArgs eee)
		{
            if (eee != null)
            {
                // Needed at least once
               GrfxI = eee.Graphics;
            }
            Graphics Grfx = GrfxI;
            Grfx.SmoothingMode = SmoothingMode.AntiAlias;
            Grfx.PixelOffsetMode = PixelOffsetMode.None;

			SuspendLayout();
			// Dessin du graphe
            ///lock (G.Nodes) foreach (SimWaypoint N in G.Nodes) DessinerNoeud(Grfx, N.Passable ? CrayonNoeuds : CrayonNoeudsInactifs, N);

            lock (G.Arcs) foreach (SimRoute A in G.Arcs)
                {
                    if (A.Passable)
                    {
                    }
                    else
                    {
                        DessinerArc(Grfx, CrayonArcsInactifs, A);
                    }
                }
            lock (G.Arcs) foreach (SimRoute A in G.Arcs)
                {
                    if (A.Passable)
                    {
                        if (A.Weight > 1f)
                        {
                            DessinerArc(Grfx, CrayonArcsWeak, A);
                        } else
                        if (A.Weight < 1f)
                        {
                            DessinerArc(Grfx, CrayonArcsStrong, A);
                        }
                    }
                    else
                    {
                        // DessinerArc(Grfx, CrayonArcsInactifs, A);
                    }
                }

			// Dessin du tracé temporaire courant
			if ( Mode==Action.Dessiner )
			{
				DessinerNoeud(Grfx, CrayonTemp, TempN1);
				DessinerNoeud(Grfx, CrayonTemp, TempN2);
				DessinerArc(Grfx, CrayonTemp, TempN1, TempN2);
			}
			else if ( Mode==Action.Effacer )
			{
				if ( TempN1!=null && TempN2!=null )
					Grfx.DrawRectangle(CrayonTemp, RectangleCentres(TempN1, TempN2));
			}

			DessinerDrapeau(Grfx, NDepart, 1);
			DessinerDrapeau(Grfx, NArrivee, 2);

			// Dessins des noeuds "pas à pas"
			if ( PasAPas && CalculPossible )
			{
				SimWaypoint[] DerniersNoeudsClosed = new SimWaypoint[AE.Closed.Length];
				for ( int i=0; i<AE.Closed.Length; i++ )
				{
					DerniersNoeudsClosed[i] = AE.Closed[i][AE.Closed[i].Length-1];
					DessinerNoeudPlein(Grfx, Brushes.Red, DerniersNoeudsClosed[i]);
				}
				if (Chemin==null)
				{
					for ( int i=0; i<AE.Open.Length; i++ )
					{
						SimWaypoint[] Nodes = AE.Open[i];
						int L = Nodes.Length;
						SimWaypoint DernierNoeud = Nodes[L-1];
						if ( Array.IndexOf(DerniersNoeudsClosed, DernierNoeud)>=0 )
							DessinerNoeudMoitie(Grfx, Brushes.LawnGreen, DernierNoeud);
						else
							DessinerNoeudPlein(Grfx, Brushes.LimeGreen, DernierNoeud);
					}
					for ( int i=0; i<AE.Open.Length; i++ )
					{
						SimWaypoint[] Nodes = AE.Open[i];
						int L = Nodes.Length;
						if ( L>1 )
						{
							DessinerArc(Grfx, CrayonArcsPas, Nodes[L-2], Nodes[L-1]);
							DessinerNumero(Grfx, Nodes[L-2], Nodes[L-1], i+1);
						}
					}
				}
			}
			
			// Dessins AEtoile
			if ( Chemin!=null ) DessinerChemin(Grfx, CrayonChemin, Chemin);
			ResumeLayout(false);
		}

		static private void DessinerNoeud(Graphics Grfx, Pen P, SimWaypoint N)
		{
			if ( N==null ) return;
			Grfx.DrawEllipse(P, (int)N.DX-Rayon, (int)N.DY-Rayon, 2*Rayon+1, 2*Rayon+1);
		}

		static private void DessinerArc(Graphics Grfx, Pen P, SimRoute A)
		{
			DessinerArc(Grfx, P, A.StartNode, A.EndNode);
		}

		static private void DessinerArc(Graphics Grfx, Pen P, SimWaypoint N1, SimWaypoint N2)
		{
			if ( N1==null || N2==null ) return;
			Grfx.DrawLine(P, (int)N1.DX, (int)N1.DY, (int)N2.DX, (int)N2.DY);
		}

		static private void DessinerNoeudPlein(Graphics Grfx, Brush B, SimWaypoint N)
		{
			if ( N==null ) return;
			Rectangle R = new Rectangle((int)N.DX-Rayon, (int)N.DY-Rayon, 2*Rayon+1, 2*Rayon+1);
			Grfx.FillEllipse(B, R);
		}

		static private void DessinerNoeudMoitie(Graphics Grfx, Brush B, SimWaypoint N)
		{
			if ( N==null ) return;
			Rectangle R = new Rectangle((int)N.DX-Rayon, (int)N.DY-Rayon, 2*Rayon+1, 2*Rayon+1);
			Grfx.FillPie(B, R, 0, 180);
		}

		static private void DessinerNumero(Graphics Grfx, SimWaypoint N1, SimWaypoint N2, int i)
		{
			StringFormat F = new StringFormat();
			F.Alignment = StringAlignment.Center;
			F.LineAlignment = StringAlignment.Center;
			Rectangle R = RectangleCentres(N1, N2);
			Font Police = DefaultFont;
			int LargeurMin = (int)Police.GetHeight();
			R.Inflate(LargeurMin, LargeurMin);
			Grfx.DrawString(i.ToString(), Police, Brushes.Black, R, F);
		}

		static private void DessinerDrapeau(Graphics Grfx, SimWaypoint N, int Numero)
		{
			if ( N==null ) return;
			Point[] Pts = new Point[5];

			double AnglePortion =(double) (2*Math.PI)/Pts.Length;
			for ( int i=0; i<Pts.Length; i++ )
			{
				double Angle = 2*i*AnglePortion;
				if ( Numero==1 ) Angle += AnglePortion/2;
				Pts[i] = new Point(1+(int)(N.DX+(Rayon+1)*Math.Cos(Angle)), 1+(int)(N.DY+(Rayon+1)*Math.Sin(Angle)));
			}
			GraphicsPath GP = new GraphicsPath();
			GP.AddLines(Pts);
			GP.FillMode = FillMode.Winding;
			Grfx.FillPath(Numero==1 ? Brushes.DarkTurquoise : Brushes.Blue, GP);
		}

		static private void DessinerChemin(Graphics Grfx, Pen P, IList<SimWaypoint> C)
		{
			Point[] Pnts = new Point[C.Count];
			if ( Pnts.Length>1 )
			{
				for ( int i=0; i<Pnts.Length; i++ )
				{
					Pnts[i].X = (int)C[i].DX;
					Pnts[i].Y = (int)C[i].DY;
				}
				Grfx.DrawCurve(P, Pnts);
			}
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void MainDebug() 
		{
			try
			{
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GraphFormer(SimPathStore.Instance));
			}
			catch (Exception e) { MessageBox.Show(e.ToString()); }
		}

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            DEBUGGER = null;
            base.OnFormClosing(e);
        }


    }
}

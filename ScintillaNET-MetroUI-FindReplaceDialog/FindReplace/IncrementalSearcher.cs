using MetroFramework;

namespace ScintillaNET_FindReplaceDialog
{
    using ScintillaNET;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class IncrementalSearcher : UserControl
    {
        #region Fields

        private bool _autoPosition = true;
        private Scintilla _scintilla;
        private bool _toolItem = false;
        private FindReplace _findReplace;

        #endregion Fields

        #region Constructors

        public IncrementalSearcher(Scintilla scintilla) : this()
        {
            this._scintilla = scintilla;
        }

        public IncrementalSearcher()
        {
            InitializeComponent();
            this.BorderStyle = BorderStyle.FixedSingle;

            // Clear
            this.btnClearHighlights.Click += btnClearHighlights_Click;
            this.btnClearHighlights.MouseHover += ( sender, args ) => this.btnClearHighlights.Theme = MetroThemeStyle.Dark;
            this.btnClearHighlights.MouseLeave += ( sender, args ) => this.btnClearHighlights.Theme = MetroThemeStyle.Dark;

            // Previous
            this.btnPrevious.Click += btnPrevious_Click;
            this.btnPrevious.MouseHover += (sender, args) => this.btnClearHighlights.Theme = MetroThemeStyle.Dark;
            this.btnPrevious.MouseLeave += (sender, args) => this.btnClearHighlights.Theme = MetroThemeStyle.Dark;

            // Next
            this.btnNext.Click += btnNext_Click;
            this.btnNext.MouseHover += (sender, args) => this.btnClearHighlights.Theme = MetroThemeStyle.Dark;
            this.btnNext.MouseLeave += (sender, args) => this.btnClearHighlights.Theme = MetroThemeStyle.Dark;

            // Highlight All
            this.btnHighlightAll.Click += btnHighlightAll_Click;
            this.btnHighlightAll.MouseHover += (sender, args) => this.btnClearHighlights.Theme = MetroThemeStyle.Dark;
            this.btnHighlightAll.MouseLeave += (sender, args) => this.btnClearHighlights.Theme = MetroThemeStyle.Dark;

            // Text
            this.txtFind.KeyDown += txtFind_KeyDown;
            this.txtFind.TextChanged += txtFind_TextChanged;
        }

        public IncrementalSearcher(bool toolItem, Scintilla scintilla = null) : this()
        {
            if ( scintilla != null )
                this.Scintilla = scintilla;

            _toolItem = toolItem;
            if (toolItem)
                BackColor = Color.Transparent;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether the control should automatically move away from the current
        /// selection to prevent obscuring it.
        /// </summary>
        /// <returns>true to automatically move away from the current selection; otherwise, false.
        /// If ToolItem is enabled, this defaults to false.</returns>
        public bool AutoPosition
        {
            get
            {
                return _autoPosition;
            }
            set
            {
                if (!ToolItem)
                    _autoPosition = value;
                else
                    _autoPosition = false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public FindReplace FindReplace
        {
            get
            {
                return _findReplace;
            }
            set
            {
                _findReplace = value;
                if (value!=null)
                {
                _scintilla = _findReplace.Scintilla;
                }
                else
                {
                    _scintilla = null;
                }
            }
        }

        [Browsable(false)]
        public Scintilla Scintilla
        {
            get
            {
                return _scintilla;
            }
            set
            {
                _scintilla = value;
            }
        }

        public bool ToolItem
        {
            get { return _toolItem; }
            set
            {
                _toolItem = value;
                if (_toolItem)
                    BackColor = Color.Transparent;
                else
                    BackColor = Color.LightSteelBlue;
            }
        }

        #endregion Properties

        #region Event Handlers

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            findPrevious();
        }

        private void btnClearHighlights_Click(object sender, EventArgs e)
        {
            if (_scintilla == null)
                return;
            _findReplace.ClearAllHighlights();
        }

        private void btnHighlightAll_Click(object sender, EventArgs e)
        {
            if (this.txtFind.Text == string.Empty)
                return;
            if (_scintilla == null)
                return;

            int foundCount = _findReplace.FindAll(this.txtFind.Text, false, true).Count;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            findNext();
        }
        private void txtFind_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.Down:
                    findNext();
                    e.Handled = true;
                    break;

                case Keys.Up:
                    findPrevious();
                    e.Handled = true;
                    break;

                case Keys.Escape:
                    if (!_toolItem)
                        Hide();
                    break;
            }
        }

        private void txtFind_TextChanged(object sender, EventArgs e)
        {
            this.txtFind.BackColor = SystemColors.Window;
            if (this.txtFind.Text == string.Empty)
                return;
            if (_scintilla == null)
                return;

            int pos = Math.Min(_scintilla.CurrentPosition, _scintilla.AnchorPosition);
            ScintillaNET_FindReplaceDialog.CharacterRange r = _findReplace.Find(pos, _scintilla.TextLength, this.txtFind.Text, _findReplace.Window.GetSearchFlags());
            if (r.cpMin == r.cpMax)
                r = _findReplace.Find(0, pos, this.txtFind.Text, _findReplace.Window.GetSearchFlags());

            if (r.cpMin != r.cpMax)
                _scintilla.SetSel(r.cpMin, r.cpMax);
            else
                this.txtFind.BackColor = Color.Yellow;

            MoveFormAwayFromSelection();
        }


        #endregion Event Handlers

        #region Methods

        public virtual void MoveFormAwayFromSelection()
        {
            if (!Visible || _scintilla == null)
                return;

            if (!AutoPosition)
                return;

            int pos = _scintilla.CurrentPosition;
            int x = _scintilla.PointXFromPosition(pos);
            int y = _scintilla.PointYFromPosition(pos);

            Point cursorPoint = new Point(x, y);

            Rectangle r = new Rectangle(Location, Size);

            if (_scintilla != null)
            {
                r.Location = new Point(_scintilla.ClientRectangle.Right - Size.Width, 0);
            }

            if (r.Contains(cursorPoint))
            {
                Point newLocation;
                if (cursorPoint.Y < (Screen.PrimaryScreen.Bounds.Height / 2))
                {
                    //TODO - replace lineheight with ScintillaNET command, when added
                    int SCI_TEXTHEIGHT = 2279;
                    int lineHeight = _scintilla.DirectMessage(SCI_TEXTHEIGHT, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // Top half of the screen
                    newLocation = new Point(r.X, cursorPoint.Y + lineHeight * 2);
                }
                else
                {
                    //TODO - replace lineheight with ScintillaNET command, when added
                    int SCI_TEXTHEIGHT = 2279;
                    int lineHeight = _scintilla.DirectMessage(SCI_TEXTHEIGHT, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // Bottom half of the screen
                    newLocation = new Point(r.X, cursorPoint.Y - Height - (lineHeight * 2));
                }

                Location = newLocation;
            }
            else
            {
                Location = r.Location;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            MoveFormAwayFromSelection();
            this.txtFind.Focus();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLostFocus(e);
            if (!_toolItem)
                Hide();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            this.txtFind.Text = string.Empty;
            this.txtFind.BackColor = SystemColors.Window;

            MoveFormAwayFromSelection();

            if (Visible)
                this.txtFind.Focus();
            else if (_scintilla != null)
                _scintilla.Focus();
        }

        private void findNext()
        {
            if (this.txtFind.Text == string.Empty)
                return;
            if (_scintilla == null)
                return;

            ScintillaNET_FindReplaceDialog.CharacterRange r = _findReplace.FindNext(this.txtFind.Text, true, _findReplace.Window.GetSearchFlags());
            if (r.cpMin != r.cpMax)
                _scintilla.SetSel(r.cpMin, r.cpMax);

            MoveFormAwayFromSelection();
        }

        private void findPrevious()
        {
            if (this.txtFind.Text == string.Empty)
                return;
            if (_scintilla == null)
                return;

            ScintillaNET_FindReplaceDialog.CharacterRange r = _findReplace.FindPrevious(this.txtFind.Text, true, _findReplace.Window.GetSearchFlags());
            if (r.cpMin != r.cpMax)
                _scintilla.SetSel(r.cpMin, r.cpMax);

            MoveFormAwayFromSelection();
        }

        #endregion Methods
    }
}
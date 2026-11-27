// ============================================================================
// NillyNote Pro — Ultra-Light Native Windows 11 Pro Notepad
// created by Nilly graphics (Sotiris Christos) — https://nillygraphics.com/
// Strictly C# 5 compatible (.NET Framework 4.x csc.exe — 0 external DLLs)
// Replaces Windows Notepad (18 MB) in ~60 KB EXE & 0 MB background RAM!
// Features:
//  - Authentic Windows 11 Clean Tabbed Interface (Dark / Light + Dark Titlebar)
//  - Automatic session restore for tabs and scratchpad drafts
//  - Single-Instance Multi-Tab IPC (WM_COPYDATA) & Drag-and-Drop
//  - 100-Step Multi-Level Undo/Redo, Fixed Ctrl+Backspace, Ctrl+Wheel Zoom
//  - Floating Find & Replace Bar (Case-Sensitive + Regex + Match Counter)
//  - Live Word / Character / Line & Selection Counter, Go To Line (Ctrl+G)
//  - Always-on-Top Pin (Ctrl+T), Opacity (100/90/80%), Word Wrap (Alt+Z)
//  - Pro Text Tools: F5 Timestamp, Checklists (Ctrl+L), Sort, Dedupe, Trim,
//    Case, Greek -> Greeklish, SEO Slug, JSON Format/Minify, Quick Diff, Print
//  - Full Greek Encoding Auto-Detection (UTF-8, UTF-8 BOM, UTF-16, Win-1253)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

[assembly: AssemblyTitle("NillyNote Pro")]
[assembly: AssemblyDescription("Ultra-Light Native Windows 11 Pro Notepad — created by Nilly graphics (Nillygraphics · Sotiris Christos) — https://nillygraphics.com/")]
[assembly: AssemblyCompany("Nilly graphics (Nillygraphics · Sotiris Christos) — https://nillygraphics.com/")]
[assembly: AssemblyProduct("NillyNote Pro")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Nilly graphics (Sotiris Christos) — https://nillygraphics.com/")]
[assembly: AssemblyTrademark("Nilly graphics / Nillygraphics — Sotiris Christos")]
[assembly: AssemblyVersion("1.1.4.0")]
[assembly: AssemblyFileVersion("1.1.4.0")]

namespace NillyNotePro
{
    internal static class Native
    {
        public const int WM_COPYDATA = 0x004A;
        public const int SW_RESTORE = 9;
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE_20 = 20;
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE_19 = 19;

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
        [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll")] private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr minSize, IntPtr maxSize);

        public static void TrimMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
                SetProcessWorkingSetSize(GetCurrentProcess(), (IntPtr)(-1), (IntPtr)(-1));
            }
            catch { }
        }
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref COPYDATASTRUCT lParam);
        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)] private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        public static void SetDarkScrollbar(IntPtr hwnd, bool dark)
        {
            if (hwnd == IntPtr.Zero) return;
            try { SetWindowTheme(hwnd, dark ? "DarkMode_Explorer" : "Explorer", null); } catch { }
        }

        public static void SetDarkTitlebar(IntPtr hwnd, bool dark)
        {
            if (hwnd == IntPtr.Zero) return;
            try
            {
                int v = dark ? 1 : 0;
                if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_20, ref v, 4) != 0)
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_19, ref v, 4);
            }
            catch { }
        }
    }

    internal sealed class NoteTheme
    {
        public bool IsDark;
        public Color StripBg;
        public Color TabInactiveBg;
        public Color TabHoverBg;
        public Color TabActiveBg;
        public Color MenuBg;
        public Color EditorBg;
        public Color EditorFg;
        public Color MutedFg;
        public Color Border;
        public Color Accent;
        public Color StatusBg;
        public Color InputBg;
        public Color BtnHoverBg;

        public static NoteTheme Dark()
        {
            NoteTheme t = new NoteTheme();
            t.IsDark = true;
            t.StripBg = Color.FromArgb(24, 26, 27);       // #181A1B Ariakit Recessed
            t.TabInactiveBg = Color.FromArgb(24, 26, 27);
            t.TabHoverBg = Color.FromArgb(39, 41, 43);
            t.TabActiveBg = Color.FromArgb(29, 31, 32);   // #1D1F20 Ariakit Canvas
            t.MenuBg = Color.FromArgb(34, 36, 38);        // #222426 Ariakit Panel
            t.EditorBg = Color.FromArgb(29, 31, 32);      // #1D1F20 Ariakit Canvas
            t.EditorFg = Color.FromArgb(242, 244, 247);
            t.MutedFg = Color.FromArgb(154, 160, 168);
            t.Border = Color.FromArgb(54, 57, 61);        // #36393D Hairline
            t.Accent = Color.FromArgb(252, 150, 31);      // #FC961F Signature Nilly Amber
            t.StatusBg = Color.FromArgb(34, 36, 38);      // #222426 Ariakit Panel
            t.InputBg = Color.FromArgb(24, 26, 27);       // #181A1B Recessed
            t.BtnHoverBg = Color.FromArgb(48, 51, 54);
            return t;
        }

        public static NoteTheme Light()
        {
            NoteTheme t = new NoteTheme();
            t.IsDark = false;
            t.StripBg = Color.FromArgb(236, 239, 245);
            t.TabInactiveBg = Color.FromArgb(236, 239, 245);
            t.TabHoverBg = Color.FromArgb(224, 229, 238);
            t.TabActiveBg = Color.FromArgb(255, 255, 255);
            t.MenuBg = Color.FromArgb(247, 248, 251);
            t.EditorBg = Color.FromArgb(255, 255, 255);
            t.EditorFg = Color.FromArgb(22, 24, 30);
            t.MutedFg = Color.FromArgb(95, 105, 120);
            t.Border = Color.FromArgb(214, 220, 230);
            t.Accent = Color.FromArgb(224, 118, 12);      // #E0760C Nilly Amber Light
            t.StatusBg = Color.FromArgb(244, 246, 250);
            t.InputBg = Color.FromArgb(255, 255, 255);
            t.BtnHoverBg = Color.FromArgb(226, 232, 242);
            return t;
        }
    }

    internal sealed class Win11MenuColors : ProfessionalColorTable
    {
        private readonly NoteTheme _t;
        public Win11MenuColors(NoteTheme t) { _t = t; UseSystemColors = false; }
        public override Color ToolStripDropDownBackground { get { return _t.MenuBg; } }
        public override Color ImageMarginGradientBegin { get { return _t.MenuBg; } }
        public override Color ImageMarginGradientMiddle { get { return _t.MenuBg; } }
        public override Color ImageMarginGradientEnd { get { return _t.MenuBg; } }
        public override Color MenuBorder { get { return _t.Border; } }
        public override Color MenuItemBorder { get { return Color.Transparent; } }
        public override Color MenuItemSelected { get { return _t.BtnHoverBg; } }
        public override Color MenuItemSelectedGradientBegin { get { return _t.BtnHoverBg; } }
        public override Color MenuItemSelectedGradientEnd { get { return _t.BtnHoverBg; } }
        public override Color MenuItemPressedGradientBegin { get { return _t.TabActiveBg; } }
        public override Color MenuItemPressedGradientEnd { get { return _t.TabActiveBg; } }
        public override Color MenuStripGradientBegin { get { return _t.MenuBg; } }
        public override Color MenuStripGradientEnd { get { return _t.MenuBg; } }
        public override Color SeparatorDark { get { return _t.Border; } }
        public override Color SeparatorLight { get { return _t.MenuBg; } }
        public override Color CheckBackground { get { return _t.BtnHoverBg; } }
        public override Color CheckSelectedBackground { get { return _t.BtnHoverBg; } }
    }

    internal sealed class Win11MenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly NoteTheme _t;
        public Win11MenuRenderer(NoteTheme t) : base(new Win11MenuColors(t)) { _t = t; RoundedEdges = false; }
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? _t.EditorFg : _t.MutedFg;
            base.OnRenderItemText(e);
        }
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = _t.MutedFg;
            base.OnRenderArrow(e);
        }
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle r = e.ImageRectangle;
            using (Pen p = new Pen(_t.Accent, 2f))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawLines(p, new Point[] {
                    new Point(r.Left + 3, r.Top + r.Height / 2),
                    new Point(r.Left + r.Width / 2 - 1, r.Bottom - 4),
                    new Point(r.Right - 3, r.Top + 4)
                });
            }
        }
    }

    internal sealed class UndoSnapshot
    {
        public string Text;
        public int SelStart;
        public int SelLength;
        public UndoSnapshot(string text, int start, int len) { Text = text; SelStart = start; SelLength = len; }
    }

    internal sealed class NoteTab
    {
        public string Id;
        public string FilePath;
        public string Title;
        public string Content;
        public string SavedContent;
        public bool IsDirty;
        public int SelStart;
        public int SelLength;
        public string EncodingName; // "UTF-8", "UTF-8 BOM", "UTF-16", "Win-1253"
        public string LineEnding;   // "CRLF" or "LF"
        public DateTime LastDiskWriteUtc;
        public readonly List<UndoSnapshot> UndoStack = new List<UndoSnapshot>();
        public readonly List<UndoSnapshot> RedoStack = new List<UndoSnapshot>();

        public NoteTab()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 10);
            FilePath = null;
            Title = "Χωρίς τίτλο";
            Content = "";
            SavedContent = "";
            IsDirty = false;
            EncodingName = "UTF-8";
            LineEnding = "CRLF";
        }

        public string DisplayTitle
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath)) return Path.GetFileName(FilePath);
                if (!string.IsNullOrEmpty(Content))
                {
                    int nl = Content.IndexOfAny(new char[] { '\r', '\n' });
                    string first = (nl >= 0 ? Content.Substring(0, nl) : Content).Trim();
                    if (first.Length > 0) return first.Length > 22 ? first.Substring(0, 22) + "…" : first;
                }
                return Title;
            }
        }
    }

    internal static class TextUtils
    {
        private static readonly UTF8Encoding Utf8Strict = new UTF8Encoding(false, true);
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false, false);
        private static readonly UTF8Encoding Utf8Bom = new UTF8Encoding(true, false);

        public static Encoding GetEncodingByName(string name)
        {
            if (name == "UTF-8 BOM") return Utf8Bom;
            if (name == "UTF-16") return Encoding.Unicode;
            if (name == "Win-1253")
            {
                try { return Encoding.GetEncoding(1253); } catch { return Encoding.Default; }
            }
            return Utf8NoBom;
        }

        public static string ReadFileAutoDetect(string path, out string encName, out string lineEnding)
        {
            byte[] raw = File.ReadAllBytes(path);
            string text;
            if (raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF)
            {
                encName = "UTF-8 BOM";
                text = Utf8NoBom.GetString(raw, 3, raw.Length - 3);
            }
            else if (raw.Length >= 2 && raw[0] == 0xFF && raw[1] == 0xFE)
            {
                encName = "UTF-16";
                text = Encoding.Unicode.GetString(raw, 2, raw.Length - 2);
            }
            else if (raw.Length >= 2 && raw[0] == 0xFE && raw[1] == 0xFF)
            {
                encName = "UTF-16";
                text = Encoding.BigEndianUnicode.GetString(raw, 2, raw.Length - 2);
            }
            else
            {
                try
                {
                    text = Utf8Strict.GetString(raw);
                    encName = "UTF-8";
                }
                catch
                {
                    encName = "Win-1253";
                    try { text = Encoding.GetEncoding(1253).GetString(raw); }
                    catch { text = Encoding.Default.GetString(raw); }
                }
            }
            bool hasCrlf = text.IndexOf("\r\n", StringComparison.Ordinal) >= 0;
            bool hasLf = text.IndexOf('\n') >= 0;
            lineEnding = (hasLf && !hasCrlf) ? "LF" : "CRLF";
            // Win32 TextBox requires CRLF internally
            return NormalizeToCrlf(text);
        }

        public static void WriteFileWithEncoding(string path, string crlfText, string encName, string lineEnding)
        {
            string toSave = (lineEnding == "LF") ? crlfText.Replace("\r\n", "\n") : NormalizeToCrlf(crlfText);
            Encoding enc = GetEncodingByName(encName);
            File.WriteAllText(path, toSave, enc);
        }

        public static string NormalizeToCrlf(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        }

        public static void CountStats(string text, out int words, out int chars, out int lines)
        {
            if (string.IsNullOrEmpty(text)) { words = 0; chars = 0; lines = 1; return; }
            int w = 0, c = 0, l = 1;
            bool inWord = false;
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch == '\r') continue;
                if (ch == '\n') { l++; inWord = false; continue; }
                c++;
                if (char.IsWhiteSpace(ch)) inWord = false;
                else if (!inWord) { inWord = true; w++; }
            }
            words = w; chars = c; lines = l;
        }

        public static string ToGreeklish(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            string s = input;
            string[,] pairs = new string[,] {
                {"ΟΥ","OU"},{"Ού","Ou"},{"ού","ου"},{"ου","ou"},{"ΑΥ","AV"},{"Αυ","Av"},{"αυ","av"},
                {"ΕΥ","EV"},{"Ευ","Ev"},{"ευ","ev"},{"ΜΠ","MP"},{"Μπ","Mp"},{"μπ","mp"},
                {"ΝΤ","NT"},{"Ντ","Nt"},{"ντ","nt"},{"ΓΚ","GK"},{"Γκ","Gk"},{"γκ","gk"},
                {"ΤΣ","TS"},{"Τσ","Ts"},{"τσ","ts"},{"ΤΖ","TZ"},{"Τζ","Tz"},{"τζ","tz"},
                {"Θ","TH"},{"θ","th"},{"Χ","CH"},{"χ","ch"},{"Ψ","PS"},{"ψ","ps"},{"Ξ","X"},{"ξ","x"}
            };
            for (int i = 0; i < pairs.GetLength(0); i++)
                s = s.Replace(pairs[i, 0], pairs[i, 1]);

            StringBuilder sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                switch (ch)
                {
                    case 'Α': case 'Ά': sb.Append('A'); break;
                    case 'α': case 'ά': sb.Append('a'); break;
                    case 'Β': sb.Append('V'); break;
                    case 'β': sb.Append('v'); break;
                    case 'Γ': sb.Append('G'); break;
                    case 'γ': sb.Append('g'); break;
                    case 'Δ': sb.Append('D'); break;
                    case 'δ': sb.Append('d'); break;
                    case 'Ε': case 'Έ': sb.Append('E'); break;
                    case 'ε': case 'έ': sb.Append('e'); break;
                    case 'Ζ': sb.Append('Z'); break;
                    case 'ζ': sb.Append('z'); break;
                    case 'Η': case 'Ή': sb.Append('I'); break;
                    case 'η': case 'ή': sb.Append('i'); break;
                    case 'Ι': case 'Ί': case 'Ϊ': sb.Append('I'); break;
                    case 'ι': case 'ί': case 'ϊ': case 'ΐ': sb.Append('i'); break;
                    case 'Κ': sb.Append('K'); break;
                    case 'κ': sb.Append('k'); break;
                    case 'Λ': sb.Append('L'); break;
                    case 'λ': sb.Append('l'); break;
                    case 'Μ': sb.Append('M'); break;
                    case 'μ': sb.Append('m'); break;
                    case 'Ν': sb.Append('N'); break;
                    case 'ν': sb.Append('n'); break;
                    case 'Ο': case 'Ό': sb.Append('O'); break;
                    case 'ο': case 'ό': sb.Append('o'); break;
                    case 'Π': sb.Append('P'); break;
                    case 'π': sb.Append('p'); break;
                    case 'Ρ': sb.Append('R'); break;
                    case 'ρ': sb.Append('r'); break;
                    case 'Σ': sb.Append('S'); break;
                    case 'σ': case 'ς': sb.Append('s'); break;
                    case 'Τ': sb.Append('T'); break;
                    case 'τ': sb.Append('t'); break;
                    case 'Υ': case 'Ύ': case 'Ϋ': sb.Append('Y'); break;
                    case 'υ': case 'ύ': case 'ϋ': case 'ΰ': sb.Append('y'); break;
                    case 'Φ': sb.Append('F'); break;
                    case 'φ': sb.Append('f'); break;
                    case 'Ω': case 'Ώ': sb.Append('O'); break;
                    case 'ω': case 'ώ': sb.Append('o'); break;
                    default: sb.Append(ch); break;
                }
            }
            return sb.ToString();
        }

        public static string ToSeoSlug(string input)
        {
            string gl = ToGreeklish(input).ToLowerInvariant();
            string cleaned = Regex.Replace(gl, @"[^a-z0-9\r\n]+", "-");
            string[] lines = NormalizeToCrlf(cleaned).Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++) lines[i] = lines[i].Trim('-');
            return string.Join("\r\n", lines);
        }

        public static string FormatJson(string input, bool minify)
        {
            if (string.IsNullOrEmpty(input)) return "";
            StringBuilder sb = new StringBuilder(input.Length + 64);
            bool inStr = false;
            bool esc = false;
            int indent = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (inStr)
                {
                    sb.Append(ch);
                    if (esc) esc = false;
                    else if (ch == '\\') esc = true;
                    else if (ch == '"') inStr = false;
                    continue;
                }
                if (char.IsWhiteSpace(ch)) continue;
                if (ch == '"') { inStr = true; sb.Append(ch); }
                else if (ch == '{' || ch == '[')
                {
                    sb.Append(ch);
                    if (!minify) { indent++; sb.Append("\r\n").Append(' ', indent * 2); }
                }
                else if (ch == '}' || ch == ']')
                {
                    if (!minify) { indent = Math.Max(0, indent - 1); sb.Append("\r\n").Append(' ', indent * 2); }
                    sb.Append(ch);
                }
                else if (ch == ',')
                {
                    sb.Append(ch);
                    if (!minify) sb.Append("\r\n").Append(' ', indent * 2);
                }
                else if (ch == ':')
                {
                    sb.Append(minify ? ":" : ": ");
                }
                else sb.Append(ch);
            }
            return sb.ToString();
        }

        public static string ToggleChecklistLines(string text, int selStart, int selLen, out int newSelStart, out int newSelLen)
        {
            int lineStart = selStart;
            while (lineStart > 0 && text[lineStart - 1] != '\n') lineStart--;
            int lineEnd = selStart + selLen;
            while (lineEnd < text.Length && text[lineEnd] != '\r' && text[lineEnd] != '\n') lineEnd++;

            string block = text.Substring(lineStart, lineEnd - lineStart);
            string[] lines = block.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                string trimmed = l.TrimStart(' ', '\t');
                string lead = l.Substring(0, l.Length - trimmed.Length);
                if (trimmed.StartsWith("☐ ")) lines[i] = lead + "☑ " + trimmed.Substring(2);
                else if (trimmed.StartsWith("☑ ")) lines[i] = lead + trimmed.Substring(2);
                else if (trimmed.StartsWith("[ ] ")) lines[i] = lead + "[x] " + trimmed.Substring(4);
                else if (trimmed.StartsWith("[x] ") || trimmed.StartsWith("[X] ")) lines[i] = lead + trimmed.Substring(4);
                else lines[i] = lead + "☐ " + trimmed;
            }
            string replaced = string.Join("\r\n", lines);
            newSelStart = lineStart;
            newSelLen = replaced.Length;
            return text.Substring(0, lineStart) + replaced + text.Substring(lineEnd);
        }
    }

    // Custom ultra-clean Windows 11 Tab Bar
    internal sealed class Win11TabBar : Control
    {
        public NoteTheme Theme;
        public List<NoteTab> Tabs;
        public int ActiveIndex;

        public event Action<int> TabSelected;
        public event Action<int> TabClosed;
        public event Action NewTabRequested;
        public event Action<int, Point> TabContextRequested;

        private readonly List<Rectangle> _tabRects = new List<Rectangle>();
        private readonly List<Rectangle> _closeRects = new List<Rectangle>();
        private Rectangle _addRect = Rectangle.Empty;
        private int _hoverTab = -1;
        private int _hoverClose = -1;
        private bool _hoverAdd = false;

        public Win11TabBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Height = 36;
            Dock = DockStyle.Top;
            Font = new Font("Segoe UI", 9f);
        }

        private void ComputeLayout()
        {
            _tabRects.Clear();
            _closeRects.Clear();
            if (Tabs == null || Tabs.Count == 0)
            {
                _addRect = new Rectangle(8, 6, 28, 24);
                return;
            }
            int maxW = 210;
            int minW = 90;
            int avail = Math.Max(120, Width - 52);
            int tabW = Math.Max(minW, Math.Min(maxW, avail / Tabs.Count));
            int x = 8;
            for (int i = 0; i < Tabs.Count; i++)
            {
                Rectangle r = new Rectangle(x, 5, tabW - 3, Height - 5);
                _tabRects.Add(r);
                _closeRects.Add(new Rectangle(r.Right - 24, r.Top + 6, 18, 18));
                x += tabW;
            }
            _addRect = new Rectangle(Math.Min(Width - 34, x + 4), 7, 26, 23);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Theme == null) return;
            ComputeLayout();
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.StripBg);

            // Bottom subtle divider line
            using (Pen bp = new Pen(Theme.Border))
                g.DrawLine(bp, 0, Height - 1, Width, Height - 1);

            for (int i = 0; Tabs != null && i < Tabs.Count && i < _tabRects.Count; i++)
            {
                Rectangle r = _tabRects[i];
                bool active = (i == ActiveIndex);
                Color bg = active ? Theme.TabActiveBg : (i == _hoverTab ? Theme.TabHoverBg : Theme.TabInactiveBg);

                using (GraphicsPath gp = RoundedTopRect(r, 7))
                using (SolidBrush br = new SolidBrush(bg))
                {
                    g.FillPath(br, gp);
                    if (active)
                    {
                        using (Pen p = new Pen(Theme.Border))
                            g.DrawPath(p, gp);
                        // Cover bottom border under active tab so it merges seamlessly with menu/editor
                        using (Pen pCover = new Pen(Theme.TabActiveBg, 2f))
                            g.DrawLine(pCover, r.Left + 1, Height - 1, r.Right - 1, Height - 1);
                        // Crisp Nilly Amber accent bar at top of active tab (twin of NillyCode Pro!)
                        using (Pen pAcc = new Pen(Theme.Accent, 2.2f))
                            g.DrawLine(pAcc, r.Left + 5, r.Top + 1, r.Right - 5, r.Top + 1);
                    }
                }

                // Colored file indicator dot (Amber active / Cyan inactive, matching NillyCode Pro)
                Color dotCol = active ? Theme.Accent : (Theme.IsDark ? Color.FromArgb(102, 217, 239) : Color.FromArgb(14, 132, 168));
                using (SolidBrush dbr = new SolidBrush(dotCol))
                    g.FillEllipse(dbr, r.Left + 10, r.Top + (r.Height - 6) / 2 + 1, 6, 6);

                // Title text
                NoteTab tab = Tabs[i];
                string title = (tab.IsDirty ? "● " : "") + tab.DisplayTitle;
                Rectangle txtRect = new Rectangle(r.Left + 21, r.Top + 4, r.Width - 47, r.Height - 6);
                Color fg = active ? Theme.EditorFg : Theme.MutedFg;
                TextRenderer.DrawText(g, title, Font, txtRect, fg,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

                // Close '×' button
                Rectangle cr = _closeRects[i];
                if (active || i == _hoverTab)
                {
                    if (i == _hoverClose)
                    {
                        using (SolidBrush cb = new SolidBrush(Theme.BtnHoverBg))
                            g.FillRectangle(cb, cr);
                    }
                    using (Pen xp = new Pen(i == _hoverClose ? Theme.EditorFg : Theme.MutedFg, 1.35f))
                    {
                        int m = 5;
                        g.DrawLine(xp, cr.Left + m, cr.Top + m, cr.Right - m, cr.Bottom - m);
                        g.DrawLine(xp, cr.Right - m, cr.Top + m, cr.Left + m, cr.Bottom - m);
                    }
                }
            }

            // '+' Add tab button
            if (_hoverAdd)
            {
                using (SolidBrush ab = new SolidBrush(Theme.BtnHoverBg))
                    g.FillRectangle(ab, _addRect);
            }
            using (Pen pp = new Pen(_hoverAdd ? Theme.EditorFg : Theme.MutedFg, 1.5f))
            {
                int cx = _addRect.Left + _addRect.Width / 2;
                int cy = _addRect.Top + _addRect.Height / 2;
                g.DrawLine(pp, cx - 5, cy, cx + 5, cy);
                g.DrawLine(pp, cx, cy - 5, cx, cy + 5);
            }
        }

        private static GraphicsPath RoundedTopRect(Rectangle r, int rad)
        {
            GraphicsPath gp = new GraphicsPath();
            int d = rad * 2;
            gp.AddArc(r.Left, r.Top, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            gp.AddLine(r.Right, r.Bottom, r.Left, r.Bottom);
            gp.CloseFigure();
            return gp;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int ht = -1, hc = -1;
            for (int i = 0; i < _tabRects.Count; i++)
            {
                if (_tabRects[i].Contains(e.Location))
                {
                    ht = i;
                    if (_closeRects[i].Contains(e.Location)) hc = i;
                    break;
                }
            }
            bool ha = _addRect.Contains(e.Location);
            if (ht != _hoverTab || hc != _hoverClose || ha != _hoverAdd)
            {
                _hoverTab = ht; _hoverClose = hc; _hoverAdd = ha;
                Cursor = (ht >= 0 || ha) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hoverTab = -1; _hoverClose = -1; _hoverAdd = false;
            Cursor = Cursors.Default;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            ComputeLayout();
            if (e.Button == MouseButtons.Left)
            {
                if (_addRect.Contains(e.Location))
                {
                    if (NewTabRequested != null) NewTabRequested();
                    return;
                }
                for (int i = 0; i < _tabRects.Count; i++)
                {
                    if (_tabRects[i].Contains(e.Location))
                    {
                        if (_closeRects[i].Contains(e.Location))
                        {
                            if (TabClosed != null) TabClosed(i);
                        }
                        else if (TabSelected != null)
                        {
                            TabSelected(i);
                        }
                        return;
                    }
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < _tabRects.Count; i++)
                {
                    if (_tabRects[i].Contains(e.Location))
                    {
                        if (TabClosed != null) TabClosed(i);
                        return;
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < _tabRects.Count; i++)
                {
                    if (_tabRects[i].Contains(e.Location))
                    {
                        if (TabContextRequested != null) TabContextRequested(i, e.Location);
                        return;
                    }
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            Point pt = PointToClient(Cursor.Position);
            for (int i = 0; i < _tabRects.Count; i++)
                if (_tabRects[i].Contains(pt)) return;
            if (NewTabRequested != null) NewTabRequested();
            base.OnDoubleClick(e);
        }
    }

    // Ultra-fast Native TextBox with fixed Ctrl+Backspace & Ctrl+Wheel Zoom
    internal sealed class NoteEditor : TextBox
    {
        public event Action<int> ZoomWheelDelta;

        public NoteEditor()
        {
            Multiline = true;
            AcceptsTab = true;
            AcceptsReturn = true;
            BorderStyle = BorderStyle.None;
            MaxLength = 0;
            HideSelection = false;
            ScrollBars = ScrollBars.Vertical;
            WordWrap = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Fix infamous WinForms TextBox Ctrl+Backspace box character!
            if (e.Control && e.KeyCode == Keys.Back)
            {
                e.SuppressKeyPress = true;
                int pos = SelectionStart;
                if (SelectionLength > 0)
                {
                    SelectedText = "";
                }
                else if (pos > 0)
                {
                    string t = Text;
                    int i = pos - 1;
                    while (i > 0 && (t[i] == ' ' || t[i] == '\t')) i--;
                    if (t[i] == '\n' && i > 0 && t[i - 1] == '\r') i--;
                    else if (char.IsLetterOrDigit(t[i]) || t[i] == '_')
                    {
                        while (i > 0 && (char.IsLetterOrDigit(t[i - 1]) || t[i - 1] == '_')) i--;
                    }
                    else
                    {
                        while (i > 0 && !char.IsWhiteSpace(t[i - 1]) && !char.IsLetterOrDigit(t[i - 1]) && t[i - 1] != '_') i--;
                    }
                    SelectionStart = i;
                    SelectionLength = pos - i;
                    SelectedText = "";
                }
                return;
            }
            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                SelectAll();
                return;
            }
            base.OnKeyDown(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control && ZoomWheelDelta != null)
            {
                ZoomWheelDelta(e.Delta > 0 ? 10 : -10);
                return;
            }
            base.OnMouseWheel(e);
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly string _appDataDir;
        private readonly string _draftsDir;
        private readonly string _settingsPath;

        private NoteTheme _theme;
        private readonly List<NoteTab> _tabs = new List<NoteTab>();
        private readonly List<string> _recentFiles = new List<string>();
        private int _activeIdx = -1;
        private bool _switching = false;
        private bool _undoCoalesce = false;
        private DateTime _lastUndoPushUtc = DateTime.MinValue;
        private bool _sessionDirty = false;
        private bool _suppressExitSave = false;

        private string _fontFamily = "Segoe UI";
        private float _baseFontSize = 11f;
        private int _zoomPercent = 100;
        private bool _wordWrap = true;
        private bool _darkMode = true;
        private bool _pinned = false;
        private int _opacityPercent = 100;

        // UI controls
        private Win11TabBar _tabBar;
        private Panel _menuHost;
        private MenuStrip _menu;
        private ToolStripMenuItem _miRecent;
        private ToolStripMenuItem _miWordWrap;
        private ToolStripMenuItem _miPin;
        private AkButton _btnQuickFind;
        private AkButton _btnQuickPin;
        private AkButton _btnQuickTheme;

        // Inline Find & Replace bar
        private Panel _findPanel;
        private TextBox _txtFind;
        private TextBox _txtReplace;
        private Label _lblFindCount;
        private AkButton _btnCase;
        private AkButton _btnRegex;
        private AkButton _btnToggleReplace;
        private Panel _replaceRow;
        private bool _findCase = false;
        private bool _findRegex = false;
        private bool _replaceVisible = false;

        // Editor
        private Panel _editorHost;
        private NoteEditor _editor;

        // Status bar
        private Panel _statusPanel;
        private Label _lblPos;
        private Label _lblStats;
        private Label _lblBrand;
        private Label _lblZoom;
        private Label _lblEol;
        private Label _lblEnc;

        private System.Windows.Forms.Timer _autoSaveTimer;
        private string _printText;
        private int _printPos;

        public MainForm(bool smokeMode) : this(null, smokeMode) { }

        public MainForm(string[] initialFiles, bool smokeMode)
        {
            _suppressExitSave = smokeMode;
            _appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NillyNotePro");
            _draftsDir = Path.Combine(_appDataDir, "drafts");
            _settingsPath = Path.Combine(_appDataDir, "settings.ini");

            Text = "NillyNote Pro";
            Size = new Size(920, 620);
            MinimumSize = new Size(460, 300);
            StartPosition = FormStartPosition.CenterScreen;
            AllowDrop = true;
            KeyPreview = true;

            try
            {
                string ico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NillyNotePro.ico");
                if (File.Exists(ico)) Icon = new Icon(ico);
                else Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            if (!smokeMode) LoadSettings();
            _theme = _darkMode ? NoteTheme.Dark() : NoteTheme.Light();

            BuildUi();
            ApplyTheme();

            if (!smokeMode) RestoreSession();

            if (initialFiles != null)
            {
                for (int i = 0; i < initialFiles.Length; i++)
                {
                    string f = initialFiles[i];
                    if (!string.IsNullOrEmpty(f) && !f.StartsWith("-"))
                        OpenOrCreateFilePath(f);
                }
            }

            if (_tabs.Count == 0) CreateNewTab(null, "", true);

            _autoSaveTimer = new System.Windows.Forms.Timer();
            _autoSaveTimer.Interval = 30000;
            _autoSaveTimer.Tick += delegate
            {
                if (_sessionDirty && !_suppressExitSave)
                {
                    SyncActiveTabFromEditor();
                    SaveSession();
                    _sessionDirty = false;
                }
            };
            if (!smokeMode) _autoSaveTimer.Start();
        }

        private void BuildUi()
        {
            // 1. Bottom Status Bar (dock Bottom first)
            _statusPanel = new Panel();
            _statusPanel.Dock = DockStyle.Bottom;
            _statusPanel.Height = 26;
            _statusPanel.Padding = new Padding(10, 0, 10, 0);

            _lblPos = MakeStatusLbl("Γρ 1, Στ 1", DockStyle.Left, 100, ContentAlignment.MiddleLeft, true, delegate { ShowGoToLineDialog(); });
            _lblStats = MakeStatusLbl("0 λέξεις · 0 χαρ. · 1 γρ.", DockStyle.Left, 240, ContentAlignment.MiddleLeft, false, null);
            _lblBrand = MakeStatusLbl("by Nilly graphics · Sotiris Christos", DockStyle.Right, 215, ContentAlignment.MiddleRight, true, delegate
            {
                try { Process.Start(new ProcessStartInfo("https://nillygraphics.com/") { UseShellExecute = true }); } catch { }
            });
            _lblEnc = MakeStatusLbl("UTF-8", DockStyle.Right, 76, ContentAlignment.MiddleCenter, true, delegate { CycleEncoding(); });
            _lblEol = MakeStatusLbl("CRLF", DockStyle.Right, 52, ContentAlignment.MiddleCenter, true, delegate { ToggleLineEnding(); });
            _lblZoom = MakeStatusLbl("100%", DockStyle.Right, 48, ContentAlignment.MiddleCenter, true, delegate { SetZoom(100); });

            _statusPanel.Controls.Add(_lblStats);
            _statusPanel.Controls.Add(_lblPos);
            _statusPanel.Controls.Add(_lblZoom);
            _statusPanel.Controls.Add(_lblEol);
            _statusPanel.Controls.Add(_lblEnc);
            _statusPanel.Controls.Add(_lblBrand);

            // 2. Editor Host (Fill)
            _editorHost = new Panel();
            _editorHost.Dock = DockStyle.Fill;
            _editorHost.Padding = new Padding(14, 10, 6, 4);

            _editor = new NoteEditor();
            _editor.Dock = DockStyle.Fill;
            _editor.WordWrap = _wordWrap;
            _editor.ScrollBars = _wordWrap ? ScrollBars.Vertical : ScrollBars.Both;
            _editor.TextChanged += OnEditorTextChanged;
            _editor.KeyUp += delegate { UpdateStatusBar(); };
            _editor.MouseUp += delegate { UpdateStatusBar(); };
            _editor.ZoomWheelDelta += delegate(int delta) { SetZoom(_zoomPercent + delta); };
            _editor.ContextMenuStrip = BuildEditorContextMenu();
            _editorHost.Controls.Add(_editor);

            // 3. Inline Find & Replace Panel (Dock Top, hidden initially)
            BuildFindPanel();

            // 4. Clean Menu Bar with compact right-side quick buttons (Find, Pin, Theme)
            _menuHost = new Panel();
            _menuHost.Dock = DockStyle.Top;
            _menuHost.Height = 29;

            _menu = new MenuStrip();
            _menu.Dock = DockStyle.Fill;
            _menu.Padding = new Padding(6, 2, 0, 2);
            _menu.Font = new Font("Segoe UI", 9f);
            BuildMenus();

            FlowLayoutPanel rightQuick = new FlowLayoutPanel();
            rightQuick.Dock = DockStyle.Right;
            rightQuick.Width = 104;
            rightQuick.FlowDirection = FlowDirection.RightToLeft;
            rightQuick.WrapContents = false;
            rightQuick.Padding = new Padding(0, 2, 6, 2);

            _btnQuickTheme = MakeMiniBtn(AkIcon.Sun, "Εναλλαγή Dark / Light", 28, delegate { _darkMode = !_darkMode; ApplyTheme(); });
            _btnQuickPin = MakeMiniBtn(AkIcon.Pin, "Καρφίτσωμα επάνω (Ctrl+T)", 28, delegate { TogglePin(); });
            _btnQuickFind = MakeMiniBtn(AkIcon.Search, "Εύρεση (Ctrl+F)", 28, delegate { OpenFindBar(false); });
            rightQuick.Controls.Add(_btnQuickTheme);
            rightQuick.Controls.Add(_btnQuickPin);
            rightQuick.Controls.Add(_btnQuickFind);

            _menuHost.Controls.Add(_menu);
            _menuHost.Controls.Add(rightQuick);

            // 5. Windows 11 Tab Bar (Topmost)
            _tabBar = new Win11TabBar();
            _tabBar.Tabs = _tabs;
            _tabBar.TabSelected += delegate(int idx) { SwitchToTab(idx); };
            _tabBar.TabClosed += delegate(int idx) { CloseTab(idx); };
            _tabBar.NewTabRequested += delegate { CreateNewTab(null, "", true); };
            _tabBar.TabContextRequested += OnTabContextMenu;

            // Add in proper docking z-order (Fill first, then Top/Bottom)
            Controls.Add(_editorHost);
            Controls.Add(_findPanel);
            Controls.Add(_menuHost);
            Controls.Add(_tabBar);
            Controls.Add(_statusPanel);

            Resize += delegate { UpdateResponsiveStatus(); };
            UpdateResponsiveStatus();
        }

        private void UpdateResponsiveStatus()
        {
            if (_lblBrand == null) return;
            int w = ClientSize.Width;
            _lblBrand.Visible = (w >= 680);
            _lblEnc.Visible = (w >= 540);
            _lblEol.Visible = (w >= 500);
        }

        private Label MakeStatusLbl(string txt, DockStyle dock, int w, ContentAlignment align, bool clickable, EventHandler onClick)
        {
            Label l = new Label();
            l.Text = txt;
            l.Dock = dock;
            l.Width = w;
            l.TextAlign = align;
            l.Font = new Font("Segoe UI", 8.5f);
            if (clickable)
            {
                l.Cursor = Cursors.Hand;
                if (onClick != null) l.Click += onClick;
            }
            return l;
        }

        private AkButton MakeMiniBtn(AkIcon icon, string tip, int width, EventHandler onClick)
        {
            AkButton b = new AkButton();
            b.Kind = AkKind.Ghost;
            b.Icon = icon;
            b.Text = "";
            b.Width = width;
            b.Height = 24;
            b.Margin = new Padding(2, 0, 2, 0);
            b.Click += onClick;
            ToolTip tt = new ToolTip();
            tt.SetToolTip(b, tip);
            return b;
        }

        private void BuildMenus()
        {
            ToolStripMenuItem mFile = new ToolStripMenuItem("Αρχείο");
            mFile.DropDownItems.Add(MkItem("Νέα καρτέλα", Keys.Control | Keys.N, delegate { CreateNewTab(null, "", true); }));
            mFile.DropDownItems.Add(MkItem("Άνοιγμα...", Keys.Control | Keys.O, delegate { OpenFileDialogFlow(); }));
            _miRecent = new ToolStripMenuItem("Πρόσφατα αρχεία");
            mFile.DropDownItems.Add(_miRecent);
            mFile.DropDownItems.Add(new ToolStripSeparator());
            mFile.DropDownItems.Add(MkItem("Αποθήκευση", Keys.Control | Keys.S, delegate { SaveActiveTab(false); }));
            mFile.DropDownItems.Add(MkItem("Αποθήκευση ως...", Keys.Control | Keys.Shift | Keys.S, delegate { SaveActiveTab(true); }));
            mFile.DropDownItems.Add(MkItem("Αποθήκευση όλων", Keys.Control | Keys.Alt | Keys.S, delegate { SaveAllTabs(); }));
            mFile.DropDownItems.Add(MkItem("Επαναφόρτωση από δίσκο", Keys.None, delegate { ReloadActiveFromDisk(); }));
            mFile.DropDownItems.Add(new ToolStripSeparator());
            string codeExe = FindSiblingExe("NillyCodePro", "NillyCodePro.exe");
            if (codeExe != null)
                mFile.DropDownItems.Add(MkItem("</> Συνέχεια στο NillyCode Pro", Keys.None, delegate { SendActiveToCode(codeExe); }));
            string filesExe = FindSiblingExe("NillyFilesPro", "NillyFilesPro.exe");
            if (filesExe != null)
                mFile.DropDownItems.Add(MkItem("Άνοιγμα Φακέλου σε NillyFiles Pro", Keys.None, delegate { OpenActiveFolderInFiles(filesExe); }));
            mFile.DropDownItems.Add(new ToolStripSeparator());
            mFile.DropDownItems.Add(MkItem("Εκτύπωση...", Keys.Control | Keys.P, delegate { PrintActiveDocument(); }));
            mFile.DropDownItems.Add(new ToolStripSeparator());
            mFile.DropDownItems.Add(MkItem("Κλείσιμο καρτέλας", Keys.Control | Keys.W, delegate { CloseTab(_activeIdx); }));
            mFile.DropDownItems.Add(MkItem("Έξοδος", Keys.Alt | Keys.F4, delegate { Close(); }));

            ToolStripMenuItem mEdit = new ToolStripMenuItem("Επεξεργασία");
            mEdit.DropDownItems.Add(MkItem("Αναίρεση (Undo)", Keys.Control | Keys.Z, delegate { DoUndo(); }));
            mEdit.DropDownItems.Add(MkItem("Επανάληψη (Redo)", Keys.Control | Keys.Y, delegate { DoRedo(); }));
            mEdit.DropDownItems.Add(new ToolStripSeparator());
            mEdit.DropDownItems.Add(MkItem("Αποκοπή", Keys.Control | Keys.X, delegate { _editor.Cut(); }));
            mEdit.DropDownItems.Add(MkItem("Αντιγραφή", Keys.Control | Keys.C, delegate { _editor.Copy(); }));
            mEdit.DropDownItems.Add(MkItem("Επικόλληση", Keys.Control | Keys.V, delegate { PastePlainText(); }));
            mEdit.DropDownItems.Add(MkItem("Επιλογή όλων", Keys.Control | Keys.A, delegate { _editor.SelectAll(); }));
            mEdit.DropDownItems.Add(new ToolStripSeparator());
            mEdit.DropDownItems.Add(MkItem("Εύρεση...", Keys.Control | Keys.F, delegate { OpenFindBar(false); }));
            mEdit.DropDownItems.Add(MkItem("Αντικατάσταση...", Keys.Control | Keys.H, delegate { OpenFindBar(true); }));
            mEdit.DropDownItems.Add(MkItem("Μετάβαση σε γραμμή...", Keys.Control | Keys.G, delegate { ShowGoToLineDialog(); }));
            mEdit.DropDownItems.Add(new ToolStripSeparator());
            mEdit.DropDownItems.Add(MkItem("Ημερομηνία / Ώρα", Keys.F5, delegate { InsertTimestamp(); }));
            mEdit.DropDownItems.Add(MkItem("Checklist ☐ / ☑", Keys.Control | Keys.L, delegate { ToggleChecklist(); }));

            ToolStripMenuItem mTools = new ToolStripMenuItem("Εργαλεία Pro");
            mTools.DropDownItems.Add(MkItem("Ταξινόμηση Γραμμών (Α → Ω)", Keys.None, delegate { TransformLines("sort_asc"); }));
            mTools.DropDownItems.Add(MkItem("Ταξινόμηση Γραμμών (Ω → Α)", Keys.None, delegate { TransformLines("sort_desc"); }));
            mTools.DropDownItems.Add(MkItem("Αφαίρεση Διπλότυπων Γραμμών", Keys.None, delegate { TransformLines("dedupe"); }));
            mTools.DropDownItems.Add(MkItem("Αφαίρεση Κενών Γραμμών", Keys.None, delegate { TransformLines("no_empty"); }));
            mTools.DropDownItems.Add(MkItem("Καθαρισμός Κενών (Trim)", Keys.None, delegate { TransformLines("trim"); }));
            mTools.DropDownItems.Add(new ToolStripSeparator());
            mTools.DropDownItems.Add(MkItem("ΚΕΦΑΛΑΙΑ (UPPERCASE)", Keys.None, delegate { TransformSelectionOrAll("upper"); }));
            mTools.DropDownItems.Add(MkItem("πεζά (lowercase)", Keys.None, delegate { TransformSelectionOrAll("lower"); }));
            mTools.DropDownItems.Add(MkItem("Ελληνικά → Greeklish", Keys.None, delegate { TransformSelectionOrAll("greeklish"); }));
            mTools.DropDownItems.Add(MkItem("Δημιουργία SEO Slug", Keys.None, delegate { TransformSelectionOrAll("slug"); }));
            mTools.DropDownItems.Add(new ToolStripSeparator());
            mTools.DropDownItems.Add(MkItem("Μορφοποίηση JSON (Format)", Keys.None, delegate { TransformSelectionOrAll("json_fmt"); }));
            mTools.DropDownItems.Add(MkItem("Συμπίεση JSON (Minify)", Keys.None, delegate { TransformSelectionOrAll("json_min"); }));
            mTools.DropDownItems.Add(new ToolStripSeparator());
            mTools.DropDownItems.Add(MkItem("Σύγκριση με άλλη Καρτέλα (Quick Diff)...", Keys.None, delegate { ShowQuickDiffDialog(); }));

            ToolStripMenuItem mView = new ToolStripMenuItem("Προβολή");
            _miWordWrap = new ToolStripMenuItem("Αναδίπλωση λέξεων (Word Wrap)");
            _miWordWrap.ShortcutKeys = Keys.Alt | Keys.Z;
            _miWordWrap.Checked = _wordWrap;
            _miWordWrap.Click += delegate { ToggleWordWrap(); };
            mView.DropDownItems.Add(_miWordWrap);

            _miPin = new ToolStripMenuItem("Πάντα στην κορυφή (Pin)");
            _miPin.ShortcutKeys = Keys.Control | Keys.T;
            _miPin.Checked = _pinned;
            _miPin.Click += delegate { TogglePin(); };
            mView.DropDownItems.Add(_miPin);

            ToolStripMenuItem mOpac = new ToolStripMenuItem("Διαφάνεια παραθύρου");
            mOpac.DropDownItems.Add(MkItem("100% (Κανονικό)", Keys.None, delegate { SetOpacityPercent(100); }));
            mOpac.DropDownItems.Add(MkItem("90% (Ελαφριά διαφάνεια)", Keys.None, delegate { SetOpacityPercent(90); }));
            mOpac.DropDownItems.Add(MkItem("80% (Overlay)", Keys.None, delegate { SetOpacityPercent(80); }));
            mView.DropDownItems.Add(mOpac);

            mView.DropDownItems.Add(new ToolStripSeparator());
            mView.DropDownItems.Add(MkItem("Μεγέθυνση", Keys.Control | Keys.Oemplus, delegate { SetZoom(_zoomPercent + 10); }));
            mView.DropDownItems.Add(MkItem("Σμίκρυνση", Keys.Control | Keys.OemMinus, delegate { SetZoom(_zoomPercent - 10); }));
            mView.DropDownItems.Add(MkItem("Επαναφορά Zoom (100%)", Keys.Control | Keys.D0, delegate { SetZoom(100); }));
            mView.DropDownItems.Add(new ToolStripSeparator());
            mView.DropDownItems.Add(MkItem("Γραμματοσειρά...", Keys.None, delegate { ChooseFontDialog(); }));
            mView.DropDownItems.Add(MkItem("Εναλλαγή Dark / Light Θέμα", Keys.None, delegate { _darkMode = !_darkMode; ApplyTheme(); }));

            ToolStripMenuItem mHelp = new ToolStripMenuItem("Βοήθεια");
            mHelp.DropDownItems.Add(MkItem("Συντομεύσεις & Σχετικά με το NillyNote Pro", Keys.None, delegate { ShowAboutDialog(); }));
            mHelp.DropDownItems.Add(MkItem("Όροι Χρήσης & Άδεια (MIT · 0 €)", Keys.None, delegate
            {
                try { Process.Start(new ProcessStartInfo("https://nillygraphics.com/terms-and-conditions/el/#free-software") { UseShellExecute = true }); } catch { }
            }));
            mHelp.DropDownItems.Add(MkItem("Nilly graphics (Sotiris Christos) — nillygraphics.com", Keys.None, delegate
            {
                try { Process.Start(new ProcessStartInfo("https://nillygraphics.com/") { UseShellExecute = true }); } catch { }
            }));

            _menu.Items.AddRange(new ToolStripItem[] { mFile, mEdit, mTools, mView, mHelp });
            RebuildRecentMenu();
        }

        private static ToolStripMenuItem MkItem(string text, Keys shortcut, EventHandler onClick)
        {
            ToolStripMenuItem mi = new ToolStripMenuItem(text);
            if (shortcut != Keys.None) mi.ShortcutKeys = shortcut;
            mi.Click += onClick;
            return mi;
        }

        private ContextMenuStrip BuildEditorContextMenu()
        {
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Items.Add("Αναίρεση (Ctrl+Z)", null, delegate { DoUndo(); });
            cms.Items.Add("Επανάληψη (Ctrl+Y)", null, delegate { DoRedo(); });
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add("Αποκοπή", null, delegate { _editor.Cut(); });
            cms.Items.Add("Αντιγραφή", null, delegate { _editor.Copy(); });
            cms.Items.Add("Επικόλληση", null, delegate { PastePlainText(); });
            cms.Items.Add("Επιλογή όλων", null, delegate { _editor.SelectAll(); });
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add("Checklist ☐ / ☑ (Ctrl+L)", null, delegate { ToggleChecklist(); });
            cms.Items.Add("Ημερομηνία / Ώρα (F5)", null, delegate { InsertTimestamp(); });
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add("Άνοιγμα Link / Αναζήτηση Google", null, delegate { OpenSelectedLinkOrSearch(); });
            return cms;
        }

        private void BuildFindPanel()
        {
            _findPanel = new Panel();
            _findPanel.Dock = DockStyle.Top;
            _findPanel.Height = 38;
            _findPanel.Visible = false;
            _findPanel.Padding = new Padding(10, 5, 10, 5);

            FlowLayoutPanel row1 = new FlowLayoutPanel();
            row1.Dock = DockStyle.Top;
            row1.Height = 28;
            row1.WrapContents = false;

            _btnToggleReplace = MakeFindBtn("", AkKind.Ghost, AkIcon.ChevRight, "Εμφάνιση Αντικατάστασης", 26, delegate { SetReplaceRowVisible(!_replaceVisible); });
            _txtFind = new TextBox();
            _txtFind.Width = 220;
            _txtFind.Font = new Font("Segoe UI", 9.5f);
            _txtFind.BorderStyle = BorderStyle.FixedSingle;
            _txtFind.Margin = new Padding(4, 2, 4, 2);
            _txtFind.TextChanged += delegate { UpdateFindCount(); };
            _txtFind.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; FindNext(e.Shift); }
                else if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; CloseFindBar(); }
            };

            _lblFindCount = new Label();
            _lblFindCount.Text = "0/0";
            _lblFindCount.Width = 58;
            _lblFindCount.Height = 24;
            _lblFindCount.TextAlign = ContentAlignment.MiddleCenter;
            _lblFindCount.Font = new Font("Segoe UI", 8.5f);

            AkButton btnPrev = MakeFindBtn("", AkKind.Secondary, AkIcon.Up, "Προηγούμενο (Shift+Enter)", 28, delegate { FindNext(true); });
            AkButton btnNext = MakeFindBtn("", AkKind.Secondary, AkIcon.Down, "Επόμενο (Enter)", 28, delegate { FindNext(false); });
            _btnCase = MakeFindBtn("Aa", AkKind.Secondary, AkIcon.None, "Διάκριση πεζών/κεφαλαίων", 32, delegate { _findCase = !_findCase; StyleFindToggles(); UpdateFindCount(); });
            _btnRegex = MakeFindBtn(".*", AkKind.Secondary, AkIcon.None, "Κανονική έκφραση (Regex)", 32, delegate { _findRegex = !_findRegex; StyleFindToggles(); UpdateFindCount(); });
            AkButton btnClose = MakeFindBtn("", AkKind.Ghost, AkIcon.Close, "Κλείσιμο (Esc)", 26, delegate { CloseFindBar(); });

            row1.Controls.AddRange(new Control[] { _btnToggleReplace, _txtFind, _lblFindCount, btnPrev, btnNext, _btnCase, _btnRegex, btnClose });

            _replaceRow = new Panel();
            _replaceRow.Dock = DockStyle.Top;
            _replaceRow.Height = 30;
            _replaceRow.Visible = false;

            FlowLayoutPanel rFlow = new FlowLayoutPanel();
            rFlow.Dock = DockStyle.Fill;
            rFlow.WrapContents = false;
            rFlow.Padding = new Padding(30, 2, 0, 0);

            _txtReplace = new TextBox();
            _txtReplace.Width = 220;
            _txtReplace.Font = new Font("Segoe UI", 9.5f);
            _txtReplace.BorderStyle = BorderStyle.FixedSingle;
            _txtReplace.Margin = new Padding(4, 2, 6, 2);
            _txtReplace.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ReplaceOne(); }
                else if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; CloseFindBar(); }
            };

            AkButton btnRep = MakeFindBtn("Αντικατάσταση", AkKind.Primary, AkIcon.None, "Αντικατάσταση τρέχοντος", 108, delegate { ReplaceOne(); });
            AkButton btnRepAll = MakeFindBtn("Όλα", AkKind.Secondary, AkIcon.None, "Αντικατάσταση όλων", 58, delegate { ReplaceAll(); });
            rFlow.Controls.AddRange(new Control[] { _txtReplace, btnRep, btnRepAll });
            _replaceRow.Controls.Add(rFlow);

            _findPanel.Controls.Add(_replaceRow);
            _findPanel.Controls.Add(row1);
        }

        private AkButton MakeFindBtn(string text, AkKind kind, AkIcon icon, string tip, int width, EventHandler onClick)
        {
            AkButton b = new AkButton();
            b.Kind = kind;
            b.Icon = icon;
            b.Text = text;
            b.Width = width;
            b.Height = 25;
            b.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            b.Margin = new Padding(2, 1, 2, 1);
            b.Click += onClick;
            ToolTip tt = new ToolTip();
            tt.SetToolTip(b, tip);
            return b;
        }

        public void ApplyTheme()
        {
            _theme = _darkMode ? NoteTheme.Dark() : NoteTheme.Light();
            Native.SetDarkTitlebar(Handle, _darkMode);

            BackColor = _theme.EditorBg;
            _tabBar.Theme = _theme;
            _tabBar.Invalidate();

            _menuHost.BackColor = _theme.MenuBg;
            _menu.BackColor = _theme.MenuBg;
            _menu.ForeColor = _theme.EditorFg;
            _menu.Renderer = new Win11MenuRenderer(_theme);

            _btnQuickTheme.Icon = _darkMode ? AkIcon.Sun : AkIcon.Moon;
            _btnQuickPin.Active = _pinned;
            foreach (AkButton b in new AkButton[] { _btnQuickFind, _btnQuickPin, _btnQuickTheme })
            {
                b.Dark = _darkMode;
                b.Invalidate();
            }

            _findPanel.BackColor = _theme.MenuBg;
            _txtFind.BackColor = _theme.InputBg; _txtFind.ForeColor = _theme.EditorFg;
            _txtReplace.BackColor = _theme.InputBg; _txtReplace.ForeColor = _theme.EditorFg;
            _lblFindCount.ForeColor = _theme.MutedFg;
            StyleFindButtonsRecursive(_findPanel);
            StyleFindToggles();

            _editorHost.BackColor = _theme.EditorBg;
            _editor.BackColor = _theme.EditorBg;
            _editor.ForeColor = _theme.EditorFg;
            if (_editor.IsHandleCreated) Native.SetDarkScrollbar(_editor.Handle, _darkMode);
            else _editor.HandleCreated += delegate { Native.SetDarkScrollbar(_editor.Handle, _darkMode); };
            if (_editor.ContextMenuStrip != null)
            {
                _editor.ContextMenuStrip.BackColor = _theme.MenuBg;
                _editor.ContextMenuStrip.ForeColor = _theme.EditorFg;
                _editor.ContextMenuStrip.Renderer = new Win11MenuRenderer(_theme);
            }

            _statusPanel.BackColor = _theme.StatusBg;
            foreach (Label l in new Label[] { _lblPos, _lblStats, _lblZoom, _lblEol, _lblEnc })
                l.ForeColor = _theme.MutedFg;
            _lblBrand.ForeColor = _theme.Accent;

            ApplyEditorFont();
        }

        private void StyleFindButtonsRecursive(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                AkButton b = c as AkButton;
                if (b != null)
                {
                    b.Dark = _darkMode;
                    b.Invalidate();
                }
                else if (c.HasChildren) StyleFindButtonsRecursive(c);
            }
        }

        private void StyleFindToggles()
        {
            if (_btnCase == null || _btnRegex == null) return;
            _btnCase.Active = _findCase;
            _btnCase.Dark = _darkMode;
            _btnCase.Invalidate();
            _btnRegex.Active = _findRegex;
            _btnRegex.Dark = _darkMode;
            _btnRegex.Invalidate();
        }

        private void ApplyEditorFont()
        {
            float sz = Math.Max(7f, Math.Min(48f, _baseFontSize * _zoomPercent / 100f));
            bool prevSwitch = _switching;
            _switching = true;
            try { _editor.Font = new Font(_fontFamily, sz, FontStyle.Regular); }
            catch { _editor.Font = new Font("Segoe UI", sz); }
            finally { _switching = prevSwitch; }
            _lblZoom.Text = _zoomPercent + "%";
        }

        public NoteTab ActiveTab
        {
            get { return (_activeIdx >= 0 && _activeIdx < _tabs.Count) ? _tabs[_activeIdx] : null; }
        }

        public NoteTab CreateNewTab(string filePath, string content, bool makeActive)
        {
            SyncActiveTabFromEditor();
            NoteTab tab = new NoteTab();
            tab.FilePath = filePath;
            tab.Content = TextUtils.NormalizeToCrlf(content ?? "");
            tab.SavedContent = tab.Content;
            tab.IsDirty = false;
            _tabs.Add(tab);
            if (makeActive) SwitchToTab(_tabs.Count - 1);
            else _tabBar.Invalidate();
            _sessionDirty = true;
            return tab;
        }

        public void OpenOrCreateFilePath(string path)
        {
            try
            {
                string full = Path.GetFullPath(path);
                // If already open in a tab, activate it
                for (int i = 0; i < _tabs.Count; i++)
                {
                    if (!string.IsNullOrEmpty(_tabs[i].FilePath) &&
                        _tabs[i].FilePath.Equals(full, StringComparison.OrdinalIgnoreCase))
                    {
                        SwitchToTab(i);
                        return;
                    }
                }
                string enc = "UTF-8", eol = "CRLF", content = "";
                DateTime writeUtc = DateTime.MinValue;
                if (File.Exists(full))
                {
                    content = TextUtils.ReadFileAutoDetect(full, out enc, out eol);
                    writeUtc = File.GetLastWriteTimeUtc(full);
                }
                // Reuse single empty clean Untitled tab if present
                if (_tabs.Count == 1 && string.IsNullOrEmpty(_tabs[0].FilePath) && !_tabs[0].IsDirty && string.IsNullOrEmpty(_tabs[0].Content))
                {
                    NoteTab t0 = _tabs[0];
                    t0.FilePath = full;
                    t0.Content = content;
                    t0.SavedContent = content;
                    t0.EncodingName = enc;
                    t0.LineEnding = eol;
                    t0.LastDiskWriteUtc = writeUtc;
                    t0.IsDirty = false;
                    t0.UndoStack.Clear(); t0.RedoStack.Clear();
                    SwitchToTab(0);
                }
                else
                {
                    NoteTab t = CreateNewTab(full, content, false);
                    t.EncodingName = enc;
                    t.LineEnding = eol;
                    t.LastDiskWriteUtc = writeUtc;
                    SwitchToTab(_tabs.Count - 1);
                }
                AddRecentFile(full);
                _sessionDirty = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Αδυναμία ανοίγματος αρχείου:\r\n" + ex.Message, "NillyNote Pro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SyncActiveTabFromEditor()
        {
            NoteTab t = ActiveTab;
            if (t == null) return;
            t.Content = _editor.Text;
            t.SelStart = _editor.SelectionStart;
            t.SelLength = _editor.SelectionLength;
        }

        public void SwitchToTab(int index)
        {
            if (index < 0 || index >= _tabs.Count) return;
            if (_activeIdx >= 0 && _activeIdx < _tabs.Count && _activeIdx != index)
                SyncActiveTabFromEditor();

            _activeIdx = index;
            _tabBar.ActiveIndex = index;
            NoteTab t = _tabs[index];

            _switching = true;
            try
            {
                _editor.Text = t.Content ?? "";
                int s = Math.Max(0, Math.Min(_editor.Text.Length, t.SelStart));
                int l = Math.Max(0, Math.Min(_editor.Text.Length - s, t.SelLength));
                _editor.SelectionStart = s;
                _editor.SelectionLength = l;
            }
            finally { _switching = false; }

            _undoCoalesce = false;
            _tabBar.Invalidate();
            UpdateWindowTitle();
            UpdateStatusBar();
            if (_findPanel.Visible) UpdateFindCount();
            _editor.Focus();
        }

        private void CloseTab(int index)
        {
            if (index < 0 || index >= _tabs.Count) return;
            if (index == _activeIdx) SyncActiveTabFromEditor();
            NoteTab t = _tabs[index];

            // Prompt only if modified (and has a file path or non-empty content)
            if (t.IsDirty && !string.IsNullOrEmpty(t.Content))
            {
                SwitchToTab(index);
                DialogResult dr = MessageBox.Show(this,
                    "Θέλεις να αποθηκεύσεις τις αλλαγές στο «" + t.DisplayTitle + "»;",
                    "NillyNote Pro", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Cancel) return;
                if (dr == DialogResult.Yes && !SaveActiveTab(false)) return;
            }

            _tabs.RemoveAt(index);
            if (_tabs.Count == 0)
            {
                _activeIdx = -1;
                CreateNewTab(null, "", true);
            }
            else
            {
                int next = Math.Min(index, _tabs.Count - 1);
                _activeIdx = -1;
                SwitchToTab(next);
            }
            _sessionDirty = true;
        }

        private void OnTabContextMenu(int idx, Point pt)
        {
            if (idx < 0 || idx >= _tabs.Count) return;
            NoteTab t = _tabs[idx];
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Renderer = new Win11MenuRenderer(_theme);
            cms.Items.Add("Κλείσιμο καρτέλας", null, delegate { CloseTab(idx); });
            cms.Items.Add("Κλείσιμο των άλλων καρτελών", null, delegate
            {
                for (int i = _tabs.Count - 1; i >= 0; i--)
                    if (i != idx) CloseTab(i);
            });
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add("Αποθήκευση ως...", null, delegate { SwitchToTab(idx); SaveActiveTab(true); });
            if (!string.IsNullOrEmpty(t.FilePath))
            {
                cms.Items.Add("Αντιγραφή διαδρομής αρχείου", null, delegate { try { Clipboard.SetText(t.FilePath); } catch { } });
                cms.Items.Add("Άνοιγμα φακέλου αρχείου", null, delegate
                {
                    try { Process.Start("explorer.exe", "/select,\"" + t.FilePath + "\""); } catch { }
                });
            }
            cms.Show(_tabBar, pt);
        }

        private void OnEditorTextChanged(object sender, EventArgs e)
        {
            if (_switching) return;
            NoteTab t = ActiveTab;
            if (t == null) return;

            DateTime now = DateTime.UtcNow;
            if (!_undoCoalesce || (now - _lastUndoPushUtc).TotalMilliseconds > 800 || Math.Abs(_editor.Text.Length - (t.Content ?? "").Length) > 3)
            {
                PushUndo(t, t.Content ?? "", t.SelStart, t.SelLength);
                _undoCoalesce = true;
            }
            _lastUndoPushUtc = now;

            t.Content = _editor.Text;
            t.SelStart = _editor.SelectionStart;
            t.SelLength = _editor.SelectionLength;
            bool dirty = (t.Content != t.SavedContent);
            if (t.IsDirty != dirty)
            {
                t.IsDirty = dirty;
                _tabBar.Invalidate();
                UpdateWindowTitle();
            }
            else if (string.IsNullOrEmpty(t.FilePath))
            {
                _tabBar.Invalidate();
            }
            _sessionDirty = true;
            UpdateStatusBar();
            if (_findPanel.Visible) UpdateFindCount();
        }

        private static void PushUndo(NoteTab t, string text, int selStart, int selLen)
        {
            if (t.UndoStack.Count > 0 && t.UndoStack[t.UndoStack.Count - 1].Text == text) return;
            t.UndoStack.Add(new UndoSnapshot(text, selStart, selLen));
            if (t.UndoStack.Count > 100) t.UndoStack.RemoveAt(0);
            t.RedoStack.Clear();
        }

        private void DoUndo()
        {
            NoteTab t = ActiveTab;
            if (t == null || t.UndoStack.Count == 0) return;
            SyncActiveTabFromEditor();
            t.RedoStack.Add(new UndoSnapshot(t.Content, t.SelStart, t.SelLength));
            UndoSnapshot snap = t.UndoStack[t.UndoStack.Count - 1];
            t.UndoStack.RemoveAt(t.UndoStack.Count - 1);
            ApplyEditorTextProgrammatically(t, snap.Text, snap.SelStart, snap.SelLength, false);
        }

        private void DoRedo()
        {
            NoteTab t = ActiveTab;
            if (t == null || t.RedoStack.Count == 0) return;
            SyncActiveTabFromEditor();
            t.UndoStack.Add(new UndoSnapshot(t.Content, t.SelStart, t.SelLength));
            UndoSnapshot snap = t.RedoStack[t.RedoStack.Count - 1];
            t.RedoStack.RemoveAt(t.RedoStack.Count - 1);
            ApplyEditorTextProgrammatically(t, snap.Text, snap.SelStart, snap.SelLength, false);
        }

        private void ApplyEditorTextProgrammatically(NoteTab t, string newText, int selStart, int selLen, bool pushUndo)
        {
            if (pushUndo) PushUndo(t, _editor.Text, _editor.SelectionStart, _editor.SelectionLength);
            _switching = true;
            try
            {
                _editor.Text = newText;
                int s = Math.Max(0, Math.Min(newText.Length, selStart));
                int l = Math.Max(0, Math.Min(newText.Length - s, selLen));
                _editor.SelectionStart = s;
                _editor.SelectionLength = l;
            }
            finally { _switching = false; }
            _undoCoalesce = false;
            t.Content = newText;
            t.SelStart = _editor.SelectionStart;
            t.SelLength = _editor.SelectionLength;
            t.IsDirty = (t.Content != t.SavedContent);
            _sessionDirty = true;
            _tabBar.Invalidate();
            UpdateWindowTitle();
            UpdateStatusBar();
        }

        private void PastePlainText()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string txt = TextUtils.NormalizeToCrlf(Clipboard.GetText());
                    NoteTab t = ActiveTab;
                    if (t != null)
                    {
                        PushUndo(t, _editor.Text, _editor.SelectionStart, _editor.SelectionLength);
                        _undoCoalesce = false;
                    }
                    _editor.SelectedText = txt;
                }
            }
            catch { }
        }

        private void OpenFileDialogFlow()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.Filter = "Αρχεία κειμένου (*.txt;*.log;*.md;*.ini;*.csv;*.json;*.srt;*.nfo)|*.txt;*.log;*.md;*.ini;*.cfg;*.conf;*.csv;*.json;*.xml;*.srt;*.nfo|Όλα τα αρχεία (*.*)|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    for (int i = 0; i < ofd.FileNames.Length; i++)
                        OpenOrCreateFilePath(ofd.FileNames[i]);
                }
            }
        }

        private bool SaveActiveTab(bool saveAs)
        {
            NoteTab t = ActiveTab;
            if (t == null) return false;
            SyncActiveTabFromEditor();
            string target = t.FilePath;
            if (saveAs || string.IsNullOrEmpty(target))
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Αρχείο κειμένου (*.txt)|*.txt|Markdown (*.md)|*.md|Log (*.log)|*.log|Όλα τα αρχεία (*.*)|*.*";
                    sfd.FileName = string.IsNullOrEmpty(target) ? "Σημείωση.txt" : Path.GetFileName(target);
                    if (!string.IsNullOrEmpty(target))
                    {
                        try { sfd.InitialDirectory = Path.GetDirectoryName(target); } catch { }
                    }
                    if (sfd.ShowDialog(this) != DialogResult.OK) return false;
                    target = sfd.FileName;
                }
            }
            try
            {
                TextUtils.WriteFileWithEncoding(target, t.Content, t.EncodingName, t.LineEnding);
                t.FilePath = target;
                t.SavedContent = t.Content;
                t.IsDirty = false;
                t.LastDiskWriteUtc = File.GetLastWriteTimeUtc(target);
                AddRecentFile(target);
                _tabBar.Invalidate();
                UpdateWindowTitle();
                _sessionDirty = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Αδυναμία αποθήκευσης:\r\n" + ex.Message, "NillyNote Pro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SaveAllTabs()
        {
            int orig = _activeIdx;
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].IsDirty)
                {
                    SwitchToTab(i);
                    SaveActiveTab(false);
                }
            }
            if (orig >= 0 && orig < _tabs.Count) SwitchToTab(orig);
        }

        private void ReloadActiveFromDisk()
        {
            NoteTab t = ActiveTab;
            if (t == null || string.IsNullOrEmpty(t.FilePath) || !File.Exists(t.FilePath)) return;
            string enc, eol;
            string text = TextUtils.ReadFileAutoDetect(t.FilePath, out enc, out eol);
            t.EncodingName = enc;
            t.LineEnding = eol;
            t.SavedContent = text;
            ApplyEditorTextProgrammatically(t, text, 0, 0, true);
        }

        private void PrintActiveDocument()
        {
            SyncActiveTabFromEditor();
            _printText = _editor.Text ?? "";
            _printPos = 0;
            using (PrintDocument pd = new PrintDocument())
            {
                pd.DocumentName = ActiveTab != null ? ActiveTab.DisplayTitle : "NillyNote Pro";
                pd.PrintPage += delegate(object s, PrintPageEventArgs e)
                {
                    int charsFitted, linesFilled;
                    using (Font pf = new Font(_fontFamily, _baseFontSize))
                    {
                        string rem = _printText.Substring(_printPos);
                        e.Graphics.MeasureString(rem, pf, e.MarginBounds.Size, StringFormat.GenericTypographic, out charsFitted, out linesFilled);
                        e.Graphics.DrawString(rem.Substring(0, charsFitted), pf, Brushes.Black, e.MarginBounds, StringFormat.GenericTypographic);
                        _printPos += charsFitted;
                        e.HasMorePages = (_printPos < _printText.Length);
                    }
                };
                using (PrintDialog dlg = new PrintDialog())
                {
                    dlg.Document = pd;
                    if (dlg.ShowDialog(this) == DialogResult.OK) pd.Print();
                }
            }
        }

        // ====================================================================
        // FIND & REPLACE
        // ====================================================================
        private void OpenFindBar(bool withReplace)
        {
            _findPanel.Visible = true;
            SetReplaceRowVisible(withReplace);
            if (_editor.SelectionLength > 0 && _editor.SelectionLength < 160 && !_editor.SelectedText.Contains("\n"))
                _txtFind.Text = _editor.SelectedText;
            _txtFind.Focus();
            _txtFind.SelectAll();
            UpdateFindCount();
        }

        private void SetReplaceRowVisible(bool show)
        {
            _replaceVisible = show;
            _replaceRow.Visible = show;
            _btnToggleReplace.Icon = show ? AkIcon.Down : AkIcon.ChevRight;
            _btnToggleReplace.Invalidate();
            _findPanel.Height = show ? 70 : 38;
        }

        private void CloseFindBar()
        {
            _findPanel.Visible = false;
            _editor.Focus();
        }

        private List<Match> FindAllMatches()
        {
            List<Match> list = new List<Match>();
            string q = _txtFind.Text;
            if (string.IsNullOrEmpty(q)) return list;
            try
            {
                string pattern = _findRegex ? q : Regex.Escape(q);
                RegexOptions opt = _findCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                foreach (Match m in Regex.Matches(_editor.Text, pattern, opt))
                    if (m.Length > 0) list.Add(m);
            }
            catch { }
            return list;
        }

        private void UpdateFindCount()
        {
            List<Match> matches = FindAllMatches();
            if (matches.Count == 0) { _lblFindCount.Text = "0/0"; return; }
            int cur = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].Index == _editor.SelectionStart && matches[i].Length == _editor.SelectionLength)
                { cur = i + 1; break; }
            }
            _lblFindCount.Text = cur + "/" + matches.Count;
        }

        private void FindNext(bool backward)
        {
            List<Match> matches = FindAllMatches();
            if (matches.Count == 0) { _lblFindCount.Text = "0/0"; return; }
            Match pick = null;
            int idxNum = 1;
            if (!backward)
            {
                int after = _editor.SelectionStart + _editor.SelectionLength;
                for (int i = 0; i < matches.Count; i++)
                {
                    if (matches[i].Index >= after) { pick = matches[i]; idxNum = i + 1; break; }
                }
                if (pick == null) { pick = matches[0]; idxNum = 1; }
            }
            else
            {
                int before = _editor.SelectionStart;
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    if (matches[i].Index < before) { pick = matches[i]; idxNum = i + 1; break; }
                }
                if (pick == null) { pick = matches[matches.Count - 1]; idxNum = matches.Count; }
            }
            _editor.SelectionStart = pick.Index;
            _editor.SelectionLength = pick.Length;
            _editor.ScrollToCaret();
            _lblFindCount.Text = idxNum + "/" + matches.Count;
        }

        private void ReplaceOne()
        {
            string q = _txtFind.Text;
            if (string.IsNullOrEmpty(q)) return;
            List<Match> matches = FindAllMatches();
            if (matches.Count == 0) return;
            Match cur = null;
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].Index == _editor.SelectionStart && matches[i].Length == _editor.SelectionLength)
                { cur = matches[i]; break; }
            }
            if (cur == null) { FindNext(false); return; }
            NoteTab t = ActiveTab;
            if (t == null) return;
            string rep = _txtReplace.Text ?? "";
            string updated = _editor.Text.Substring(0, cur.Index) + rep + _editor.Text.Substring(cur.Index + cur.Length);
            ApplyEditorTextProgrammatically(t, updated, cur.Index + rep.Length, 0, true);
            FindNext(false);
        }

        private void ReplaceAll()
        {
            string q = _txtFind.Text;
            NoteTab t = ActiveTab;
            if (string.IsNullOrEmpty(q) || t == null) return;
            try
            {
                string pattern = _findRegex ? q : Regex.Escape(q);
                RegexOptions opt = _findCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                string rep = _txtReplace.Text ?? "";
                string updated = Regex.Replace(_editor.Text, pattern, _findRegex ? rep : rep.Replace("$", "$$"), opt);
                if (updated != _editor.Text)
                    ApplyEditorTextProgrammatically(t, updated, 0, 0, true);
                UpdateFindCount();
            }
            catch { }
        }

        // ====================================================================
        // PRO NOTEPAD TOOLS
        // ====================================================================
        private void InsertTimestamp()
        {
            NoteTab t = ActiveTab;
            if (t == null) return;
            PushUndo(t, _editor.Text, _editor.SelectionStart, _editor.SelectionLength);
            _undoCoalesce = false;
            _editor.SelectedText = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private void ToggleChecklist()
        {
            NoteTab t = ActiveTab;
            if (t == null) return;
            int ns, nl;
            string updated = TextUtils.ToggleChecklistLines(_editor.Text, _editor.SelectionStart, _editor.SelectionLength, out ns, out nl);
            ApplyEditorTextProgrammatically(t, updated, ns, nl, true);
        }

        private void TransformLines(string op)
        {
            NoteTab t = ActiveTab;
            if (t == null) return;
            bool hasSel = _editor.SelectionLength > 0;
            string src = hasSel ? _editor.SelectedText : _editor.Text;
            string[] lines = TextUtils.NormalizeToCrlf(src).Split(new string[] { "\r\n" }, StringSplitOptions.None);
            List<string> res = new List<string>(lines);

            if (op == "sort_asc") res.Sort(StringComparer.CurrentCultureIgnoreCase);
            else if (op == "sort_desc") { res.Sort(StringComparer.CurrentCultureIgnoreCase); res.Reverse(); }
            else if (op == "dedupe")
            {
                HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
                List<string> uniq = new List<string>();
                for (int i = 0; i < res.Count; i++) if (seen.Add(res[i])) uniq.Add(res[i]);
                res = uniq;
            }
            else if (op == "no_empty")
            {
                res.RemoveAll(delegate(string s) { return string.IsNullOrWhiteSpace(s); });
            }
            else if (op == "trim")
            {
                for (int i = 0; i < res.Count; i++) res[i] = res[i].Trim();
            }

            string outStr = string.Join("\r\n", res.ToArray());
            if (hasSel)
            {
                int ss = _editor.SelectionStart;
                string full = _editor.Text.Substring(0, ss) + outStr + _editor.Text.Substring(ss + _editor.SelectionLength);
                ApplyEditorTextProgrammatically(t, full, ss, outStr.Length, true);
            }
            else ApplyEditorTextProgrammatically(t, outStr, 0, 0, true);
        }

        private void TransformSelectionOrAll(string op)
        {
            NoteTab t = ActiveTab;
            if (t == null) return;
            bool hasSel = _editor.SelectionLength > 0;
            string src = hasSel ? _editor.SelectedText : _editor.Text;
            string outStr = src;
            if (op == "upper") outStr = src.ToUpper();
            else if (op == "lower") outStr = src.ToLower();
            else if (op == "greeklish") outStr = TextUtils.ToGreeklish(src);
            else if (op == "slug") outStr = TextUtils.ToSeoSlug(src);
            else if (op == "json_fmt") outStr = TextUtils.FormatJson(src, false);
            else if (op == "json_min") outStr = TextUtils.FormatJson(src, true);

            if (hasSel)
            {
                int ss = _editor.SelectionStart;
                string full = _editor.Text.Substring(0, ss) + outStr + _editor.Text.Substring(ss + _editor.SelectionLength);
                ApplyEditorTextProgrammatically(t, full, ss, outStr.Length, true);
            }
            else ApplyEditorTextProgrammatically(t, outStr, 0, 0, true);
        }

        private void OpenSelectedLinkOrSearch()
        {
            string s = _editor.SelectedText.Trim();
            if (string.IsNullOrEmpty(s))
            {
                int lineIdx = _editor.GetLineFromCharIndex(_editor.SelectionStart);
                if (lineIdx >= 0 && lineIdx < _editor.Lines.Length) s = _editor.Lines[lineIdx].Trim();
            }
            if (string.IsNullOrEmpty(s)) return;
            Match m = Regex.Match(s, @"https?://[^\s<>""]+", RegexOptions.IgnoreCase);
            string url = m.Success ? m.Value : ("https://www.google.com/search?q=" + Uri.EscapeDataString(s));
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
        }

        private void ShowGoToLineDialog()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "Μετάβαση σε γραμμή";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.ClientSize = new Size(270, 96);
                dlg.BackColor = _theme.MenuBg; dlg.ForeColor = _theme.EditorFg;
                Native.SetDarkTitlebar(dlg.Handle, _darkMode);

                Label lbl = new Label { Text = "Αριθμός γραμμής:", Left = 14, Top = 14, Width = 140, ForeColor = _theme.EditorFg };
                TextBox tb = new TextBox { Left = 14, Top = 36, Width = 150, BackColor = _theme.InputBg, ForeColor = _theme.EditorFg, BorderStyle = BorderStyle.FixedSingle };
                int curLine = _editor.GetLineFromCharIndex(_editor.SelectionStart) + 1;
                tb.Text = curLine.ToString();
                tb.SelectAll();

                Button ok = new Button { Text = "Μετάβαση", Left = 174, Top = 34, Width = 82, Height = 26, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = _theme.Accent, ForeColor = Color.FromArgb(12, 20, 32) };
                ok.FlatAppearance.BorderSize = 0;
                dlg.AcceptButton = ok;
                dlg.Controls.AddRange(new Control[] { lbl, tb, ok });

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    int target;
                    if (int.TryParse(tb.Text.Trim(), out target) && target >= 1)
                    {
                        int idx = _editor.GetFirstCharIndexFromLine(Math.Min(target - 1, Math.Max(0, _editor.Lines.Length - 1)));
                        if (idx >= 0) { _editor.SelectionStart = idx; _editor.SelectionLength = 0; _editor.ScrollToCaret(); UpdateStatusBar(); }
                    }
                }
            }
        }

        private void ShowQuickDiffDialog()
        {
            if (_tabs.Count < 2)
            {
                MessageBox.Show(this, "Άνοιξε τουλάχιστον 2 καρτέλες για να τις συγκρίνεις.", "Quick Diff — NillyNote Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SyncActiveTabFromEditor();
            int otherIdx = (_activeIdx == 0) ? 1 : 0;
            NoteTab a = _tabs[_activeIdx];
            NoteTab b = _tabs[otherIdx];
            string[] la = TextUtils.NormalizeToCrlf(a.Content).Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string[] lb = TextUtils.NormalizeToCrlf(b.Content).Split(new string[] { "\r\n" }, StringSplitOptions.None);
            int max = Math.Max(la.Length, lb.Length);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Σύγκριση: [" + a.DisplayTitle + "]  vs  [" + b.DisplayTitle + "]");
            sb.AppendLine(new string('─', 64));
            int diffs = 0;
            for (int i = 0; i < max; i++)
            {
                string sa = i < la.Length ? la[i] : null;
                string sbStr = i < lb.Length ? lb[i] : null;
                if (sa != sbStr)
                {
                    diffs++;
                    sb.AppendLine(string.Format("Γραμμή {0}:", i + 1));
                    if (sa != null) sb.AppendLine("  - " + sa);
                    if (sbStr != null) sb.AppendLine("  + " + sbStr);
                }
            }
            if (diffs == 0) sb.AppendLine("✓ Τα δύο έγγραφα είναι 100% πανομοιότυπα!");

            using (Form f = new Form())
            {
                f.Text = "Quick Diff (" + diffs + " διαφορές) — NillyNote Pro";
                f.Size = new Size(720, 480);
                f.StartPosition = FormStartPosition.CenterParent;
                f.BackColor = _theme.EditorBg;
                Native.SetDarkTitlebar(f.Handle, _darkMode);
                TextBox box = new TextBox
                {
                    Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both, WordWrap = false,
                    Font = new Font("Consolas", 10f), BackColor = _theme.EditorBg, ForeColor = _theme.EditorFg, BorderStyle = BorderStyle.None,
                    Text = sb.ToString()
                };
                f.Controls.Add(box);
                f.ShowDialog(this);
            }
        }

        private void ShowAboutDialog()
        {
            MessageBox.Show(this,
                "NillyNote Pro v1.1.4 — Ultra-Light Windows 11 Pro Notepad\r\n" +
                "created by Nilly graphics (Sotiris Christos) — https://nillygraphics.com/\r\n\r\n" +
                "Κύριες συντομεύσεις:\r\n" +
                "• Ctrl+N / Ctrl+W : Νέα / Κλείσιμο καρτέλας (ή Μεσαίο κλικ)\r\n" +
                "• Ctrl+F / Ctrl+H : Εύρεση / Αντικατάσταση (με Regex)\r\n" +
                "• Ctrl+G : Μετάβαση σε γραμμή\r\n" +
                "• F5 : Εισαγωγή Ημερομηνίας & Ώρας\r\n" +
                "• Ctrl+L : Εναλλαγή Checklist ☐ / ☑\r\n" +
                "• Ctrl+T : Πάντα στην κορυφή (Pin)\r\n" +
                "• Alt+Z : Αναδίπλωση λέξεων (Word Wrap)\r\n" +
                "• Ctrl + Ροδέλα : Zoom\r\n" +
                "• Αυτόματη επαναφορά καρτελών & πρόχειρων σημειώσεων!",
                "Σχετικά — NillyNote Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ====================================================================
        // VIEW & STATUS BAR ACTIONS
        // ====================================================================
        private void ToggleWordWrap()
        {
            _wordWrap = !_wordWrap;
            _editor.WordWrap = _wordWrap;
            _editor.ScrollBars = _wordWrap ? ScrollBars.Vertical : ScrollBars.Both;
            _miWordWrap.Checked = _wordWrap;
            _sessionDirty = true;
        }

        private void TogglePin()
        {
            _pinned = !_pinned;
            TopMost = _pinned;
            _miPin.Checked = _pinned;
            _btnQuickPin.Active = _pinned;
            _btnQuickPin.Invalidate();
        }

        private void SetOpacityPercent(int pct)
        {
            _opacityPercent = Math.Max(50, Math.Min(100, pct));
            Opacity = _opacityPercent / 100.0;
        }

        private void SetZoom(int pct)
        {
            _zoomPercent = Math.Max(60, Math.Min(300, pct));
            ApplyEditorFont();
            _sessionDirty = true;
        }

        private void ChooseFontDialog()
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.Font = new Font(_fontFamily, _baseFontSize);
                fd.ShowEffects = false;
                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    _fontFamily = fd.Font.FontFamily.Name;
                    _baseFontSize = fd.Font.Size;
                    ApplyEditorFont();
                    _sessionDirty = true;
                }
            }
        }

        private void ToggleLineEnding()
        {
            NoteTab t = ActiveTab;
            if (t == null) return;
            t.LineEnding = (t.LineEnding == "CRLF") ? "LF" : "CRLF";
            t.IsDirty = true;
            _tabBar.Invalidate();
            UpdateStatusBar();
            _sessionDirty = true;
        }

        private void CycleEncoding()
        {
            NoteTab t = ActiveTab;
            if (t == null) return;
            string[] encs = new string[] { "UTF-8", "UTF-8 BOM", "UTF-16", "Win-1253" };
            int idx = Array.IndexOf(encs, t.EncodingName);
            t.EncodingName = encs[(idx + 1) % encs.Length];
            t.IsDirty = true;
            _tabBar.Invalidate();
            UpdateStatusBar();
            _sessionDirty = true;
        }

        private void UpdateWindowTitle()
        {
            NoteTab t = ActiveTab;
            if (t == null) { Text = "NillyNote Pro"; return; }
            Text = (t.IsDirty ? "● " : "") + t.DisplayTitle + " — NillyNote Pro";
        }

        private void UpdateStatusBar()
        {
            NoteTab t = ActiveTab;
            if (t == null) return;
            int pos = _editor.SelectionStart;
            int lineIdx = _editor.GetLineFromCharIndex(pos);
            int lineStart = _editor.GetFirstCharIndexFromLine(lineIdx);
            int col = Math.Max(1, pos - lineStart + 1);
            _lblPos.Text = "Γρ " + (lineIdx + 1) + ", Στ " + col;

            int w, c, l;
            if (_editor.SelectionLength > 0)
            {
                TextUtils.CountStats(_editor.SelectedText, out w, out c, out l);
                _lblStats.Text = "Επιλογή: " + w + " λέξεις · " + c + " χαρ.";
            }
            else
            {
                TextUtils.CountStats(_editor.Text, out w, out c, out l);
                _lblStats.Text = w + " λέξεις · " + c + " χαρ. · " + l + " γρ.";
            }
            _lblEol.Text = t.LineEnding;
            _lblEnc.Text = t.EncodingName;
            _lblZoom.Text = _zoomPercent + "%";
        }

        // ====================================================================
        // RECENT FILES, SETTINGS & ZERO-DATA-LOSS SESSION RESTORE
        // ====================================================================
        private void AddRecentFile(string fullPath)
        {
            _recentFiles.RemoveAll(delegate(string s) { return s.Equals(fullPath, StringComparison.OrdinalIgnoreCase); });
            _recentFiles.Insert(0, fullPath);
            if (_recentFiles.Count > 10) _recentFiles.RemoveAt(_recentFiles.Count - 1);
            RebuildRecentMenu();
        }

        private void RebuildRecentMenu()
        {
            if (_miRecent == null) return;
            _miRecent.DropDownItems.Clear();
            if (_recentFiles.Count == 0)
            {
                ToolStripMenuItem none = new ToolStripMenuItem("(Κανένα)");
                none.Enabled = false;
                _miRecent.DropDownItems.Add(none);
                return;
            }
            for (int i = 0; i < _recentFiles.Count; i++)
            {
                string fp = _recentFiles[i];
                _miRecent.DropDownItems.Add(MkItem(Path.GetFileName(fp) + "   (" + fp + ")", Keys.None, delegate { OpenOrCreateFilePath(fp); }));
            }
        }

        private static string FindSiblingExe(string folder, string exeName)
        {
            try
            {
                string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"NillySuite\" + folder + @"\" + exeName);
                if (File.Exists(appData)) return appData;
                DirectoryInfo par = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
                if (par != null)
                {
                    string sib = Path.Combine(Path.Combine(par.FullName, folder), exeName);
                    if (File.Exists(sib)) return sib;
                }
            }
            catch { }
            return null;
        }

        private void SendActiveToCode(string codeExe)
        {
            try
            {
                string path = null;
                if (_activeIdx >= 0 && _activeIdx < _tabs.Count)
                {
                    NoteTab tab = _tabs[_activeIdx];
                    if (!string.IsNullOrEmpty(tab.FilePath) && File.Exists(tab.FilePath)) path = tab.FilePath;
                }
                if (path == null)
                {
                    path = Path.Combine(Path.GetTempPath(), "NillyNote_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
                    File.WriteAllText(path, _editor.Text ?? "", new UTF8Encoding(false));
                }
                Process.Start(new ProcessStartInfo(codeExe, "\"" + path + "\"") { WorkingDirectory = Path.GetDirectoryName(codeExe), UseShellExecute = true });
            }
            catch { }
        }

        private void OpenActiveFolderInFiles(string filesExe)
        {
            try
            {
                if (_activeIdx >= 0 && _activeIdx < _tabs.Count && !string.IsNullOrEmpty(_tabs[_activeIdx].FilePath))
                {
                    string dir = Path.GetDirectoryName(_tabs[_activeIdx].FilePath);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                        Process.Start(new ProcessStartInfo(filesExe, "\"" + dir + "\"") { WorkingDirectory = Path.GetDirectoryName(filesExe), UseShellExecute = true });
                }
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                string suiteTheme = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"NillySuite\suite.theme");
                bool hasLocal = File.Exists(_settingsPath);
                if (hasLocal)
                {
                    string[] lines = File.ReadAllLines(_settingsPath, Encoding.UTF8);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        int eq = lines[i].IndexOf('=');
                        if (eq <= 0) continue;
                        string k = lines[i].Substring(0, eq).Trim();
                        string v = lines[i].Substring(eq + 1).Trim();
                        if (k == "DarkMode") _darkMode = (v == "1");
                        else if (k == "WordWrap") _wordWrap = (v == "1");
                        else if (k == "FontFamily" && v.Length > 0) _fontFamily = v;
                        else if (k == "FontSize") { float fs; if (float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fs)) _baseFontSize = Math.Max(8f, Math.Min(32f, fs)); }
                        else if (k == "Zoom") { int z; if (int.TryParse(v, out z)) _zoomPercent = Math.Max(60, Math.Min(300, z)); }
                        else if (k == "Width") { int w; if (int.TryParse(v, out w) && w >= 460) Width = w; }
                        else if (k == "Height") { int h; if (int.TryParse(v, out h) && h >= 300) Height = h; }
                        else if (k == "Recent" && File.Exists(v)) _recentFiles.Add(v);
                    }
                }
                if (File.Exists(suiteTheme) && (!hasLocal || File.GetLastWriteTimeUtc(suiteTheme) > File.GetLastWriteTimeUtc(_settingsPath)))
                {
                    string s = File.ReadAllText(suiteTheme, Encoding.ASCII).Trim();
                    if (s.Equals("light", StringComparison.OrdinalIgnoreCase)) _darkMode = false;
                    else if (s.Equals("dark", StringComparison.OrdinalIgnoreCase)) _darkMode = true;
                }
            }
            catch { }
        }

        private void SaveSession()
        {
            if (_suppressExitSave) return;
            try
            {
                Directory.CreateDirectory(_appDataDir);
                Directory.CreateDirectory(_draftsDir);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DarkMode=" + (_darkMode ? "1" : "0"));
                sb.AppendLine("WordWrap=" + (_wordWrap ? "1" : "0"));
                sb.AppendLine("FontFamily=" + _fontFamily);
                sb.AppendLine("FontSize=" + _baseFontSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine("Zoom=" + _zoomPercent);
                if (WindowState == FormWindowState.Normal)
                {
                    sb.AppendLine("Width=" + Width);
                    sb.AppendLine("Height=" + Height);
                }
                sb.AppendLine("ActiveTab=" + _activeIdx);
                for (int i = 0; i < _recentFiles.Count; i++)
                    sb.AppendLine("Recent=" + _recentFiles[i]);

                HashSet<string> keepDrafts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < _tabs.Count; i++)
                {
                    NoteTab t = _tabs[i];
                    string draftFile = "";
                    if (string.IsNullOrEmpty(t.FilePath) || t.IsDirty)
                    {
                        draftFile = Path.Combine(_draftsDir, "tab_" + t.Id + ".txt");
                        File.WriteAllText(draftFile, t.Content ?? "", new UTF8Encoding(false));
                        keepDrafts.Add(draftFile);
                    }
                    // Tab line: Tab=Id|Encoding|LineEnding|SelStart|IsDirty|FilePath|DraftFile
                    sb.AppendLine(string.Format("Tab={0}|{1}|{2}|{3}|{4}|{5}|{6}",
                        t.Id, t.EncodingName, t.LineEnding, t.SelStart, t.IsDirty ? "1" : "0",
                        t.FilePath ?? "", draftFile));
                }
                File.WriteAllText(_settingsPath, sb.ToString(), new UTF8Encoding(false));

                // Clean stale draft files
                foreach (string f in Directory.GetFiles(_draftsDir, "tab_*.txt"))
                    if (!keepDrafts.Contains(f)) { try { File.Delete(f); } catch { } }
            }
            catch { }
        }

        private void RestoreSession()
        {
            try
            {
                if (!File.Exists(_settingsPath)) return;
                string[] lines = File.ReadAllLines(_settingsPath, Encoding.UTF8);
                int active = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("ActiveTab="))
                        int.TryParse(lines[i].Substring(10).Trim(), out active);
                    else if (lines[i].StartsWith("Tab="))
                    {
                        string[] p = lines[i].Substring(4).Split('|');
                        if (p.Length < 7) continue;
                        string id = p[0], enc = p[1], eol = p[2];
                        int sel = 0; int.TryParse(p[3], out sel);
                        bool dirty = (p[4] == "1");
                        string fp = p[5];
                        string draft = p[6];

                        string content = "";
                        string savedContent = "";
                        if (!string.IsNullOrEmpty(fp) && File.Exists(fp))
                        {
                            string dEnc, dEol;
                            savedContent = TextUtils.ReadFileAutoDetect(fp, out dEnc, out dEol);
                            enc = dEnc; eol = dEol;
                            content = (dirty && !string.IsNullOrEmpty(draft) && File.Exists(draft))
                                ? TextUtils.NormalizeToCrlf(File.ReadAllText(draft, Encoding.UTF8))
                                : savedContent;
                        }
                        else if (!string.IsNullOrEmpty(draft) && File.Exists(draft))
                        {
                            content = TextUtils.NormalizeToCrlf(File.ReadAllText(draft, Encoding.UTF8));
                            savedContent = "";
                            fp = null;
                        }
                        else continue;

                        NoteTab t = new NoteTab();
                        t.Id = string.IsNullOrEmpty(id) ? t.Id : id;
                        t.FilePath = string.IsNullOrEmpty(fp) ? null : fp;
                        t.Content = content;
                        t.SavedContent = savedContent;
                        t.EncodingName = string.IsNullOrEmpty(enc) ? "UTF-8" : enc;
                        t.LineEnding = (eol == "LF") ? "LF" : "CRLF";
                        t.SelStart = sel;
                        t.IsDirty = (t.Content != t.SavedContent);
                        _tabs.Add(t);
                    }
                }
                if (_tabs.Count > 0) SwitchToTab(Math.Max(0, Math.Min(_tabs.Count - 1, active)));
            }
            catch { }
        }

        // ====================================================================
        // DRAG & DROP, KEYBOARD SHORTCUTS, SINGLE-INSTANCE IPC
        // ====================================================================
        protected override void OnDragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            base.OnDragEnter(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            try
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                        if (File.Exists(files[i])) OpenOrCreateFilePath(files[i]);
                }
            }
            catch { }
            base.OnDragDrop(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Tab) && _tabs.Count > 1)
            {
                SwitchToTab((_activeIdx + 1) % _tabs.Count);
                return true;
            }
            if (keyData == (Keys.Control | Keys.Shift | Keys.Tab) && _tabs.Count > 1)
            {
                SwitchToTab((_activeIdx - 1 + _tabs.Count) % _tabs.Count);
                return true;
            }
            if (keyData == Keys.Escape && _findPanel.Visible)
            {
                CloseFindBar();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_COPYDATA)
            {
                try
                {
                    Native.COPYDATASTRUCT cds = (Native.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(Native.COPYDATASTRUCT));
                    if (cds.cbData > 0 && cds.lpData != IntPtr.Zero)
                    {
                        string payload = Marshal.PtrToStringUni(cds.lpData, cds.cbData / 2).TrimEnd('\0');
                        if (!string.IsNullOrEmpty(payload))
                        {
                            string[] paths = payload.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < paths.Length; i++) OpenOrCreateFilePath(paths[i].Trim());
                        }
                    }
                    if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
                    Activate();
                }
                catch { }
                m.Result = (IntPtr)1;
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_autoSaveTimer != null) _autoSaveTimer.Stop();
            SyncActiveTabFromEditor();
            // Save the current session when closing the window.
            SaveSession();
            base.OnFormClosing(e);
        }
    }

    internal static class Program
    {
        private const string MUTEX_NAME = "Local\\NillyNotePro_SingleInstance_v1";

        [STAThread]
        public static int Main(string[] args)
        {
            try { Native.SetProcessDPIAware(); } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args != null && args.Length > 0 && args[0].Equals("--test-smoke", StringComparison.OrdinalIgnoreCase))
                return RunSmokeTest(args.Length > 1 ? args[1] : null);

            bool createdNew;
            using (Mutex m = new Mutex(true, MUTEX_NAME, out createdNew))
            {
                if (!createdNew)
                {
                    ForwardToRunningInstance(args);
                    return 0;
                }
                MainForm mf = new MainForm(args, false);
                mf.Shown += delegate { Native.TrimMemory(); };
                Application.Run(SuiteUpdater.Attach(mf, "nillynote-pro", "NillyNote-Pro", "Install_NillyNotePro.ps1"));
            }
            return 0;
        }

        private static void ForwardToRunningInstance(string[] args)
        {
            try
            {
                // Find any open window ending with "NillyNote Pro"
                IntPtr target = IntPtr.Zero;
                Process[] procs = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                int myId = Process.GetCurrentProcess().Id;
                for (int i = 0; i < procs.Length; i++)
                {
                    if (procs[i].Id != myId && procs[i].MainWindowHandle != IntPtr.Zero)
                    {
                        target = procs[i].MainWindowHandle;
                        break;
                    }
                }
                if (target != IntPtr.Zero)
                {
                    Native.ShowWindow(target, Native.SW_RESTORE);
                    Native.SetForegroundWindow(target);
                    if (args != null && args.Length > 0)
                    {
                        string payload = string.Join("\n", args) + "\0";
                        IntPtr ptr = Marshal.StringToHGlobalUni(payload);
                        try
                        {
                            Native.COPYDATASTRUCT cds = new Native.COPYDATASTRUCT();
                            cds.dwData = (IntPtr)1;
                            cds.cbData = payload.Length * 2;
                            cds.lpData = ptr;
                            Native.SendMessage(target, Native.WM_COPYDATA, IntPtr.Zero, ref cds);
                        }
                        finally { Marshal.FreeHGlobal(ptr); }
                    }
                }
            }
            catch { }
        }

        private static int RunSmokeTest(string logPath)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                // 1. Greek encoding round-trip (UTF-8 & Windows-1253)
                string tmpUtf8 = Path.Combine(Path.GetTempPath(), "nillynote_test_utf8.txt");
                string tmp1253 = Path.Combine(Path.GetTempPath(), "nillynote_test_1253.txt");
                string sampleGr = "Καλημέρα Ελλάδα — Chromodomi Design 100%\r\nΔεύτερη γραμμή";
                TextUtils.WriteFileWithEncoding(tmpUtf8, sampleGr, "UTF-8", "CRLF");
                TextUtils.WriteFileWithEncoding(tmp1253, sampleGr, "Win-1253", "CRLF");
                string e1, l1, e2, l2;
                string r1 = TextUtils.ReadFileAutoDetect(tmpUtf8, out e1, out l1);
                string r2 = TextUtils.ReadFileAutoDetect(tmp1253, out e2, out l2);
                try { File.Delete(tmpUtf8); File.Delete(tmp1253); } catch { }
                sb.AppendLine("GREEK_UTF8_OK=" + (r1 == sampleGr && e1 == "UTF-8"));
                sb.AppendLine("GREEK_1253_OK=" + (r2 == sampleGr && e2 == "Win-1253"));

                // 2. Pro text utilities checks
                string gl = TextUtils.ToGreeklish("Αυτοκόλλητο Κατάλογος");
                string slug = TextUtils.ToSeoSlug("Ετικέτα Red Sun 750ml!");
                string jf = TextUtils.FormatJson("{\"a\":1,\"b\":[2,3]}", false);
                int w, c, ln;
                TextUtils.CountStats("Ένα δύο τρία\r\nΤέσσερα", out w, out c, out ln);
                int ns, nl;
                string chk = TextUtils.ToggleChecklistLines("Αγορά χαρτιού", 0, 0, out ns, out nl);
                sb.AppendLine("GREEKLISH_OK=" + (gl.StartsWith("Avtokollito") && slug == "etiketa-red-sun-750ml"));
                sb.AppendLine("JSON_FMT_OK=" + (jf.Contains("\r\n") && jf.Contains("\"a\": 1")));
                sb.AppendLine("STATS_OK=" + (w == 4 && ln == 2));
                sb.AppendLine("CHECKLIST_OK=" + (chk == "☐ Αγορά χαρτιού"));

                // 3. UI Form + Multi-Tab + Theme Switch + RAM check
                using (MainForm form = new MainForm(null, true))
                {
                    form.Show();
                    form.CreateNewTab(null, "Δεύτερη καρτέλα δοκιμής", true);
                    form.SwitchToTab(0);
                    Application.DoEvents();
                    long ramMb = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);
                    sb.AppendLine("WINDOW_OPENED_OK=" + form.IsHandleCreated);
                    sb.AppendLine("WORKING_SET_MB=" + ramMb);
                    form.Close();
                }
                sb.AppendLine("SMOKE_RESULT=PASS");
            }
            catch (Exception ex)
            {
                sb.AppendLine("SMOKE_RESULT=FAIL");
                sb.AppendLine(ex.ToString());
                if (!string.IsNullOrEmpty(logPath)) File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return 1;
            }
            if (!string.IsNullOrEmpty(logPath)) File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
            return 0;
        }
    }

    // ========================================================================
    // BUILT-IN ULTRA-LIGHT AUTO-UPDATER (1x/24h shared CDN check · SHA-256)
    // ========================================================================
    internal static class SuiteUpdater
    {
        private const string CATALOG_URL = "https://raw.githubusercontent.com/nillynp/nillyhub/main/catalog.json";

        public static T Attach<T>(T form, string repoName, string pkgPrefix, string installerScript) where T : System.Windows.Forms.Form
        {
            if (form == null) return null;
            form.Shown += delegate
            {
                System.Threading.Thread th = new System.Threading.Thread(delegate()
                {
                    try
                    {
                        System.Threading.Thread.Sleep(2200);
                        if (form.IsDisposed) return;
                        string curTag, newTag;
                        if (!CheckForUpdate(repoName, out curTag, out newTag)) return;
                        if (form.IsDisposed || !form.IsHandleCreated) return;
                        form.BeginInvoke((System.Action)delegate
                        {
                            if (form.IsDisposed || form.FormBorderStyle == System.Windows.Forms.FormBorderStyle.None) return;
                            ShowUpdateBanner(form, repoName, pkgPrefix, installerScript, curTag, newTag);
                        });
                    }
                    catch { }
                });
                th.IsBackground = true;
                th.Start();
            };
            return form;
        }

        private static void ShowUpdateBanner(System.Windows.Forms.Form form, string repoName, string pkgPrefix, string installerScript, string curTag, string newTag)
        {
            System.Drawing.Font fTxt = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Regular);
            System.Drawing.Font fBtn = new System.Drawing.Font("Segoe UI", 8.2f, System.Drawing.FontStyle.Bold);

            System.Windows.Forms.Panel bar = new System.Windows.Forms.Panel();
            bar.Dock = System.Windows.Forms.DockStyle.Top;
            bar.Height = 28;
            bar.BackColor = System.Drawing.Color.FromArgb(40, 29, 18);
            bar.Padding = new System.Windows.Forms.Padding(10, 0, 8, 1);
            bar.Paint += delegate(object s, System.Windows.Forms.PaintEventArgs e)
            {
                using (System.Drawing.Pen p = new System.Drawing.Pen(System.Drawing.Color.FromArgb(252, 150, 31)))
                    e.Graphics.DrawLine(p, 0, bar.Height - 1, bar.Width, bar.Height - 1);
            };

            System.Windows.Forms.Button btnClose = new System.Windows.Forms.Button();
            btnClose.Text = "×";
            btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            btnClose.Width = 26;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = System.Drawing.Color.Transparent;
            btnClose.ForeColor = System.Drawing.Color.FromArgb(200, 170, 130);
            btnClose.Font = fBtn;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.TabStop = false;
            btnClose.Click += delegate { bar.Dispose(); };

            System.Windows.Forms.Button btnUpd = new System.Windows.Forms.Button();
            btnUpd.Text = "Ενημέρωση";
            btnUpd.Dock = System.Windows.Forms.DockStyle.Right;
            btnUpd.Width = 88;
            btnUpd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnUpd.FlatAppearance.BorderSize = 0;
            btnUpd.BackColor = System.Drawing.Color.FromArgb(252, 150, 31);
            btnUpd.ForeColor = System.Drawing.Color.FromArgb(20, 22, 26);
            btnUpd.Font = fBtn;
            btnUpd.Cursor = System.Windows.Forms.Cursors.Hand;
            btnUpd.TabStop = false;

            System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
            lbl.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lbl.Font = fTxt;
            lbl.ForeColor = System.Drawing.Color.FromArgb(255, 185, 95);
            lbl.UseMnemonic = false;
            lbl.Text = "Νέα έκδοση " + newTag + " διαθέσιμη (έχεις " + curTag + ")";

            btnUpd.Click += delegate
            {
                btnUpd.Enabled = false;
                btnUpd.Text = "Λήψη...";
                System.Threading.Thread uth = new System.Threading.Thread(delegate()
                {
                    try
                    {
                        ApplySelfUpdate(repoName, pkgPrefix, installerScript,
                            delegate(string st)
                            {
                                try { if (!form.IsDisposed) form.BeginInvoke((System.Action)delegate { lbl.Text = st; }); } catch { }
                            },
                            delegate
                            {
                                try { if (!form.IsDisposed) form.BeginInvoke((System.Action)delegate { System.Windows.Forms.Application.Exit(); }); } catch { }
                            });
                    }
                    catch (System.Exception ex)
                    {
                        try
                        {
                            if (!form.IsDisposed) form.BeginInvoke((System.Action)delegate
                            {
                                lbl.Text = "Σφάλμα ενημέρωσης: " + ex.Message;
                                btnUpd.Text = "Ενημέρωση";
                                btnUpd.Enabled = true;
                            });
                        }
                        catch { }
                    }
                });
                uth.IsBackground = true;
                uth.Start();
            };

            bar.Disposed += delegate { fTxt.Dispose(); fBtn.Dispose(); };
            bar.Controls.Add(lbl);
            bar.Controls.Add(btnUpd);
            bar.Controls.Add(btnClose);
            form.Controls.Add(bar);
        }

        private static bool TryParseVer(string raw, out System.Version ver)
        {
            ver = null;
            if (string.IsNullOrEmpty(raw)) return false;
            string s = raw.Trim();
            if (s.StartsWith("v", System.StringComparison.OrdinalIgnoreCase)) s = s.Substring(1);
            string[] p = s.Split('.');
            if (p.Length < 2 || p.Length > 4) return false;
            int maj, min, pat = 0;
            if (!int.TryParse(p[0], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out maj)) return false;
            if (!int.TryParse(p[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out min)) return false;
            if (p.Length >= 3 && !int.TryParse(p[2], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out pat)) return false;
            ver = new System.Version(maj, min, pat, 0);
            return true;
        }

        private static string FmtVer(System.Version v)
        {
            return v.Build > 0
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "v{0}.{1}.{2}", v.Major, v.Minor, v.Build)
                : string.Format(System.Globalization.CultureInfo.InvariantCulture, "v{0}.{1}", v.Major, v.Minor);
        }

        public static bool CheckForUpdate(string repoName, out string curTag, out string newTag)
        {
            curTag = null; newTag = null;
            System.Version rawCur = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            System.Version curVer = new System.Version(rawCur.Major, rawCur.Minor, rawCur.Build >= 0 ? rawCur.Build : 0, 0);
            curTag = FmtVer(curVer);

            string suiteDir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "NillySuite");
            string cachePath = System.IO.Path.Combine(suiteDir, ".nillyhub_catalog.json");
            string json = null;
            try
            {
                if (System.IO.File.Exists(cachePath))
                {
                    System.DateTime wt = System.IO.File.GetLastWriteTimeUtc(cachePath);
                    if ((System.DateTime.UtcNow - wt).TotalHours < 24.0)
                    {
                        string cached = System.IO.File.ReadAllText(cachePath, System.Text.Encoding.UTF8);
                        if (cached.IndexOf("\"latestVersion\"", System.StringComparison.Ordinal) >= 0) json = cached;
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(json))
            {
                json = HttpGetText(CATALOG_URL, 5000);
                if (string.IsNullOrEmpty(json) || json.Length > 256 * 1024) return false;
                try
                {
                    System.IO.Directory.CreateDirectory(suiteDir);
                    System.IO.File.WriteAllText(cachePath, json, new System.Text.UTF8Encoding(false));
                }
                catch { }
            }

            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(
                json,
                "\"repo\"\\s*:\\s*\"" + System.Text.RegularExpressions.Regex.Escape(repoName) + "\"[^\\}]{0,500}?\"latestVersion\"\\s*:\\s*\"(v[0-9]+\\.[0-9]+(?:\\.[0-9]+)?)\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!m.Success) return false;
            System.Version remVer;
            if (!TryParseVer(m.Groups[1].Value, out remVer) || remVer <= curVer) return false;
            newTag = FmtVer(remVer);
            return true;
        }

        private static string HttpGetText(string url, int timeoutMs)
        {
            try { System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)3072 | System.Net.SecurityProtocolType.Tls12; } catch { }
            System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            req.UserAgent = "NillySuite-Updater/1.0 (https://nillygraphics.com/)";
            req.Accept = "application/vnd.github+json, application/json";
            req.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            req.Timeout = timeoutMs;
            req.ReadWriteTimeout = timeoutMs;
            using (System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)req.GetResponse())
            using (System.IO.Stream st = resp.GetResponseStream())
            using (System.IO.StreamReader sr = new System.IO.StreamReader(st, System.Text.Encoding.UTF8))
                return sr.ReadToEnd();
        }

        private static string ExtractAssetBlock(string json, string assetName)
        {
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(
                json, "\"name\"\\s*:\\s*\"" + System.Text.RegularExpressions.Regex.Escape(assetName) + "\"");
            if (!m.Success) return null;
            int start = m.Index;
            int depth = 0;
            while (start > 0)
            {
                char c = json[start];
                if (c == '}') depth++;
                else if (c == '{') { if (depth == 0) break; depth--; }
                start--;
            }
            int end = start;
            depth = 0;
            bool inStr = false;
            for (int i = start; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '"' && (i == 0 || json[i - 1] != '\\')) inStr = !inStr;
                if (inStr) continue;
                if (c == '{') depth++;
                else if (c == '}') { depth--; if (depth == 0) { end = i; break; } }
            }
            return end > start ? json.Substring(start, end - start + 1) : null;
        }

        public static void ApplySelfUpdate(string repoName, string pkgPrefix, string installerScript, System.Action<string> onStatus, System.Action onExit)
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appDir = System.IO.Path.GetDirectoryName(exePath);
            if (System.IO.Directory.Exists(System.IO.Path.Combine(appDir, ".git")))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com/nillynp/" + repoName + "/releases/latest") { UseShellExecute = true });
                if (onStatus != null) onStatus("Άνοιξε το GitHub Release (φάκελος Git).");
                return;
            }

            if (onStatus != null) onStatus("Έλεγχος SHA-256 στο GitHub...");
            string relJson = HttpGetText("https://api.github.com/repos/nillynp/" + repoName + "/releases/latest", 12000);
            if (string.IsNullOrEmpty(relJson)) throw new System.IO.InvalidDataException("Κενή απόκριση GitHub.");
            if (System.Text.RegularExpressions.Regex.IsMatch(relJson, "\"draft\"\\s*:\\s*true") ||
                System.Text.RegularExpressions.Regex.IsMatch(relJson, "\"prerelease\"\\s*:\\s*true"))
                throw new System.IO.InvalidDataException("Μη σταθερή έκδοση.");

            System.Text.RegularExpressions.Match tm = System.Text.RegularExpressions.Regex.Match(relJson, "\"tag_name\"\\s*:\\s*\"(v[0-9]+\\.[0-9]+(?:\\.[0-9]+)?)\"");
            if (!tm.Success) throw new System.IO.InvalidDataException("Μη έγκυρο tag έκδοσης.");
            string tag = tm.Groups[1].Value;

            System.Version curRaw = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            System.Version curVer = new System.Version(curRaw.Major, curRaw.Minor, curRaw.Build >= 0 ? curRaw.Build : 0, 0);
            System.Version remVer;
            if (!TryParseVer(tag, out remVer) || remVer <= curVer)
                throw new System.InvalidOperationException("Έχεις ήδη την τελευταία έκδοση.");

            string zipName = pkgPrefix + "-" + tag + "-Portable.zip";
            string expectedUrl = "https://github.com/nillynp/" + repoName + "/releases/download/" + tag + "/" + zipName;
            string blk = ExtractAssetBlock(relJson, zipName);
            if (string.IsNullOrEmpty(blk)) throw new System.IO.FileNotFoundException("Δεν βρέθηκε το Portable ZIP.");
            if (!System.Text.RegularExpressions.Regex.IsMatch(blk, "\"state\"\\s*:\\s*\"uploaded\""))
                throw new System.IO.InvalidDataException("Το αρχείο δεν είναι έτοιμο.");
            System.Text.RegularExpressions.Match um = System.Text.RegularExpressions.Regex.Match(blk, "\"browser_download_url\"\\s*:\\s*\"([^\"]+)\"");
            if (!um.Success || !string.Equals(um.Groups[1].Value, expectedUrl, System.StringComparison.Ordinal))
                throw new System.IO.InvalidDataException("Μη έγκυρο URL λήψης.");
            System.Text.RegularExpressions.Match dm = System.Text.RegularExpressions.Regex.Match(blk, "\"digest\"\\s*:\\s*\"sha256:([0-9a-f]{64})\"");
            if (!dm.Success) throw new System.IO.InvalidDataException("Απουσιάζει SHA-256.");
            string expectedSha = dm.Groups[1].Value;

            string tmpDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NillyUpd-" + System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(tmpDir);
            string zipFile = System.IO.Path.Combine(tmpDir, "pkg.zip");

            if (onStatus != null) onStatus("Λήψη " + tag + "...");
            try { System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)3072 | System.Net.SecurityProtocolType.Tls12; } catch { }
            System.Net.HttpWebRequest dreq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(expectedUrl);
            dreq.UserAgent = "NillySuite-Updater/1.0 (https://nillygraphics.com/)";
            dreq.Timeout = 20000;
            dreq.ReadWriteTimeout = 30000;
            using (System.Net.HttpWebResponse dresp = (System.Net.HttpWebResponse)dreq.GetResponse())
            using (System.IO.Stream src = dresp.GetResponseStream())
            using (System.IO.FileStream dst = new System.IO.FileStream(zipFile, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
            {
                byte[] buf = new byte[16384];
                int n; long total = 0;
                while ((n = src.Read(buf, 0, buf.Length)) > 0)
                {
                    total += n;
                    if (total > 32L * 1024L * 1024L) throw new System.IO.InvalidDataException("Υπέρβαση μεγέθους ZIP.");
                    dst.Write(buf, 0, n);
                }
            }

            if (onStatus != null) onStatus("Επαλήθευση SHA-256...");
            string actualSha;
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            using (System.IO.FileStream fs = System.IO.File.OpenRead(zipFile))
            {
                byte[] hash = sha.ComputeHash(fs);
                System.Text.StringBuilder sb = new System.Text.StringBuilder(64);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
                actualSha = sb.ToString();
            }
            if (!string.Equals(actualSha, expectedSha, System.StringComparison.OrdinalIgnoreCase))
            {
                try { System.IO.Directory.Delete(tmpDir, true); } catch { }
                throw new System.IO.InvalidDataException("Αποτυχία ελέγχου SHA-256.");
            }

            if (onStatus != null) onStatus("Εγκατάσταση " + tag + " & επανεκκίνηση...");
            string ps1 = System.IO.Path.Combine(tmpDir, "apply.ps1");
            string script =
                "param($myPid,$zip,$appDir,$instName,$exePath,$tag)\r\n" +
                "for($i=0;$i -lt 60;$i++){ if(-not (Get-Process -Id $myPid -ErrorAction SilentlyContinue)){break}; Start-Sleep -Milliseconds 150 }\r\n" +
                "Add-Type -AssemblyName System.IO.Compression.FileSystem\r\n" +
                "$za = [System.IO.Compression.ZipFile]::OpenRead($zip)\r\n" +
                "try {\r\n" +
                "  $pref = $null; $shared = $true\r\n" +
                "  foreach($e in $za.Entries){ $fn=$e.FullName -replace '\\\\','/'; $sl=$fn.IndexOf('/'); if($sl -le 0){$shared=$false;break}; $p=$fn.Substring(0,$sl+1); if(-not $pref){$pref=$p}elseif(-not $pref.Equals($p,[StringComparison]::OrdinalIgnoreCase)){$shared=$false;break} }\r\n" +
                "  $rootFull = [System.IO.Path]::GetFullPath($appDir).TrimEnd('\\') + '\\'\r\n" +
                "  foreach($e in $za.Entries){\r\n" +
                "    $rel = $e.FullName -replace '\\\\','/'\r\n" +
                "    if($shared -and $pref -and $rel.StartsWith($pref,[StringComparison]::OrdinalIgnoreCase)){ $rel=$rel.Substring($pref.Length) }\r\n" +
                "    if([string]::IsNullOrEmpty($rel) -or $rel.EndsWith('/') -or $rel.Contains(':')){ continue }\r\n" +
                "    $dst = [System.IO.Path]::GetFullPath((Join-Path $appDir ($rel -replace '/','\\')))\r\n" +
                "    if(-not $dst.StartsWith($rootFull,[StringComparison]::OrdinalIgnoreCase)){ continue }\r\n" +
                "    $par = Split-Path -Parent $dst; if($par){ New-Item -ItemType Directory -Force -Path $par | Out-Null }\r\n" +
                "    for($k=0;$k -lt 5;$k++){ try { [System.IO.Compression.ZipFileExtensions]::ExtractToFile($e,$dst,$true); break } catch { Start-Sleep -Milliseconds 250 } }\r\n" +
                "  }\r\n" +
                "} finally { $za.Dispose() }\r\n" +
                "$bak = $exePath + '.bak'\r\n" +
                "if(Test-Path $exePath){ Copy-Item -LiteralPath $exePath -Destination $bak -Force -ErrorAction SilentlyContinue; for($k=0;$k -lt 5;$k++){ try { Remove-Item -LiteralPath $exePath -Force -ErrorAction Stop; break } catch { Start-Sleep -Milliseconds 250 } } }\r\n" +
                "Get-ChildItem (Join-Path $appDir 'desktop') -Filter '*.cs' -ErrorAction SilentlyContinue | ForEach-Object { $_.LastWriteTimeUtc = [DateTime]::UtcNow }\r\n" +
                "if($instName -and (Test-Path (Join-Path $appDir $instName))){ & (Join-Path $appDir $instName) -Silent 2>$null | Out-Null }\r\n" +
                "if(-not (Test-Path $exePath) -and (Test-Path $bak)){ Move-Item -LiteralPath $bak -Destination $exePath -Force -ErrorAction SilentlyContinue } else { Remove-Item -LiteralPath $bak -Force -ErrorAction SilentlyContinue }\r\n" +
                "if($tag){ Set-Content -LiteralPath (Join-Path $appDir '.nilly_version') -Value $tag -Encoding UTF8 -ErrorAction SilentlyContinue }\r\n" +
                "if(Test-Path $exePath){ Start-Process -FilePath $exePath -WorkingDirectory $appDir }\r\n" +
                "Start-Sleep -Milliseconds 400\r\n" +
                "Remove-Item -LiteralPath (Split-Path $zip) -Recurse -Force -ErrorAction SilentlyContinue\r\n";
            System.IO.File.WriteAllText(ps1, script, new System.Text.UTF8Encoding(false));

            int myPid = System.Diagnostics.Process.GetCurrentProcess().Id;
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(
                "powershell.exe",
                "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"" + ps1 + "\" " +
                myPid + " \"" + zipFile + "\" \"" + appDir + "\" \"" + installerScript + "\" \"" + exePath + "\" \"" + tag + "\"");
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            System.Diagnostics.Process.Start(psi);
            if (onExit != null) onExit();
        }
    }

    // ========================================================================
    // ARIAKIT-INSPIRED MINIMAL VECTOR UI KIT (Zero DLLs · 1.5px Stroke Icons)
    // ========================================================================
    internal enum AkKind { Primary, Secondary, Ghost, Tab, AccentOutline, Danger }

    internal enum AkIcon
    {
        None, Refresh, Sun, Moon, Search, Pin, Info, Play, Pause, Stop,
        Plus, Minus, Close, Up, Down, Folder, Copy, Trash, Check, Swap,
        Download, Link, Split, Pipette, Bolt, List, Filter, Shield, Lock, Unlock,
        Edit, Save, External, Bold, Italic, Undo, ChevLeft, ChevRight, Star
    }

    internal static class AkDraw
    {
        public static System.Drawing.Drawing2D.GraphicsPath RoundRect(float x, float y, float w, float h, float r)
        {
            System.Drawing.Drawing2D.GraphicsPath p = new System.Drawing.Drawing2D.GraphicsPath();
            float d = System.Math.Min(r * 2f, System.Math.Min(w, h));
            if (d <= 0.5f) { p.AddRectangle(new System.Drawing.RectangleF(x, y, w, h)); return p; }
            p.AddArc(x, y, d, d, 180, 90);
            p.AddArc(x + w - d, y, d, d, 270, 90);
            p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            p.AddArc(x, y + h - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void DrawIcon(System.Drawing.Graphics g, AkIcon icon, float cx, float cy, float sz, System.Drawing.Color color)
        {
            if (icon == AkIcon.None) return;
            System.Drawing.Drawing2D.SmoothingMode oldSm = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            float r = sz * 0.5f;
            using (System.Drawing.Pen pen = new System.Drawing.Pen(color, 1.5f))
            using (System.Drawing.SolidBrush br = new System.Drawing.SolidBrush(color))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                switch (icon)
                {
                    case AkIcon.Refresh:
                        g.DrawArc(pen, cx - r * 0.82f, cy - r * 0.82f, r * 1.64f, r * 1.64f, -55, 295);
                        g.DrawLine(pen, cx + r * 0.28f, cy - r * 0.85f, cx + r * 0.62f, cy - r * 0.52f);
                        g.DrawLine(pen, cx + r * 0.62f, cy - r * 0.52f, cx + r * 0.20f, cy - r * 0.42f);
                        break;
                    case AkIcon.Sun:
                        g.DrawEllipse(pen, cx - r * 0.42f, cy - r * 0.42f, r * 0.84f, r * 0.84f);
                        for (int i = 0; i < 8; i++)
                        {
                            double a = i * System.Math.PI / 4.0;
                            float x1 = cx + (float)(System.Math.Cos(a) * r * 0.68f);
                            float y1 = cy + (float)(System.Math.Sin(a) * r * 0.68f);
                            float x2 = cx + (float)(System.Math.Cos(a) * r * 0.94f);
                            float y2 = cy + (float)(System.Math.Sin(a) * r * 0.94f);
                            g.DrawLine(pen, x1, y1, x2, y2);
                        }
                        break;
                    case AkIcon.Moon:
                        g.DrawArc(pen, cx - r * 0.78f, cy - r * 0.78f, r * 1.56f, r * 1.56f, 40, 235);
                        g.DrawArc(pen, cx - r * 0.30f, cy - r * 0.92f, r * 1.32f, r * 1.32f, 115, 130);
                        break;
                    case AkIcon.Search:
                        g.DrawEllipse(pen, cx - r * 0.82f, cy - r * 0.82f, r * 1.24f, r * 1.24f);
                        g.DrawLine(pen, cx + r * 0.25f, cy + r * 0.25f, cx + r * 0.82f, cy + r * 0.82f);
                        break;
                    case AkIcon.Pin:
                        g.DrawLine(pen, cx - r * 0.48f, cy - r * 0.72f, cx + r * 0.48f, cy - r * 0.72f);
                        g.DrawLine(pen, cx - r * 0.34f, cy - r * 0.72f, cx - r * 0.56f, cy + r * 0.14f);
                        g.DrawLine(pen, cx + r * 0.34f, cy - r * 0.72f, cx + r * 0.56f, cy + r * 0.14f);
                        g.DrawLine(pen, cx - r * 0.66f, cy + r * 0.14f, cx + r * 0.66f, cy + r * 0.14f);
                        g.DrawLine(pen, cx, cy + r * 0.14f, cx, cy + r * 0.86f);
                        break;
                    case AkIcon.Info:
                        g.DrawEllipse(pen, cx - r * 0.85f, cy - r * 0.85f, r * 1.70f, r * 1.70f);
                        g.DrawLine(pen, cx, cy - r * 0.08f, cx, cy + r * 0.48f);
                        g.FillEllipse(br, cx - 1.1f, cy - r * 0.52f, 2.2f, 2.2f);
                        break;
                    case AkIcon.Play:
                        g.FillPolygon(br, new System.Drawing.PointF[] {
                            new System.Drawing.PointF(cx - r * 0.45f, cy - r * 0.68f),
                            new System.Drawing.PointF(cx + r * 0.68f, cy),
                            new System.Drawing.PointF(cx - r * 0.45f, cy + r * 0.68f)
                        });
                        break;
                    case AkIcon.Pause:
                        g.DrawLine(pen, cx - r * 0.36f, cy - r * 0.62f, cx - r * 0.36f, cy + r * 0.62f);
                        g.DrawLine(pen, cx + r * 0.36f, cy - r * 0.62f, cx + r * 0.36f, cy + r * 0.62f);
                        break;
                    case AkIcon.Stop:
                        g.FillRectangle(br, cx - r * 0.56f, cy - r * 0.56f, r * 1.12f, r * 1.12f);
                        break;
                    case AkIcon.Plus:
                        g.DrawLine(pen, cx - r * 0.68f, cy, cx + r * 0.68f, cy);
                        g.DrawLine(pen, cx, cy - r * 0.68f, cx, cy + r * 0.68f);
                        break;
                    case AkIcon.Minus:
                        g.DrawLine(pen, cx - r * 0.68f, cy, cx + r * 0.68f, cy);
                        break;
                    case AkIcon.Close:
                        g.DrawLine(pen, cx - r * 0.58f, cy - r * 0.58f, cx + r * 0.58f, cy + r * 0.58f);
                        g.DrawLine(pen, cx + r * 0.58f, cy - r * 0.58f, cx - r * 0.58f, cy + r * 0.58f);
                        break;
                    case AkIcon.Up:
                        g.DrawLine(pen, cx, cy + r * 0.68f, cx, cy - r * 0.64f);
                        g.DrawLine(pen, cx - r * 0.52f, cy - r * 0.14f, cx, cy - r * 0.66f);
                        g.DrawLine(pen, cx + r * 0.52f, cy - r * 0.14f, cx, cy - r * 0.66f);
                        break;
                    case AkIcon.Down:
                        g.DrawLine(pen, cx, cy - r * 0.68f, cx, cy + r * 0.64f);
                        g.DrawLine(pen, cx - r * 0.52f, cy + r * 0.14f, cx, cy + r * 0.66f);
                        g.DrawLine(pen, cx + r * 0.52f, cy + r * 0.14f, cx, cy + r * 0.66f);
                        break;
                    case AkIcon.Folder:
                        g.DrawPolygon(pen, new System.Drawing.PointF[] {
                            new System.Drawing.PointF(cx - r * 0.85f, cy - r * 0.56f),
                            new System.Drawing.PointF(cx - r * 0.22f, cy - r * 0.56f),
                            new System.Drawing.PointF(cx - r * 0.04f, cy - r * 0.34f),
                            new System.Drawing.PointF(cx + r * 0.85f, cy - r * 0.34f),
                            new System.Drawing.PointF(cx + r * 0.85f, cy + r * 0.64f),
                            new System.Drawing.PointF(cx - r * 0.85f, cy + r * 0.64f)
                        });
                        break;
                    case AkIcon.Copy:
                        g.DrawRectangle(pen, cx - r * 0.26f, cy - r * 0.26f, r * 1.02f, r * 1.02f);
                        g.DrawLine(pen, cx - r * 0.68f, cy + r * 0.30f, cx - r * 0.68f, cy - r * 0.68f);
                        g.DrawLine(pen, cx - r * 0.68f, cy - r * 0.68f, cx + r * 0.30f, cy - r * 0.68f);
                        break;
                    case AkIcon.Trash:
                        g.DrawLine(pen, cx - r * 0.76f, cy - r * 0.44f, cx + r * 0.76f, cy - r * 0.44f);
                        g.DrawLine(pen, cx - r * 0.28f, cy - r * 0.74f, cx + r * 0.28f, cy - r * 0.74f);
                        g.DrawRectangle(pen, cx - r * 0.54f, cy - r * 0.42f, r * 1.08f, r * 1.16f);
                        break;
                    case AkIcon.Check:
                        g.DrawLine(pen, cx - r * 0.68f, cy + r * 0.04f, cx - r * 0.14f, cy + r * 0.58f);
                        g.DrawLine(pen, cx - r * 0.14f, cy + r * 0.58f, cx + r * 0.74f, cy - r * 0.52f);
                        break;
                    case AkIcon.Swap:
                        g.DrawLine(pen, cx - r * 0.70f, cy - r * 0.32f, cx + r * 0.70f, cy - r * 0.32f);
                        g.DrawLine(pen, cx + r * 0.32f, cy - r * 0.68f, cx + r * 0.72f, cy - r * 0.32f);
                        g.DrawLine(pen, cx + r * 0.70f, cy + r * 0.34f, cx - r * 0.70f, cy + r * 0.34f);
                        g.DrawLine(pen, cx - r * 0.32f, cy + r * 0.70f, cx - r * 0.72f, cy + r * 0.34f);
                        break;
                    case AkIcon.Download:
                        g.DrawLine(pen, cx, cy - r * 0.72f, cx, cy + r * 0.28f);
                        g.DrawLine(pen, cx - r * 0.44f, cy - r * 0.12f, cx, cy + r * 0.32f);
                        g.DrawLine(pen, cx + r * 0.44f, cy - r * 0.12f, cx, cy + r * 0.32f);
                        g.DrawLine(pen, cx - r * 0.74f, cy + r * 0.72f, cx + r * 0.74f, cy + r * 0.72f);
                        break;
                    case AkIcon.Link:
                    case AkIcon.External:
                        g.DrawLine(pen, cx - r * 0.58f, cy + r * 0.58f, cx + r * 0.62f, cy - r * 0.62f);
                        g.DrawLine(pen, cx - r * 0.06f, cy - r * 0.64f, cx + r * 0.64f, cy - r * 0.64f);
                        g.DrawLine(pen, cx + r * 0.64f, cy - r * 0.64f, cx + r * 0.64f, cy + r * 0.06f);
                        break;
                    case AkIcon.Split:
                        g.DrawRectangle(pen, cx - r * 0.82f, cy - r * 0.64f, r * 1.64f, r * 1.28f);
                        g.DrawLine(pen, cx, cy - r * 0.64f, cx, cy + r * 0.64f);
                        break;
                    case AkIcon.Pipette:
                        g.DrawLine(pen, cx - r * 0.66f, cy + r * 0.66f, cx + r * 0.40f, cy - r * 0.40f);
                        g.DrawLine(pen, cx + r * 0.12f, cy - r * 0.68f, cx + r * 0.68f, cy - r * 0.12f);
                        break;
                    case AkIcon.Bolt:
                        g.DrawPolygon(pen, new System.Drawing.PointF[] {
                            new System.Drawing.PointF(cx + r * 0.14f, cy - r * 0.82f),
                            new System.Drawing.PointF(cx - r * 0.62f, cy + r * 0.08f),
                            new System.Drawing.PointF(cx - r * 0.04f, cy + r * 0.08f),
                            new System.Drawing.PointF(cx - r * 0.18f, cy + r * 0.82f),
                            new System.Drawing.PointF(cx + r * 0.62f, cy - r * 0.10f),
                            new System.Drawing.PointF(cx + r * 0.04f, cy - r * 0.10f)
                        });
                        break;
                    case AkIcon.List:
                        g.DrawLine(pen, cx - r * 0.72f, cy - r * 0.46f, cx + r * 0.72f, cy - r * 0.46f);
                        g.DrawLine(pen, cx - r * 0.72f, cy, cx + r * 0.72f, cy);
                        g.DrawLine(pen, cx - r * 0.72f, cy + r * 0.46f, cx + r * 0.72f, cy + r * 0.46f);
                        break;
                    case AkIcon.Filter:
                        g.DrawPolygon(pen, new System.Drawing.PointF[] {
                            new System.Drawing.PointF(cx - r * 0.78f, cy - r * 0.60f),
                            new System.Drawing.PointF(cx + r * 0.78f, cy - r * 0.60f),
                            new System.Drawing.PointF(cx + r * 0.16f, cy + r * 0.08f),
                            new System.Drawing.PointF(cx + r * 0.16f, cy + r * 0.68f),
                            new System.Drawing.PointF(cx - r * 0.16f, cy + r * 0.52f),
                            new System.Drawing.PointF(cx - r * 0.16f, cy + r * 0.08f)
                        });
                        break;
                    case AkIcon.Shield:
                        g.DrawPolygon(pen, new System.Drawing.PointF[] {
                            new System.Drawing.PointF(cx, cy - r * 0.82f),
                            new System.Drawing.PointF(cx + r * 0.72f, cy - r * 0.44f),
                            new System.Drawing.PointF(cx + r * 0.58f, cy + r * 0.34f),
                            new System.Drawing.PointF(cx, cy + r * 0.82f),
                            new System.Drawing.PointF(cx - r * 0.58f, cy + r * 0.34f),
                            new System.Drawing.PointF(cx - r * 0.72f, cy - r * 0.44f)
                        });
                        break;
                    case AkIcon.Lock:
                        g.DrawRectangle(pen, cx - r * 0.58f, cy - r * 0.08f, r * 1.16f, r * 0.84f);
                        g.DrawArc(pen, cx - r * 0.38f, cy - r * 0.76f, r * 0.76f, r * 0.84f, 180, 180);
                        break;
                    case AkIcon.Unlock:
                        g.DrawRectangle(pen, cx - r * 0.58f, cy - r * 0.08f, r * 1.16f, r * 0.84f);
                        g.DrawArc(pen, cx - r * 0.38f, cy - r * 0.76f, r * 0.76f, r * 0.84f, 180, 130);
                        break;
                    case AkIcon.Save:
                        g.DrawRectangle(pen, cx - r * 0.72f, cy - r * 0.72f, r * 1.44f, r * 1.44f);
                        g.DrawRectangle(pen, cx - r * 0.42f, cy - r * 0.72f, r * 0.72f, r * 0.48f);
                        g.DrawRectangle(pen, cx - r * 0.44f, cy + r * 0.14f, r * 0.88f, r * 0.58f);
                        break;
                    case AkIcon.Edit:
                        g.DrawLine(pen, cx - r * 0.62f, cy + r * 0.62f, cx + r * 0.56f, cy - r * 0.56f);
                        g.DrawLine(pen, cx + r * 0.24f, cy - r * 0.74f, cx + r * 0.74f, cy - r * 0.24f);
                        g.DrawLine(pen, cx - r * 0.72f, cy + r * 0.72f, cx - r * 0.34f, cy + r * 0.62f);
                        break;
                    case AkIcon.Undo:
                        g.DrawArc(pen, cx - r * 0.52f, cy - r * 0.34f, r * 1.24f, r * 1.04f, -135, 225);
                        g.DrawLine(pen, cx - r * 0.72f, cy - r * 0.22f, cx - r * 0.28f, cy - r * 0.62f);
                        g.DrawLine(pen, cx - r * 0.72f, cy - r * 0.22f, cx - r * 0.24f, cy + r * 0.10f);
                        break;
                    case AkIcon.ChevLeft:
                        g.DrawLine(pen, cx + r * 0.28f, cy - r * 0.58f, cx - r * 0.34f, cy);
                        g.DrawLine(pen, cx - r * 0.34f, cy, cx + r * 0.28f, cy + r * 0.58f);
                        break;
                    case AkIcon.ChevRight:
                        g.DrawLine(pen, cx - r * 0.28f, cy - r * 0.58f, cx + r * 0.34f, cy);
                        g.DrawLine(pen, cx + r * 0.34f, cy, cx - r * 0.28f, cy + r * 0.58f);
                        break;
                    case AkIcon.Star:
                        System.Drawing.PointF[] sp = new System.Drawing.PointF[10];
                        for (int k = 0; k < 10; k++)
                        {
                            double ang = -System.Math.PI / 2.0 + k * System.Math.PI / 5.0;
                            float rad = (k % 2 == 0) ? r * 0.85f : r * 0.38f;
                            sp[k] = new System.Drawing.PointF(cx + (float)(System.Math.Cos(ang) * rad), cy + (float)(System.Math.Sin(ang) * rad));
                        }
                        g.DrawPolygon(pen, sp);
                        break;
                }
            }
            g.SmoothingMode = oldSm;
        }
    }

    internal sealed class AkButton : System.Windows.Forms.Button
    {
        public AkKind Kind = AkKind.Secondary;
        public AkIcon Icon = AkIcon.None;
        public bool Active = false;
        public bool Dark = true;
        public int Radius = 6;
        private bool _hov = false;
        private bool _down = false;

        public AkButton()
        {
            SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                     System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer |
                     System.Windows.Forms.ControlStyles.UserPaint |
                     System.Windows.Forms.ControlStyles.ResizeRedraw, true);
            FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = System.Windows.Forms.Cursors.Hand;
            TabStop = false;
            UseMnemonic = false;
        }

        protected override void OnMouseEnter(System.EventArgs e) { _hov = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(System.EventArgs e) { _hov = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) { if (e.Button == System.Windows.Forms.MouseButtons.Left) { _down = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            System.Drawing.Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            System.Drawing.Color parentBg = Parent != null ? Parent.BackColor : (Dark ? System.Drawing.Color.FromArgb(29, 31, 32) : System.Drawing.Color.FromArgb(248, 249, 250));
            g.Clear(parentBg);

            System.Drawing.Color accent = Dark ? System.Drawing.Color.FromArgb(252, 150, 31) : System.Drawing.Color.FromArgb(224, 118, 12);
            System.Drawing.Color accentHov = Dark ? System.Drawing.Color.FromArgb(255, 169, 64) : System.Drawing.Color.FromArgb(204, 102, 8);
            System.Drawing.Color surf = Dark ? System.Drawing.Color.FromArgb(39, 41, 43) : System.Drawing.Color.White;
            System.Drawing.Color surfHov = Dark ? System.Drawing.Color.FromArgb(50, 53, 56) : System.Drawing.Color.FromArgb(236, 239, 243);
            System.Drawing.Color border = Dark ? System.Drawing.Color.FromArgb(58, 62, 66) : System.Drawing.Color.FromArgb(210, 216, 224);
            System.Drawing.Color borderHov = Dark ? System.Drawing.Color.FromArgb(84, 89, 95) : System.Drawing.Color.FromArgb(176, 184, 196);
            System.Drawing.Color fgMain = Dark ? System.Drawing.Color.FromArgb(242, 244, 247) : System.Drawing.Color.FromArgb(26, 29, 33);
            System.Drawing.Color fgMuted = Dark ? System.Drawing.Color.FromArgb(154, 160, 168) : System.Drawing.Color.FromArgb(95, 104, 116);
            System.Drawing.Color fgDis = Dark ? System.Drawing.Color.FromArgb(92, 97, 104) : System.Drawing.Color.FromArgb(170, 178, 188);

            bool en = Enabled;
            System.Drawing.Color bg = parentBg;
            System.Drawing.Color bd = System.Drawing.Color.Transparent;
            System.Drawing.Color fg = en ? fgMain : fgDis;
            bool drawFill = false;

            if (Kind == AkKind.Primary)
            {
                drawFill = true;
                bg = en ? (_hov ? accentHov : accent) : (Dark ? System.Drawing.Color.FromArgb(56, 48, 38) : System.Drawing.Color.FromArgb(228, 224, 218));
                fg = en ? (Dark ? System.Drawing.Color.FromArgb(20, 21, 23) : System.Drawing.Color.White) : fgDis;
            }
            else if (Kind == AkKind.AccentOutline)
            {
                drawFill = true;
                bg = en ? (_hov ? (Dark ? System.Drawing.Color.FromArgb(52, 42, 30) : System.Drawing.Color.FromArgb(255, 243, 228)) : surf) : (Dark ? System.Drawing.Color.FromArgb(32, 34, 36) : System.Drawing.Color.FromArgb(244, 246, 248));
                bd = en ? accent : border;
                fg = en ? accent : fgDis;
            }
            else if (Kind == AkKind.Danger)
            {
                drawFill = true;
                System.Drawing.Color red = Dark ? System.Drawing.Color.FromArgb(240, 104, 118) : System.Drawing.Color.FromArgb(196, 40, 56);
                bg = en ? (_hov ? (Dark ? System.Drawing.Color.FromArgb(62, 30, 36) : System.Drawing.Color.FromArgb(254, 228, 231)) : (Dark ? System.Drawing.Color.FromArgb(46, 28, 32) : System.Drawing.Color.FromArgb(255, 240, 242))) : surf;
                bd = en ? (Dark ? System.Drawing.Color.FromArgb(116, 48, 58) : System.Drawing.Color.FromArgb(236, 154, 164)) : border;
                fg = en ? red : fgDis;
            }
            else if (Kind == AkKind.Tab)
            {
                drawFill = _hov && en;
                bg = surfHov;
                fg = en ? (Active ? fgMain : (_hov ? fgMain : fgMuted)) : fgDis;
            }
            else if (Kind == AkKind.Ghost)
            {
                drawFill = (Active || _hov) && en;
                bg = Active ? (Dark ? System.Drawing.Color.FromArgb(52, 42, 30) : System.Drawing.Color.FromArgb(255, 243, 228)) : surfHov;
                bd = Active ? accent : System.Drawing.Color.Transparent;
                fg = en ? (Active ? accent : (_hov ? fgMain : fgMuted)) : fgDis;
            }
            else // Secondary (Ariakit tactile elevated pill)
            {
                drawFill = true;
                bg = en ? (Active ? (Dark ? System.Drawing.Color.FromArgb(52, 42, 30) : System.Drawing.Color.FromArgb(255, 243, 228)) : (_hov ? surfHov : surf)) : (Dark ? System.Drawing.Color.FromArgb(32, 34, 36) : System.Drawing.Color.FromArgb(245, 247, 249));
                bd = en ? (Active ? accent : (_hov ? borderHov : border)) : (Dark ? System.Drawing.Color.FromArgb(44, 47, 50) : System.Drawing.Color.FromArgb(224, 228, 234));
                fg = en ? (Active ? accent : fgMain) : fgDis;
            }

            float ox = 0.5f, oy = (_down && en) ? 1.5f : 0.5f;
            float rw = Width - 1.5f, rh = Height - 1.5f - ((_down && en) ? 1f : 0f);
            if (drawFill || bd.A > 0)
            {
                using (System.Drawing.Drawing2D.GraphicsPath gp = AkDraw.RoundRect(ox, oy, rw, rh, Radius))
                {
                    if (drawFill) using (System.Drawing.SolidBrush br = new System.Drawing.SolidBrush(bg)) g.FillPath(br, gp);
                    if (bd.A > 0) using (System.Drawing.Pen p = new System.Drawing.Pen(bd, 1f)) g.DrawPath(p, gp);
                }
            }
            if (Kind == AkKind.Tab && Active)
            {
                using (System.Drawing.SolidBrush ab = new System.Drawing.SolidBrush(accent))
                    g.FillRectangle(ab, 6, Height - 3, System.Math.Max(8, Width - 12), 2.5f);
            }

            string txt = Text ?? "";
            float yShift = (_down && en) ? 1f : 0f;
            if (Icon != AkIcon.None && txt.Length == 0)
            {
                AkDraw.DrawIcon(g, Icon, Width * 0.5f, Height * 0.5f + yShift, 13.5f, fg);
            }
            else if (Icon != AkIcon.None)
            {
                System.Drawing.Size ts = System.Windows.Forms.TextRenderer.MeasureText(txt, Font, new System.Drawing.Size(Width, Height), System.Windows.Forms.TextFormatFlags.NoPrefix | System.Windows.Forms.TextFormatFlags.NoPadding);
                float iconW = 13f, gap = 6f;
                float totalW = iconW + gap + ts.Width;
                float startX = System.Math.Max(8f, (Width - totalW) * 0.5f);
                AkDraw.DrawIcon(g, Icon, startX + iconW * 0.5f, Height * 0.5f + yShift, 12.5f, fg);
                System.Drawing.Rectangle tr = new System.Drawing.Rectangle((int)(startX + iconW + gap), (int)yShift, System.Math.Max(10, Width - (int)(startX + iconW + gap) - 4), Height);
                System.Windows.Forms.TextRenderer.DrawText(g, txt, Font, tr, fg, System.Windows.Forms.TextFormatFlags.Left | System.Windows.Forms.TextFormatFlags.VerticalCenter | System.Windows.Forms.TextFormatFlags.NoPrefix | System.Windows.Forms.TextFormatFlags.EndEllipsis);
            }
            else
            {
                System.Drawing.Rectangle tr = new System.Drawing.Rectangle(4, (int)yShift, System.Math.Max(8, Width - 8), Height);
                System.Windows.Forms.TextRenderer.DrawText(g, txt, Font, tr, fg, System.Windows.Forms.TextFormatFlags.HorizontalCenter | System.Windows.Forms.TextFormatFlags.VerticalCenter | System.Windows.Forms.TextFormatFlags.NoPrefix | System.Windows.Forms.TextFormatFlags.EndEllipsis);
            }
        }
    }

}

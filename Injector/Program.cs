using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpMonoInjector;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new InjectorForm());
    }
}

public class InjectorForm : Form
{
    private TextBox txtProcessName;
    private Button btnInject;
    private Button btnEject;
    private Button btnRefresh;
    private ComboBox cmbProcesses;
    private Label lblStatus;
    private TextBox txtDllPath;
    private Button btnBrowseDll;
    private Panel titleBar;
    private Label lblTitle;
    private Button btnMin;
    private Button btnClose;

    public InjectorForm()
    {
        InitializeComponents();
        this.Shown += (s, e) => RefreshProcessList();
    }

    private void InitializeComponents()
    {
        this.Text = "Unknown Casting";
        this.Size = new Size(480, 340);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(20, 20, 20);
        this.ForeColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.None;
        this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 8, 8));

        titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = Color.FromArgb(25, 25, 25)
        };
        this.Controls.Add(titleBar);

        lblTitle = new Label
        {
            Text = "UNKNOWN CASTING",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(180, 180, 180),
            AutoSize = true,
            Location = new Point(15, 9)
        };
        titleBar.Controls.Add(lblTitle);

        btnMin = new Button
        {
            Text = "—",
            Size = new Size(36, 30),
            Location = new Point(this.Width - 72, 3),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(150, 150, 150),
            Cursor = Cursors.Hand
        };
        btnMin.FlatAppearance.BorderSize = 0;
        btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
        titleBar.Controls.Add(btnMin);

        btnClose = new Button
        {
            Text = "✕",
            Size = new Size(36, 30),
            Location = new Point(this.Width - 36, 3),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(150, 150, 150),
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Color.FromArgb(200, 50, 50); btnClose.ForeColor = Color.White; };
        btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; btnClose.ForeColor = Color.FromArgb(150, 150, 150); };
        btnClose.Click += (s, e) => Application.Exit();
        titleBar.Controls.Add(btnClose);

        titleBar.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left) ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        };

        Label lblCustom = new Label
        {
            Text = "CUSTOM PROCESS",
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 120, 120),
            Location = new Point(25, 55),
            Size = new Size(110, 15)
        };
        this.Controls.Add(lblCustom);

        txtProcessName = new TextBox
        {
            Location = new Point(25, 73),
            Size = new Size(280, 26),
            Font = new Font("Segoe UI", 9),
            BackColor = Color.FromArgb(35, 35, 35),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        this.Controls.Add(txtProcessName);

        btnInject = new Button
        {
            Text = "INJECT",
            Location = new Point(315, 71),
            Size = new Size(140, 28),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(80, 80, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnInject.FlatAppearance.BorderSize = 0;
        btnInject.MouseEnter += (s, e) => btnInject.BackColor = Color.FromArgb(100, 100, 100);
        btnInject.MouseLeave += (s, e) => btnInject.BackColor = Color.FromArgb(80, 80, 80);
        btnInject.Click += (s, e) => Inject();
        this.Controls.Add(btnInject);

        btnEject = new Button
        {
            Text = "EJECT",
            Location = new Point(315, 103),
            Size = new Size(140, 28),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(80, 80, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnEject.FlatAppearance.BorderSize = 0;
        btnEject.MouseEnter += (s, e) => btnEject.BackColor = Color.FromArgb(100, 100, 100);
        btnEject.MouseLeave += (s, e) => btnEject.BackColor = Color.FromArgb(80, 80, 80);
        btnEject.Click += (s, e) => Eject();
        this.Controls.Add(btnEject);

        Label lblProcesses = new Label
        {
            Text = "RUNNING PROCESSES",
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 120, 120),
            Location = new Point(25, 115),
            Size = new Size(150, 15)
        };
        this.Controls.Add(lblProcesses);

        btnRefresh = new Button
        {
            Text = "↻",
            Location = new Point(425, 113),
            Size = new Size(30, 24),
            Font = new Font("Segoe UI", 14),
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += (s, e) => RefreshProcessList();
        this.Controls.Add(btnRefresh);

        cmbProcesses = new ComboBox
        {
            Location = new Point(25, 133),
            Size = new Size(430, 26),
            Font = new Font("Segoe UI", 9),
            BackColor = Color.FromArgb(35, 35, 35),
            ForeColor = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
        cmbProcesses.SelectedIndexChanged += CmbProcesses_SelectedIndexChanged;
        this.Controls.Add(cmbProcesses);

        Label lblDll = new Label
        {
            Text = "DLL FILE",
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 120, 120),
            Location = new Point(25, 175),
            Size = new Size(60, 15)
        };
        this.Controls.Add(lblDll);

        txtDllPath = new TextBox
        {
            Text = "UnknownCasting.dll",
            Location = new Point(25, 193),
            Size = new Size(330, 26),
            Font = new Font("Segoe UI", 9),
            BackColor = Color.FromArgb(35, 35, 35),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        this.Controls.Add(txtDllPath);

        btnBrowseDll = new Button
        {
            Text = "...",
            Location = new Point(365, 191),
            Size = new Size(90, 28),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnBrowseDll.FlatAppearance.BorderSize = 0;
        btnBrowseDll.Click += BtnBrowseDll_Click;
        this.Controls.Add(btnBrowseDll);

        lblStatus = new Label
        {
            Text = "Ready",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(100, 100, 100),
            Location = new Point(25, 300),
            Size = new Size(430, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };
        this.Controls.Add(lblStatus);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int nRadiusX, int nRadiusY);

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (titleBar != null)
        {
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 8, 8));
            btnMin.Location = new Point(this.Width - 72, 3);
            btnClose.Location = new Point(this.Width - 36, 3);
        }
    }

    private void BtnBrowseDll_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog ofd = new OpenFileDialog())
        {
            ofd.Filter = "DLL Files|*.dll";
            ofd.Title = "Select DLL to Inject";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtDllPath.Text = ofd.FileName;
            }
        }
    }

    private void Inject()
    {
        string processName;

        if (!string.IsNullOrWhiteSpace(cmbProcesses.SelectedItem?.ToString()))
        {
            string selected = cmbProcesses.SelectedItem.ToString();
            processName = selected.Split('(')[0].Trim();
        }
        else if (!string.IsNullOrWhiteSpace(txtProcessName.Text))
        {
            processName = txtProcessName.Text;
        }
        else
        {
            processName = "Gorilla Tag";
        }

        string dllPath = txtDllPath.Text;

        if (!System.IO.File.Exists(dllPath))
        {
            SetStatus($"DLL not found: {dllPath}", Color.FromArgb(220, 80, 80));
            return;
        }

        try
        {
            var process = Process.GetProcesses()
                .FirstOrDefault(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));

            if (process == null)
            {
                SetStatus($"Process '{processName}' not found!", Color.FromArgb(220, 80, 80));
                return;
            }

            SetStatus($"Injecting into {processName} (PID: {process.Id})...", Color.FromArgb(80, 180, 220));
            this.Refresh();

            byte[] dllBytes = System.IO.File.ReadAllBytes(dllPath);

            using (var injector = new Injector(process.Id))
            {
                IntPtr remotePtr = injector.Inject(dllBytes, "Loader", "Loader", "Load");

                if (remotePtr == IntPtr.Zero)
                {
                    SetStatus("Injection failed!", Color.FromArgb(220, 80, 80));
                    return;
                }

                SetStatus($"[SUCCESS] Injected at 0x{remotePtr.ToInt64():X}", Color.FromArgb(80, 220, 120));
            }
        }
        catch (InjectorException ex)
        {
            SetStatus($"Error: {ex.Message}", Color.FromArgb(220, 80, 80));
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Color.FromArgb(220, 80, 80));
        }
    }

    private void RefreshProcessList()
    {
        cmbProcesses.Items.Clear();

        try
        {
            Process[] procs = Process.GetProcesses();
            
            foreach (var p in procs.OrderBy(x => x.ProcessName))
            {
                try
                {
                    string title = "";
                    try { title = p.MainWindowTitle; } catch { }
                    
                    if (!string.IsNullOrEmpty(title))
                    {
                        cmbProcesses.Items.Add($"{p.ProcessName} (PID: {p.Id})");
                    }
                    else if (p.ProcessName.ToLower().Contains("gorilla") || p.ProcessName.ToLower().Contains("tag"))
                    {
                        cmbProcesses.Items.Add($"{p.ProcessName} (PID: {p.Id})");
                    }
                }
                catch { }
            }
            
            SetStatus($"Found {cmbProcesses.Items.Count} processes", Color.FromArgb(100, 100, 100));
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Color.FromArgb(220, 80, 80));
        }
    }

    private void SetStatus(string text, Color color)
    {
        lblStatus.Text = text;
        lblStatus.ForeColor = color;
    }

    private void CmbProcesses_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbProcesses.SelectedItem != null)
        {
            string selected = cmbProcesses.SelectedItem.ToString();
            string processName = selected.Split('(')[0].Trim();
            txtProcessName.Text = processName;
        }
    }

    private void Eject()
    {
        string processName;

        if (!string.IsNullOrWhiteSpace(cmbProcesses.SelectedItem?.ToString()))
        {
            string selected = cmbProcesses.SelectedItem.ToString();
            processName = selected.Split('(')[0].Trim();
        }
        else if (!string.IsNullOrWhiteSpace(txtProcessName.Text))
        {
            processName = txtProcessName.Text;
        }
        else
        {
            processName = "Gorilla Tag";
        }

        string dllPath = txtDllPath.Text;

        if (!System.IO.File.Exists(dllPath))
        {
            SetStatus($"DLL not found: {dllPath}", Color.FromArgb(220, 80, 80));
            return;
        }

        try
        {
            var process = Process.GetProcesses()
                .FirstOrDefault(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));

            if (process == null)
            {
                SetStatus($"Process '{processName}' not found!", Color.FromArgb(220, 80, 80));
                return;
            }

            SetStatus($"Ejecting from {processName} (PID: {process.Id})...", Color.FromArgb(80, 180, 220));
            this.Refresh();

            IntPtr processHandle = Native.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, process.Id);
            if (processHandle == IntPtr.Zero)
            {
                SetStatus("Failed to open process", Color.FromArgb(220, 80, 80));
                return;
            }

            IntPtr[] modules = new IntPtr[1024];
            int bytesNeeded = 0;

            if (!Native.EnumProcessModulesEx(processHandle, modules, modules.Length * 8, out bytesNeeded, ModuleFilter.LIST_MODULES_ALL))
            {
                Native.CloseHandle(processHandle);
                SetStatus("Failed to enumerate modules", Color.FromArgb(220, 80, 80));
                return;
            }

            int moduleCount = bytesNeeded / 8;
            string dllName = System.IO.Path.GetFileName(dllPath).ToLower();
            bool ejected = false;

            for (int i = 0; i < moduleCount; i++)
            {
                StringBuilder sb = new StringBuilder(260);
                Native.GetModuleFileNameEx(processHandle, modules[i], sb, (uint)sb.Capacity);
                
                string moduleName = System.IO.Path.GetFileName(sb.ToString()).ToLower();
                if (moduleName == dllName || moduleName == dllName.Replace(".dll", ""))
                {
                    IntPtr freeLibResult = IntPtr.Zero;
                    
                    IntPtr remoteThread = Native.CreateRemoteThread(
                        processHandle,
                        IntPtr.Zero,
                        0,
                        (IntPtr)GetProcAddress(GetModuleHandle("kernel32.dll"), "FreeLibrary"),
                        modules[i],
                        ThreadCreationFlags.None,
                        out int threadId);

                    if (remoteThread != IntPtr.Zero)
                    {
                        Native.WaitForSingleObject(remoteThread, 5000);
                        Native.CloseHandle(remoteThread);
                        ejected = true;
                    }
                    break;
                }
            }

            Native.CloseHandle(processHandle);

            if (!ejected)
            {
                SetStatus("DLL not found in process modules", Color.FromArgb(220, 80, 80));
                return;
            }

            SetStatus("[SUCCESS] Ejected successfully", Color.FromArgb(80, 220, 120));
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Color.FromArgb(220, 80, 80));
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
}
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System.Windows.Forms;

namespace EposBridge;

static class Program
{
    private static Mutex? _mutex;

    [STAThread]
    static void Main(string[] args)
    {
        Console.WriteLine("[DEBUG] Inicio de Main...");
        const string appName = "EposBridge_SingleInstance_Mutex";
        bool createdNew;

        _mutex = new Mutex(true, appName, out createdNew);

        if (!createdNew)
        {
            Console.WriteLine("[ERROR] La aplicación ya se está ejecutando (Mutex detectado).");
            MessageBox.Show("EPOS Bridge ya se está ejecutando.", "EPOS Bridge", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Console.WriteLine("[DEBUG] Mutex adquirido. Inicializando config...");

        ApplicationConfiguration.Initialize();

        // Run Splash on a separate thread to prevent freezing
        ManualResetEvent splashLoaded = new ManualResetEvent(false);
        ManualResetEvent closeSplash = new ManualResetEvent(false);
        
        Thread splashThread = new Thread(() =>
        {
            using (var splash = new SplashForm())
            {
                splash.Shown += (s, e) => splashLoaded.Set();
                // Check periodically if we need to close
                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                t.Interval = 100;
                t.Tick += (s, e) => {
                    if (closeSplash.WaitOne(0)) 
                    {
                        t.Stop();
                        splash.Close();
                    }
                };
                t.Start();
                Application.Run(splash);
            }
        });
        splashThread.SetApartmentState(ApartmentState.STA);
        splashThread.Start();

        // Wait for splash to actually show
        splashLoaded.WaitOne();

        Console.WriteLine("[DEBUG] Creando Host Builder...");
        var host = CreateHostBuilder(args).Build();

        Console.WriteLine("[DEBUG] Iniciando Host (Background)...");
        // Start the host in a background task
        Task.Run(async () => 
        {
            try {
                Console.WriteLine("[DEBUG] Ejecutando host.RunAsync()...");
                await host.RunAsync(); 
            }
            catch (Exception ex) {
                Console.WriteLine($"[FATAL] Error en Host: {ex}");
            }
        });

        // Ensure splash stays for at least a moment so user sees it
        Thread.Sleep(2000); 
        
        // Signal splash to close
        closeSplash.Set();
        splashThread.Join();

        Console.WriteLine("[DEBUG] Iniciando Application.Run (Tray Icon)...");
        
        // Lógica de "Primera Vez"
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EposBridge");
        Directory.CreateDirectory(appDataPath);
        string firstRunFile = Path.Combine(appDataPath, ".first_run_completed");

        if (!File.Exists(firstRunFile))
        {
            // Mostrar pantalla de bienvenida solo si no existe el archivo
            // We need to ensure the icon is loaded correctly here too
            using (var welcome = new WelcomeForm())
            {
                welcome.ShowDialog();
            }
            // Crear el archivo para marcar que ya se mostró
            File.WriteAllText(firstRunFile, DateTime.Now.ToString());
        }

        // Run the Windows Forms Application
        Application.Run(new BridgeApplicationContext(host.Services));
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                Console.WriteLine("[DEBUG] Configurando WebHostDefaults...");
                webBuilder.UseStartup<Startup>();
                webBuilder.UseKestrel((context, options) =>
                {
                    int port = context.HostingEnvironment.IsDevelopment() ? 8001 : 8000;
                    try 
                    {
                        var cert = CertManager.GetOrGenerateCertificate();
                        options.ListenAnyIP(port, listenOptions => listenOptions.UseHttps(cert));
                        Console.WriteLine($"[INFO] Servidor escuchando en puerto: {port} (HTTPS)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[FATAL] Error iniciando Kestrel: {ex.Message}");
                        MessageBox.Show("Error iniciando servidor: " + ex.Message);
                    }
                });
            });
}

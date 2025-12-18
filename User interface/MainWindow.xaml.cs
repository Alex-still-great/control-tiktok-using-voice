using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace User_interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpeechService _speechService = new SpeechService();
        private bool _isListening = false;
        public MainWindow()
        {
            InitializeComponent();
            _speechService.OnLogReceived += UpdateLog;

        }

        private void runcode_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (!_isListening)
            {
                btn.IsEnabled = false; // Matikan tombol sementara agar tidak diklik dua kali
                btn.Content = "Loading Model...";

                Task.Run(() =>
                {
                    try
                    {
                        _speechService.StartListening();

                        // Jika berhasil, update UI melalui Dispatcher
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _isListening = true;
                            btn.Content = "Berhenti";
                            btn.IsEnabled = true;
                            UpdateLog("Sistem Siap!");
                        }));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            btn.Content = "Mulai Deteksi Suara";
                            btn.IsEnabled = true;
                            UpdateLog("Error: " + ex.Message);
                        }));
                    }
                });
            }
            else
            {
                _speechService.StopListening();
                btn.Content = "Mulai Deteksi Suara";
                _isListening = false;
                UpdateLog("Sistem Dimatikan.");
            }
        
        }

        private void settings(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateLog(string message)
        {
            // PENTING: Gunakan Dispatcher agar bisa update UI dari thread lain
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string timeStamp = DateTime.Now.ToString("HH:mm:ss");
                ListLogs.Items.Insert(0, $"[{timeStamp}] {message}"); // Tambahkan ke paling atas

                // Batasi agar log tidak terlalu panjang (opsional)
                if (ListLogs.Items.Count > 100) ListLogs.Items.RemoveAt(100);
            }));
        }
    }
}
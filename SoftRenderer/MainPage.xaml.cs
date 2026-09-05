using SoftRenderer.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using SharpDX;

namespace SoftRenderer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Device device;
        private Mesh mesh = new Mesh("Cube", 8);
        private Camera camera = new Camera();

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WriteableBitmap bitmap = new WriteableBitmap(640, 480);
            device = new Device(bitmap);

            frontBuffer.Source = bitmap;

            mesh.vertices[0] = new Vector3(-1, 1, 1);
            mesh.vertices[1] = new Vector3(1, 1, 1);
            mesh.vertices[2] = new Vector3(-1, -1, 1);
            mesh.vertices[3] = new Vector3(-1, -1, -1);
            mesh.vertices[4] = new Vector3(-1, 1, -1);
            mesh.vertices[5] = new Vector3(1, 1, -1);
            mesh.vertices[6] = new Vector3(1, -1, 1);
            mesh.vertices[7] = new Vector3(1, -1, -1);

            camera.position = new Vector3(0, 0, 10f);
            camera.target = Vector3.Zero;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, object e)
        {
            device.Clear(0, 0, 0, 225);
            mesh.rotation = new Vector3(mesh.rotation.X + 0.01f, mesh.rotation.Y + 0.01f, mesh.rotation.Z);

            device.Render(camera, mesh);
            device.Present();
        }
    }
}

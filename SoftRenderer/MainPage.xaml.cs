using SoftRenderer.Models;
using SoftRenderer.Rendering;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using SharpDX;
using System.Diagnostics;

namespace SoftRenderer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Device device;
        private Mesh[] meshes;
        private Camera camera = new Camera();

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WriteableBitmap bitmap = new WriteableBitmap(640, 480);
            frontBuffer.Source = bitmap;
            device = new Device(bitmap);

            meshes = await device.LoadJSONFileAsync("monkey.babylon");

            camera.position = new Vector3(0, 0, 10f);
            camera.target = Vector3.Zero;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, object e)
        {
            device.Clear(0, 0, 0, 225);
            foreach (var mesh in meshes)
            {
                mesh.rotation = new Vector3(mesh.rotation.X + 0.01f, mesh.rotation.Y + 0.01f, mesh.rotation.Z);
            }

            device.Render(camera, meshes);
            device.Present();
        }
    }
}

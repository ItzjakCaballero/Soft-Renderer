using SharpDX;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Media.Imaging;

namespace SoftRenderer.Models
{
    public class Device
    {
        private byte[] backBuffer;
        private WriteableBitmap bitmap;

        public Device(WriteableBitmap bmp)
        {
            bitmap = bmp;

            backBuffer = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        }

        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (int i = 0; i < backBuffer.Length; i += 4)
            {
                backBuffer[i] = b;
                backBuffer[i + 1] = g;
                backBuffer[i + 2] = r;
                backBuffer[i + 3] = a;
            }
        }

        public void Present()
        {
            using (Stream stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(backBuffer, 0, backBuffer.Length);
            }

            bitmap.Invalidate();
        }

        public void PutPixel(int x, int y, Color4 color)
        {
            int index = (x + y * bitmap.PixelWidth) * 4;

            backBuffer[index] = (byte)(color.Blue * 255);
            backBuffer[index + 1] = (byte)(color.Green * 255);
            backBuffer[index + 2] = (byte)(color.Red * 255);
            backBuffer[index + 3] = (byte)(color.Alpha * 255);
        }

        public Vector2 Project(Vector3 coord, Matrix transformMatrix)
        {
            Vector3 point = Vector3.TransformCoordinate(coord, transformMatrix);

            float x = point.X * bitmap.PixelWidth + bitmap.PixelWidth / 2f;
            float y = point.Y * bitmap.PixelHeight + bitmap.PixelHeight / 2f;

            return new Vector2(x, y);
        }

        public void DrawPoint(Vector2 point)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < bitmap.PixelWidth && point.Y < bitmap.PixelHeight)
            {
                PutPixel((int)point.X, (int)point.Y, new Color4(1, 1, 0, 1));
            }
        }

        public void DrawLine(Vector2 point1, Vector2 point2)
        {
            float distance = (point2 - point1).Length();

            if (distance < 2)
            {
                return;
            }

            Vector2 midPoint = point1 + (point2 - point1)/2;

            DrawPoint(midPoint);
            DrawLine(point1, midPoint);
            DrawLine(midPoint, point2);
        }

        public void Render(Camera camera, params Mesh[] meshes)
        {
            Matrix viewMatrix = Matrix.LookAtLH(camera.position, camera.target, Vector3.UnitY);
            Matrix projectionMatrix = Matrix.PerspectiveFovRH(0.78f, (float)bitmap.PixelWidth / bitmap.PixelHeight, 01f, 1f);

            foreach (Mesh mesh in meshes)
            {
                Matrix worldMatrix = Matrix.RotationYawPitchRoll(mesh.rotation.Y, mesh.rotation.X, mesh.rotation.Z) * Matrix.Translation(mesh.posiiton);
                Matrix transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                for (int i = 0; i < mesh.vertices.Length - 1; i++)
                {
                    Vector2 point1 = Project(mesh.vertices[i], transformMatrix);
                    Vector2 point2 = Project(mesh.vertices[i + 1], transformMatrix);
                    DrawLine(point1, point2);
                }
            }
        }
    }
}

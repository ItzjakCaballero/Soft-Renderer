using SharpDX;
using SoftRenderer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SoftRenderer.Rendering
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

        public async Task<Mesh[]> LoadJSONFileAsync(string fileName)
        {
            List<Mesh> meshes = new List<Mesh>();
            StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(fileName);
            string data = await FileIO.ReadTextAsync(file);
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(data);

            for (int i = 0; i < json.meshes.Count; i++)
            {
                var vertices = json.meshes[i].vertices;
                var indices = json.meshes[i].indices;
                var uvCount = json.meshes[i].uvCount.Value;
                int verticesStep = 1;

                switch ((int)uvCount)
                {
                    case 0:
                        verticesStep = 6;
                        break;
                    case 1:
                        verticesStep = 8;
                        break;
                    case 2:
                        verticesStep = 10;
                        break;
                }

                int vertexCount = vertices.Count / verticesStep;
                int faceCount = indices.Count / 3;
                Mesh mesh = new Mesh(json.meshes[i].name.Value, vertexCount, faceCount);

                for (int j = 0; j < vertexCount; j++)
                {
                    float x = (float)vertices[j * verticesStep].Value;
                    float y = (float)vertices[j * verticesStep + 1].Value;
                    float z = (float)vertices[j * verticesStep + 2].Value;
                    mesh.vertices[j] = new Vector3(x, y, z);
                }

                for (int index = 0; index < faceCount; index++)
                {
                    var a = (int)indices[index * 3].Value;
                    var b = (int)indices[index * 3 + 1].Value;
                    var c = (int)indices[index * 3 + 2].Value;
                    mesh.faces[index] = new Face { a = a, b = b, c = c };
                }

                var position = json.meshes[i].position;
                mesh.posiiton = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
                meshes.Add(mesh);

                mesh.PrintVertices();
                mesh.PrintFaces();
            }

            return meshes.ToArray();
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

        //public void DrawLine(Vector2 point1, Vector2 point2)
        //{
        //    float distance = (point2 - point1).Length();

        //    if (distance < 2)
        //    {
        //        return;
        //    }

        //    Vector2 midPoint = point1 + (point2 - point1)/2;

        //    DrawPoint(midPoint);
        //    DrawLine(point1, midPoint);
        //    DrawLine(midPoint, point2);
        //}

        public void DrawLine(Vector2 point1, Vector2 point2)
        {
            int x1 = (int)point1.X;
            int y1 = (int)point1.Y;
            int x2 = (int)point2.X;
            int y2 = (int)point2.Y;

            int distX = Math.Abs(x2 - x1);
            int distY = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = distX - distY;

            while (true)
            {
                DrawPoint(new Vector2(x1, y1));

                if ((x1 == x2) && (y1 == y2))
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 > -distY)
                {
                    err -= distY;
                    x1 += sx;
                }
                if (e2 < distX)
                {
                    err += distX;
                    y1 += sy;
                }
            }
        }

        public void Render(Camera camera, params Mesh[] meshes)
        {
            Matrix viewMatrix = Matrix.LookAtLH(camera.position, camera.target, Vector3.UnitY);
            Matrix projectionMatrix = Matrix.PerspectiveFovRH(0.78f, (float)bitmap.PixelWidth / bitmap.PixelHeight, 01f, 1f);

            foreach (Mesh mesh in meshes)
            {
                Matrix worldMatrix = Matrix.RotationYawPitchRoll(mesh.rotation.Y, mesh.rotation.X, mesh.rotation.Z) * Matrix.Translation(mesh.posiiton);
                Matrix transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                foreach (Face face in mesh.faces)
                {
                    Vector3 v1 = mesh.vertices[face.a];
                    Vector3 v2 = mesh.vertices[face.b];
                    Vector3 v3 = mesh.vertices[face.c];

                    Vector2 p1 = Project(v1, transformMatrix);
                    Vector2 p2 = Project(v2, transformMatrix);
                    Vector2 p3 = Project(v3, transformMatrix);

                    DrawLine(p1, p2);
                    DrawLine(p2, p3);
                    DrawLine(p3, p1);
                }
            }
        }
    }
}

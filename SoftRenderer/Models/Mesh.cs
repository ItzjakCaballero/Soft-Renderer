using SharpDX;
using System.Diagnostics;

namespace SoftRenderer.Models
{
    public struct Face
    {
        public int a, b, c;
    }

    public class Mesh
    {
        public string name;
        public Vector3 posiiton;
        public Vector3 rotation;
        public Face[] faces;

        public Vector3[] vertices { get; private set; }

        public Mesh(string name, int vertexCount, int faceCount)
        {
            this.name = name;
            vertices = new Vector3[vertexCount];
            faces = new Face[faceCount];
        }

        public void PrintVertices()
        {
            string output = $"{name} vertices: ";
            foreach (Vector3 vertex in vertices)
            {
                output += $"({vertex.X}, {vertex.Y}, {vertex.Z}), ";
            }
            output.Substring(0, output.Length - 2);

            Debug.WriteLine(output);
        }

        public void PrintFaces()
        {
            string output = $"{name} faces: ";
            foreach (Face face in faces)
            {
                output += $"({face.a}, {face.b}, {face.c}), ";
            }
            output.Substring(0, output.Length - 2);

            Debug.WriteLine(output);
        }
    }
}

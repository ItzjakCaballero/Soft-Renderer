using SharpDX;

namespace SoftRenderer.Models
{
    public class Mesh
    {
        public string name;
        public Vector3 posiiton;
        public Vector3 rotation;

        public Vector3[] vertices { get; private set; }

        public Mesh(string name, int vertexCount)
        {
            this.name = name;
            vertices = new Vector3[vertexCount];
        }
    }
}

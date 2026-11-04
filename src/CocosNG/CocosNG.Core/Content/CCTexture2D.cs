namespace CocosNG.Core.Content
{
    public class CCTexture2D
    {
        private static int s_nextTextureId = 1;

        public int TextureId { get; } = s_nextTextureId++;
    }
}

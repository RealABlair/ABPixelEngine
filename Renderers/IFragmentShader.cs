using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ABSoftware.ABPixelEngine.Renderers
{
    public interface IFragmentShader
    {
        void Apply(RenderBuffer buffer);
    }
}

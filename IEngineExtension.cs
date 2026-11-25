using System;

namespace ABSoftware.ABPixelEngine
{
    public interface IEngineExtension
    {
        void OnInitialize(PixelEngineContext context);
        void OnStart();
        void OnUpdate();
        void OnFixedUpdate();
        void OnRender();
        void OnStop();
    }
}

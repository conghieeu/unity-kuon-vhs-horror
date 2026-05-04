using System;

namespace UHFPS.Runtime
{
    public interface IReticleProvider
    {
        (Type, Reticle, bool) OnProvideReticle();
    }
}
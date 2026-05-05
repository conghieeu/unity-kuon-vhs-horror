namespace UHFPS.Runtime
{
    public interface IDynamicUnlock
    {
        void OnTryUnlock(DynamicObject dynamicObject);
    }
}
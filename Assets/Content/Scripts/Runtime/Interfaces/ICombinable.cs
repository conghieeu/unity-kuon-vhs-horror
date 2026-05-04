namespace UHFPS.Runtime
{
    public interface ICombinable
    {
        void OnCombine();
        string OnCombineGetKey();
        bool OnCanCombine();
    }
}
namespace UHFPS.Runtime
{
    public interface IExamineClick
    {
        public void OnExamineClick();
    }

    public interface IExamineDragVertical
    {
        public void OnExamineDragVertical(float dragDelta);
    }

    public interface IExamineDragHorizontal
    {
        public void OnExamineDragHorizontal(float dragDelta);
    }
}
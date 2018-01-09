namespace Virtualization
{
    public interface ISelectable
    {
        string Id { get; set; }
        bool IsSelected { get; set; }
    }
}

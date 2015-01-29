namespace Kidozen
{
    public interface IDeviceStorage
    {
        string GetBasePath();
        void WriteText(string path, string content);
        string ReadAllText(string path);
    }
}

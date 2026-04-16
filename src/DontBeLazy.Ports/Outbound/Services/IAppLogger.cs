namespace DontBeLazy.Ports.Outbound.Services;

public interface IAppLogger
{
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? ex = null);
}

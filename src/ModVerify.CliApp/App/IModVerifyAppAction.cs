using System.Threading.Tasks;

namespace AET.ModVerify.App;

internal interface IModVerifyAppAction
{
    Task<int> ExecuteAsync();
}
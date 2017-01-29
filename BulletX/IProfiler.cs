
namespace BulletX
{
    /// <summary>
    /// BulletX時間計測用プロファイラ
    /// </summary>
    public interface IProfiler
    {
        void StartProfile(string profile);
        void EndProfile(string profile);

        void BeginProfileFrame();

        void EndProfileFrame();
    }
}

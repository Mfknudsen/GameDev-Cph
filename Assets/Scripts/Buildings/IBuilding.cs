#region Packages

using GameDev.Multiplayer;

#endregion

namespace GameDev.Buildings
{
    public interface IBuilding
    {
        int GetCost();

        void Place();

        void Destroy();
    }
}
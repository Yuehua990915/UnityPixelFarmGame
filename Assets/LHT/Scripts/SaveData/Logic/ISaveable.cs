namespace Farm.Save
{
    public interface ISaveable
    {
        //唯一标识符
        string GUID { get; }
        
        //注册
        void RegisterSaveable()
        {
            SaveLoadManager.Instance.RegisterSaveable(this);
        }
        //存储数据
        GameSaveData GenerateSaveData();
        //加载数据
        void RestoreData(GameSaveData gameSaveData);
    }
}
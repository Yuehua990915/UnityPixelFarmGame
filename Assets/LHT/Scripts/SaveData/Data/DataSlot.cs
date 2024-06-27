using System.Collections.Generic;

namespace Farm.Save
{
    //一个存档对应一个slot
    public class DataSlot
    {
        //GUID、data
        public Dictionary<string, GameSaveData> dataDic = new Dictionary<string, GameSaveData>();

        public string SaveDataTime
        {
            get
            {
                var key = TimeManager.Instance.GUID;

                if (dataDic.ContainsKey(key))
                {
                    string year = dataDic[key].timeDic["Year"].ToString();
                    string month = dataDic[key].timeDic["Month"].ToString();
                    string day = dataDic[key].timeDic["Day"].ToString();

                    return new string("第" + year + "年" + month + "月" + day + "日");
                }
                else
                    return string.Empty;
            }
            
        }
    }
}
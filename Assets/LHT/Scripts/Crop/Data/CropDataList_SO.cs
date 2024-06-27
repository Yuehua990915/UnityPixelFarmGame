namespace Farm.CropPlant
{
    using UnityEngine;
    using System.Collections.Generic;

    [CreateAssetMenu(fileName = "CropDataList_SO", menuName = "Crop/CropDataList")]
    public class CropDataList_SO : ScriptableObject
    {
        public List<CropDetails> cropDetailsList;
    }
}
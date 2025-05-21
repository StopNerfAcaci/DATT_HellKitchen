using UnityEngine;

public class ResetStaticDataManager : MonoBehaviour
{
    private void Awake()
    {
        CuttingCounter.ResetCutStaticData();
        BaseCounter.ResetStaticData();
        TrashCounter.ResetTrashStaticData();
        Player.ResetStaticData();
    }
}

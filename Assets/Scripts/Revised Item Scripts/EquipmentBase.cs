using UnityEngine;

public abstract class EquipmentBase : MonoBehaviour
{
    protected bool isEquipped = false;

    public virtual void EnableEquipment()
    {
        isEquipped = true;
        SoundManager.PlaySound(SoundType.SWAP);
        gameObject.SetActive(true);
    }

    public virtual void DisableEquipment()
    {
        isEquipped = false;
        gameObject.SetActive(false);
    }
}

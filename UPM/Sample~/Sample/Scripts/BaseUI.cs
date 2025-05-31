using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    public virtual void Init() { }
    public virtual void Deinit() { }
    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}

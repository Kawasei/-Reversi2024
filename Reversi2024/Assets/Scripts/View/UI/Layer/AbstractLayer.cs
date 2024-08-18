using UnityEngine;

namespace Reversi2024.View.UI
{
    public abstract class AbstractLayer : MonoBehaviour, ILayer
    {
        public virtual void Open()
        {
            this.gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            this.gameObject.SetActive(false);
        }
    }
}

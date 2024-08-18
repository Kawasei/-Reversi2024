
using UnityEngine;

namespace Reversi2024.View.UI
{
    public class LoadingLayer : AbstractLayer
    {
        [SerializeField] private RectTransform stoneImages;
        
        private void Update()
        {
            var angles = stoneImages.transform.localEulerAngles;
            angles.y += Time.deltaTime * 120.0f;
            stoneImages.transform.localEulerAngles = angles;
        }
    }
}

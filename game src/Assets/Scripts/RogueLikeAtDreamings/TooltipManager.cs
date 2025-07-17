using System;
using TMPro;
using UnityEngine;

namespace RogueLikeAtDreamings.Tooltips
{
    [Serializable]
    public struct TooltipInformation
    {
        public RectTransform attached;
        public string text;
    }

    public class TooltipManager : MonoBehaviour
    {
        public Challenge[] challenges;
        public GameObject tooltipObject;
        public TextMeshProUGUI information;

        void Start()
        {
            tooltipObject.SetActive(false);
        }

        void Update()
        {
            bool isHoveringOverButton = false;
            foreach (Challenge challenge in challenges)
            {
                TooltipInformation info = challenge.tooltip;
                if (RectTransformContainsPoint(info.attached, Input.mousePosition))
                {
                    information.text = info.text;

                    Vector3 newPosition = tooltipObject.transform.position;
                    newPosition.y = info.attached.position.y;
                    tooltipObject.transform.position = newPosition;

                    isHoveringOverButton = true;
                }
            }
            tooltipObject.SetActive(isHoveringOverButton);
        }

        private bool RectTransformContainsPoint(RectTransform rect, Vector3 point)
        {
            Vector2 localPosition = rect.InverseTransformPoint(point);
            return rect.rect.Contains(localPosition);
        }
    }
}
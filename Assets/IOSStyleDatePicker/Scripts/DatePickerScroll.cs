// Scroll is inspired from https://assetstore.unity.com/packages/tools/input-management/scroll-flow-190674

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IOSStyleDatePicker.Scripts
{
    internal enum DateObject { Month, Day, Year }

    public class DatePickerScroll : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private DateObject dateObject;

        public GameObject datePickerScrollItem;

        [Header("Required objects")]
        public RectTransform content;

        [Header("Settings")]
        public float heightTemplate;
        public AnimationCurve curve;
        public AnimationCurve curveShift;
        public float shiftUp;
        public float shiftDown;
        [Range(0, 1)]
        public float colorPad;
        public float maxFontSize;

        bool isDragging;
        float inertia, startPosContent, startPosMouse, swipeDistance, middle, heightText = 27;
        int countCheck = 4, currentCenter, yearsMin;

        private List<string> list = new();

        private void Awake()
        {
            if (dateObject == DateObject.Month)
            {
                list = new List<string> {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};
            }
            else if (dateObject == DateObject.Day)
            {
                IEnumerable<int> days = Enumerable.Range(1, 31);
                foreach (var item in days) list.Add(item.ToString());
            }
            else if (dateObject == DateObject.Year)
            {
                yearsMin = System.DateTime.Now.Year - 100;
                int yearsMax = System.DateTime.Now.Year - 5;
                
                IEnumerable<int> years = Enumerable.Range(yearsMin, yearsMax - yearsMin + 1);
                foreach (var item in years) list.Add(item.ToString());
            }
        }

        private void Start()
        {
            float widthParent = transform.parent.GetComponent<RectTransform>().rect.width;
            float width = 0f, moveX = 0f;
        
            if (dateObject == DateObject.Month)
            {
                width = widthParent * 0.58f;
            }
            else if (dateObject == DateObject.Day)
            {
                width = widthParent * 0.15f;
                moveX = widthParent * 0.58f;
            }
            else if (dateObject == DateObject.Year)
            {
                width = widthParent * 0.27f;
                moveX = widthParent * 0.73f;
            }
        
            RectTransform rectTransform = GetComponent<RectTransform>();  
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
            transform.localPosition += new Vector3(moveX, 0, 0);
            heightText = heightTemplate / 2;
            middle = GetComponent<RectTransform>().sizeDelta.y / 2;
            countCheck = Mathf.CeilToInt(middle * 2 / heightTemplate);

            Initialize(list);
        }

        private void Initialize(List<string> items)
        {
            for (int i = 0; i < content.childCount; i++) Destroy(content.GetChild(i).gameObject);
            
            content.anchoredPosition = new Vector2(0, 0);

            foreach (string item in items)
            {
                Transform component = Instantiate(datePickerScrollItem, content.transform).transform;

                TextMeshProUGUI textComponent = component.GetChild(1).GetComponent<TextMeshProUGUI>();
                textComponent.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, heightTemplate);
                textComponent.transform.parent.name = item;
                textComponent.text = item;
                textComponent.fontSize = heightTemplate * 2/3;
                if (dateObject == DateObject.Month) textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                if (dateObject == DateObject.Day) textComponent.alignment = TextAlignmentOptions.Midline;
                if (dateObject == DateObject.Year) textComponent.alignment = TextAlignmentOptions.MidlineRight;

                if (item == items.Last()) component.GetChild(2).GetComponent<Image>().gameObject.SetActive(true);
            }

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            float sizeTotal = middle - heightText;
        
            foreach (Transform transform in content.transform)
            {
                RectTransform rect = transform.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, -rect.sizeDelta.y * (1 - rect.pivot.y) - sizeTotal);
                sizeTotal += rect.sizeDelta.y;
            }

            sizeTotal += middle - heightText;

            content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, sizeTotal);
        }

        private void Update()
        {
            if (isDragging)
            {
                inertia = startPosContent + swipeDistance;
                content.anchoredPosition = new Vector2(0, Mathf.Clamp(inertia, 0, content.sizeDelta.y - middle * 2));
            }
            else
            {
                content.anchoredPosition = new Vector2(0, Mathf.Lerp(content.anchoredPosition.y, -content.transform.GetChild(currentCenter).GetComponent<RectTransform>().anchoredPosition.y - middle, 10*Time.deltaTime));
            }

            float contentPos = content.anchoredPosition.y;

            int startPoint = Mathf.CeilToInt((contentPos - (middle + heightText)) / (heightText * 2));
            int minID = Mathf.Max(0, startPoint);
            int maxID = Mathf.Min(content.transform.childCount, startPoint + countCheck + 1);
            minID = Mathf.Clamp(minID, 0, int.MaxValue);
            maxID = Mathf.Clamp(maxID, 0, int.MaxValue);
            currentCenter = Mathf.Clamp(Mathf.RoundToInt(contentPos / (heightText * 2)), 0, content.childCount - 1);

            if (maxID <= minID) return;
            for (int i = minID; i < maxID; i++)
            {
                RectTransform currentRect = content.transform.GetChild(i).GetComponent<RectTransform>();
                TextMeshProUGUI currentText = content.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>();
                float ratio = Mathf.Clamp(1 - Mathf.Abs(contentPos + currentRect.anchoredPosition.y + middle) / middle, 0, 1);
                currentText.GetComponent<RectTransform>().anchoredPosition = contentPos + currentRect.anchoredPosition.y + middle > 0 ? new Vector2(0, -curveShift.Evaluate(1 - ratio) * shiftUp) : new Vector2(0, curveShift.Evaluate(1 - ratio) * shiftDown);
                currentText.fontSize = maxFontSize * curve.Evaluate(ratio);
                currentText.color = new Vector4(currentText.color.r, currentText.color.g, currentText.color.b, Mathf.Clamp((ratio - colorPad) / (1 - colorPad), 0, 1));
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            startPosMouse = eventData.position.y;
            startPosContent = content.anchoredPosition.y;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            isDragging = true; 
            swipeDistance = eventData.position.y - startPosMouse;
        }

        public string GetValue()
        {
            int value = currentCenter + 1;
            if (dateObject == DateObject.Year) value += yearsMin - 1;
            return value.ToString();
        }
    }
}

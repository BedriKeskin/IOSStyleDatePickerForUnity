using System;
using UnityEngine;
using UnityEngine.UI;

namespace IOSStyleDatePicker.Scripts
{
    public class PickDateSample : MonoBehaviour
    {
        public DatePickerPrefab datePickerPrefab;
        public Button pickDate;

        private void Start()
        {
            pickDate.onClick.AddListener(() =>
            {
                DateTimeOffset? dateTimeOffset = datePickerPrefab.GetDate();

                if (dateTimeOffset != null)
                {
                    Debug.Log("Selected Date: " + dateTimeOffset);
                }
                else
                {
                    Debug.Log("Invalid Date");
                }
            });
        }
    }
}

using System;
using UnityEngine;

namespace IOSStyleDatePicker.Scripts
{
    public class DatePickerPrefab : MonoBehaviour
    {
        public DatePickerScroll day, month, year;

        public DateTimeOffset? GetDate()
        {
            try
            {
                return DateTime.Parse(year.GetValue() + "-" + month.GetValue() + "-" + day.GetValue());
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

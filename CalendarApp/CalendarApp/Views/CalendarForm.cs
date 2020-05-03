﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Data.SqlClient;
using CalendarApp.Models;
using CalendarApp.Views;

namespace CalendarApp
{
    public partial class CalendarForm : Form
    {
        DateTime selectedDate;
        int daysInSelectedMonth;
        int daysBetweenMondayAndFirstDayOfSelectedMonth;
        int weeksInSelectedMonth;
        int iteratorDay;
        private readonly DayOfWeek[] weekDays = {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                                                 DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday};
        public CalendarForm()
        {
            InitializeComponent();
            selectedDate = DateTime.Today;
            ShowCalendar();
        }

        private void ShowCalendar()
        {
            calendarGridView.Rows.Clear();
            UpdateBasicCalendarInformation();
            MakeCalendarTable();
            if (selectedDate == DateTime.Today)
            {
                PaintToday();
            }
        }

        private void UpdateBasicCalendarInformation()
        {
            if (calendarGridView.Columns.Count > Constants.DaysInWeek)
            {
                calendarGridView.Columns.RemoveAt(Constants.DefaultInitialIndex);
            }
            for (int dayColumn = Constants.DefaultInitialIndex; dayColumn < calendarGridView.Columns.Count; dayColumn++)
            {
                calendarGridView.Columns[dayColumn].HeaderText = weekDays[dayColumn].ToString();
            }
            monthLabel.Text = selectedDate.ToString(Constants.MonthAndYearFormat, new CultureInfo(Constants.EnglishLanguageCode));
            daysInSelectedMonth = DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month);
            daysBetweenMondayAndFirstDayOfSelectedMonth = GetDaysBetweenMondayAndFirstDayOfSelectedMonth();
            weeksInSelectedMonth = (int)Math.Ceiling((daysInSelectedMonth + daysBetweenMondayAndFirstDayOfSelectedMonth) / (float)Constants.DaysInWeek);
            iteratorDay = Constants.DefaultFirstDay;
        }

        private void MakeCalendarTable()
        {
            for (int week = Constants.DefaultInitialIndex; week < weeksInSelectedMonth; week++)
            {
                calendarGridView.Rows.Add(GetWeekRow(week.Equals(Constants.DefaultInitialIndex)));
            }
        }
        private string[] GetWeekRow(bool isFirstWeek)
        {
            List<string> weekRow = new List<string>();
            for (int weekDay = Constants.DefaultInitialIndex; weekDay < Constants.DaysInWeek; weekDay++)
            {
                if ((weekDay < daysBetweenMondayAndFirstDayOfSelectedMonth && isFirstWeek) || (iteratorDay > daysInSelectedMonth))
                {
                    weekRow.Add(Constants.Empty);
                }
                else
                {
                    weekRow.Add(iteratorDay.ToString());
                    iteratorDay++;
                }
            }
            return weekRow.ToArray();
        }

        private void PaintToday()
        {
            for (int row = Constants.DefaultInitialIndex; row < calendarGridView.RowCount; row++)
            {
                for (int column = Constants.DefaultInitialIndex; column < calendarGridView.ColumnCount; column++)
                {
                    if (calendarGridView.Rows[row].Cells[column].Value.ToString() == selectedDate.Day.ToString()){
                        calendarGridView.Rows[row].Cells[column].Style.BackColor = Color.LightCoral;
                    }
                }
            }
        }

        private int GetDaysBetweenMondayAndFirstDayOfSelectedMonth()
        {
            DateTime firstDateOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, Constants.DefaultFirstDay);
            return ((int)firstDateOfMonth.DayOfWeek) - Constants.GapBetweenIndexAndNumber;
        }

        private void NextTimePeriodButton_Click(object sender, EventArgs e)
        {
            if (calendarDisplayMenuListBox.SelectedItem.ToString() == Constants.MonthOptionFromCalendarDisplayMenu)
            {
                selectedDate = selectedDate.AddMonths(Constants.NextTimeInterval);
            }
            else if (calendarDisplayMenuListBox.SelectedItem.ToString() == Constants.WeekOptionFromCalendarDisplayMenu)
            {
                selectedDate = selectedDate.AddDays(Constants.NextTimeInterval * Constants.DaysInWeek);
            }
            ShowSelectedDisplay();
        }

        private void PreviousTimePeriodButton_Click(object sender, EventArgs e)
        {
            if (calendarDisplayMenuListBox.SelectedItem.ToString() == Constants.MonthOptionFromCalendarDisplayMenu)
            {
                selectedDate = selectedDate.AddMonths(Constants.PreviousTimeInterval);
            }
            else if (calendarDisplayMenuListBox.SelectedItem.ToString() == Constants.WeekOptionFromCalendarDisplayMenu)
            {
                selectedDate = selectedDate.AddDays(Constants.PreviousTimeInterval * Constants.DaysInWeek);
            }
            ShowSelectedDisplay();
        }

        private void TodayButton_Click(object sender, EventArgs e)
        {
            selectedDate = DateTime.Today;
            ShowSelectedDisplay();
        }

        private void GoToCreateEventFormButton_Click(object sender, EventArgs e)
        {
            CreateEventForm createEventForm = new CreateEventForm();
            createEventForm.Show();
        }

        private void CalendarDisplayMenuListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelectedDisplay();
        }

        private void ShowSelectedDisplay()
        {
            if (calendarDisplayMenuListBox.SelectedItem.ToString() == Constants.MonthOptionFromCalendarDisplayMenu)
            {
                ShowCalendar();
            }
            else if (calendarDisplayMenuListBox.SelectedItem.ToString() == Constants.WeekOptionFromCalendarDisplayMenu)
            {
                ShowWeek();
            }
        }

        private void ShowWeek()
        {
            calendarGridView.Rows.Clear();
            if (calendarGridView.Columns.Count == Constants.DaysInWeek)
            {
                DataGridViewTextBoxColumn columnOfHours = new DataGridViewTextBoxColumn();
                calendarGridView.Columns.Insert(Constants.DefaultInitialIndex, columnOfHours);
            }
            UpdateColumnsHeaders();
        }

        private void UpdateColumnsHeaders()
        {
            DateTime iteratorDayOfWeek = selectedDate;
            while (iteratorDayOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                iteratorDayOfWeek = iteratorDayOfWeek.AddDays(Constants.PreviousTimeInterval);
            }
            for (int dayColumn = Constants.GapBetweenHoursColumnAndMondayColumn; dayColumn < calendarGridView.Columns.Count; dayColumn++)
            {
                calendarGridView.Columns[dayColumn].HeaderText = weekDays[dayColumn - Constants.GapBetweenHoursColumnAndMondayColumn].ToString() + Constants.Space + iteratorDayOfWeek.Day.ToString();
                iteratorDayOfWeek = iteratorDayOfWeek.AddDays(Constants.NextTimeInterval);
            }
            for (int hour = Constants.DefaultInitialIndex; hour < Constants.HoursInDay; hour++)
            {
                calendarGridView.Rows.Add(GetHourlyRow(hour));
            }
        }
        
        private string[] GetHourlyRow(int hour)
        {
            List<string> hourlyRow = new List<string>();
            foreach (DataGridViewColumn column in calendarGridView.Columns)
            {
                if (column.Index == 0)
                {
                    hourlyRow.Add(hour.ToString() + Constants.ZerosOfHour);
                }
                else
                {
                    hourlyRow.Add(Constants.Empty);
                }
            }
            return hourlyRow.ToArray();
        }
    }
}

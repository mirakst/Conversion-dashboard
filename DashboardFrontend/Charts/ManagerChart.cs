﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Markup;
using DashboardFrontend.Charts;
using LiveChartsCore.Defaults;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace DashboardFrontend.Charts
{
    /// <summary>
    /// Empty chart class to add manager data to.
    /// </summary>
    public class ManagerChart : BaseChart
    {
        public ManagerChart(string YAxisLabel)
        {
            Values = new();
            Series = new();
            XAxis = new()
            {
                new Axis
                {
                    Name = "Time",
                    Labeler = value => DateTime.FromOADate(value).ToString("HH:mm:ss"),
                    MinLimit = DateTime.Now.ToOADate(),
                    MaxLimit = DateTime.Now.ToOADate(),
                    LabelsPaint = new SolidColorPaint(new SKColor(255,255,255)),
                }
            };

            YAxis = new()
            {
                new Axis
                {
                    Name = $"{YAxisLabel}",
                    Labeler = (value) => value.ToString("P"),
                    MaxLimit = 1,
                    MinLimit = 0,
                    LabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    MinStep = 0.25,
                    ForceStepToMin = true,
                }
            };
        }
    }
}
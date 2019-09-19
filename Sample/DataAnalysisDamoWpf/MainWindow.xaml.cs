﻿using AnalogDataAnalysisWpf.LiveData;
using CsvHelper;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataAnalysisDamoWpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LiveDataViewModel1 = LiveDataView1.DataContext as LiveDataViewModel;
            LiveDataViewModel2 = LiveDataView2.DataContext as LiveDataViewModel;
        }

        public LiveDataViewModel LiveDataViewModel1 { get; set; }
        public LiveDataViewModel LiveDataViewModel2 { get; set; }

        private double[] deviceData = new double[0];
        private double[] derivativeData = new double[0];

        /// <summary>
        /// 滤波后的数据
        /// </summary>
        private IntPtr FilterData = IntPtr.Zero;
        private int FilterDataCount = 0;

        /// <summary>
        /// 数据采样数量
        /// </summary>
        private int sampleCount = 200;

        private void ReadDataButton_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("./data");
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "csv文件|*.csv",
                InitialDirectory = directoryInfo.FullName,
            };

            if (ofd.ShowDialog() != true)
            {
                return;
            }

            //从CSV中获取数据
            using (var reader = new StreamReader(ofd.FileName))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<double>();
                deviceData = records.ToArray();
            }

            var collection = new System.Collections.ObjectModel.ObservableCollection<Data>();
            for (int i = 0; i < deviceData.Length / sampleCount; i++)
            {
                collection.Add(new Data() { Value1 = deviceData[i * sampleCount], Value = i });
            }

            LiveDataViewModel1.Collection = collection;

            if (FilterData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(FilterData);
                FilterData = IntPtr.Zero;
                FilterDataCount = 0;
            }

            //数据滤波
            IntPtr source = IntPtr.Zero;
            source = Marshal.AllocHGlobal(sizeof(double) * deviceData.Length);
            FilterData = Marshal.AllocHGlobal(sizeof(double) * deviceData.Length);
            FilterDataCount = deviceData.Length;

            //将原始数据从数组转成指针
            Marshal.Copy(deviceData, 0, source, deviceData.Length);

            //均值滤波,得到滤波后的数据FilterData
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            DataFilter.MeanFilter(source, FilterData, deviceData.Length, 15);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);

            var collection2 = new System.Collections.ObjectModel.ObservableCollection<Data>();
            unsafe
            {
                double* destinationIntPtr = (double*)FilterData.ToPointer();

                for (int i = 0; i < deviceData.Length / sampleCount; i++)
                {
                    collection2.Add(new Data() { Value1 = destinationIntPtr[i * sampleCount], Value = i });
                }
            }

            if (source != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(source);
                source = IntPtr.Zero;
            }

            LiveDataViewModel2.Collection = collection2;

        }

        private void DerivativeDataButton_Click(object sender, RoutedEventArgs e)
        {
            //double[] source = new double[deviceData.Length / sampleCount];

            //for (int i = 0; i < deviceData.Length / sampleCount; i++)
            //{
            //    source[i] = deviceData[i * sampleCount];
            //}

            ////微分求导
            //Analysis.Derivative(source, out derivativeData);

            //var collection = new System.Collections.ObjectModel.ObservableCollection<Data>();
            //for (int i = 0; i < derivativeData.Length; i++)
            //{
            //    collection.Add(new Data() { Value1 = derivativeData[i], Value = i });
            //}

            //LiveDataViewModel2.Collection = collection;

            //微分求导
            Analysis.Derivative(deviceData, out derivativeData);

            var collection = new System.Collections.ObjectModel.ObservableCollection<Data>();
            for (int i = 1; i < derivativeData.Length / sampleCount; i++)
            {
                collection.Add(new Data() { Value1 = derivativeData[i * sampleCount], Value = i });
            }

            LiveDataViewModel2.Collection = collection;

        }

        private void FindEdgeButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterData != IntPtr.Zero)
            {
                IntPtr topLocation = Marshal.AllocHGlobal(sizeof(double) * FilterDataCount);
                IntPtr buttomLocation = Marshal.AllocHGlobal(sizeof(double) * FilterDataCount);
                int risingCount = 0;
                int fallingCount = 0;

                DataFilter.FindEdge(FilterData, FilterDataCount, 2, 3, topLocation, buttomLocation, ref risingCount, ref fallingCount);

            }
        }
    }
}

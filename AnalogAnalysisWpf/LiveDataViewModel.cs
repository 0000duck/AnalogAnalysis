﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogAnalysisWpf
{
    public class LiveDataViewModel : Screen
    {
        public LiveDataViewModel()
        {
            Collection = new ObservableCollection<Data>();
        }


        private ObservableCollection<Data> collection;
        public ObservableCollection<Data> Collection
        {
            get
            {
                return collection;
            }
            set
            {
                collection = value;
                NotifyOfPropertyChange(() => Collection);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels
{
    internal class WaveformSourceViewModel : CorePropChangedVM
    {
        private bool _isSelected;
        public string SourceName { get; }
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }

        public WaveformSourceViewModel(string sourceName, bool isSelected)
        {
            SourceName = sourceName;
            _isSelected = isSelected;
        }

    }
}

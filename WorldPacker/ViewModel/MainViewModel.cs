using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using WorldPacker.Classes;

namespace WorldPacker.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IWorldLoader _worldLoader;
        private string _fileName;
        private List<SectionModel> _sections;

        public RelayCommand OpenCommand { get; }
        public RelayCommand PackCommand { get; }

        public MainViewModel(IWorldLoader worldLoader)
        {
            _worldLoader = worldLoader;

            OpenCommand = new RelayCommand(DoOpenCommand);
            PackCommand = new RelayCommand(() => _worldLoader.WriteSections(_sections, _fileName));
        }

        private void DoOpenCommand()
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = "Select location file",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = false,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _fileName = dialog.FileName;

                _sections = _worldLoader.LoadSections(_fileName);

                foreach (var section in _sections)
                {
                    Console.WriteLine($"{section.SectionStruct.Name} ({section.SectionStruct.StreamChunkNumber}) -> file 0x{section.SectionStruct.SubSectionID:X8}, type {section.SectionStruct.Unknown5}, size {section.SectionStruct.FileSize1}");
                }
            }
        }
    }
}

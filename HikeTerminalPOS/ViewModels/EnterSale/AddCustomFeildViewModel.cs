using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HikePOS.Models.Customer;

namespace HikePOS.ViewModels
{
    public class AddCustomFeildViewModel : BaseViewModel
    {
        public EventHandler<ObservableCollection<CustomField>> Saved;
        ObservableCollection<CustomField> _CustomFeildList { get; set; }
        public ObservableCollection<CustomField> CustomFeildList { 
            get
            {
                return _CustomFeildList;
            } 
            set
            {
                _CustomFeildList = value;
                SetPropertyChanged(nameof(CustomFeildList));
            } 
        }
       

        public AddCustomFeildViewModel()
        {
            RemoveCustomFeildListCommand = new Command<CustomField>(removeCustomFieldItem);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            UpdateCustomFeild();
        }
        #region Command
        public ICommand RemoveCustomFeildListCommand { get; }
        public ICommand SaveCommand => new Command(SaveTapped);
        public ICommand AddNewCommand => new Command(AddNewTapped);

        #endregion

        #region Command Execution

        public async void SaveTapped()
        {
            try
            {
                Saved?.Invoke(this, CustomFeildList);
                await Close();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void AddNewTapped()
        {
            if (CustomFeildList != null && CustomFeildList.Count != 4)
            {
                CustomFeildList.Add(new CustomField() { CustomFieldType = CustomFeildList.Count + 1 });
                int index = 1;
                foreach (var item in CustomFeildList)
                {
                    item.Index = index;
                    index++;
                }
            }
        }

        public void removeCustomFieldItem(CustomField customField)
        {
            if (customField.Id == 0)
            {
                CustomFeildList.Remove(CustomFeildList.FirstOrDefault(x => x.Index == customField.Index));
                UpdateCustomFeild();
            }
        }

        #endregion

        public void UpdateCustomFeild()
        {
            if (CustomFeildList != null && CustomFeildList.Count() > 0)
            {
                int index = 1;
                foreach (var item in CustomFeildList)
                {
                    item.Index = index;
                    index++;
                }
            }
        }

    }
}
